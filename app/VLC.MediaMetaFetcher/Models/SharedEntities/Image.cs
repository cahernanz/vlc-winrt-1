﻿/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC.MusicMetaFetcher.Models.LastFm;

namespace VLC.MediaMetaFetcher.Models.SharedEntities
{
    /// <summary>
    /// Entity for images. This may be expanded to use more than just Urls.
    /// </summary>
    public class Image
    {
        public string Url { get; set; }

        /// <summary>
        /// Map from TopAlbum LastFmClient.
        /// </summary>
        /// <param name="image">A TopAlbum image.</param>
        public void MapFrom(TopImage image)
        {
            this.Url = image.Text;
        }

        /// <summary>
        /// Map from SimilarArtist LastFmClient.
        /// </summary>
        /// <param name="image">A SimilarArtist image.</param>
        public void MapFrom(SimilarArtistImage image)
        {
            this.Url = image.Text;
        }

        /// <summary>
        /// Map from LastFmClient.
        /// </summary>
        /// <param name="image">An image.</param>
        public void MapFrom(MusicMetaFetcher.Models.LastFm.Image image)
        {
            this.Url = image.Text;
        }

        public static Image MapFrom(MovieDB.MovieImagesInformation.Backdrop backdrop)
        {
            var i = new Image();
            i.Url = backdrop.file_path;
            return i;
        }
    }
}
