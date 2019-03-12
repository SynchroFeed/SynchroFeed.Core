#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="ICommand.cs">
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
using SynchroFeed.Library.Model;

namespace SynchroFeed.Library.Command
{
    /// <summary>
    /// The interface for the Command pattern that allows creating classes that handle certain types of events.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Gets the settings associated with this Command
        /// </summary>
        /// <value>The settings associated with this Command.</value>
        Settings.Command Settings { get; }

        /// <summary>
        /// Gets the type of this command.
        /// </summary>
        /// <value>The name of this command.</value>
        string Type { get; }

        /// <summary>The method that executes the appropriate command to process the package.</summary>
        /// <param name="package">The package for the command to handle.</param>
        /// <param name="packageEvent">The event associated with the package.</param>
        /// <returns>Returns the CommandResult for the package.</returns>
        CommandResult Execute(Package package, PackageEvent packageEvent);
    }
}