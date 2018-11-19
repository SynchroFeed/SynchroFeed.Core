#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="CommandResult.cs">
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

namespace SynchroFeed.Library.Command
{
    /// <summary>
    /// The CommandResult class contains the results of the execution of a command against a package.
    /// </summary>
    public class CommandResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandResult"/> class.
        /// </summary>
        public CommandResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandResult" /> class.
        /// </summary>
        /// <param name="command">The command associated with the result.</param>
        /// <param name="resultValid">if set to <c>true</c> the result is considered valid.</param>
        /// <param name="result">The string containing the command result.</param>
        public CommandResult(ICommand command = null, bool resultValid = true, string result = null)
        {
            ResultValid = resultValid;
            Command = command;
            Result = result;
        }

        /// <summary>
        /// Gets a value indicating whether the result is considered valid.
        /// </summary>
        /// <value><c>true</c> if the result is considered valid; otherwise, <c>false</c>.</value>
        public bool ResultValid { get; private set; }

        /// <summary>
        /// Gets or sets the command associated with the result.
        /// </summary>
        /// <value>The command associated with the result.</value>
        public ICommand Command { get; private set; }

        /// <summary>
        /// Gets or sets the result of the command execution.
        /// </summary>
        /// <value>The result of the command execution.</value>
        public string Result { get; private set; }
    }
}