using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stoffi.Core.Enums;
using Stoffi.Core.Models;
using Stoffi.Core.Services;
using Stoffi.Core.Tests.Mocks;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Stoffi.Core.Tests.Services
{
    [TestClass]
    public class PlaybackServiceTest
    {
        [TestMethod]
        public void SetSong_NotNull_RaisePropertyChanged()
        {
            string firedEvent = null;
            var media = new MockMediaService();
            var service = new PlaybackService(media, new SettingsService(new MockSettingsStorage()));
            service.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                firedEvent = e.PropertyName;
            };
            service.Song = new Song();
            Assert.IsNotNull(firedEvent);
            Assert.AreEqual("Song", firedEvent);
        }

        [TestMethod]
        public void SetSong_Same_DontRaisePropertyChanged()
        {
            var song = new Song();
            string firedEvent = null;
            var media = new MockMediaService();
            var service = new PlaybackService(media, new SettingsService(new MockSettingsStorage()));
            service.Song = song;
            service.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
            {
                firedEvent = e.PropertyName;
            };
            service.Song = song;
            Assert.IsNull(firedEvent);
        }

        [TestMethod]
        public void SetState_PlayingWithoutSong_PlayMedia()
        {
            var media = new MockMediaService();
            var service = new PlaybackService(media, new SettingsService(new MockSettingsStorage()));
            service.State = PlaybackState.Playing;
            Assert.IsTrue(media.CalledMethods.Contains("Play()"));
        }

        [TestMethod]
        public void SetState_PlayingWithSong_PlayMedia()
        {
            var song = new Song { Path = "foobar" };
            var media = new MockMediaService();
            var service = new PlaybackService(media, new SettingsService(new MockSettingsStorage()));
            service.Song = song;
            service.State = PlaybackState.Playing;
            Assert.IsTrue(media.CalledMethods.Contains("Play(Stoffi.Core.Models.Song)"));
            Assert.AreEqual(1, media.CalledMethods.Count);
        }

        [TestMethod]
        public void SetSong_PausedWithSong_PlayMedia()
        {
            var song = new Song();
            var media = new MockMediaService();
            var service = new PlaybackService(media, new SettingsService(new MockSettingsStorage()));
            service.State = PlaybackState.Paused;
            media.CalledMethods.Clear();
            service.Song = song;
            Assert.AreEqual(0, media.CalledMethods.Count);
        }

        [TestMethod]
        public void SetSong_LoadingWithSong_PlayMedia()
        {
            var song = new Song();
            var media = new MockMediaService();
            var service = new PlaybackService(media, new SettingsService(new MockSettingsStorage()));
            service.State = PlaybackState.Loading;
            media.CalledMethods.Clear();
            service.Song = song;
            Assert.IsTrue(media.CalledMethods.Contains("Play(Stoffi.Core.Models.Song)"));
            Assert.AreEqual(1, media.CalledMethods.Count);
        }

        [TestMethod]
        public void SetSong_Null_PauseMedia()
        {
            var media = new MockMediaService();
            var service = new PlaybackService(media, new SettingsService(new MockSettingsStorage()));
            service.State = PlaybackState.Playing;
            service.Song = new Song();
            media.CalledMethods.Clear();
            service.Song = null;
            Assert.IsTrue(media.CalledMethods.Contains("Pause()"));
            Assert.AreEqual(1, media.CalledMethods.Count);
        }

        [TestMethod]
        public async Task ChangeSong_SavesToSettings()
        {
            var media = new MockMediaService();
            var settings = new SettingsService(new MockSettingsStorage());
            var service = new PlaybackService(media, settings);
            var song = new Song() { Id = 1337 };
            service.State = PlaybackState.Playing;
            service.Song = song;
            Assert.AreEqual(song, await settings.Read<Song>("PlaybackSong", null));
        }

        [TestMethod]
        public async Task Constructor_LoadsSongFromSettings()
        {
            var media = new MockMediaService();
            var settings = new SettingsService(new MockSettingsStorage());
            var song = new Song() { Id = 1337 };
            await settings.Write("PlaybackSong", song);
            var service = new PlaybackService(media, settings);
            Assert.AreEqual(song, service.Song);
        }

        [TestMethod]
        public async Task ChangeState_SavesToSettings()
        {
            var media = new MockMediaService();
            var settings = new SettingsService(new MockSettingsStorage());
            var service = new PlaybackService(media, settings);
            service.Song = new Song() { Id = 1337 };
            service.State = PlaybackState.Loading;
            Assert.AreEqual(PlaybackState.Loading, await settings.Read<PlaybackState>("PlaybackState", PlaybackState.Paused));
        }

        [TestMethod]
        public async Task Constructor_LoadsStateFromSettings()
        {
            var media = new MockMediaService();
            var settings = new SettingsService(new MockSettingsStorage());
            await settings.Write("PlaybackState", PlaybackState.Loading);
            var service = new PlaybackService(media, settings);
            Assert.AreEqual(PlaybackState.Loading, service.State);
        }
    }
}
