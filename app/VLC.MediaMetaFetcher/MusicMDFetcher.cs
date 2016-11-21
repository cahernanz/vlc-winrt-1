﻿/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VLC.MusicMetaFetcher.Fetchers;
using VLC.MusicMetaFetcher.Models.MusicEntities;

namespace VLC.MusicMetaFetcher
{
    public class MusicMDFetcher
    {
        public static string DeezerAppId;
        public static string ApiKeyLastFm;

        public MusicMDFetcher(string deezerAppId, string lastFmApiKey)
        {
            DeezerAppId = deezerAppId;
            ApiKeyLastFm = lastFmApiKey;
        }

        private async Task<byte[]> DownloadArtistPictureFromDeezer(string artistName)
        {
            var deezerClient = new DeezerClient();
            var deezerArtist = await deezerClient.GetArtistInfo(artistName);
            if (deezerArtist == null) return null;
            if (deezerArtist.Images == null) return null;
            try
            {
                var clientPic = new HttpClient();
                HttpResponseMessage responsePic = await clientPic.GetAsync(deezerArtist.Images.LastOrDefault().Url);
                string uri = responsePic.RequestMessage.RequestUri.AbsoluteUri;
                // A cheap hack to avoid using Deezers default image for bands.
                if (uri.Equals("http://cdn-images.deezer.com/images/artist//400x400-000000-80-0-0.jpg"))
                {
                    return null;
                }
                byte[] img = await responsePic.Content.ReadAsByteArrayAsync();
                return img;
            }
            catch (Exception)
            {
                Debug.WriteLine("Error getting or saving art from deezer.");
                return null;
            }
        }

        private async Task<byte[]> DownloadArtistPictureFromLastFm(string artistName)
        {
            var lastFmClient = new LastFmClient();
            var lastFmArtist = await lastFmClient.GetArtistInfo(artistName);
            if (lastFmArtist == null) return null;
            try
            {
                var clientPic = new HttpClient();
                var nonEmptyImgs = lastFmArtist.Images.Where(node => !string.IsNullOrEmpty(node.Url)).ToList();
                var index = nonEmptyImgs.Count - 1;
                if (nonEmptyImgs.Count == 6)
                    index -= 1;
                if (index == -1) return null;
                var imageElement = nonEmptyImgs.ElementAt(index);
                if (imageElement == null) return null;
                HttpResponseMessage responsePic = await clientPic.GetAsync(imageElement.Url);
                byte[] img = await responsePic.Content.ReadAsByteArrayAsync();
                return img;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error getting or saving art from LastFm.");
                return null;
            }
        }

        private async Task<byte[]> DownloadAlbumPictureFromDeezer(string albumName, string albumArtist)
        {
            var deezerClient = new DeezerClient();
            var deezerAlbum = await deezerClient.GetAlbumInfo(albumName, albumArtist);
            if (deezerAlbum == null) return null;
            if (deezerAlbum.Images == null) return null;
            try
            {
                var clientPic = new HttpClient();
                string url = deezerAlbum.Images.Count == 1 ? deezerAlbum.Images[0].Url : deezerAlbum.Images[deezerAlbum.Images.Count - 2].Url;
                HttpResponseMessage responsePic = await clientPic.GetAsync(url);
                var uri = responsePic.RequestMessage.RequestUri.AbsoluteUri;
                // A cheap hack to avoid using Deezers default image for bands.
                if (uri.Equals("http://cdn-images.deezer.com/images/album//400x400-000000-80-0-0.jpg"))
                {
                    return null;
                }
                byte[] img = await responsePic.Content.ReadAsByteArrayAsync();
                return img;
            }
            catch (Exception)
            {
                Debug.WriteLine("Error getting or saving art from deezer.");
                return null;
            }
        }

        private async Task<byte[]> DownloadAlbumPictureFromLastFm(string albumName, string albumArtist)
        {
            var lastFmClient = new LastFmClient();
            var lastFmAlbum = await lastFmClient.GetAlbumInfo(albumName, albumArtist);
            if (lastFmAlbum == null) return null;
            if (lastFmAlbum.Images == null || lastFmAlbum.Images.Count == 0) return null;
            try
            {
                if (string.IsNullOrEmpty(lastFmAlbum.Images.LastOrDefault().Url)) return null;
                var clientPic = new HttpClient();
                var url = lastFmAlbum.Images.Count == 1 ? lastFmAlbum.Images[0].Url : lastFmAlbum.Images[lastFmAlbum.Images.Count - 2].Url;
                HttpResponseMessage responsePic = await clientPic.GetAsync(url);
                byte[] img = await responsePic.Content.ReadAsByteArrayAsync();
                return img;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error getting or saving art from lastFm. {0}", ex));
            }
            return null;
        }

        public async Task<byte[]> GetArtistPicture(string artistName)
        {
            artistName = System.Net.WebUtility.UrlEncode(artistName);

            var gotArt = await DownloadArtistPictureFromLastFm(artistName) ?? await DownloadArtistPictureFromDeezer(artistName);
            return gotArt;
        }

        public async Task<byte[]> GetAlbumPictureFromInternet(string albumName, string albumArtist)
        {
            byte[] gotArt = null;
            if (!string.IsNullOrEmpty(albumArtist) && !string.IsNullOrEmpty(albumName))
            {
                albumArtist = System.Net.WebUtility.UrlEncode(albumArtist);
                albumName = System.Net.WebUtility.UrlEncode(albumName);

                gotArt = await DownloadAlbumPictureFromLastFm(albumName, albumArtist);
                if (gotArt == null)
                {
                    gotArt = await DownloadAlbumPictureFromDeezer(albumName, albumArtist);
                }
            }
            return gotArt;
        }

        public async Task<List<Album>> GetArtistTopAlbums(string artistName)
        {
            try
            {
                if (string.IsNullOrEmpty(artistName)) return null;
                Debug.WriteLine("Getting TopAlbums from LastFM API");
                var lastFmClient = new LastFmClient();
                var albums = await lastFmClient.GetArtistTopAlbums(artistName);
                Debug.WriteLine("Receive TopAlbums from LastFM API");
                return albums;
            }
            catch
            {
                Debug.WriteLine("Error getting top albums from artist.");
            }
            return null;
        }

        public async Task<List<Artist>> GetArtistSimilarsArtist(string artistName)
        {
            try
            {
                if (string.IsNullOrEmpty(artistName))
                    return null;

                artistName = System.Net.WebUtility.UrlEncode(artistName);

                var lastFmClient = new LastFmClient();
                var similarArtists = await lastFmClient.GetSimilarArtists(artistName);
                return similarArtists;
            }
            catch
            {
                Debug.WriteLine("Error getting similar artists from this artist.");
            }
            return null;
        }

        public async Task<List<Artist>> GetTopArtistGenre(string genre)
        {
            try
            {
                if (string.IsNullOrEmpty(genre)) return null;
                var lastFmClient = new LastFmClient();
                var artists = await lastFmClient.GetTopArtistsGenre(genre);
                return artists;
            }
            catch
            {
                
            }
            return null;
        }

        public async Task<string> GetArtistBiography(string artistName)
        {
            if (string.IsNullOrEmpty(artistName))
                return null;

            artistName = System.Net.WebUtility.UrlEncode(artistName);

            var biography = string.Empty;
            try
            {
                var lastFmClient = new LastFmClient();
                var artistInformation = await lastFmClient.GetArtistInfo(artistName);
                biography = artistInformation != null ? artistInformation.Biography : String.Empty;
            }
            catch
            {
                Debug.WriteLine("Failed to get artist biography from LastFM. Returning nothing.");
            }
            return biography;
        }


    }
}
