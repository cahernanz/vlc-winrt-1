﻿using Windows.UI.Xaml.Controls;
using VLC.Helpers;
using VLC.Model.Video;
using VLC.ViewModels;
using VLC.Model;
using VLC.Utils;
using System.Collections.Generic;

namespace VLC.Commands.VideoPlayer
{
    public class PlayVideoCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            LogHelper.Log("PlayVideoCommand called");

            VideoItem videoVm = null;
            if (parameter is ItemClickEventArgs)
            {
                ItemClickEventArgs args = parameter as ItemClickEventArgs;
                videoVm = args.ClickedItem as VideoItem;
            }
            else if (parameter is VideoItem)
            {
                videoVm = parameter as VideoItem;
            }

            // If the VM is null, we can't do anything. So just return.
            if (videoVm == null)
            {
#if DEBUG
                ToastHelper.Basic("Failed to load the selected video, the video view model is null.");
#endif
                LogHelper.Log("PLAYVIDEO: VideoVm is null, returning");
                return;
            }

            LogHelper.Log("PLAYVIDEO: VideoVm is not null, continuing");
            try
            {
                // If the video file is null (For example, the user deleted the video, and it's on
                // their favorites list.) We need to make sure the whole app does not crash.

                // TODO: If user selectes a video from their favoites, and it has been moved or deleted, we should ask them if we want to remove it from their list

                await Locator.MediaPlaybackViewModel.PlaybackService.SetPlaylist(new List<IMediaItem> { videoVm }, true, true, videoVm);
            }
            catch (System.Exception e)
            {
                // TODO: Enhance error handling
                // TODO: Remove hardcoded English
                ToastHelper.Basic(Strings.FailOpenVideo);
                return;
            }

            try
            {
                Locator.NavigationService.Go(VLCPage.VideoPlayerPage);
                LogHelper.Log("PLAYVIDEO: Navigating to VideoPlayerPage");
            }
            catch
            {
                // TODO: Enhance error handling
                // TODO: Remove hardcoded English
                ToastHelper.Basic(string.Format(Strings.FailNavigateVideoPlayerPage));
                LogHelper.Log("PLAYVIDEO: failed to navigate to video player page.");
            }
        }
    }
}
