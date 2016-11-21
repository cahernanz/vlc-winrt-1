﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using VLC.Model;

namespace VLC.Converters
{
    public class MiniPlayerVisibilityFromPlayingTypeConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((PlayingType) value == PlayingType.Music)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
