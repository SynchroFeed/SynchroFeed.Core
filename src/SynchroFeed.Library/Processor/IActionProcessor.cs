#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="IActionProcessor.cs">
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
using Microsoft.Extensions.DependencyInjection;

namespace SynchroFeed.Library.Processor
{
    /// <summary>
    /// The IActionProcessor is an interface for processing actions.
    /// </summary>
    /// <remarks>
    /// The Execute method executes the configured actions.
    /// </remarks>
    public interface IActionProcessor
    {
        /// <summary>
        /// The Execute method is the main entry point of processing actions.
        /// </summary>
        /// <param name="actions">A list of actions within the config file to run. Run all enabled actions if the parameter is empty.</param>
        void Execute(List<string> actions = null);

        /// <summary>
        /// Creates an action given the ServiceScope and the action settings.
        /// </summary>
        /// <param name="scope">The service scope.</param>
        /// <param name="actionSettings">The action settings to create the action from.</param>
        /// <returns>Action.IAction.</returns>
        Action.IAction CreateAction(IServiceScope scope, Settings.Action actionSettings);
    }
}