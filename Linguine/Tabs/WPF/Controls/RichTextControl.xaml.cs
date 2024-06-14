
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

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


        private int LocalCursor;
        private int EndOfPage;
        private String FullText;
        private String PageText;
        private List<int> SortedStatementStartIndices;
        private ICommand PageLocatedCommand; 

        public RichTextControl()
        {
            InitializeComponent();

            this.Loaded += OnLoaded;
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is TextualMediaViewerViewModel oldViewModel)
            {
                oldViewModel.PageForwards  -= HandlePageForwards;
                oldViewModel.PageBackwards -= HandlePageBackwards;

                oldViewModel.StatementsCoveringPageChanged -= ProcessStatementInformation;
                oldViewModel.UnderlyingStatementsChanged   -= UnderlyingStatementsChanged;
            }

            if (e.NewValue is TextualMediaViewerViewModel newViewModel)
            {
                // order matters here, don't move reorder unless sure

                FullText                    = newViewModel.FullText;
                SortedStatementStartIndices = newViewModel.SortedStatementStartIndices;
                PageLocatedCommand          = newViewModel.PageLocatedCommand;
                LocalCursor                 = newViewModel.LocalCursor;

                newViewModel.StatementsCoveringPageChanged += ProcessStatementInformation;
                newViewModel.UnderlyingStatementsChanged   += UnderlyingStatementsChanged;

                if (IsLoaded)
                {
                    // if not loaded the text display region is 0 tall and nothing displays
                    CalculatePageFromStartIndex(LocalCursor); // populates EndOfPage
                    PageLocatedCommand?.Execute(Tuple.Create(LocalCursor, EndOfPage)); // updates the ViewModel
                }

                newViewModel.PageForwards  += HandlePageForwards;
                newViewModel.PageBackwards += HandlePageBackwards;
            }
        }

        private void UnderlyingStatementsChanged(object? sender, List<int> e)
        {
            SortedStatementStartIndices = e;

            // redraw the page
            CalculatePageFromStartIndex(LocalCursor);
            PageLocatedCommand?.Execute(Tuple.Create(LocalCursor, EndOfPage)); 
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is TextualMediaViewerViewModel viewModel)
            {
                // OnLoaded called after DataContext updates, so these won't be doing weird stuff
                CalculatePageFromStartIndex(LocalCursor); // populates EndOfPage
                PageLocatedCommand?.Execute(Tuple.Create(LocalCursor, EndOfPage)); // updates the ViewModel
                return;
            }
            else
            {
                MessageBox.Show("invalid data context!");
            }
        }

        Brush faintGreyBrush = new SolidColorBrush(Color.FromArgb(20, 128, 128, 128));

        private void ProcessStatementInformation(object? sender, List<Statement> statementsCoveringPage)
        {
            TextDisplayRegion.Text = ""; // now using fancier text placement
            TextDisplayRegion.Inlines.Clear();

            List<Tuple<int, int>> statementSpans = statementsCoveringPage.Select(
                s => Tuple.Create(s.FirstCharIndex, s.LastCharIndex)).ToList();

            int currentIndex = LocalCursor;

            foreach (var section in statementSpans)
            {
                int start = section.Item1;
                int end = section.Item2;

                // Add unhighlighted text before the highlighted section
                if (currentIndex < start)
                {
                    TextDisplayRegion.Inlines.Add(new Run(FullText.Substring(currentIndex, start - currentIndex)));
                }

                // Add highlighted text
                if (start < end && end <= EndOfPage)
                {
                    Run highlightRun = new Run(FullText.Substring(start, end - start + 1));
                    highlightRun.Background = faintGreyBrush;
                    TextDisplayRegion.Inlines.Add(highlightRun);
                }

                currentIndex = end+1;
            }

            // Add any remaining unhighlighted text
            if (currentIndex <= EndOfPage)
            {
                TextDisplayRegion.Inlines.Add(new Run(FullText.Substring(currentIndex, EndOfPage - currentIndex + 1)));
            }
        }


        private void HandlePageBackwards(object? sender, int pages)
        {
            if (pages <= 0)
            {
                return;
            }

            for (int i = 0; i != pages; i++)
            {
                CalculatePageFromEndIndex(LocalCursor - 1);
            }

            PageLocatedCommand?.Execute(Tuple.Create(LocalCursor, EndOfPage));
        }

        private void HandlePageForwards(object? sender, int pages)
        {
            if (pages <= 0)
            {
                return;
            }

            for (int i = 0; i != pages; i++)
            {
                CalculatePageFromStartIndex(EndOfPage + 1);
            }

            PageLocatedCommand?.Execute(Tuple.Create(LocalCursor, EndOfPage));
        }



        private String ClippedSubstring(String str, int start, int span)
        {
            var indices = DoClip(str, start, span);
            return str.Substring(indices.Item1, indices.Item2);
        }

        public Tuple<int,int> DoClip(String str, int start, int span)
        {
            start = Math.Max(start, 0);
            start = Math.Min(str.Length - 1, start);

            span = Math.Max(span, 0);
            span = Math.Min(span, str.Length - start - 1);

            return Tuple.Create(start, span);
        }

        int _charsPerPageStartGuess = 1000;
        private void CalculatePageFromStartIndex(int newStartIndex)
        {
            if (newStartIndex >= FullText.Length) { return; }

            TextDisplayRegion.Inlines.Clear();

            int maxHeight = (int)TextDisplayRegion.ActualHeight;

            Typeface typeface = CreateTypeface();

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

            PageText = FullText.Substring(bounds.Item1, bounds.Item2);
            TextDisplayRegion.Text = PageText;

            LocalCursor = bounds.Item1;
            EndOfPage   = bounds.Item1 + bounds.Item2 - 1;
        }

        private void CalculatePageFromEndIndex(int newEndIndex)
        {
            if (newEndIndex < 0) { return; }

            TextDisplayRegion.Inlines.Clear();

            int maxHeight = (int)TextDisplayRegion.ActualHeight;

            Typeface typeface = CreateTypeface();

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

            PageText = FullText.Substring(bounds.Item1, bounds.Item2);
            TextDisplayRegion.Text = PageText;

            LocalCursor = bounds.Item1;
            EndOfPage   = bounds.Item1 + bounds.Item2 - 1;
        }

        private Typeface CreateTypeface()
        {
            return new Typeface(
                TextDisplayRegion.FontFamily,
                TextDisplayRegion.FontStyle,
                TextDisplayRegion.FontWeight,
                TextDisplayRegion.FontStretch);
        }



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

  

    }


}
