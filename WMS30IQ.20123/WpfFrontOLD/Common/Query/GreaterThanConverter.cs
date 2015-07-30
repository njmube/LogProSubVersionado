using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WpfFront.Common.Query
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class GreaterThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!targetType.IsAssignableFrom(typeof(bool)))
                return DependencyProperty.UnsetValue;

            if (value == null)
                return false;

            double number = 0d;
            double comparedToValue = 0d;

            try
            {
                number = System.Convert.ToDouble(value);
            }
            catch
            {
                return false;
            }

            if (parameter != null)
            {
                try
                {
                    comparedToValue = System.Convert.ToDouble(parameter);
                }
                catch
                {
                    return DependencyProperty.UnsetValue;
                }
            }

            return number > comparedToValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}