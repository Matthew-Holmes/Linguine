
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
            hyperlinkStyle = new Style(typeof(Hyperlink));
            hyperlinkStyle.Setters.Add(new Setter(Hyperlink.FocusVisualStyleProperty, null));
            hyperlinkStyle.Setters.Add(new Setter(Hyperlink.ForegroundProperty, Brushes.Black));
            hyperlinkStyle.Setters.Add(new Setter(TextBlock.TextDecorationsProperty, null));

            // Use attached event handlers instead of triggers
            hyperlinkStyle.Setters.Add(new EventSetter(Hyperlink.MouseEnterEvent, new MouseEventHandler(Hyperlink_MouseEnter)));
            hyperlinkStyle.Setters.Add(new EventSetter(Hyperlink.MouseLeaveEvent, new MouseEventHandler(Hyperlink_MouseLeave)));
        }

        private static readonly Brush DefaultHyperlinkBrush = Brushes.Black;
        private static readonly Brush HoverHyperlinkBrush = Brushes.Red;

        private static void Hyperlink_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Hyperlink link)
            {
                link.Foreground = HoverHyperlinkBrush;
                // Uncomment if you want underline on hover
                // link.TextDecorations = TextDecorations.Underline;
            }
        }

        private static void Hyperlink_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Hyperlink link)
            {
                link.Foreground = DefaultHyperlinkBrush;
                // Uncomment if you want to remove underline on leave
                // link.TextDecorations = null;
            }
        }


        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is TextualMediaViewerViewModel oldViewModel)
            {
                oldViewModel.UnderlyingStatementsChanged -= UnderlyingStatementsChanged;
            }

            if (e.NewValue is TextualMediaViewerViewModel newViewModel)
            {
                // order matters here, don't move reorder unless sure

                FullText = newViewModel.FullText;
                UnitSelectedCommand = newViewModel.UnitSelectedCommand;

                newViewModel.UnderlyingStatementsChanged += UnderlyingStatementsChanged;

                // if not loaded the text display region is 0 tall and nothing displays
                ProcessStatementInformation(this, newViewModel.TextStatements);
            }
        }

        private void UnderlyingStatementsChanged(object? sender, EventArgs e)
        {
            if (sender is TextualMediaViewerViewModel tmv_mv)
            {
                ProcessStatementInformation(this, tmv_mv.TextStatements);
            }
            else
            {
                throw new Exception("wrong sender!");
            }
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
        Brush faintRedBrush  = new SolidColorBrush(Color.FromArgb(20, 255, 0, 0));

        // TODO - this needs to be chunked into paragraphs for streaming in the data

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
                    highlights.Add(Tuple.Create(start, end, faintGreyBrush, j, -1)); continue;
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
                    if (i != unitStarts.Count - 1 && unitStarts[i] + unitLengths[i] < unitStarts[i + 1])
                    {
                        highlights.Add(Tuple.Create(
                            unitStarts[i] + unitLengths[i], unitStarts[i + 1] - 1, faintGreyBrush, j, -1));
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

            // add Runs/ Hyperlinks to para.Inlines here

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
                    if (FullText[end] == '\r' && FullText[end + 1] == '\n')
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
                        var hyperlink = new Hyperlink(highlightRun)
                        {
                            Style = hyperlinkStyle,
                            Tag = Tuple.Create(section.Item4, section.Item5),
                        };
                        hyperlink.Click += Hyperlink_Click;
                        para.Inlines.Add(hyperlink);
                    }
                }

                currentIndex = end + 1;
            }

            // Add any remaining unhighlighted text
            if (currentIndex < FullText.Length - 1)
            {
                para.Inlines.Add(new Run(FullText.Substring(currentIndex, FullText.Length - 1 - currentIndex)));
            }


            //int cursor = 0;

            //while (cursor < FullText.Length)
            //{
            //    Paragraph para = new Paragraph();

            //    int toTake = Math.Min(FullText.Length - cursor, 1_000);

            //    para.Inlines.Add(FullText.Substring(cursor, toTake));

            //    doc.Blocks.Add(para);

            //    cursor += 1_000;

            //    if (cursor > 10_000 && cursor <= 11_000)
            //    {
            //        para.Loaded += para_loaded;
            //    }
            //}

            doc.Blocks.Add(para);
            TextDisplayRegion.Document = doc;
        }

        //private void para_loaded(object sender, RoutedEventArgs e)
        //{
        //    Paragraph para = sender as Paragraph;

        //    if (para is not null)
        //    {
        //        para.BringIntoView();
        //    }
        //}

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink hyperlink && hyperlink.Tag is Tuple<int, int>data)
            {
                OnUnitClick(data);
            }
        }


        private void OnUnitClick(Tuple<int, int> tuple)
        {
            UnitSelectedCommand?.Execute(tuple);
        }
    }
}
