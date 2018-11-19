#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="WebPostActionObserver.cs">
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
using System.Net.Http;
using System.Text;
using Microsoft.Extensions.Logging;
using SynchroFeed.Library;
using SynchroFeed.Library.Action;
using SynchroFeed.Library.Action.Observer;
using Settings=SynchroFeed.Library.Settings;

namespace SynchroFeed.ActionObserver.WebPost
{
    /// <summary>
    /// The WebPostActionObserver class is used to send a notification to web service
    /// when an event is send from an action.
    /// </summary>
    /// <seealso cref="SynchroFeed.Library.Action.Observer.IActionObserver" />
    public class WebPostActionObserver : IActionObserver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebPostActionObserver"/> class.
        /// </summary>
        /// <param name="observerSettings">The observer settings.</param>
        /// <param name="loggingFactory">The logging factory.</param>
        /// <exception cref="System.ArgumentNullException">
        /// loggingFactory
        /// or
        /// observerSettings
        /// </exception>
        /// <exception cref="System.InvalidOperationException">A URL to the room must be provided for the WebPostActionObserver.</exception>
        public WebPostActionObserver(Settings.Observer observerSettings, ILoggerFactory loggingFactory)
        {
            if (loggingFactory == null) throw new ArgumentNullException(nameof(loggingFactory));
            Logger = loggingFactory.CreateLogger<WebPostActionObserver>();
            ObserverSettings = observerSettings ?? throw new ArgumentNullException(nameof(observerSettings));

            if (string.IsNullOrEmpty(ObserverSettings.Settings.Url())) throw new InvalidOperationException("A URL to the room must be provided for the WebPostActionObserver.");
        }

        /// <summary>
        /// Gets the observer settings.
        /// </summary>
        /// <value>The observer settings.</value>
        public Settings.Observer ObserverSettings { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        private ILogger Logger { get; }

        /// <summary>
        /// Gets the name of the observer.
        /// </summary>
        /// <value>The name of the action observer.</value>
        public string ActionObserverName
        {
            get { return "WebPost"; }
        }

        /// <summary>
        /// Notifies the observer of the specified action.
        /// </summary>
        /// <param name="action">The action that originated this message.</param>
        /// <param name="actionEvent">The action event to notify the observer.</param>
        public void Notify(IAction action, IActionEvent actionEvent)
        {
            var messageTemplate = ObserverSettings.Settings.MessageTemplate(actionEvent.EventType);
            if (messageTemplate != null)
                SendWebPost(actionEvent, messageTemplate);
        }

        /// <summary>
        /// Sends the web post to the configured URL.
        /// </summary>
        /// <param name="actionEvent">The action event.</param>
        /// <param name="messageTemplate">The message template.</param>
        private void SendWebPost(IActionEvent actionEvent, string messageTemplate)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Post, ObserverSettings.Settings.Url()))
                {
                    var message = messageTemplate.FormatWith(actionEvent);
                    request.Content = new StringContent(message, Encoding.UTF8, ObserverSettings.Settings.ContentType());

                    using (var response = HttpClientFactory.GetHttpClient().SendAsync(request))
                    {
                        response.Wait();
                        response.Result.EnsureSuccessStatusCode();
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.LogError($"Error sending WebPost message. Exception: {ex.Message}, Ignoring.", ex);
            }
        }
    }
}
