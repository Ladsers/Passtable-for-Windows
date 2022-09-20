using System;
using System.Globalization;
using System.Windows.Data;

namespace Passtable.Components
{
    public class IsDataNotEmpty : IValueConverter {
        public static readonly IValueConverter Instance = new IsDataNotEmpty();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var str = (string) value;
            var length = str.Length;

            return length > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}