﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using VLC.Database;
using VLC.Model.Stream;
using VLC.Utils;
using VLC.Commands;
using Windows.UI.Xaml;
using Autofac;
using VLC.Services.RunTime;
using VLC.Commands.StreamsLibrary;

namespace VLC.ViewModels.Others
{
    public class StreamsViewModel : BindableBase, IDisposable
    {
        public IEnumerable<StreamMedia> StreamsHistoryAndFavoritesGrouped
        {
            get { return Locator.MediaLibrary.Streams?.OrderBy(x => x.Order); }
        }

        public bool IsCollectionEmpty
        {
            get { return !Locator.MediaLibrary.Streams.Any(); }
        }

        public Visibility NoInternetPlaceholderEnabled => NetworkListenerService.IsConnected ? Visibility.Collapsed : Visibility.Visible;

        public PlayNetworkMRLCommand PlayStreamCommand { get; } = new PlayNetworkMRLCommand();

        public void OnNavigatedTo()
        {
            Task.Run(() => Initialize());
        }

        public void OnNavigatedFrom()
        {
            Dispose();
        }

        async Task Initialize()
        {
            App.Container.Resolve<NetworkListenerService>().InternetConnectionChanged += StreamsViewModel_InternetConnectionChanged;
            Locator.MediaLibrary.Streams.CollectionChanged += Streams_CollectionChanged;
            await Locator.MediaLibrary.LoadStreamsFromDatabase();
        }

        private async void StreamsViewModel_InternetConnectionChanged(object sender, Model.Events.InternetConnectionChangedEventArgs e)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () => OnPropertyChanged(nameof(NoInternetPlaceholderEnabled)));
        }

        private async void Streams_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged(nameof(StreamsHistoryAndFavoritesGrouped));
                OnPropertyChanged(nameof(IsCollectionEmpty));
            });
        }

        public void Dispose()
        {
            Locator.MediaLibrary.Streams.CollectionChanged -= Streams_CollectionChanged;
            App.Container.Resolve<NetworkListenerService>().InternetConnectionChanged -= StreamsViewModel_InternetConnectionChanged;
        }
    }
}
