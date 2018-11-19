#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="FeedCollection.cs">
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
using System.Collections.Generic;

namespace SynchroFeed.Library.Settings
{
    /// <summary>
    /// The FeedCollection class contains a collection of feeds.
    /// </summary>
    /// <seealso cref="Feed"/>
    public class FeedCollection : List<Feed>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedCollection"/> class.
        /// </summary>
        public FeedCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedCollection"/> class.
        /// </summary>
        /// <param name="capacity">The capacity to initialize the collection.</param>
        public FeedCollection(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedCollection"/> class.
        /// </summary>
        /// <param name="collection">The collection to initialize the collection.</param>
        public FeedCollection(IEnumerable<Feed> collection)
            : base(collection)
        {
        }
    }
}