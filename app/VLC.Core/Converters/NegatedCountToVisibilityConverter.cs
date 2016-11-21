﻿/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VLC.Converters
{
    public class NegatedCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int || value is double)
            {
                if (value is int)
                {
                    if ((int)value == 0)
                        return Visibility.Collapsed;
                    else
                        return Visibility.Visible;
                }
                if (value is double)
                {
                    if ((double)value == 0)
                        return Visibility.Collapsed;
                    else
                        return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
