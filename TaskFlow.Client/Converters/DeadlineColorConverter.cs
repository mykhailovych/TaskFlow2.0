using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TaskFlow.Client.Converters
{
    public class DeadlineColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DateTime deadline)
                return Brushes.Gray;

            var timeLeft = deadline - DateTime.Now;

            if (timeLeft.TotalSeconds <= 0)
                return Brushes.Red; // Час настав або вийшов

            if (timeLeft.TotalHours <= 3)
                return Brushes.Gold; // Залишилось до 3 годин

            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}