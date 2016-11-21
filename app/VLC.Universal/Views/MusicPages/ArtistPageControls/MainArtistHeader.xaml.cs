﻿using Microsoft.Xaml.Interactivity;
using System.Threading.Tasks;
using VLC.Model.Music;
using VLC.Utils;
using VLC.ViewModels;
using VLC.ViewModels.MusicVM;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace VLC.UI.Views.MusicPages.ArtistPageControls
{
    public sealed partial class MainArtistHeader : UserControl
    {
        public MainArtistHeader()
        {
            this.InitializeComponent();
            this.Loaded += MainArtistHeader_Loaded;
        }
        
        private async void MainArtistHeader_Loaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded += MainArtistHeader_Unloaded;
            Window.Current.SizeChanged += Current_SizeChanged;
            Responsive();

            Locator.MusicLibraryVM.PropertyChanged += MusicLibraryVM_PropertyChanged;

            if (!await UpdateThumbnail())
                await Locator.MusicLibraryVM.CurrentArtist.ResetArtistPicture(true);

            if (!await UpdateBackground())
                await Locator.MusicLibraryVM.CurrentArtist.ResetArtistPicture(false);
        }

        async Task<bool> UpdateThumbnail()
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
            {
                EllipseImage.Fill = new ImageBrush()
                {
                    ImageSource = Locator.MusicLibraryVM.CurrentArtist.ArtistImageThumbnail,
                    Stretch = Stretch.UniformToFill
                };
            });
            return true;
        }

        async Task<bool> UpdateBackground()
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () => BackgroundImage.Source = Locator.MusicLibraryVM.CurrentArtist.ArtistImage);
            return true;
        }

        private async void MusicLibraryVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MusicLibraryVM.CurrentArtist))
            {
                if (Locator.MusicLibraryVM.CurrentArtist == null)
                    return;
                await UpdateThumbnail();
                await UpdateBackground();
                Locator.MusicLibraryVM.CurrentArtist.PropertyChanged += CurrentArtist_PropertyChanged;
            }
        }

        private async void CurrentArtist_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Locator.MusicLibraryVM.CurrentArtist == null)
                return;

            if (e.PropertyName == nameof(ArtistItem.ArtistImageThumbnail))
                await UpdateThumbnail();
            else if (e.PropertyName == nameof(ArtistItem.ArtistImage))
                await UpdateBackground();
        }

        private void MainArtistHeader_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive();
        }

        void Responsive()
        {
            BackgroundEffect.StartAnimation();
            if (Window.Current.Bounds.Width < 650)
            {
                VisualStateUtilities.GoToState(this, "Snap", false);
            }
            else
            {
                VisualStateUtilities.GoToState(this, "Wide", false);
            }

            if (Window.Current.Bounds.Height < 600)
            {
                //HeaderGrid.Height = 100;
                VisualStateUtilities.GoToState(this, "Tiny", false);
            }
            else
            {
                VisualStateUtilities.GoToState(this, "Tall", false);
            }
        }
    }
}
