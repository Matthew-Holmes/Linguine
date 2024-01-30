using System;
using System.Windows;
using System.Windows.Controls;

namespace Linguine.Tabs.WPF
{
    public class TabViewTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (item is Tabs.ConfigManagerViewModel)
                return element.FindResource("ConfigManagerTabTemplate") as DataTemplate;

            if (item is Tabs.HomeViewModel)
                return element.FindResource("HomeTabTemplate") as DataTemplate;

            if (item is Tabs.TextualMediaViewerViewModel)
                return element.FindResource("TextualMediaViewerTabTemplate") as DataTemplate;
            return null;
            throw new NotImplementedException();
        }
    }
}
