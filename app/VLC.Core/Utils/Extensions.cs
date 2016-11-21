﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using VLC.Model;
using VLC.Model.Music;

namespace VLC.Utils
{
    public static class Extensions
    {
        public static bool Contains(this string source, string value, StringComparison comparisonType)
        {
            return source.IndexOf(value, comparisonType) >= 0;
        }

        public static SmartCollection<T> ToObservable<T>(this IEnumerable<T> source)
        {
            return new SmartCollection<T>(source);
        }

        public static async Task<SmartCollection<T>> ToObservableAsync<T>(this Task<List<T>> source)
        {
            return new SmartCollection<T>(await source);
        }

        public static ObservableCollection<IMediaItem> ToPlaylist(this IEnumerable<TrackItem> source)
        {
            return new ObservableCollection<IMediaItem>(source);
        }

        public static List<TrackItem> ToTrackItemPlaylist(this IEnumerable<IMediaItem> source)
        {
            return source.OfType<TrackItem>().ToList();
        }

        public static string ToUpperFirstChar(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
    }
}
