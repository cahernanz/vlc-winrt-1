﻿using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Slide2D.Images;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VLC.Slideshow.Texts;
using VLC.ViewModels;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Slide2D
{
    public class MetroSlideshow
    {
        public TaskCompletionSource<bool> IsLoaded = new TaskCompletionSource<bool>();
        public static CanvasAnimatedControl canvas;
        public static double WindowHeight;
        public static double WindowWidth;

        public delegate void WindowSizeChanged();

        public static event WindowSizeChanged WindowSizeUpdated;
        private ImgSlideshow slideshow;

        public MetroSlideshow()
        {
            SetWindowSize();
        }

        public void Initialize(ref CanvasAnimatedControl cac)
        {
            canvas = cac;
            canvas.CreateResources += Canvas_CreateResources;
            canvas.Update += Canvas_Update;
            canvas.Draw += Canvas_Draw;
            canvas.ForceSoftwareRenderer = false;
            canvas.Paused = true;

            float dpiLimit = 96.0f;
            if (canvas.Dpi > dpiLimit)
            {
                canvas.DpiScale = dpiLimit / canvas.Dpi;
            }

            Window.Current.SizeChanged += Current_SizeChanged;
            slideshow = new ImgSlideshow();
        }
        
        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            SetWindowSize();
            WindowSizeUpdated?.Invoke();
        }

        private void SetWindowSize()
        {
            WindowWidth = Window.Current.Bounds.Width;
            WindowHeight = Window.Current.Bounds.Height;
        }

        private void Canvas_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            slideshow.Update(sender, args);
        }

        private void Canvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            slideshow.Draw(sender, args);
        }

        private void Canvas_CreateResources(CanvasAnimatedControl sender,
            Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            IsLoaded.TrySetResult(true);
        }
        
        public bool IsPaused
        {
            get { return canvas != null && canvas.Paused; }
            set { if (canvas != null) canvas.Paused = value; }
        }
    }
}