﻿using System;
using System.Threading.Tasks;
using VLC.Model.Video;
using VLC.Utils;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace VLC.UI.Views.UserControls
{
    public sealed partial class VideoItem : UserControl
    {
        public VideoItem()
        {
            this.InitializeComponent();
        }

        private void RootAlbumItem_Holding(object sender, HoldingRoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout((Grid)sender);
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout((Grid)sender);
        }

        public Model.Video.VideoItem Video
        {
            get { return (Model.Video.VideoItem)GetValue(VideoProperty); }
            set { SetValue(VideoProperty, value); }
        }

        private static readonly DependencyProperty VideoProperty =
            DependencyProperty.Register(nameof(Video), typeof(VideoItem), typeof(VideoItem), new PropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dO, DependencyPropertyChangedEventArgs args)
        {
            var that = (VideoItem)dO;
            that.Init();
        }

        public void Init()
        {
            if (Video == null)
                return;

            this.Opacity = Video.IsAvailable ? 1 : Numbers.NotAvailableFileItemOpacity;
            NameTextBlock.Text = Video.Name;
            UpdateVideoDurations();
            Video.PropertyChanged += Video_PropertyChanged;
            if (Video.VideoImage != null)
                FadeOutCover.Begin();
        }

        private async void Video_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Video.VideoImage))
            {
                await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    FadeOutCover.Begin();
                });
            }
            else if (e.PropertyName == nameof(Video.Duration) || e.PropertyName == nameof(Video.TimeWatched))
            {
                await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    UpdateVideoDurations();
                });
            }
        }

        void UpdateVideoDurations()
        {
            TimeWatchedTextBlock.Text = Strings.HumanizedTimeSpan(Video.TimeWatched);
            DurationTextBlock.Text = Strings.HumanizedTimeSpan(Video.Duration);

            VideoProgressBar.Value = Video.TimeWatched.TotalSeconds;
            VideoProgressBar.Maximum = Video.Duration.TotalSeconds;

            VideoProgressBar.Visibility = Video.TimeWatched.TotalSeconds > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void FadeOutCover_Completed(object sender, object e)
        {
            if (Video != null && Video.VideoImage != null)
            {
                ThumbnailImage.Source = Video.VideoImage;
                FadeInCover.Begin();
            }
        }
    }
}
