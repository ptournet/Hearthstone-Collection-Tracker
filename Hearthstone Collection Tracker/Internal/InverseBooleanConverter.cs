using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Hearthstone_Collection_Tracker.Internal
{
    /// <summary>
    /// Returns the inverse of a boolean value.
    /// Needed because WPF doesn't have a built-in way to check for not values.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    internal class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("InverseBooleanConverter is a OneWay converter.");
        }
    }
}
