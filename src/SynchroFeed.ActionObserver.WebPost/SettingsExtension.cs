#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="SettingsExtension.cs">
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
using SynchroFeed.Library;
using SynchroFeed.Library.Action.Observer;
using Settings = SynchroFeed.Library.Settings;

namespace SynchroFeed.ActionObserver.WebPost
{
    /// <summary>
    /// The SettingsExtension class is an extension helper class for getting settings from the settings dictionary.
    /// </summary>
    internal static class SettingsExtension
    {
        /// <summary>
        /// An extension method that gets the URL setting from the settings collection.
        /// </summary>
        /// <param name="settings">The settings collection.</param>
        /// <returns>System.String.</returns>
        public static string Url(this Settings.SettingsCollection settings)
        {
            return settings.GetCustomSetting<string>("Url");
        }

        /// <summary>
        /// An extension method that gets the ContentType setting from the settings collection.
        /// </summary>
        /// <param name="settings">The settings collection.</param>
        /// <returns>System.String.</returns>
        public static string ContentType(this Settings.SettingsCollection settings)
        {
            return settings.GetCustomSetting("ContentType", "application/json");
        }

        /// <summary>
        /// An extension method that gets the MessageTemplate setting for the specified event type from the settings collection.
        /// </summary>
        /// <param name="settings">The settings collection.</param>
        /// <param name="eventType">Type of the event.</param>
        /// <returns>Returns a System.String representing the template or null if the template doesn't exist.</returns>
        public static string MessageTemplate(this Settings.SettingsCollection settings, ActionEventType eventType)
        {
            return settings.GetCustomSetting<string>($"MessageTemplate-{eventType.EventType}");
        }
    }
}