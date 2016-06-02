using Stoffi.Core.Models;
using Stoffi.Core.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Stoffi.Core.Services
{
    public interface IPlaybackService : INotifyPropertyChanged
    {
        Song Song { get; set; }
        ObservableCollection<Song> Upcoming { get; }
        PlaybackState State { get; set; }
    }
}
