using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

namespace Stoffi.Core.Tests.Mocks
{
    public class MockResponseHandler : DelegatingHandler
    {
        /// <summary>
        /// Holds all registered fake responses.
        /// </summary>
        private readonly Dictionary<Uri, HttpResponseMessage> fakeResponses = new Dictionary<Uri, HttpResponseMessage>();

        /// <summary>
        /// Add a mock response to return for a given URI.
        /// </summary>
        /// <param name="uri">The URI for the response</param>
        /// <param name="responseFile">The name of the SampleData file to use as response body</param>
        /// <param name="statusCode">The status code to return</param>
        public void AddFakeResponse(string uri, string responseFile, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var responseFileName = $"Stoffi.Core.Tests.SampleData.{responseFile}";
            using (var stream = typeof(MockResponseHandler).Assembly.GetManifestResourceStream(responseFileName))
            {
                using (var buffer = new MemoryStream())
                {
                    stream.CopyTo(buffer);
                    buffer.Position = 0;
                    var sr = new StreamReader(buffer);
                    var responseBody = sr.ReadToEnd();
                    AddFakeResponse(uri, new StringContent(responseBody));
                }
            }
        }

        /// <summary>
        /// Add a mock response to return for a given URI.
        /// </summary>
        /// <param name="uri">The URI for the response</param>
        /// <param name="responseBody">The response body</param>
        /// <param name="statusCode">The status code to return</param>
        public void AddFakeResponse(string uri, StringContent responseBody, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var response = new HttpResponseMessage(statusCode);
            response.Content = responseBody;
            AddFakeResponse(uri, response);
        }

        /// <summary>
        /// Add a mock response to return for a given URI.
        /// </summary>
        /// <param name="uri">The URI for the response</param>
        /// <param name="response">The response object</param>
        public void AddFakeResponse(string uri, HttpResponseMessage response)
        {
            fakeResponses.Add(new Uri(uri), response);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// Send a request and return either a fake response or a 404.
        /// </summary>
        /// <param name="request">The request to send</param>
        /// <param name="cancellationToken">Not used</param>
        /// <returns>Either a registered fake response, or 404 if no fake response exist</returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            if (fakeResponses.ContainsKey(request.RequestUri))
                return fakeResponses[request.RequestUri];
            else
                return new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request };
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}
