#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="ActionEventType.cs">
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
    /// The ActionEventType class encapsulates an event type related to an action.
    /// </summary>
    public class ActionEventType
    {
        /// <summary>
        /// The event used to denote the Action started
        /// </summary>
        public static readonly ActionEventType ActionStarted = new ActionEventType(nameof(ActionStarted));

        /// <summary>
        /// The event used to denote the Action has completed
        /// </summary>
        public static readonly ActionEventType ActionCompleted = new ActionEventType(nameof(ActionCompleted));

        /// <summary>
        /// The event used to denote the package failed and the Action is being stopped from processing further packages
        /// </summary>
        public static readonly ActionEventType ActionFailed = new ActionEventType(nameof(ActionFailed));

        /// <summary>
        /// The event used to denote the Action Command result was successful
        /// </summary>
        public static readonly ActionEventType ActionCommandSuccess = new ActionEventType(nameof(ActionCommandSuccess));

        /// <summary>
        /// The event used to denote the Action Command result was a failure
        /// </summary>
        public static readonly ActionEventType ActionCommandFailed = new ActionEventType(nameof(ActionCommandFailed));

        /// <summary>
        /// The event used to denote the Action is ignoring the package
        /// </summary>
        public static readonly ActionEventType ActionPackageIgnored = new ActionEventType(nameof(ActionPackageIgnored));

        /// <summary>
        /// The event used to denote the package failed but the Action continues with the next package
        /// </summary>
        public static readonly ActionEventType ActionPackageFailed = new ActionEventType(nameof(ActionPackageFailed));

        /// <summary>
        /// The event used to denote the package was successfully processed by the commands
        /// </summary>
        public static readonly ActionEventType ActionPackageSuccess = new ActionEventType(nameof(ActionPackageSuccess));

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionEventType"/> class.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        public ActionEventType(string eventType)
        {
            EventType = eventType;
        }

        /// <summary>
        /// Gets the type of the event.
        /// </summary>
        /// <value>The type of the event.</value>
        public string EventType { get; }

        /// <summary>
        /// Returns a hash code for this EventType instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return EventType.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this EventType instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this EventType instance.</returns>
        public override string ToString()
        {
            return EventType;
        }
    }
}