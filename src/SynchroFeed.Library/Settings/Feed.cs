#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="Feed.cs">
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

namespace SynchroFeed.Library.Settings
{
    /// <summary>
    /// The Feed class contains the configuration related to a feed like Nuget, Proget, Chocolatey or a directory.
    /// The feed configuration is used to create a repository to access the feed.
    /// </summary>
    public class Feed
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Feed" /> class.
        /// </summary>
        public Feed()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Feed"/> class.
        /// </summary>
        /// <param name="name">The name of the feed.</param>
        /// <param name="type">The type of feed.</param>
        /// <param name="settingsGroup">The settings group associated with this feed.</param>
        public Feed(string name = "", string type = "", string settingsGroup = "")
        {
            Name = name;
            Type = type;
            SettingsGroup = settingsGroup;
        }

        /// <summary>
        /// Gets or sets the name of the feed.
        /// </summary>
        /// <value>The name of the feed.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the settings associated with this feed.
        /// </summary>
        /// <value>The settings associated with this feed.</value>
        public SettingsCollection Settings { get; protected set; } = new SettingsCollection();

        /// <summary>
        /// Gets or sets the name of the settings group associated with this feed.
        /// </summary>
        /// <value>The settings group name.</value>
        public string SettingsGroup { get; set; }

        /// <summary>
        /// Gets or sets the type of the feed.
        /// </summary>
        /// <value>The type of the feed.</value>
        public string Type { get; set; }

        /// <summary>
        /// Deep clone of this instance.
        /// </summary>
        /// <returns>Returns a clone of this Action</returns>
        public Feed Clone()
        {
            var newFeed = new Feed()
                          {
                              Name = this.Name,
                              Type = this.Type,
                              SettingsGroup = this.SettingsGroup,
                              Settings = new SettingsCollection(this.Settings),
                          };

            return newFeed;
        }

        /// <summary>Clones and merge the settings with the settings in this instance.</summary>
        /// <param name="settings">The settings to clone and merge with the instance.</param>
        /// <returns>Feed.</returns>
        public Feed CloneAndMergeSettings(SettingsCollection settings)
        {
            var newFeed = this.Clone();
            newFeed.Settings.Combine(settings);
            return newFeed;
        }
    }
}