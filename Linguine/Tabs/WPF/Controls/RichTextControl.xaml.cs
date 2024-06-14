
using Infrastructure;
using LearningExtraction;
using System;
using System.Collections;
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
        private ICommand UnitSelectedCommand;

        Style hyperlinkStyle;

        public RichTextControl()
        {
            InitializeComponent();

            this.Loaded += OnLoaded;
            this.DataContextChanged += OnDataContextChanged;

            CreateHyperlinkStyle();
        }

        private void CreateHyperlinkStyle()
        {
            // Create the style for the hyperlinks
            hyperlinkStyle = new Style(typeof(Hyperlink));
            hyperlinkStyle.Setters.Add(new Setter(Hyperlink.FocusVisualStyleProperty, null));
            hyperlinkStyle.Setters.Add(new Setter(Hyperlink.ForegroundProperty, Brushes.Black));
            hyperlinkStyle.Setters.Add(new Setter(TextBlock.TextDecorationsProperty, null));

            Trigger mouseOverTrigger = new Trigger { Property = Hyperlink.IsMouseOverProperty, Value = true };
            mouseOverTrigger.Setters.Add(new Setter(Hyperlink.ForegroundProperty, Brushes.Red));
            //mouseOverTrigger.Setters.Add(new Setter(TextBlock.TextDecorationsProperty, TextDecorations.Underline));

            hyperlinkStyle.Triggers.Add(mouseOverTrigger);
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
                UnitSelectedCommand         = newViewModel.UnitSelectedCommand;
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
        Brush faintRedBrush  = new SolidColorBrush(Color.FromArgb(20, 255, 0,   0));

        #region typesetting

        private void ProcessStatementInformation(object? sender, List<Statement> statementsCoveringPage)
        {
            // decide how to do the highlights before we flush the text
            //        start end  colour stat unit
            List<Tuple<int, int, Brush, int, int>> highlights = new List<Tuple<int, int, Brush, int, int>>();

            #region highlight computation
            for (int j = 0; j != statementsCoveringPage.Count; j++)
            {
                Statement s = statementsCoveringPage[j];

                int start = s.FirstCharIndex;
                int end = s.LastCharIndex;
                int offset = s.FirstCharIndex;

                List<int> unitStarts = DecompositionHelper.GetUnitLocations(s.InjectiveDecomposition)
                    .Select(s => s + offset).ToList();
                List<int> unitLengths = s.InjectiveDecomposition.Units?.Select(s => s.Length).ToList() ?? new List<int>();

                // TODO - empty units
                if (unitStarts.Count == 0)
                {
                    highlights.Add(Tuple.Create(start, end, faintGreyBrush, j , -1)); continue;
                }

                // initial grey highlight
                if (unitStarts[0] > start)
                {
                    highlights.Add(Tuple.Create(start, unitStarts[0] - 1, faintGreyBrush, j, -1));
                }

                for (int i = 0; i != unitStarts.Count; i++)
                {
                    // main unit highlight
                    highlights.Add(Tuple.Create(unitStarts[i], unitStarts[i] + unitLengths[i] - 1, faintRedBrush, j, i));

                    // grey after
                    if (i != unitStarts.Count - 1 && unitStarts[i] + unitLengths[i] < unitStarts[i+1])
                    {
                        highlights.Add(Tuple.Create(
                            unitStarts[i] + unitLengths[i], unitStarts[i + 1] - 1, faintGreyBrush, j , -1));
                    }
                }

                // trailing grey highlight
                if (unitStarts.Last() + unitLengths.Last() <= end)
                {
                    highlights.Add(Tuple.Create(unitStarts.Last() + unitLengths.Last(), end, faintGreyBrush, j, -1));
                }

            }
            #endregion

            // clear so we can use the fancy placement
            TextDisplayRegion.Text = ""; 
            TextDisplayRegion.Inlines.Clear();

            int currentIndex = LocalCursor;

            for (int i = 0; i != highlights.Count; i++)
            {
                var section = highlights[i];
                int start = section.Item1;
                int end = section.Item2;

                // Add unhighlighted text before the highlighted section
                if (currentIndex < start)
                {
                    TextDisplayRegion.Inlines.Add(new Run(FullText.Substring(currentIndex, start - currentIndex)));
                }

                // Add highlighted text
                if (start <= end && end <= EndOfPage)
                {
                    if (FullText[end] == '\r' && FullText[end+1] == '\n')
                    {
                        end++; // don't split these chars as the agents sometimes like to do
                    }

                    Run highlightRun = new Run(FullText.Substring(start, end - start + 1));
                    highlightRun.Background = section.Item3;

                    if (section.Item5 == -1)
                    {
                        // indicates no unit here
                        TextDisplayRegion.Inlines.Add(highlightRun);
                    }
                    else
                    {
                        TextDisplayRegion.Inlines.Add(new Hyperlink(highlightRun)
                        {
                            Style = hyperlinkStyle,
                        });
                        ((Hyperlink)TextDisplayRegion.Inlines.LastInline).Click += (sender, args) 
                            => OnUnitClick(Tuple.Create(section.Item4, section.Item5));
                    }
                }

                currentIndex = end+1;
            }

            // Add any remaining unhighlighted text
            if (currentIndex <= EndOfPage)
            {
                TextDisplayRegion.Inlines.Add(new Run(FullText.Substring(currentIndex, EndOfPage - currentIndex + 1)));
            }
        }

        private void OnUnitClick(Tuple<int, int> tuple)
        {
            UnitSelectedCommand?.Execute(tuple);
        }

        #endregion


        #region paging

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

            span = Math.Min(span, FullText.Length - newStartIndex - 1);

            if (SortedStatementStartIndices.Count > 0 && SortedStatementStartIndices.Last() > newStartIndex + span)
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
                for (int i = 0; i != 50 /* don't strip more than 50 chars */&& span > 0; i++)
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
            span = Math.Max(span, newEndIndex - LocalCursor + 1);

            if (SortedStatementStartIndices.Count != 0 && SortedStatementStartIndices.Last() > newEndIndex - span)
            {
                // if we have statements use them to for page intervals
                int firstStatementStartListIndex = BinarySearchForLargestIndexBefore(newEndIndex - span);

                // edge case at the end, we'll just use the whitespace method
                if (firstStatementStartListIndex + 1 < SortedStatementStartIndices.Count)
                {
                    int firstStatementStartIndex = SortedStatementStartIndices[firstStatementStartListIndex + 1];
                    // use the first statement after
                    if (firstStatementStartIndex > newEndIndex - span - 100)
                    {
                        span = newEndIndex - firstStatementStartIndex + 1;
                    }
                }
            }
            else
            {

                // try to get somewhere to break that is less jarring
                for (int i = 0; i != 50 /* don't strip more than 50 chars */&& span >= 0; i++)
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


        #endregion

    }


}
