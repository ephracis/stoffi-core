using Stoffi.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stoffi.Core.Models;

namespace Stoffi.Core.Services
{
    public interface IMediaService : INotifyPropertyChanged
    {
        PlaybackState State { get; }
        TimeSpan CurrentTime { get; set; }
        TimeSpan RemainingTime { get; }
        PlaybackQuality PreferedQuality { get; set; }
        void Play(Song song = null);
        void Pause();
    }
}
