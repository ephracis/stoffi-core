using Stoffi.Core.Enums;
using Stoffi.Core.Models;
using Microsoft.Practices.Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Stoffi.Core.Services
{
    public class PlaybackService : BindableBase, IPlaybackService
    {
        private Song song;
        /// <summary>
        /// The currently loaded song.
        /// </summary>
        public Song Song
        {
            get { return song; }
            set { SetProperty<Song>(ref song, value); }
        }

        private PlaybackState state = PlaybackState.Paused;
        /// <summary>
        /// The current state of the playback service.
        /// </summary>
        public PlaybackState State
        {
            get { return state; }
            set {
                SetProperty<PlaybackState>(ref state, value);
            }
        }
        
        /// <summary>
        /// A list of upcoming songs.
        /// </summary>
        public ObservableCollection<Song> Upcoming { get; private set; }

        /// <summary>
        /// Platform specific code for managing media.
        /// </summary>
        private IMediaService mediaService;

        private ISettingsService settings;

        public PlaybackService(IMediaService mediaService, ISettingsService settingsService)
        {
            Upcoming = new ObservableCollection<Song>();
            this.mediaService = mediaService;
            this.settings = settingsService;
            PropertyChanged += PlaybackService_PropertyChanged;
            LoadSettings();
        }

        /// <summary>
        /// Acts on changes in the properties of this service.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event data</param>
        private void PlaybackService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "State":
                    StateChanged();
                    break;

                case "Song":
                    SongChanged();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Acts on the media service according to a recent change in state.
        /// </summary>
        private void StateChanged()
        {
            switch (State)
            {
                case PlaybackState.Loading:
                    break;

                case PlaybackState.Playing:
                    mediaService.Play(Song);
                    break;

                case PlaybackState.Paused:
                    mediaService.Pause();
                    break;

                default:
                    break;
            }
            settings.Write("PlaybackState", State);
        }

        /// <summary>
        /// Acts on the media service according to a recent change in state.
        /// </summary>
        private void SongChanged()
        {
            if (Song == null)
                mediaService.Pause();
            else if (State != PlaybackState.Paused)
                mediaService.Play(Song);
            settings.Write("PlaybackSong", Song);
        }

        private async Task LoadSettings()
        {
            Song = await settings.Read<Song>("PlaybackSong", null);
            State = await settings.Read<PlaybackState>("PlaybackState", state);
        }
    }
}
