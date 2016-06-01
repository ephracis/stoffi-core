using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stoffi.Core.Enums
{
    /// <summary>
    /// The current state of the playback service.
    /// </summary>
    public enum PlaybackState
    {
        /// <summary>
        /// A song is about to be played, possibly buffering the stream.
        /// </summary>
        Loading,

        /// <summary>
        /// A song is currently being played.
        /// </summary>
        Playing,

        /// <summary>
        /// Nothing is being played.
        /// </summary>
        Paused
    }
}
