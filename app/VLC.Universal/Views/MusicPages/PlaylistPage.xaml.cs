﻿using System.Linq;
using Windows.UI.Xaml.Controls;
using VLC.Model.Music;
using VLC.ViewModels;

namespace VLC.UI.Views.MusicPages
{
    public sealed partial class PlaylistPage
    {
        public PlaylistPage()
        {
            this.InitializeComponent();
        }

        private void PlayListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Any())
            {
                foreach (var addedItem in e.AddedItems)
                {
                    Locator.MusicLibraryVM.CurrentTrackCollection.SelectedTracks.Add(addedItem as TrackItem);
                }
            }
            if (e.RemovedItems != null && e.RemovedItems.Any())
            {
                foreach (var removedItem in e.RemovedItems)
                {
                    Locator.MusicLibraryVM.CurrentTrackCollection.SelectedTracks.Remove(removedItem as TrackItem);
                }
            }
        }
    }
}
