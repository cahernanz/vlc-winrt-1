﻿using System;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Commands.MediaLibrary
{
    public class IndexMediaLibraryCommand : AlwaysExecutableCommand
    {
        public async override void Execute(object parameter)
        {
            await Locator.MediaLibrary.Initialize().ConfigureAwait(false);
        }
    }
}
