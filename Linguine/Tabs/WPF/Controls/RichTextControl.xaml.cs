using Azure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        public static readonly DependencyProperty FullTextProperty =
            DependencyProperty.Register(
                "FullText", typeof(String), typeof(RichTextControl), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LocalCursorProperty = 
            DependencyProperty.Register(
                "LocalCursor", typeof(int), typeof(RichTextControl), new PropertyMetadata(null));

        public static readonly DependencyProperty EndOfPageProperty = 
            DependencyProperty.Register(
                "EndOfPage",   typeof(int), typeof(RichTextControl), new PropertyMetadata(null));

        public static readonly DependencyProperty PageEndLocatedCommandProperty =
            DependencyProperty.Register(
                "PageEndLocatedCommand",   
                typeof(ICommand), typeof(RichTextControl), new PropertyMetadata(null));

        public static readonly DependencyProperty PageStartLocatedCommandProperty =
            DependencyProperty.Register(
                "PageStartLocatedCommand", 
                typeof(ICommand), typeof(RichTextControl), new PropertyMetadata(null));


        public ICommand PageEndLocatedCommand
        {
            get => (ICommand)GetValue(PageEndLocatedCommandProperty);
            set => SetValue(PageEndLocatedCommandProperty, value);
        }

        public ICommand PageStartLocatedCommand
        {
            get => (ICommand)GetValue(PageStartLocatedCommandProperty);
            set => SetValue(PageStartLocatedCommandProperty, value);
        }

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

        public RichTextControl()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is TextualMediaViewerViewModel viewModel)
            {
                return;
            }
            else
            {
                MessageBox.Show("invalid data context!");
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is INotifyPropertyChanged oldViewModel)
            {
                oldViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }

            if (e.NewValue is INotifyPropertyChanged newViewModel)
            {
                newViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        // when these property changed events fire, begins the back and forth between
        // ViewModel and UI to place the text
        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LocalCursor))
            {
                CalculatePageForwards();
            }
            else if (e.PropertyName == nameof(EndOfPage))
            {
                throw new NotImplementedException();
                // Respond to EndOfPage changes
            }
        }


        int _charsPerPageStartGuess = 1000;
        private void CalculatePageForwards()
        {
            int maxHeight = (int)TextDisplayRegion.ActualHeight;
            int startIndex = LocalCursor;

            Typeface typeface = new Typeface(
                TextDisplayRegion.FontFamily,
                TextDisplayRegion.FontStyle,
                TextDisplayRegion.FontWeight,
                TextDisplayRegion.FontStretch);

            FormattedText formattedText;

            while (true)
            {
                formattedText = new FormattedText(
                    FullText.Substring(startIndex, _charsPerPageStartGuess),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    TextDisplayRegion.FontSize,
                    Brushes.Black,
                    new NumberSubstitution(),
                    VisualTreeHelper.GetDpi(TextDisplayRegion).PixelsPerDip);

                if (formattedText.Height < maxHeight)
                {
                    _charsPerPageStartGuess *= 2;
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
                    FullText.Substring(startIndex, (int)charSpan),
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

            // try to get somewhere to break that is less jarring
            for (int i = 0; i != 50 /* don't strip more than 50 chars */; i++)
            {
                if (Char.IsWhiteSpace(FullText[LocalCursor + (int)charSpan]))
                {
                    break;
                } else if (Char.IsPunctuation(FullText[LocalCursor+(int)charSpan - 1]))
                {
                    break;
                }
                charSpan--;
            }

            TextDisplayRegion.Text = FullText.Substring(LocalCursor, (int)charSpan);
            EndOfPage = LocalCursor + (int)charSpan - 1;
            // this shouldn't message anything, just keeps parity with the ViewModel
            PageEndLocatedCommand?.Execute(LocalCursor + (int)charSpan - 1);

        }
    }
}
