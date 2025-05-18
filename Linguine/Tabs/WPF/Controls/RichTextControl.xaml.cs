
using LearningExtraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using DataClasses;

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
        // button --------------> invoke PageForwards<int>
        //                               |
        // HandlePageForwards(int) <-----
        //        | 
        // Computes pages until reach final
        //        |
        // PageLocatedCommand(start,end) --
        //                                 |
        //                          loads statements for range
        //                                 |
        //                  invoke StatementsCoveringPageChanged 
        //                                 |
        // ProcessStatementInformation <---
        //        |
        // adds highlights and hyperlinks

        private int LocalCursor;
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
                oldViewModel.StatementsCoveringPageChanged -= ProcessStatementInformation;
                oldViewModel.UnderlyingStatementsChanged   -= UnderlyingStatementsChanged;
            }

            if (e.NewValue is TextualMediaViewerViewModel newViewModel)
            {
                // order matters here, don't move reorder unless sure

                FullText                    = newViewModel.FullText;
                SortedStatementStartIndices = newViewModel.SortedStatementStartIndices;
                UnitSelectedCommand         = newViewModel.UnitSelectedCommand;
                LocalCursor                 = newViewModel.LocalCursor;

                newViewModel.StatementsCoveringPageChanged += ProcessStatementInformation;
                newViewModel.UnderlyingStatementsChanged   += UnderlyingStatementsChanged;

                // if not loaded the text display region is 0 tall and nothing displays
                ProcessStatementInformation(this, newViewModel.StatementsCoveringPage);
            }
        }

        private void UnderlyingStatementsChanged(object? sender, List<int> e)
        {
            SortedStatementStartIndices = e;
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

            FlowDocument doc = new FlowDocument();
            Paragraph para = new Paragraph();
            // add Runs/Hyperlinks to para.Inlines here

            int currentIndex = 0; // TODO - sort this out

            for (int i = 0; i != highlights.Count; i++)
            {
                var section = highlights[i];
                int start = section.Item1;
                int end = section.Item2;

                // Add unhighlighted text before the highlighted section
                if (currentIndex < start)
                {
                    para.Inlines.Add(new Run(FullText.Substring(currentIndex, start - currentIndex)));
                }

                // Add highlighted text
                if (start <= end)
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
                        para.Inlines.Add(highlightRun);
                    }
                    else
                    {
                        para.Inlines.Add(new Hyperlink(highlightRun)
                        {
                            Style = hyperlinkStyle,
                        });
                        ((Hyperlink)para.Inlines.LastInline).Click += (sender, args) 
                            => OnUnitClick(Tuple.Create(section.Item4, section.Item5));
                    }
                }

                currentIndex = end+1;
            }

            // Add any remaining unhighlighted text
            if (currentIndex < FullText.Length - 1)
            {
                para.Inlines.Add(new Run(FullText.Substring(currentIndex, FullText.Length - 1 - currentIndex)));
            }

            doc.Blocks.Add(para);
            TextDisplayRegion.Document = doc;
        }

        private void OnUnitClick(Tuple<int, int> tuple)
        {
            UnitSelectedCommand?.Execute(tuple);
        }

        #endregion





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
