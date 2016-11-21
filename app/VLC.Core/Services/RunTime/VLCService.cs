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
using VLC.Helpers;
using VLC.Model;
using VLC.Services.Interface;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VLC.Model.Music;
using VLC.Model.Stream;
using libVLCX;
using VLC.Utils;
using MediaPlayer = libVLCX.MediaPlayer;
using VLC.ViewModels;
using Windows.Media.Devices;
using VLC.UI.Views.UserControls.Shell;

namespace VLC.Services.RunTime
{
    public sealed class VLCService : IMediaService
    {
        public event EventHandler<MediaState> StatusChanged;
        public event TimeChanged TimeChanged;
        public event EventHandler MediaFailed;
        public event Action OnStopped;
        public event Action<long> OnLengthChanged;
        public event Action OnEndReached;
        public event Action<int> OnBuffering;

        public TaskCompletionSource<bool> PlayerInstanceReady { get; set; } = new TaskCompletionSource<bool>();

        public Instance Instance { get; private set; }
        // Contains the IAudioClient address, as a string.
        private AudioDeviceHandler AudioClient { get; set; }
        private String _audioDeviceID;
        private String AudioDeviceID
        {
            get
            {
                if ( _audioDeviceID == null )
                   _audioDeviceID = MediaDevice.GetDefaultAudioRenderId(AudioDeviceRole.Default);
                return _audioDeviceID;
            }
            set { _audioDeviceID = value; }
        }
        public MediaPlayer MediaPlayer { get; private set; }
        private VLCDialog CurrentDialog;

        public Task Initialize(object o = null)
        {
            return DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                var param = new List<string>
                {
                    "-I",
                    "dummy",
                    "--no-osd",
                    "--verbose=3",
                    "--no-stats",
                    "--avcodec-fast",
                    string.Format("--freetype-font={0}\\NotoSans-Regular.ttf",Windows.ApplicationModel.Package.Current.InstalledLocation.Path),
                    "--subsdec-encoding",
                    Locator.SettingsVM.SubtitleEncodingValue == "System" ? "" : Locator.SettingsVM.SubtitleEncodingValue,
                    "--aout=winstore",
                    string.Format("--keystore-file={0}\\keystore", ApplicationData.Current.LocalFolder.Path),
                };

                // So far, this NEEDS to be called from the main thread
                try
                {
                    Instance = new Instance(param, App.RootPage.SwapChainPanel);
                    Instance?.setDialogHandlers(
                        async (title, text) =>
                        {
                            await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                            {
                                await VLCDialog.WaitForDialogLock();
                                CurrentDialog = new VLCDialog(title, text);
                                await CurrentDialog.ShowAsync();
                            });
                        },
                        async (dialog, title, text, defaultUserName, askToStore) =>
                        {
                            await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                            {
                                await VLCDialog.WaitForDialogLock();
                                CurrentDialog = new VLCDialog(title, text, dialog, defaultUserName, askToStore);
                                await CurrentDialog.ShowAsync();
                            });
                        },

                        async (dialog, title, text, qType, cancel, action1, action2) =>
                        {
                            await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                            {
                                if (qType == Question.warning)
                                {
                                    dialog.postAction(1);
                                    return;
                                }
                                await VLCDialog.WaitForDialogLock();
                                CurrentDialog = new VLCDialog(title, text, dialog, qType, cancel, action1, action2);
                                await CurrentDialog.ShowAsync();
                            });
                        },

                        (dialog, title, text, intermidiate, position, cancel) => { },
                        async (dialog) =>
                        {
                            await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => CurrentDialog.Cancel());
                        },
                        (dialog, position, text) => { });

                    // Audio device management also needs to be called from the main thread
                    AudioClient = new AudioDeviceHandler(AudioDeviceID);
                    MediaDevice.DefaultAudioRenderDeviceChanged += onDefaultAudioRenderDeviceChanged;
                    PlayerInstanceReady.TrySetResult(Instance != null);
                }
                catch (Exception e)
                {
                    LogHelper.Log("VLC Service : Couldn't create VLC Instance\n" + StringsHelper.ExceptionToString(e));
                    ToastHelper.Basic(Strings.FailStartVLCEngine);
                }
            });
        }

        private async void onDefaultAudioRenderDeviceChanged(object sender, DefaultAudioRenderDeviceChangedEventArgs args)
        {
            if (args.Role != AudioDeviceRole.Default || args.Id == AudioDeviceID)
                return;

            AudioDeviceID = args.Id;
            // If we don't have an instance yet, no need to fetch the audio client as it will be done upon
            // instance creation.
            if (Instance == null)
                return;
            await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                // Always fetch the new audio client, as we always assign it when starting a new playback
                AudioClient = new AudioDeviceHandler(AudioDeviceID);
                // But if a playback is in progress, inform VLC backend that we changed device
                if (MediaPlayer != null)
                    MediaPlayer.outputDeviceSet(AudioClient.audioClient());
            });
        }

        public async Task SetMediaFile(IMediaItem media)
        {
            Media vlcMedia = null;
            if (media.VlcMedia != null)
            {
                vlcMedia = media.VlcMedia;
            }
            else
            {
                var mrl_fromType = media.GetMrlAndFromType();
                LogHelper.Log("SetMRL: " + mrl_fromType.Item2);
                if (Instance == null)
                {
                    await Initialize();
                }
                await PlayerInstanceReady.Task;

                if (!PlayerInstanceReady.Task.Result)
                {
                    LogHelper.Log($"Couldn't play media {media.Name} as VLC failed to init");
                    return;
                }

                vlcMedia = new Media(Instance, mrl_fromType.Item2, mrl_fromType.Item1);
            }

            // Hardware decoding
            vlcMedia.addOption(!Locator.SettingsVM.HardwareAccelerationEnabled ? ":avcodec-hw=none" : ":avcodec-hw=d3d11va");
            vlcMedia.addOption(!Locator.SettingsVM.HardwareAccelerationEnabled ? ":avcodec-threads=0" : ":avcodec-threads=1");

            MediaPlayer = new MediaPlayer(vlcMedia);
            LogHelper.Log("PLAYWITHVLC: MediaPlayer instance created");
            MediaPlayer.outputDeviceSet(AudioClient.audioClient());
            SetEqualizer(Locator.SettingsVM.Equalizer);
            var em = MediaPlayer.eventManager();
            em.OnOpening += Em_OnOpening;
            em.OnBuffering += EmOnOnBuffering;
            em.OnStopped += EmOnOnStopped;
            em.OnPlaying += OnPlaying;
            em.OnPaused += OnPaused;
            if (TimeChanged != null)
                em.OnTimeChanged += TimeChanged;
            em.OnEndReached += EmOnOnEndReached;
            em.OnEncounteredError += em_OnEncounteredError;
            em.OnLengthChanged += em_OnLengthChanged;
        }

        #region events
        private void EmOnOnBuffering(float param0)
        {
            OnBuffering?.Invoke((int)param0);
        }

        void em_OnLengthChanged(long __param0)
        {
            OnLengthChanged?.Invoke(__param0);
        }

        private void EmOnOnEndReached()
        {
            OnEndReached?.Invoke();
        }

        private void EmOnOnStopped()
        {
            StatusChanged?.Invoke(this, MediaState.Stopped);
            OnStopped?.Invoke();
        }

        void em_OnEncounteredError()
        {
            Debug.WriteLine("VLCService: An error occurred ");
            MediaFailed?.Invoke(this, new EventArgs());
        }

        #endregion
        #region file meta info
        public async Task<Media> GetMediaFromPath(string filePath)
        {
            if (Instance == null)
            {
                await Initialize();
            }
            await PlayerInstanceReady.Task;
            if (string.IsNullOrEmpty(filePath))
                return null;
            return new Media(Instance, filePath, FromType.FromPath);
        }

        public async Task<MediaProperties> GetVideoProperties(MediaProperties mP, Media media)
        {
            if (Instance == null)
            {
                await Initialize();
            }
            await PlayerInstanceReady.Task;
            if (media == null)
                return mP;
            if (media.parsedStatus() != ParsedStatus.Done)
            {
                var res = await media.parseWithOptionsAsync(ParseFlags.FetchLocal | ParseFlags.Local | ParseFlags.Network, 5000);
                if (res != ParsedStatus.Done)
                    return mP;
            }
            
            mP.Title = media.meta(MediaMeta.Title);

            var showName = media.meta(MediaMeta.ShowName);
            if (string.IsNullOrEmpty(showName))
            {
                showName = media.meta(MediaMeta.Artist);
            }
            if (!string.IsNullOrEmpty(showName))
            {
                mP.ShowTitle = showName;
            }

            var episodeString = media.meta(MediaMeta.Episode);
            if (string.IsNullOrEmpty(episodeString))
            {
                episodeString = media.meta(MediaMeta.TrackNumber);
            }
            var episode = 0;
            if (!string.IsNullOrEmpty(episodeString) && int.TryParse(episodeString, out episode))
            {
                mP.Episode = episode;
            }

            var episodesTotal = 0;
            var episodesTotalString = media.meta(MediaMeta.TrackTotal);
            if (!string.IsNullOrEmpty(episodesTotalString) && int.TryParse(episodesTotalString, out episodesTotal))
            {
                mP.Episodes = episodesTotal;
            }

            var videoTrack = media.tracks().FirstOrDefault(x => x.type() == TrackType.Video);
            if (videoTrack != null)
            {
                mP.Width = videoTrack.width();
                mP.Height = videoTrack.height();
            }

            var durationLong = media.duration();
            var duration = TimeSpan.FromMilliseconds(durationLong);
            mP.Duration = duration;

            return mP;
        }

        public async Task<MediaProperties> GetMusicProperties(Media media)
        {
            if (Instance == null)
            {
                await Initialize();
            }
            await PlayerInstanceReady.Task;
            if (media == null)
                return null;
            if (media.parsedStatus() != ParsedStatus.Done)
            {
                var res = await media.parseWithOptionsAsync(ParseFlags.FetchLocal | ParseFlags.Local | ParseFlags.Network, 5000);
                if (res != ParsedStatus.Done)
                    return null;
            }

            var mP = new MediaProperties();
            mP.AlbumArtist = media.meta(MediaMeta.AlbumArtist);
            mP.Artist = media.meta(MediaMeta.Artist);
            mP.Album = media.meta(MediaMeta.Album);
            mP.Title = media.meta(MediaMeta.Title);
            mP.AlbumArt = media.meta(MediaMeta.ArtworkURL);
            var yearString = media.meta(MediaMeta.Date);
            var year = 0;
            if (int.TryParse(yearString, out year))
            {
                mP.Year = year;
            }

            var durationLong = media.duration();
            TimeSpan duration = TimeSpan.FromMilliseconds(durationLong);
            mP.Duration = duration;

            var trackNbString = media.meta(MediaMeta.TrackNumber);
            uint trackNbInt = 0;
            uint.TryParse(trackNbString, out trackNbInt);
            mP.Tracknumber = trackNbInt;

            var discNb = media.meta(MediaMeta.DiscNumber);
            if (discNb.Contains("/"))
            {
                // if discNb = "1/2"
                var discNumDen = discNb.Split('/');
                if (discNumDen.Any())
                    discNb = discNumDen[0];
            }
            int discNbInt = 1;
            int.TryParse(discNb, out discNbInt);
            mP.DiscNumber = discNbInt;

            var genre = media.meta(MediaMeta.Genre);
            mP.Genre = genre;
            
            return mP;
        }
        #endregion
        #region playback actions
        public void SetSubtitleFile(string mrl)
        {
            MediaPlayer?.addSlave(SlaveType.Subtitle, mrl, true);
        }

        public void SetSubtitleTrack(int i)
        {
            MediaPlayer?.setSpu(i);
        }

        public void SetAudioTrack(int i)
        {
            MediaPlayer?.setAudioTrack(i);
        }

        public void SetAudioDelay(long delay)
        {
            MediaPlayer?.setAudioDelay(delay);
        }

        public void SetSpuDelay(long delay)
        {
            MediaPlayer?.setSpuDelay(delay);
        }
        
        public void Play()
        {
            MediaPlayer?.play();
        }

        public void Play(int trackId)
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            MediaPlayer?.pause();
        }

        public void Stop()
        {
            MediaPlayer?.stop();
        }


        public void FastForward()
        {
            throw new NotImplementedException();
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }

        public void SkipAhead()
        {
            MediaPlayer?.setTime(MediaPlayer.time() + 10000);
        }

        public void SkipBack()
        {
            MediaPlayer?.setTime(MediaPlayer.time() - 10000);
        }

        public float GetLength()
        {
            return MediaPlayer?.length() ?? 0;
        }

        public long GetTime()
        {
            return MediaPlayer?.time() ?? 0;
        }

        public void SetTime(long desiredTime)
        {
            MediaPlayer?.setTime(desiredTime);
        }

        public float GetPosition()
        {
            return MediaPlayer?.position() ?? 0.0f;
        }

        public void SetPosition(float desiredPosition)
        {
            MediaPlayer?.setPosition(desiredPosition);
        }

        public void SetVolume(int volume)
        {
            MediaPlayer?.setVolume(volume);
        }


        public int GetVolume()
        {
            return MediaPlayer.volume();
        }

        public void SetSpeedRate(float desiredRate)
        {
            MediaPlayer?.setRate(desiredRate);
        }

        public void Trim()
        {
            Instance?.Trim();
        }

        private void OnPaused()
        {
            StatusChanged(this, MediaState.Paused);
        }

        private void OnPlaying()
        {
            StatusChanged(this, MediaState.Playing);
        }

        private void Em_OnOpening()
        {
            StatusChanged(this, MediaState.Opening);
        }

        public void SetSizeVideoPlayer(uint x, uint y)
        {
            Instance?.UpdateSize(x, y);
        }

        public IList<VLCEqualizer> GetEqualizerPresets()
        {
            var presetCount = Equalizer.presetCount();
            var presets = new List<VLCEqualizer>();
            for (uint i = 0; i < presetCount; i++)
            {
                presets.Add(new VLCEqualizer(i));
            }
            return presets;
        }

        public void SetEqualizer(VLCEqualizer vlcEq)
        {
            var eq = new Equalizer(vlcEq.Index);
            MediaPlayer?.setEqualizer(eq);
        }
        #endregion
    }
}
