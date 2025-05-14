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
            if (value is EditMethod em)
            {
                switch (em)
                {
                    case EditMethod.NotEdited:
                        return Brushes.White;
                    case EditMethod.MachineEdited:
                        return Brushes.LightGray;
                    case EditMethod.UserEdited:
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
