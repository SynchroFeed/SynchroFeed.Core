#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="ActionEvent.cs">
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
    /// The ActionEvent class is an implementation of the <see cref="IActionEvent"/> interface.
    /// It contains the information related to an action event.
    /// </summary>
    /// <seealso cref="SynchroFeed.Library.Action.Observer.IActionEvent" />
    public class ActionEvent : IActionEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionEvent"/> class.
        /// </summary>
        public ActionEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionEvent" /> class.
        /// </summary>
        /// <param name="action">The action associated with this event.</param>
        /// <param name="eventType">Type of event that was generated.</param>
        /// <param name="message">The message associated with the event.</param>
        /// <param name="command">The command associated with this event or null if not available.</param>
        /// <param name="package">The package associated with this event or null if not available.</param>
        public ActionEvent(IAction action, ActionEventType eventType, string message, ICommand command = null, Package package = null)
        {
            this.Action = action;
            this.EventType = eventType;
            this.Message = message;
            this.Command = command;
            this.Package = package;
        }

        /// <summary>
        /// Gets the action associated with this event.
        /// </summary>
        /// <value>The action associated with this event.</value>
        public IAction Action { get; }

        /// <summary>
        /// Gets the Command associated with this event.
        /// </summary>
        /// <value>The Command associated with this event.</value>
        public ICommand Command { get; }

        /// <summary>
        /// Gets the unique identifier for this event.
        /// </summary>
        /// <value>The unique event identifier.</value>
        public Guid EventId { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets the type of event that was generated.
        /// </summary>
        /// <value>The type of the event.</value>
        public ActionEventType EventType { get; }

        /// <summary>
        /// Gets the message associated with the event.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; }

        /// <summary>
        /// Gets the package associated with this event.
        /// </summary>
        /// <value>The package associated with this event.</value>
        public Package Package { get; }
    }
}