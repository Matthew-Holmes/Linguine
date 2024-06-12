using Azure;
using Azure.Core;
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
        public ICommand PageLocatedCommand { get; set; }

        public string FullText { get; set; }
        public int LocalCursor { get; set; }
        public int EndOfPage { get; set; }

        public RichTextControl()
        {
            InitializeComponent();
            
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is TextualMediaViewerViewModel viewModel)
            {
                LocalCursor = viewModel.LocalCursor;
                FullText = viewModel.FullText;

                viewModel.PageForward   += CalculatePageForward;
                viewModel.PageBackwards += CalculatePageBackwards;

                PageLocatedCommand = viewModel.PageLocated;

                CalculatePageFromStartIndex(LocalCursor);

                return;
            }
            else
            {
                MessageBox.Show("invalid data context!");
            }
        }

        private String ClippedSubstring(String str, int start, int span)
        {
            start = Math.Max(start, 0);
            span = Math.Min(span, str.Length - start - 1);
            span = Math.Max(span, 0);
            start = Math.Max(start, 0);
            return str.Substring(start, span);
        }

        int _charsPerPageStartGuess = 1000;

        private void CalculatePageForward(Object? sender, EventArgs e)
        {
            int newStartIndex = EndOfPage + 1;
            CalculatePageFromStartIndex(newStartIndex);
        }

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
            
            // try to get somewhere to break that is less jarring
            for (int i = 0; i != 50 /* don't strip more than 50 chars */&& FullText.Length - charSpan > 0 ; i++)
            {
                if (Char.IsWhiteSpace(FullText[newStartIndex + (int)charSpan - 1]))
                {
                    break;
                } else if (Char.IsPunctuation(FullText[newStartIndex+(int)charSpan - 1]))
                {
                    break;
                }
                charSpan--;
            }

            TextDisplayRegion.Text = ClippedSubstring(FullText, newStartIndex, (int)charSpan);
            LocalCursor = Math.Min(newStartIndex, FullText.Length - 1);
            EndOfPage = newStartIndex + (int)charSpan - 1;
            // this shouldn't message anything, just keeps parity with the ViewModel
            PageLocatedCommand?.Execute(Tuple.Create(LocalCursor, EndOfPage));

        }

        private void CalculatePageBackwards(Object? sender, EventArgs e)
        {
            int maxHeight = (int)TextDisplayRegion.ActualHeight;
            int newEndIndex = LocalCursor - 1;

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

            // try to get somewhere to break that is less jarring
            for (int i = 0; i != 50 /* don't strip more than 50 chars */&& newEndIndex - (int)charSpan >= 0; i++)
            {
                if (Char.IsWhiteSpace(FullText[newEndIndex - (int)charSpan]))
                {
                    break;
                }
                else if (Char.IsPunctuation(FullText[newEndIndex - (int)charSpan]))
                {
                    break;
                }
                charSpan--;
            }

            TextDisplayRegion.Text = ClippedSubstring(FullText, newEndIndex - (int)charSpan + 1, (int)charSpan);
            LocalCursor = newEndIndex - (int)charSpan + 1;
            EndOfPage = newEndIndex;
            // this shouldn't message anything, just keeps parity with the ViewModel
            PageLocatedCommand?.Execute(Tuple.Create(LocalCursor, EndOfPage));
        }
    }
}
