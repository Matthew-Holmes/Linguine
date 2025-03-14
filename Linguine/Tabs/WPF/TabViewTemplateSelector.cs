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

            if (item is Tabs.TextualMediaLaunchpadViewModel)
                return element.FindResource("TextualMediaLaunchpadTabTemplate") as DataTemplate;

            if (item is Tabs.TextualMediaViewerViewModel)
                return element.FindResource("TextualMediaViewerTabTemplate") as DataTemplate;

            if (item is Tabs.TestLearningViewModel)
                return element.FindResource("TestLearningTabTemplate") as DataTemplate;

            if (item is null)
                return null;

            throw new NotImplementedException();
        }
    }
}
