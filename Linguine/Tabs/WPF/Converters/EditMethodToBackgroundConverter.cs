using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Linguine.Helpers;


namespace Linguine.Tabs.WPF
{
    public class EditMethodToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TextualEditMethod em)
            {
                switch (em)
                {
                    case TextualEditMethod.NotEdited:
                        return Brushes.White;
                    case TextualEditMethod.MachineEdited:
                        return Brushes.LightGray;
                    case TextualEditMethod.UserEdited:
                        return Brushes.LightBlue;
                    default:
                        throw new NotImplementedException();
                }
            }

            throw new ArgumentException("should only pass EditMethod enum to this");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
