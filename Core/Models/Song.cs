using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stoffi.Core.Models
{
    /// <summary>
    /// Describes a song which is performed by artists.
    /// </summary>
	public class Song : Model
	{
        /// <summary>
        /// The unique database ID of the song.
        /// </summary>
		public int Id { get; set; }

        /// <summary>
        /// The name of the song.
        /// </summary>
        [JsonProperty("title")]
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int Listens { get; set; }

        private ObservableCollection<Source> sources = new ObservableCollection<Source>();
        public ObservableCollection<Source> Sources
        {
            get { return sources; }
            set { SetProperty(ref sources, value); }
        }

        private ObservableCollection<Artist> artists = new ObservableCollection<Artist>();
        public ObservableCollection<Artist> Artists
        {
            get { return artists; }
            set { SetProperty(ref artists, value); }
        }

        private ObservableCollection<Genre> genres = new ObservableCollection<Genre>();
        public ObservableCollection<Genre> Genres
        {
            get { return genres; }
            set { SetProperty(ref genres, value); }
        }

        private string path = null;
        /// <summary>
        /// The unique path of the song.
        /// 
        /// Expected format: stoffi://&gt;
        /// For example: stoffi://song/youtube/dQw4w9WgXcQ
        /// </summary>
		public string Path
        {
            set { path = value;  }
            get
            {
                if (Sources.Count == 0)
                    return path;
                return Sources[0].Path;
            }
        }

        /// <summary>
        /// A display name for the artists of the song.
        /// </summary>
        public string Artist
        {
            get
            {
                if (Artists.Count == 0)
                    return "DJ Anonymous";
                return string.Join(" ft. ", Artists.Select(x => x.Name));
            }
        }

        static int videoN = 0;
        static string[] videos = {
            "dQw4w9WgXcQ", // *rick-roll*
            "9Riq6IMqXz8", // Vigiland - Pong Dance
            "Bznxx12Ptl0", // AronChupa - I'm an Albatraoz
            "60ItHLz5WEA", // Alan Walker - Faded
            "oyEuk8j8imI", // Justin Beiber - Love Yourself
            "fyeTJVU4wVo", // Amy Schumer - Girl, You Don't Need Makeup
            "oeCihv9A3ac", // Eminem - Phenomenal
            "0AqnCSdkjQ0", // Eminem - Guts Over Fear ft. Sia
            "enSDiaZJzEM" // Randy - I am LORDE!
        };
        public Song()
        {
            // TODO: temporary demo feature - fill youtube path
            if (String.IsNullOrWhiteSpace(path))
            {
                var id = videos[videoN % videos.Count()];
                path = $"stoffi://song/youtube/{id}";
                videoN++;
            }
        }

        /// <summary>
        /// Gets the ID section of a path.
        /// </summary>
        /// <returns>123 for stoffi://resource/backend/123</returns>
        public string GetPathId()
        {
            if (String.IsNullOrWhiteSpace(Path))
                throw new ArgumentException("Trying to extract ID from null Path");
            var uri = new Uri(Path);
            var fields = uri.AbsolutePath.Split('/');
            if (fields.Length != 3) // first is empty
                throw new FormatException($"Path '${Path}' is not in format stoffi://<resource>/<backend>/<id>");
            return fields[2];
        }
    }
}
