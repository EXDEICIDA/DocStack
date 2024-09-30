using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace DocStack.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isStarred && parameter is string colors)
            {
                var colorStrings = colors.Split(';');
                if (colorStrings.Length == 2)
                {
                    return (SolidColorBrush)(new BrushConverter().ConvertFrom(isStarred ? colorStrings[0] : colorStrings[1]));
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
