using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stoffi.Core.Models;
using Stoffi.Core.Services;
using Stoffi.Core.Tests.Mocks;
using System.Collections.Generic;
using System.Net.Http;

namespace Stoffi.Core.Tests.Services
{
    [TestClass]
    public class SearchServiceTest
    {
        [TestMethod]
        public void GetFilteredSongs_Match_ReturnsObject()
        {
            var mockResponseHandler = new MockResponseHandler();
            mockResponseHandler.AddFakeResponse(
                "http://l.stoffi.io/search.json?q=love&limit=3",
                "SearchResult_Love.json");
            var httpClient = new HttpClient(mockResponseHandler);
            var searchService = new SearchService(httpClient);
            var searchResults = searchService.GetFilteredSongsAsync("love", 3).Result;
            Assert.AreEqual(947, searchResults.TotalHits);
            Assert.AreEqual(428, searchResults.Hits[0].Id);
            Assert.AreEqual("Love", searchResults.Hits[0].Name);
        }

        [TestMethod]
        public void GetFilteredSongs_NoMatch_ReturnsEmptyObject()
        {
            var mockResponseHandler = new MockResponseHandler();
            mockResponseHandler.AddFakeResponse(
                "http://l.stoffi.io/search.json?q=&limit=3",
                "SearchResult_Empty.json");
            var httpClient = new HttpClient(mockResponseHandler);
            var searchService = new SearchService(httpClient);
            var searchResults = searchService.GetFilteredSongsAsync("", 3).Result;
            Assert.AreEqual(0, searchResults.TotalHits);
            Assert.AreEqual(0, searchResults.Hits.Count);
        }

        [TestMethod]
        public void GetSearchSuggestions_Match_ReturnsMatches()
        {
            var mockResponseHandler = new MockResponseHandler();
            mockResponseHandler.AddFakeResponse(
                "http://l.stoffi.io/search/suggestions.json?q=lo",
                "SearchSuggestions_Lo.json");
            var httpClient = new HttpClient(mockResponseHandler);
            var searchService = new SearchService(httpClient);
            var suggestions = searchService.GetSearchSuggestionsAsync("lo").Result;
            var suggestionList = new List<SearchSuggestion>(suggestions);
            Assert.AreEqual(3, suggestions.Count);
            Assert.AreEqual("love", suggestionList[0].Value);
            Assert.AreEqual(60.0, suggestionList[0].Score);
        }

        [TestMethod]
        public void GetSearchSuggestions_NoMatch_ReturnsEmptyList()
        {
            var mockResponseHandler = new MockResponseHandler();
            mockResponseHandler.AddFakeResponse(
                "http://l.stoffi.io/search/suggestions.json?q=",
                "SearchSuggestions_Empty.json");
            var httpClient = new HttpClient(mockResponseHandler);
            var searchService = new SearchService(httpClient);
            var suggestions = searchService.GetSearchSuggestionsAsync("").Result;
            Assert.AreEqual(0, suggestions.Count);
        }
    }
}
