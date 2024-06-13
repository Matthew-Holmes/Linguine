using Azure;
using Azure.Core;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Linguine.Tabs.WPF.Controls
{
    /// <summary>
    /// Interaction logic for RichTextControl.xaml
    /// </summary>
    /// 

    // This class requires a parent with certain properties, namely:
    // - FullText, String

    public partial class RichTextControl : UserControl
    {
        // UI                      VM
        // button -------------->  updates LocalCursor
        //                               |
        //                         raises LocalCursorChanged
        // OnLocalCursorChanged  <-------
        //        | 
        // Computes page
        // PageLocatedCommand(int,int) --> invokes further paging/or updates statements

        public static readonly DependencyProperty FullTextProperty =
                    DependencyProperty.Register("FullText", typeof(string), typeof(RichTextControl),
                        new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LocalCursorProperty =
            DependencyProperty.Register(
                "LocalCursor", typeof(int), typeof(RichTextControl),
                    new PropertyMetadata(OnLocalCursorChanged));

        public static readonly DependencyProperty EndOfPageProperty =
            DependencyProperty.Register(
                "EndOfPage", typeof(int), typeof(RichTextControl),
                    new PropertyMetadata(OnEndOfPageChanged));

        public static readonly DependencyProperty SortedStatementStartIndicesProperty =
            DependencyProperty.Register(
                "SortedStatementStartIndices", typeof(List<int>), typeof(RichTextControl),
                    new PropertyMetadata());

        public static readonly DependencyProperty PageLocatedCommandProperty =
            DependencyProperty.Register("PageLocatedCommand", typeof(ICommand), typeof(RichTextControl),
                new PropertyMetadata(null));


        public string FullText
        {
            get => (string)GetValue(FullTextProperty);
            set => SetValue(FullTextProperty, value);
        }

        public int LocalCursor
        {
            get => (int)GetValue(LocalCursorProperty);
            set => SetValue(LocalCursorProperty, value);
        }

        public int EndOfPage
        {
            get => (int)GetValue(EndOfPageProperty);
            set => SetValue(EndOfPageProperty, value);
        }

        public List<int> SortedStatementStartIndices
        {
            get => (List<int>)GetValue(SortedStatementStartIndicesProperty); 
            set => SetValue(SortedStatementStartIndicesProperty, value);
        }

        public ICommand PageLocatedCommand
        {
            get => (ICommand)GetValue(PageLocatedCommandProperty);
            set => SetValue(PageLocatedCommandProperty, value);
        }

        public RichTextControl()
        {
            //https://learn.microsoft.com/en-us/dotnet/desktop/wpf/properties/collection-type-dependency-properties?view=netdesktop-8.0
            //SetValue(SortedStatementStartIndicesProperty, new ObservableCollection<int>());

            InitializeComponent();
            
            this.Loaded += OnLoaded;
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is not null && e.NewValue is TextualMediaViewerViewModel newViewModel)
            {
                loaded = false;
                // Rebind properties or reset if needed
                BindingOperations.ClearBinding(this, FullTextProperty);
                BindingOperations.ClearBinding(this, LocalCursorProperty);
                BindingOperations.ClearBinding(this, EndOfPageProperty);
                BindingOperations.ClearBinding(this, SortedStatementStartIndicesProperty);
                BindingOperations.ClearBinding(this, PageLocatedCommandProperty);

                BindingOperations.SetBinding(this, FullTextProperty, new Binding("FullText") { Source = newViewModel });
                BindingOperations.SetBinding(this, LocalCursorProperty, new Binding("LocalCursor") { Source = newViewModel });
                BindingOperations.SetBinding(this, EndOfPageProperty, new Binding("EndOfPage") { Source = newViewModel });
                BindingOperations.SetBinding(this, SortedStatementStartIndicesProperty, new Binding("SortedStatementStartIndices") { Source = newViewModel });
                BindingOperations.SetBinding(this, PageLocatedCommandProperty, new Binding("PageLocatedCommand") { Source = newViewModel });

                this.InvalidateVisual();
                CalculatePageFromStartIndex(LocalCursor);
                loaded = true;
            }
        }

        public bool loaded = false;
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is TextualMediaViewerViewModel viewModel)
            {
                loaded = true;
                CalculatePageFromStartIndex(LocalCursor);
                return;
            }
            else
            {
                MessageBox.Show("invalid data context!");
            }
        }

        private static void OnLocalCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RichTextControl control)
            {
                if (control.loaded)
                {
                    control.CalculatePageFromStartIndex(control.LocalCursor);
                }
            }
            else
            {
                MessageBox.Show("Failed to respond to local cursor change due to wrong dependency object");
            }
        }

        private static void OnEndOfPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RichTextControl control)
            {
                if (control.loaded)
                {
                    control.CalculatePageFromEndIndex(control.EndOfPage);
                }
            }
            else
            {
                MessageBox.Show("Failed to respond to local cursor change due to wrong dependency object");
            }
        }

        #region paging logic

        private String ClippedSubstring(String str, int start, int span)
        {
            var indices = DoClip(str, start, span);
            return str.Substring(indices.Item1, indices.Item2);
        }

        public Tuple<int,int> DoClip(String str, int start, int span)
        {
            start = Math.Max(start, 0);
            span = Math.Max(span, 0);
            span = Math.Min(span, str.Length - start - 1);
            span = Math.Max(span, 0);

            return Tuple.Create(start, span);
        }

        int _charsPerPageStartGuess = 1000;
        private void CalculatePageFromStartIndex(int newStartIndex)
        {
            int maxHeight = (int)TextDisplayRegion.ActualHeight;

            if (newStartIndex >= FullText.Length) { return; }

            Typeface typeface = new Typeface(
                TextDisplayRegion.FontFamily,
                TextDisplayRegion.FontStyle,
                TextDisplayRegion.FontWeight,
                TextDisplayRegion.FontStretch);

            FormattedText formattedText;

            while (true)
            {
                formattedText = new FormattedText(
                    ClippedSubstring(FullText, newStartIndex, _charsPerPageStartGuess),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    TextDisplayRegion.FontSize,
                    Brushes.Black,
                    new NumberSubstitution(),
                    VisualTreeHelper.GetDpi(TextDisplayRegion).PixelsPerDip);

                bool clipped = FullText.Length < newStartIndex + _charsPerPageStartGuess;

                if (formattedText.Height < maxHeight && !clipped /* if at the end then a small page is fine */)
                {
                    _charsPerPageStartGuess = (int)(_charsPerPageStartGuess * 1.618);
                } 
                else
                {
                    break;
                }
            }

            double ratioOnDisplay = maxHeight / formattedText.Height;
            double charSpan = _charsPerPageStartGuess * ratioOnDisplay;

            while (true)
            {
                formattedText = new FormattedText(
                    ClippedSubstring(FullText, newStartIndex, (int)charSpan),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    TextDisplayRegion.FontSize,
                    Brushes.Black,
                    new NumberSubstitution(),
                    VisualTreeHelper.GetDpi(TextDisplayRegion).PixelsPerDip);

                if (formattedText.Height > maxHeight && charSpan != 0 /* no infinite loop*/)
                {
                    charSpan = charSpan * 0.9; 
                }
                else
                {
                    break;
                }
            }

            int span = (int)charSpan;

            if (SortedStatementStartIndices.Last() > newStartIndex + span)
            {
                // if we have statements use them to for page intervals
                int lastStatementStartListIndex = BinarySearchForLargestIndexBefore(newStartIndex + span);
                int lastStatementStartIndex = SortedStatementStartIndices[lastStatementStartListIndex];
                if (lastStatementStartIndex > newStartIndex + span - 100)
                {
                    span = lastStatementStartIndex - newStartIndex;
                }
            }
            else
            {
                // try to get somewhere to break that is less jarring
                for (int i = 0; i != 50 /* don't strip more than 50 chars */&& FullText.Length - span > 0; i++)
                {
                    if (Char.IsWhiteSpace(FullText[newStartIndex + span - 1]))
                    {
                        break;
                    }
                    else if (Char.IsPunctuation(FullText[newStartIndex + span - 1]))
                    {
                        break;
                    }
                    span--;
                }
            }

            var bounds = DoClip(FullText, newStartIndex, span);

            TextDisplayRegion.Text = FullText.Substring(bounds.Item1, bounds.Item2);

            // this shouldn't message anything, just keeps parity with the ViewModel
            PageLocatedCommand?.Execute(Tuple.Create(bounds.Item1, bounds.Item1 + bounds.Item2 - 1));

        }

        private void CalculatePageFromEndIndex(int newEndIndex)
        {
            int maxHeight = (int)TextDisplayRegion.ActualHeight;

            if (newEndIndex < 0) { return; }

            Typeface typeface = new Typeface(
                TextDisplayRegion.FontFamily,
                TextDisplayRegion.FontStyle,
                TextDisplayRegion.FontWeight,
                TextDisplayRegion.FontStretch);

            FormattedText formattedText;

            while (true)
            {
                formattedText = new FormattedText(
                    ClippedSubstring(FullText, newEndIndex - _charsPerPageStartGuess + 1, _charsPerPageStartGuess),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    TextDisplayRegion.FontSize,
                    Brushes.Black,
                    new NumberSubstitution(),
                    VisualTreeHelper.GetDpi(TextDisplayRegion).PixelsPerDip);

                bool clipped = FullText.Length < newEndIndex + _charsPerPageStartGuess;

                if (formattedText.Height < maxHeight && !clipped /* if at the end then a small page is fine */)
                {
                    _charsPerPageStartGuess = (int)(_charsPerPageStartGuess * 1.618);
                }
                else
                {
                    break;
                }
            }

            double ratioOnDisplay = maxHeight / formattedText.Height;
            double charSpan = _charsPerPageStartGuess * ratioOnDisplay;

            if (newEndIndex - charSpan <= 0)
            {
                CalculatePageFromStartIndex(0); // display the full first page
                return;
            }


            while (true)
            {
                formattedText = new FormattedText(
                    ClippedSubstring(FullText, newEndIndex - (int)charSpan + 1, (int)charSpan),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    TextDisplayRegion.FontSize,
                    Brushes.Black,
                    new NumberSubstitution(),
                    VisualTreeHelper.GetDpi(TextDisplayRegion).PixelsPerDip);

                if (formattedText.Height > maxHeight && charSpan != 0 /* no infinite loop*/)
                {
                    charSpan = charSpan * 0.9;
                }
                else
                {
                    break;
                }
            }

            int span = (int)charSpan;

            if (SortedStatementStartIndices.Last() > newEndIndex - span)
            {
                // if we have statements use them to for page intervals
                int firstStatementStartListIndex = BinarySearchForLargestIndexBefore(newEndIndex - span);
                int firstStatementStartIndex = SortedStatementStartIndices[firstStatementStartListIndex + 1];
                // use the first statement after
                if (firstStatementStartIndex > newEndIndex - span - 100)
                {
                    span = newEndIndex - firstStatementStartIndex + 1;
                }
            }
            else
            {

                // try to get somewhere to break that is less jarring
                for (int i = 0; i != 50 /* don't strip more than 50 chars */&& newEndIndex - span >= 0; i++)
                {
                    if (Char.IsWhiteSpace(FullText[newEndIndex - span]))
                    {
                        break;
                    }
                    else if (Char.IsPunctuation(FullText[newEndIndex - span]))
                    {
                        break;
                    }
                    charSpan--;
                }
            }

            var bounds = DoClip(FullText, newEndIndex - span + 1, span);

            TextDisplayRegion.Text = FullText.Substring(bounds.Item1, bounds.Item2);

            // this shouldn't message anything, just keeps parity with the ViewModel
            PageLocatedCommand?.Execute(Tuple.Create(bounds.Item1, bounds.Item1 + bounds.Item2 - 1));
        }

        // Assuming _statementsStartIndices is a List<int> that is sorted in ascending order

        int BinarySearchForLargestIndexBefore(int target)
        {
            int left = 0;
            int right = SortedStatementStartIndices.Count - 1;

            int mid = 0;
            while (left <= right)
            {
                mid = left + (right - left) / 2;

                if (SortedStatementStartIndices[mid] < target)
                {
                    // Move to the right half to potentially find a larger value
                    left = mid + 1;
                }
                else
                {
                    // If _statementsStartIndices[mid] >= target, move to the left half
                    right = mid - 1;
                }
            }

            return mid;
        }

        #endregion

    }


}
