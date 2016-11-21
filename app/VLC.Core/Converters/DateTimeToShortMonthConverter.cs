﻿using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace VLC.Converters
{
    public class DateTimeToShortMonthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime)
            {
                var date = (DateTime) value;
                return date.ToString("MMM", CultureInfo.InvariantCulture);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}