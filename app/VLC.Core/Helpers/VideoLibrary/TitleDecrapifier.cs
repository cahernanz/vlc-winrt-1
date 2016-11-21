﻿/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/


using System.Collections.Generic;
using System.Linq;
using VLC.Model.Music;
using VLC.Utils;
using Windows.ApplicationModel.Resources;

namespace VLC.Helpers.VideoLibrary
{
    public static class TitleDecrapifier
    {
        public static string Decrapify(string title)
        {
            string[] ignoredWords = null;
            ignoredWords = new[]
            {
                @"xvid",
                @"h264",
                @"dvd",
                @"rip",
                @"divx",
                @"[fr]",
                @"720p",
                @"1080i",
                @"1080p",
                @"x264",
                @"hdtv",
                @"aac",
                @"bluray"
            };

            title = ignoredWords.Aggregate(title, (current, word) => current.Replace(word, ""));
            title = title.Replace(".", " ");
            title = title.Replace("_", " ");
            title = title.Replace("+", " ");
            title = title.Replace("-", " ");
            title = title.Replace("  ", " ");

            if (title.Length > 2)
            {
                try
                {
                    if (title.ElementAt(0) == 0x20)
                    {
                        title = title.Remove(0, 1);
                    }
                }
                catch
                {

                }
            }
            return title;
        }

        public static bool isTvShowEpisodeTitle(string title)
        {
            title = title.ToLower();
            for (int i = 0; i < title.Length - 5; i++)
            {
                if (title[i] == 's' &&
                    isDigit(title[i + 1]) &&
                    isDigit(title[i + 2]) &&
                    title[i + 3] == 'e' &&
                    isDigit(title[i + 4]) &&
                    isDigit(title[i + 5]))
                    return true;
            }
            return false;
        }

        public static MediaProperties tvShowEpisodeInfoFromString(MediaProperties mP, string title)
        {
            if (string.IsNullOrEmpty(title))
                return mP;
            title = title.ToLower();
            bool successfulSearch = false;
            int stringLength = title.Length;

            if (stringLength < 6)
                return mP;
            try
            {
                // Search for s01e10.
                for (int i = 0; i < stringLength - 5; i++)
                {
                    if (title.ElementAt(i) != 's' || !isDigit(title.ElementAt(i + 1)) ||
                        !isDigit(title.ElementAt(i + 2)) ||
                        title.ElementAt(i + 3) != 'e' || !isDigit(title.ElementAt(i + 4)) ||
                        !isDigit(title.ElementAt(i + 5)))
                    {
                        // Inverted "if" statement to reduce nesting.
                        continue;
                    }
                    string seasonString = title.ElementAt(i + 1).ToString() + title.ElementAt(i + 2).ToString();
                    string episodeString;
                    if (title.Length > i + 6 && isDigit(title.ElementAt(i + 6)))
                        episodeString = title.ElementAt(i + 4).ToString() + title.ElementAt(i + 5).ToString() +
                                  title.ElementAt(i + 6).ToString();
                    else
                        episodeString = title.ElementAt(i + 4).ToString() + title.ElementAt(i + 5).ToString();

                    string tvShowName = i > 0 ? title.Substring(0, i) : Strings.UnknownShow;
                    if (tvShowName != null)
                    {
                        tvShowName = CapitalizedString(Decrapify(tvShowName));
                    }

                    string episodeName = stringLength > i + 4 ? title.Substring(0, i + 6) : null;
                    if (episodeName != null)
                    {
                        episodeName = Decrapify(episodeName);
                    }

                    // Fill the MediaProperties object
                    var season = 0;
                    if(int.TryParse(seasonString, out season))
                    {
                        mP.Season = season;
                    }

                    var episode = 0;
                    if(int.TryParse(episodeString, out episode))
                    {
                        mP.Episode = episode;
                    }

                    if (!string.IsNullOrEmpty(tvShowName))
                    {
                        mP.ShowTitle = tvShowName;
                    }
                    if (!string.IsNullOrEmpty(episodeName))
                    {
                        mP.Title = CapitalizedString(episodeName);
                    }
                    successfulSearch = true;
                }

                // search for 0x00
                if (!successfulSearch)
                {
                    for (int i = 0; i < stringLength - 4; i++)
                    {
                        if (!isDigit(title.ElementAt(i)) || title.ElementAt(i + 1) != 'x' ||
                            !isDigit(title.ElementAt(i + 2)) || !isDigit(title.ElementAt(i + 3)))
                        {
                            // Inverted "if" statement to reduce nesting.
                            continue;
                        }

                        string seasonString = title.ElementAt(i).ToString();
                        string episodeString = title.ElementAt(i + 2).ToString() + title.ElementAt(i + 3).ToString();

                        string tvShowName = i > 0 ? title.Substring(0, i) : Strings.UnknownShow;
                        ;
                        if (tvShowName != null)
                        {
                            tvShowName = CapitalizedString(Decrapify(tvShowName));
                        }

                        string episodeName = stringLength > i + 4 ? title.Substring(0, i + 4) : null;
                        if (episodeName != null)
                        {
                            episodeName = Decrapify(episodeName);
                        }

                        var season = 0;
                        if (int.TryParse(seasonString, out season))
                        {
                            mP.Season = season;
                        }

                        // 'episode' will never be null according to conditions above, so checking for it is not needed.
                        var episode = 0;
                        if(int.TryParse(episodeString, out episode))
                        {
                            mP.Episode = episode;
                        }

                        if (!string.IsNullOrEmpty(tvShowName))
                        {
                            mP.ShowTitle = tvShowName;
                        }
                        if (!string.IsNullOrEmpty(episodeName))
                        {
                            mP.Title = CapitalizedString(episodeName);
                        }
                        successfulSearch = true;
                    }
                }
            }
            catch { }
            return mP;
        }

        static bool isDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private static string CapitalizedString(string input)
        {
            string[] arr = input.Split(' ');
            // Converted from foreach to LINQ
            string result = arr.Where(s => s.Length > 1).Aggregate(string.Empty, (current, s) => current + (s.First().ToString().ToUpper() + s.Substring(1) + " "));
            return result.Trim();
        }
    }
}
