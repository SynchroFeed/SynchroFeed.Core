#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="Action.cs">
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
    /// The Action class contains the configuration related to an action. An Action
    /// runs against a source and optionally a target feed.
    /// </summary>
    public class Action
    {
        /// <summary>
        /// Gets the commands associated with this action.
        /// </summary>
        /// <value>The validators associated with this action.</value>
        public CommandCollection Commands { get; protected set; } = new CommandCollection();

        /// <summary>
        /// Gets or sets the name of the action.
        /// </summary>
        /// <value>The name of the action.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the action.
        /// </summary>
        /// <value>The type of the action.</value>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the source feed related to the action.
        /// </summary>
        /// <value>The source feed associatged with the action.</value>
        public string SourceFeed { get; set; }

        /// <summary>
        /// Gets or sets the target feed related to the action.
        /// </summary>
        /// <value>The target feed associatged with the action.</value>
        public string TargetFeed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to fail the action when an error is encountered.
        /// </summary>
        /// <value><c>true</c> if the action should fail when an error is encountered; otherwise, <c>false</c>.</value>
        public bool FailOnError { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to only retrieve latest version.
        /// </summary>
        /// <value><c>true</c> if only the latest version should be retrieved; otherwise, <c>false</c>.</value>
        public bool OnlyLatestVersion { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Action"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to prerelease versions.
        /// </summary>
        /// <value><c>true</c> if prerelease versions should be retrieved; otherwise, <c>false</c>.</value>
        public bool IncludePrerelease { get; set; }

        /// <summary>
        /// Gets the observers associated with this action.
        /// </summary>
        /// <value>The observers associated with this action.</value>
        public ObserverCollection Observers { get; protected set; } = new ObserverCollection();

        /// <summary>
        /// Gets or sets the package IDs to ignore.
        /// </summary>
        /// <value>The package IDs to ignore.</value>
        public List<string> PackagesToIgnore { get; set; } = new List<string>();

        /// <summary>
        /// Gets the settings associated with this action.
        /// </summary>
        /// <value>The settings associated with this action.</value>
        public SettingsCollection Settings { get; protected set; } = new SettingsCollection();

        /// <summary>
        /// Gets or sets the settings group associated with this action.
        /// </summary>
        /// <value>The settings group associated with this action.</value>
        public string SettingsGroup { get; set; }

        /// <summary>
        /// Deep clone of this instance.
        /// </summary>
        /// <returns>Returns a clone of this Action</returns>
        public Action Clone()
        {
            var newAction = new Action()
                            {
                                Name = this.Name,
                                Type = this.Type,
                                SourceFeed = this.SourceFeed,
                                TargetFeed = this.TargetFeed,
                                OnlyLatestVersion = this.OnlyLatestVersion,
                                IncludePrerelease = this.IncludePrerelease,
                                FailOnError = this.FailOnError,
                                PackagesToIgnore = new List<string>(PackagesToIgnore),
                                SettingsGroup = this.SettingsGroup,
                                Enabled = this.Enabled,
                                Settings = new SettingsCollection(this.Settings),
                                Commands = new CommandCollection(this.Commands),
                                Observers = new ObserverCollection(this.Observers)
                            };

            return newAction;
        }

        public Action CloneAndMergeSettings(SettingsCollection settings)
        {
            var newAction = this.Clone();
            newAction.Settings.Combine(settings);
            return newAction;
        }
    }
}