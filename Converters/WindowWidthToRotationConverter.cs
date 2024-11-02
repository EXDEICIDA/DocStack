using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DocStack.Converters
{
    public class WindowWidthToRotationConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is double width && values[1] is IEnumerable<string> labels)
            {
                // Calculate rotation based on available width and label length
                return width < 600 ? -45 : 0;
            }
            return 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
