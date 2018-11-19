#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="ActionObserverManager.cs">
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
using Microsoft.Extensions.Logging;
using SynchroFeed.Library.Factory;

namespace SynchroFeed.Library.Action.Observer
{
    /// <summary>
    /// The ObserverManager is a class that notifies observers of a particular event.
    /// </summary>
    public class ActionObserverManager : IActionObserverManager
    {
        private readonly object synclock = new object();

        private List<IActionObserver> observers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionObserverManager" /> class.
        /// </summary>
        /// <param name="action">The action associated with this observer manager.</param>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <exception cref="System.ArgumentNullException">
        /// loggerFactory
        /// or
        /// serviceProvider
        /// </exception>
        public ActionObserverManager(IAction action, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            Logger = loggerFactory.CreateLogger<ActionObserverManager>();
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <summary>
        /// Gets the action associated with this observer manager.
        /// </summary>
        /// <value>The action associated with this observer manager.</value>
        public IAction Action { get; }

        /// <summary>
        /// Gets the observers registered with this manager.
        /// </summary>
        /// <value>The observers registered with this manager.</value>
        public List<IActionObserver> Observers
        {
            get
            {
                if (observers == null)
                {
                    lock (synclock)
                    {
                        if (observers == null)
                        {
                            observers = new List<IActionObserver>(Action.ActionSettings.Observers.Count);
                            foreach (var observer in Action.ActionSettings.Observers)
                            {
                                var actionObserverFactory = ServiceProvider.GetNamedFactory<IActionObserverFactory>(observer.Name);
                                var observerInstance = actionObserverFactory.Create(observer);
                                observers.Add(observerInstance);
                            }
                        }
                    }
                }

                return observers;
            }
        }

        /// <summary>
        /// Gets the service provider used for dependency injection
        /// </summary>
        /// <value>The service provider.</value>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger Logger { get; }

        /// <summary>
        /// Notifies the observers of the specified event.
        /// </summary>
        /// <param name="actionEvent">The action event being raised.</param>
        /// <exception cref="System.ArgumentNullException">Raised when action is null</exception>
        /// <exception cref="System.ArgumentNullException">Raised when actionEvent is null.</exception>
        public void NotifyObservers(IActionEvent actionEvent)
        {
            if (actionEvent == null)
                throw new ArgumentNullException(nameof(actionEvent));

            foreach (var observer in Observers)
            {
                Logger.LogDebug($"Notifying Observer ({observer.ActionObserverName})");
                observer.Notify(Action, actionEvent);
            }
        }
    }
}