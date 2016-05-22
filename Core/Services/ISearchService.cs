using Stoffi.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stoffi.Core.Services
{
    public interface ISearchService
    {
        Task<SearchResult> GetFilteredSongsAsync(string searchQuery, int maxResults);

        Task<IReadOnlyCollection<SearchSuggestion>> GetSearchSuggestionsAsync(string searchQuery);
    }
}
