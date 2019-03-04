#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="ApplicationSettings.cs">
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

// ReSharper disable CollectionNeverQueried.Global
// ReSharper disable UnassignedGetOnlyAutoProperty
namespace SynchroFeed.Library.Settings
{
    /// <summary>
    /// The FeedSettings class contains all of the configuration for the feeds,
    /// actions and settings groups.
    /// </summary>
    public class ApplicationSettings
    {
        /// <summary>
        /// Gets a collection that contains all of the configured actions.
        /// </summary>
        /// <value>The actions that are configured.</value>

        // ReSharper disable once CollectionNeverUpdated.Global
        public ActionCollection Actions { get; } = new ActionCollection();

        /// <summary>
        /// Gets a collection that contains all of the configured feeds.
        /// </summary>
        /// <value>The feeds that are configured.</value>

        // ReSharper disable once CollectionNeverUpdated.Global
        public FeedCollection Feeds { get; } = new FeedCollection();

        /// <summary>
        /// Gets a collection of the settings groups which contained
        /// a collection of settings associated with a name.
        /// </summary>
        /// <value>The settings groups that have been configured.</value>
        public SettingsGroupCollection SettingsGroups { get; } = new SettingsGroupCollection();
    }
}