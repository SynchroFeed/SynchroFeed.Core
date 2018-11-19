#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="IActionEvent.cs">
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
using SynchroFeed.Library.Command;
using SynchroFeed.Library.Model;

namespace SynchroFeed.Library.Action.Observer
{
    /// <summary>
    /// The IActionEvent interface is the event message sent to an observer
    /// in the Notify method to inform them of an event.
    /// </summary>
    public interface IActionEvent
    {
        /// <summary>
        /// Gets the action associated with this event.
        /// </summary>
        /// <value>The action associated with this event.</value>
        IAction Action { get; }

        /// <summary>
        /// Gets the Command associated with this event.
        /// </summary>
        /// <value>The Command associated with this event.</value>
        ICommand Command { get; }

        /// <summary>
        /// Gets the unique identifier for this event.
        /// </summary>
        /// <value>The unique event identifier.</value>
        Guid EventId { get; }

        /// <summary>
        /// Gets the type of event that was generated.
        /// </summary>
        /// <value>The type of the event.</value>
        ActionEventType EventType { get; }

        /// <summary>
        /// Gets the message associated with the event.
        /// </summary>
        /// <value>The message.</value>
        string Message { get; }

        /// <summary>
        /// Gets the package associated with this event.
        /// </summary>
        /// <value>The package associated with this event.</value>
        Package Package { get; }
    }
}