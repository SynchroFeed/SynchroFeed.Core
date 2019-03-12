#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="BaseCommand.cs">
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
using Microsoft.Extensions.Logging;
using SynchroFeed.Library.Action;
using SynchroFeed.Library.Model;

namespace SynchroFeed.Library.Command
{
    public abstract class BaseCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCommand" /> class.
        /// </summary>
        /// <param name="action">The action that runs this Command.</param>
        /// <param name="commandSettings">The command associated with this Command.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <exception cref="ArgumentNullException">action</exception>
        protected BaseCommand(IAction action, Settings.Command commandSettings, ILoggerFactory loggerFactory)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Settings = commandSettings ?? throw new ArgumentNullException(nameof(commandSettings));
            Logger = loggerFactory.CreateLogger<BaseCommand>();
        }

        /// <summary>
        /// Gets the action associated with this command.
        /// </summary>
        /// <value>The action associated with this command.</value>
        public IAction Action { get; }

        /// <summary>
        /// Gets the settings associated with this Command
        /// </summary>
        /// <value>The settings associated with this Command.</value>
        public Settings.Command Settings { get; }

        /// <summary>
        /// Gets the action type of this action.
        /// </summary>
        /// <value>The action type of this action.</value>
        public abstract string Type { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private ILogger Logger { get; }

        /// <summary>The method that executes the appropriate command to process the package.</summary>
        /// <param name="package">The package for the command to handle.</param>
        /// <param name="packageEvent">The event associated with the package.</param>
        /// <returns>Returns the CommandResult for the package.</returns>
        public abstract CommandResult Execute(Package package, PackageEvent packageEvent);
    }
}