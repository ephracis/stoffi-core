using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        /// The name of the song.
        /// </summary>
        [JsonProperty("title")]
		public string Name { get; set; }

        /// <summary>
        /// The song ID.
        /// </summary>
		public int Id { get; set; }
	}
}
