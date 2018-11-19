#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="Command.cs">
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
using System.Linq;
using SynchroFeed.Library.Command;

namespace SynchroFeed.Library.Settings
{
    /// <summary>
    /// The Command class contains the configuration related to an command. A Command
    /// runs against package.
    /// </summary>
    public class Command
    {
        /// <summary>
        /// Gets the action when the command fails.
        /// </summary>
        /// <value>The failure action.</value>
        public CommandFailureAction FailureAction { get; set; }

        /// <summary>
        /// Gets the settings associated with this command.
        /// </summary>
        /// <value>The settings associated with this command.</value>
        public SettingsCollection Settings { get; protected set; } = new SettingsCollection();

        /// <summary>
        /// Gets or sets the settings group associated with this command.
        /// </summary>
        /// <value>The settings group associated with this command.</value>
        public string SettingsGroup { get; set; }

        /// <summary>
        /// Gets or sets the type of the command.
        /// </summary>
        /// <value>The type of the command.</value>
        public string Type { get; set; }

        /// <summary>
        /// Deep clone of this instance.
        /// </summary>
        /// <returns>Returns a clone of this Command</returns>
        public Command Clone()
        {
            var newCommand = new Command()
                             {
                                 Type = this.Type,
                                 SettingsGroup = this.SettingsGroup,
                                 Settings = new SettingsCollection(this.Settings),
                                 FailureAction = this.FailureAction
                             };

            return newCommand;
        }

        /// <summary>
        /// Clones this instance of the Command and merges the settings parameter
        /// with the instances settings.
        /// </summary>
        /// <param name="settings">The settings to merge with the instance settings.</param>
        /// <returns>Returns a new instance of the Command with the settings merged.</returns>
        public Command CloneAndMergeSettings(SettingsCollection settings)
        {
            var newCommand = this.Clone();
            newCommand.Settings.Combine(settings);
            return newCommand;
        }
    }
}