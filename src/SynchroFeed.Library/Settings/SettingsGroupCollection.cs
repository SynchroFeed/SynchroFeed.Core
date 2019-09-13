#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="SettingsGroupCollection.cs">
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
using System.Linq;

namespace SynchroFeed.Library.Settings
{
    /// <summary>
    /// The SettingsGroupCollection class is a collection of Settings associated with
    /// a Settings Group name. This is useful since the same settings can typically
    /// be applied to multiple feeds.
    /// </summary>
    /// <seealso cref="SettingsCollection" />
    public class SettingsGroupCollection : Dictionary<string, SettingsCollection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsGroupCollection"/> class.
        /// </summary>
        public SettingsGroupCollection()
            : base(StringComparer.CurrentCultureIgnoreCase)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsGroupCollection"/> class.
        /// </summary>
        /// <param name="capacity">The capacity to initialize the collection.</param>
        public SettingsGroupCollection(int capacity)
            : base(capacity, StringComparer.CurrentCultureIgnoreCase)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsGroupCollection"/> class.
        /// </summary>
        /// <param name="collection">The collection to initialize the collection.</param>
        public SettingsGroupCollection(SettingsGroupCollection collection)
            : base(collection, StringComparer.CurrentCultureIgnoreCase)
        {
        }

        /// <summary>
        /// Finds the specified settings for the given group name.
        /// </summary>
        /// <param name="groupName">Name of the settings group.</param>
        /// <returns>Library.Settings.SettingsCollection.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public SettingsCollection Find(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
                return new SettingsCollection();

            if (!this.TryGetValue(groupName, out var value))
                throw new InvalidOperationException($"SettingsGroup \"{groupName}\" not found in SettingsGroupCollection");

            return value;
        }
    }
}