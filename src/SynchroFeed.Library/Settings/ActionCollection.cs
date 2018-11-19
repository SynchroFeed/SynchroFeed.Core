#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="ActionCollection.cs">
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
    /// The ActionCollection class contains a collection of <see cref="Action"/>s.
    /// </summary>
    /// <seealso cref="Action" />
    public class ActionCollection : List<Action>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionCollection"/> class.
        /// </summary>
        public ActionCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionCollection"/> class.
        /// </summary>
        /// <param name="capacity">The capacity to initialize the collection.</param>
        public ActionCollection(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionCollection"/> class.
        /// </summary>
        /// <param name="collection">The collection to initialize the collection.</param>
        public ActionCollection(IEnumerable<Action> collection)
            : base(collection)
        {
        }
    }
}