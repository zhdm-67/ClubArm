// Converters/BalanceToColorConverter.cs

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ClubArm.Converters
{
    public class BalanceToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal balance)
            {
                if (balance <= 0) return Brushes.Red;
                if (balance < 5) return Brushes.Orange;
                return Brushes.Black;
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}