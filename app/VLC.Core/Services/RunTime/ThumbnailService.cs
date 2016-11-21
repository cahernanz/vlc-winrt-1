﻿/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;
using VLC.Services.Interface;
using Windows.Storage;
using Windows.Storage.FileProperties;
using VLC.Helpers;
using libVLCX;

namespace VLC.Services.RunTime
{
    public class ThumbnailService : IDisposable
    {
        private Thumbnailer _thumbnailer = new Thumbnailer();
        private bool _disposed = false;

        public async Task<StorageItemThumbnail> GetThumbnail(StorageFile file)
        {
            StorageItemThumbnail thumb = null;

            try
            {
                thumb = await file.GetThumbnailAsync(ThumbnailMode.VideosView);
            }
            catch (Exception e)
            {
                LogHelper.Log("Error getting thumbnail: ");
                LogHelper.Log(StringsHelper.ExceptionToString(e));
            }
            return thumb;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                _thumbnailer?.Dispose();
            }
            _disposed = true;
        }

        public async Task<PreparseResult> GetScreenshot(string token)
        {
            //if (file == null) return null;
            //string token = StorageApplicationPermissions.FutureAccessList.Add(file);
            //var res = await _thumbnailer.TakeScreenshot("winrt://" + token, 320, 200, 2500);
            var res = await _thumbnailer.TakeScreenshot(token, 320, 200, 2500);
            return res;
        }
    }
}
