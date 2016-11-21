﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using VLC.Model.Music;
using Windows.UI.Xaml.Media;
using VLC.Utils;
using System.ComponentModel;
using VLC.ViewModels;
using VLC.ViewModels.MusicVM;
using VLC.Model;

namespace VLC.UI.Views.UserControls
{
    public sealed partial class TrackItemTemplate : UserControl
    {
        private Brush previousBrush = null;
        public TrackItemTemplate()
        {
            this.InitializeComponent();
            this.Unloaded += TrackItemTemplate_Unloaded;
        }

        private void TrackItemTemplate_Unloaded(object sender, RoutedEventArgs e)
        {
            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaSet -= UpdateTrack;
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout((Grid)sender);
        }


        public TrackItem Track
        {
            get { return (TrackItem)GetValue(TrackProperty); }
            set { SetValue(TrackProperty, value); }
        }
        
        public static readonly DependencyProperty TrackProperty = DependencyProperty.Register(nameof(Track), typeof(TrackItem), typeof(TrackItemTemplate), new PropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (TrackItemTemplate)dependencyObject;
            that.Init();
        }

        public void Init()
        {
            if (Track == null)
                return;

            this.Opacity = Track.IsAvailable ? 1 : Numbers.NotAvailableFileItemOpacity;
            NameTextBlock.Text = Track.Name;
            DurationTextBlock.Text = Strings.HumanizeSeconds(Track.Duration.TotalSeconds);

            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaSet += UpdateTrack;
            UpdateTrack(Track);
        }

        async void UpdateTrack(IMediaItem media)
        {
            await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                if (Track == null)
                    return;

                if (Locator.MediaPlaybackViewModel.PlaybackService.CurrentMedia == -1 || Locator.MediaPlaybackViewModel.PlaybackService.Playlist?.Count == 0)
                    return;

                if (Track.IsCurrentPlaying())
                {
                    previousBrush = NameTextBlock.Foreground;
                    NameTextBlock.Foreground = (Brush)App.Current.Resources["MainColor"];
                }
                else
                {
                    if (previousBrush != null)
                        NameTextBlock.Foreground = previousBrush;
                }
            });
        }
    }
}
