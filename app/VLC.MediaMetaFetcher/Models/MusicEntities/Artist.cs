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
using System.Linq;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using VLC.MusicMetaFetcher.Models.LastFm;
using Image = VLC.MediaMetaFetcher.Models.SharedEntities.Image;
using VLC.Utils;

namespace VLC.MusicMetaFetcher.Models.MusicEntities
{
    public class Artist
    {
        public string Name { get; set; }

        public string XboxId { get; set; }

        public string DeezerId { get; set; }

        public string Url { get; set; }

        public List<Image> Images { get; set; }

        public long Listeners { get; set; }

        public long Playcount { get; set; }

        public string Biography { get; set; }

        public bool OnTour { get; set; }
        

        /// <summary>
        /// Map from LastFmClient TopArtist LastFmClient Entity.
        /// </summary>
        /// <param name="artist">TopArist Entity.</param>
        public void MapFrom(TopArtist artist)
        {
            this.Name = artist.Name;
            this.Url = artist.Url;
        }

        public void MapFrom(SimilarArtist artist)
        {
            this.Name = artist.Name;
            this.Url = artist.Url;
            this.Images = new List<Image>();
            foreach (var image in artist.Image)
            {
                var artistImage = new Image();
                artistImage.MapFrom(image);
                this.Images.Add(artistImage);
            }
        }

        public void MapFrom(ArtistInformation artistInformation)
        {
            var artist = artistInformation.Artist;
            this.Name = artist.Name;
            this.Url = Url;

            // I hope this is not stupid code. It broke on Boolean.Parse :(.
            switch (artist.Ontour)
            {
                case "0":
                    this.OnTour = false;
                    break;
                case "1":
                    this.OnTour = true;
                    break;
                default:
                    this.OnTour = false;
                    break;
            }

            this.Images = new List<Image>();
            foreach (var image in artist.Image)
            {
                var artistImage = new Image();
                artistImage.MapFrom(image);
                this.Images.Add(artistImage);
            }

            this.Playcount = Convert.ToInt64(this.Playcount);
            this.Listeners = Convert.ToInt64(this.Listeners);
            string biography;
            
            var bioSummary = artist.Bio.Summary;
            if (bioSummary != null)
            {
                // Deleting the html tags
                biography = Regex.Replace(bioSummary, "<.*?>", string.Empty);
                // Remove leading new lines.
                biography = biography.TrimStart('\r', '\n');
                // Remove leading and ending white spaces.
                biography = biography.Trim();
                // TODO: Replace string "remove" with something better. It may not work on all artists and in all languages.
                var startIndex = biography.Length - "Read more on Last.fm.".Length;
                biography = startIndex > 0 ? biography.Remove(startIndex) : Strings.NoBiographyFound;
            }
            else
            {
                biography = Strings.NoBiographyFound;
            }
            this.Biography = biography;
        }
        
        public void MapFrom(Deezer.Artist deezerArtist)
        {
            this.Name = deezerArtist.Name;
            this.Url = deezerArtist.Link;
            var smallImage = new Image() { Url = string.Format("{0}?size=small", deezerArtist.Picture) };
            var mediumImage = new Image() { Url = string.Format("{0}?size=medium", deezerArtist.Picture) };
            var bigImage = new Image() { Url = string.Format("{0}?size=big", deezerArtist.Picture) };
            this.Images = new List<Image>() { smallImage, mediumImage, bigImage };
            this.DeezerId = deezerArtist.Id.ToString();
        }
    }
}
