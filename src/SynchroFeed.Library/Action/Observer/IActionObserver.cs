#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="IActionObserver.cs">
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

namespace SynchroFeed.Library.Action.Observer
{
    /// <summary>
    /// The IActionObserver interface is used to implement the observer pattern
    /// that can be used to send emails, create issues, instant message, etc.
    /// </summary>
    public interface IActionObserver
    {
        /// <summary>
        /// Gets the name of the observer.
        /// </summary>
        /// <value>The name of the action observer.</value>
        string ActionObserverName { get; }

        /// <summary>
        /// Notifies the observer of the specified action.
        /// </summary>
        /// <param name="action">The action that originated this message.</param>
        /// <param name="actionEvent">The action event to notify the observer.</param>
        void Notify(IAction action, IActionEvent actionEvent);
    }
}