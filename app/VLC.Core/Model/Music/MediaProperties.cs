﻿using System;

namespace VLC.Model.Music
{
    public class MediaProperties
    {
        public string Path { get; set; }
        public string Album { get; set; }
        public string Artist { get; set; }
        public string AlbumArtist { get; set; }
        public TimeSpan Duration { get; set; }
        public string Title { get; set; }
        public uint Tracknumber { get; set; }
        public int Year { get; set; }
        public string Genre { get; set; }
        public string AlbumArt { get; set; }
        public int DiscNumber { get; set; }


        public int Season { get; set; }
        public int Episode { get; set; }
        public int Episodes { get; set; }
        public string ShowTitle { get; set; }

        public uint Height { get; set; }
        public uint Width { get; set; }
    }
}
