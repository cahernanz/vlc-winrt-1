﻿using System;
using Windows.Storage;
using Windows.UI.Xaml.Data;
using VLC.Model;

namespace VLC.Converters
{
    public class StorageTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is VLCStorageFolder)
            {
                return App.Current.Resources["FolderFilledSymbol"];
            }
            else
            {
                return App.Current.Resources["FileFilledSymbol"].ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
