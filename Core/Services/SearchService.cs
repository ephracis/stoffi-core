using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stoffi.Core.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Serialization;
using System.Text.RegularExpressions;

namespace Stoffi.Core.Services
{
    /// <summary>
    /// Service for performing searches on the web API.
    /// </summary>
    public class SearchService : ISearchService
    {
        /// <summary>
        /// The HTTP client to use for sending web requests to the API.
        /// </summary>
        private HttpClient httpClient;

        private JsonSerializerSettings settings = new JsonSerializerSettings();

        /// <summary>
        /// Create an instance of the class.
        /// </summary>
        /// <param name="httpClient">The HTTP client dependency to use for sending web requests</param>
        public SearchService(HttpClient httpClient = null)
        {
            this.httpClient = httpClient ?? new HttpClient();
            settings.ContractResolver = new UnderscorePropertyNamesContractResolver();
        }

        /// <summary>
        /// Get a list of songs matching a given query.
        /// </summary>
        /// <param name="searchQuery">The query to match songs against</param>
        /// <param name="maxResults">The maximum number of hits to return</param>
        /// <returns>An object describing the search result</returns>
        public async Task<SearchResult> GetFilteredSongsAsync(string searchQuery, int maxResults)
        {
            var uri = new Uri($"{Constants.ServerAddress}/search.json?q={searchQuery}&limit={maxResults}");
            var response = await httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<SearchResult>(responseContent, settings);
            return result;
        }

        /// <summary>
        /// Get a list of suggestions given a search query.
        /// </summary>
        /// <param name="searchQuery">The query to generate suggestions from</param>
        /// <returns>A collection of query suggestions</returns>
        public async Task<IReadOnlyCollection<SearchSuggestion>> GetSearchSuggestionsAsync(string searchQuery)
        {
            var uri = new Uri($"{Constants.ServerAddress}/search/suggestions.json?q={searchQuery}");
            var response = await httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ReadOnlyCollection<SearchSuggestion>>(responseContent, settings);
            return result;
        }
    }

    public class UnderscorePropertyNamesContractResolver : DefaultContractResolver
    {
        public UnderscorePropertyNamesContractResolver() : base()
        {
        }

        protected override string ResolvePropertyName(string propertyName)
        {
            return Regex.Replace(propertyName, @"(\w)([A-Z])", "$1_$2").ToLower();
        }
    }
}
