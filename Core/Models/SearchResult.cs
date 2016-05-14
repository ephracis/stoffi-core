using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stoffi.Core.Models
{
    /// <summary>
    /// Represents a result from a search.
    /// </summary>
    public class SearchResult
    {
        /// <summary>
        /// The total number of songs that matched the query.
        /// </summary>
        public int TotalHits { get; set; }

        /// <summary>
        /// A list of all songs that matched the query.
        /// </summary>
        public List<Song> Hits { get; set; }

        /// <summary>
        /// Create a new instance of the class.
        /// </summary>
        /// <param name="totalHits">The total number of songs that matched the query</param>
        /// <param name="hits">A list of all songs that matched the query</param>
        public SearchResult(int totalHits, List<Song> hits)
        {
            TotalHits = totalHits;
            Hits = hits;
        }
    }
}
