#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="SettingsCollection.cs">
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
    /// THe SettingsCollection class contains a collection of name/value pairs.
    /// </summary>
    public class SettingsCollection : Dictionary<string, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsCollection"/> class.
        /// </summary>
        public SettingsCollection()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsCollection"/> class.
        /// </summary>
        /// <param name="capacity">The capacity to initilize the collection.</param>
        public SettingsCollection(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsCollection"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public SettingsCollection(SettingsCollection settings)
            : base(settings)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsCollection"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public SettingsCollection(IEnumerable<KeyValuePair<string, string>> settings)
        {
            foreach (var setting in settings)
            {
                this.Add(setting.Key, setting.Value);
            }
        }

        /// <summary>
        /// Combines the unique values from the settings parameter to this collection.
        /// </summary>
        /// <param name="settings">The settings to combine with this instance.</param>
        /// <remarks>
        /// Only items with a unique key in the settings parameter are combined with the base settings.
        /// </remarks>
        public void Combine(SettingsCollection settings)
        {
            foreach (var kvp in settings.Where(kvp => !this.ContainsKey(kvp.Key)))
            {
                this.Add(kvp.Key, kvp.Value);
            }
        }
    }
}