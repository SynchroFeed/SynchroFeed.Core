#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="CommandCollection.cs">
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
    /// The CommandCollection class contains a collection of <see cref="Command"/>s.
    /// </summary>
    /// <seealso cref="Command" />
    public class CommandCollection : List<Command>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandCollection"/> class.
        /// </summary>
        public CommandCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandCollection"/> class.
        /// </summary>
        /// <param name="capacity">The capacity to initialize the collection.</param>
        public CommandCollection(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandCollection"/> class.
        /// </summary>
        /// <param name="collection">The collection to initialize the collection.</param>
        public CommandCollection(IEnumerable<Command> collection)
            : base(collection)
        {
        }
    }
}