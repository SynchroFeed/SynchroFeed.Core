#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="Observer.cs">
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
    /// The Observer class contains the name of an observer. Observers can be assigned to
    /// <see cref="Action"/>s to provide feedback on the result of the status of an action
    /// or validator.
    /// </summary>
    public class Observer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Observer"/> class.
        /// </summary>
        public Observer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Observer"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Observer(string name = "")
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the name of the observer.
        /// </summary>
        /// <value>The name of the observer.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the settings associated with this observer.
        /// </summary>
        /// <value>The settings associated with this observer.</value>
        public SettingsCollection Settings { get; protected set; } = new SettingsCollection();

        /// <summary>
        /// Gets or sets the settings group associated with this observer.
        /// </summary>
        /// <value>The settings group associated with this observer.</value>
        public string SettingsGroup { get; set; }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="Observer"/>.
        /// </summary>
        /// <param name="name">The name of the observer.</param>
        /// <returns>The Observer instance that is the result of the conversion.</returns>
        public static implicit operator Observer(string name)
        {
            return new Observer(name);
        }

        /// <summary>
        /// Deep clone of this instance.
        /// </summary>
        /// <returns>Returns a clone of this Observer</returns>
        public Observer Clone()
        {
            var newObserver = new Observer()
                              {
                                  Name = this.Name,
                                  SettingsGroup = this.SettingsGroup,
                                  Settings = new SettingsCollection(this.Settings),
                              };

            return newObserver;
        }

        /// <summary>Clones and merge the settings.</summary>
        /// <param name="settings">The settings to clone and merge with the instance settings.</param>
        /// <returns>Observer.</returns>
        public Observer CloneAndMergeSettings(SettingsCollection settings)
        {
            var newObserver = this.Clone();
            newObserver.Settings.Combine(settings);
            return newObserver;
        }
    }
}