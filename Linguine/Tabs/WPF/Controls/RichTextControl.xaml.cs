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

        private void CalculatePageForwards()
        {
            int maxHeight = (int)TextDisplayRegion.ActualHeight;
            double lineHeight = TextDisplayRegion.FontSize * TextDisplayRegion.LineHeight;
            int maxLines = (int)(maxHeight / lineHeight);

            TextDisplayRegion.Text = FullText.Substring(LocalCursor, 1000);
            EndOfPage = LocalCursor + 999;
            // this shouldn't message anything, just keeps parity with the ViewModel
            PageEndLocatedCommand?.Execute(LocalCursor + 999);
            
            /*
            int startIndex = 0;
            foreach (var endIndex in _sentenceEndMarkers)
            {
                string pageText = _fullText.Substring(startIndex, endIndex - startIndex + 1);
                var formattedText = new System.Windows.Media.FormattedText(
                    pageText,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(TextDisplayRegion.FontFamily, TextDisplayRegion.FontStyle, TextDisplayRegion.FontWeight, TextDisplayRegion.FontStretch),
                    TextDisplayRegion.FontSize,
                    System.Windows.Media.Brushes.Black
                );

                if (formattedText.Height > maxHeight)
                {
                    _pages.Add(_fullText.Substring(startIndex, _sentenceEndMarkers[_pages.Count] - startIndex + 1));
                    startIndex = _sentenceEndMarkers[_pages.Count];
                }
            }
            _pages.Add(_fullText.Substring(startIndex));
            */
        }
    }
}
