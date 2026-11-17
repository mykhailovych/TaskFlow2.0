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
            if (value is DateTime deadline)
            {
                var timeLeft = deadline - DateTime.Now;

                if (timeLeft.TotalHours <= 3 && timeLeft.TotalHours > 0)
                {
                    return new SolidColorBrush(Colors.Gold);
                }
                else if (timeLeft.TotalHours <= 0)
                {
                    return new SolidColorBrush(Colors.Red);
                }
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}