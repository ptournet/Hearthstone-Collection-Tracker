using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Hearthstone_Collection_Tracker.Internal
{
    /// <summary>
    /// Converts boolean values to Visibility enumerations.
    /// If all of the values are true, then the control is visible.
    /// Otherwise, we collapse the control.
    /// </summary>
    internal class VisibilityConverter : IMultiValueConverter, IValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var value in values)
            {
                if ((value is bool) && (bool)value == false)
                {
                    return Visibility.Collapsed;
                }
            }
            return Visibility.Visible;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible = (bool)value;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("VisibilityConverter is a OneWay converter.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("VisibilityConverter is a OneWay converter.");
        }
    }
}
