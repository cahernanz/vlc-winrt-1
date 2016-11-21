﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC.Model.Music;
using VLC.ViewModels;
using VLC.Model;

namespace VLC.UI.Views.UserControls.Flyouts
{
    public partial class TrackItemFlyout : Flyout
    {
        public TrackItemFlyout() : base()
        {
            this.InitializeComponent();
            this.Opened += TrackItemFlyout_Opened;
        }

        public TrackItemFlyout(object trackItem)
        {
            this.InitializeComponent();
            this.Opened += TrackItemFlyout_Opened;
            this.FlyoutGrid.DataContext = trackItem;
        }

        private void TrackItemFlyout_Opened(object sender, object e)
        {
            Locator.MusicLibraryVM.InitializePlaylists();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            Locator.MusicLibraryVM.CurrentTrackCollection = e.AddedItems[0] as PlaylistItem;
            Locator.MusicLibraryVM.AddToPlaylistCommand.Execute((this.Content as FrameworkElement).DataContext as TrackItem);
            (sender as ListView).SelectedIndex = -1;
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var root = sender as FrameworkElement;
            root.MaxHeight = 600;
            root.MaxWidth = 400;
        }
    }
}
