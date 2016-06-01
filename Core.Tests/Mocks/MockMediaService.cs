using System;
using System.Collections.Generic;
using Stoffi.Core.Enums;
using Stoffi.Core.Services;
using Microsoft.Practices.Prism.Mvvm;
using Stoffi.Core.Models;

namespace Stoffi.Core.Tests.Mocks
{
    public class MockMediaService : BindableBase, IMediaService
    {
        public TimeSpan CurrentTime { get; set; }
        public PlaybackQuality PreferedQuality { get; set; }
        public TimeSpan RemainingTime { get; set; }
        public PlaybackState State { get; set; }
        public List<string> CalledMethods = new List<string>();

        public void Pause()
        {
            CalledMethods.Add("Pause()");
        }

        public void Play(Song song = null)
        {
            CalledMethods.Add($"Play({song})");
        }
    }
}
