#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="HttpClientFactory.cs">
// MIT License
// 
// Copyright(c) 2018 Robert Vandehey
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion
using System;
using System.Net.Http;
using System.Threading;

namespace SynchroFeed.Library
{
    /// <summary>
    /// The HttpClientFactory is a factory class that enables sharing of the HttpClient instance.
    /// </summary>
    public static class HttpClientFactory
    {
        private static readonly HttpClient client;

        static HttpClientFactory()
        {
            // Not sure why this is necessary but the default
            client = new HttpClient
                     {
                         Timeout = Timeout.InfiniteTimeSpan
                     };
        }

        /// <summary>
        /// Gets the shared instance of HttpClient.
        /// </summary>
        /// <returns>Returns the shared instance of HttpClient. This instance should not be disposed.</returns>
        public static HttpClient GetHttpClient()
        {
            return client;
        }
    }
}