#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="IAction.cs">
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
using SynchroFeed.Library.Action.Observer;
using SynchroFeed.Library.Model;
using SynchroFeed.Library.Repository;
using SynchroFeed.Library.Settings;

namespace SynchroFeed.Library.Action
{
    /// <summary>
    /// The IAction interface is used to implement an Action.
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Gets the action settings.
        /// </summary>
        /// <value>The action settings.</value>
        Settings.Action ActionSettings { get; }

        /// <summary>
        /// Gets the type of this action.
        /// </summary>
        /// <value>The name of this action.</value>
        string ActionType { get; }

        /// <summary>
        /// Gets the application settings.
        /// </summary>
        /// <value>The application settings.</value>
        ApplicationSettings ApplicationSettings { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IAction"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        bool Enabled { get; }

        /// <summary>
        /// Gets the observer manager associated with this action.
        /// </summary>
        /// <value>The observer manager associated with this action.</value>
        IActionObserverManager ObserverManager { get; }

        /// <summary>
        /// Gets the service provider that is used to resolve dependencies
        /// </summary>
        /// <value>The service provider that is used to resolve dependencies.</value>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the source repository.
        /// </summary>
        /// <value>The source repository.</value>
        IRepository<Package> SourceRepository { get; }

        /// <summary>
        /// Runs the specified Action using the feed configuration section.
        /// </summary>
        void Run();

        /// <summary>
        /// The method in the action that processes the package.
        /// </summary>
        /// <param name="package">The package to process.</param>
        /// <param name="packageEvent">The type of event associated with the package.</param>
        /// <returns><c>true</c> if the process was successful and processing should continue, <c>false</c> otherwise.</returns>
        bool ProcessPackage(Package package, PackageEvent packageEvent);
    }
}