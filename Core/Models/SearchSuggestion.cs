using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stoffi.Core.Models
{
    /// <summary>
    /// Describe a suggestion for a search query.
    /// </summary>
    public class SearchSuggestion
    {
        /// <summary>
        /// The suggested search query.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// A relative score of the suggestion for ranking purposes.
        /// </summary>
        public double Score { get; set; }
    }
}
