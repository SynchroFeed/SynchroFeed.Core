#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="UtilityExtensions.cs">
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
using System.Linq;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using SynchroFeed.Library.Exceptions;
using SynchroFeed.Library.Factory;
using SynchroFeed.Library.Settings;

namespace SynchroFeed.Library
{
    /// <summary>
    /// A static utility class that contains some useful extension methods.
    /// </summary>
    public static class UtilityExtensions
    {
        private static readonly Regex preReleaseRegex = new Regex(@"^\d+(?:\.\d+){1,3}$", RegexOptions.Compiled);

        /// <summary>
        /// Combines a URI to another path in a way that doesn't matter if the URL ends with a backslash
        /// </summary>
        /// <param name="uri">The URI with the base address.</param>
        /// <param name="path">The path to add to the base address.</param>
        /// <returns>Returns a Uri that combines the Uri and the path</returns>
        public static Uri Combine(this Uri uri, string path)
        {
            if (uri == null || path == null)
                return null;

            return new Uri(new Uri(uri.AbsoluteUri + (uri.AbsolutePath.EndsWith("/") ? string.Empty : "/")), path);
        }

        /// <summary>
        /// Converts the input to the necessary type.
        /// </summary>
        /// <typeparam name="T">The type to convert the input to.</typeparam>
        /// <param name="input">The input to convert.</param>
        /// <param name="throwException">if set to <c>true</c> an exception will be thrown if the conversion isn't possible.</param>
        /// <returns>Returns the input in the converted type if possible.</returns>
        public static T Convert<T>(this string input, bool throwException = false)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                return (T)converter.ConvertFromString(input);
            }
            catch (Exception ex)
            {
                if (!throwException)
                    return default(T);

                if (ex is FormatException)
                    throw;
                if (ex.InnerException is FormatException)
                    throw ex.InnerException;
                throw;
            }
        }

        // Define other methods and classes here
        /// <summary>Formats the with.</summary>
        /// <param name="format">The string to format. The string contains formatting tokens in the format of {&lt;FieldName&gt;:&lt;format specification&gt;}</param>
        /// <param name="source">The source object to bind to the tokens in the format string.</param>
        /// <param name="throwException">if set to <c>true, throw an </c>exception on a formatting error.</param>
        /// <returns>System.String.</returns>
        public static string FormatWith(this string format, object source, bool throwException=false)
        {
            return FormatWith(format, null, source, throwException);
        }

        // Define other methods and classes here
        /// <summary>Formats the with.</summary>
        /// <param name="format">The string to format. The string contains formatting tokens in the format of {&lt;FieldName&gt;:&lt;format specification&gt;}</param>
        /// <param name="provider">An instance of the IFormatProvider to use to interpret the format string.</param>
        /// <param name="source">The source object to bind to the tokens in the format string.</param>
        /// <param name="throwException">if set to <c>true, throw an </c>exception on a formatting error.</param>
        /// <returns>System.String.</returns>
        public static string FormatWith(this string format, IFormatProvider provider, object source, bool throwException = false)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            Regex r = new Regex(@"\{+(?<property>[\w\.\[\]]+)(?<format>:[^}]+)?\}+", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            string formattedString = r.Replace(
                                               format,
                                               delegate (Match m)
                                               {
                                                   Group propertyGroup = m.Groups["property"];
                                                   Group formatGroup = m.Groups["format"];

                                                   try
                                                   {
                                                       // ReSharper disable FormatStringProblem
                                                       return string.Format(provider, "{0" + formatGroup.Value + "}",
                                                                            DataBinder.Eval(source, propertyGroup.Value));
                                                   }
                                                   catch (Exception ex)
                                                   {
                                                       if (throwException)
                                                           throw new ParsingException($"Unable to parse {propertyGroup.Value}", ex);

                                                       return "{" + propertyGroup.Value + "}";
                                                   }
                                               });

            return formattedString;
        }

        /// <summary>Gets the custom setting from the settings collection with the settingName. If the setting is not found, return the defaultValue.</summary>
        /// <typeparam name="T">The type of the setting.</typeparam>
        /// <param name="settings">The settings collection to search for a setting with the settingName.</param>
        /// <param name="settingName">Name of the setting to search for in the settings collection.</param>
        /// <param name="defaultValue">The default value to use if the settingName is not found in the settings collection.</param>
        /// <returns>T.</returns>
        public static T GetCustomSetting<T>(this SettingsCollection settings, string settingName, T defaultValue = default(T))
        {
            if (settings.ContainsKey(settingName))
            {
                return settings[settingName].Convert<T>();
            }

            return defaultValue;
        }

        /// <summary>A helper extension method to find a factory class with the specified name.</summary>
        /// <typeparam name="T">The type of factory to return.</typeparam>
        /// <param name="serviceProvider">The service provider instance to search for a provider with the given name.</param>
        /// <param name="name">The name of the factory to search for in the service provider.</param>
        public static T GetNamedFactory<T>(this IServiceProvider serviceProvider, string name)
            where T : INamedFactory
        {
            var factory = serviceProvider.GetServices<T>().FirstOrDefault(f => f.Type.Equals(name, StringComparison.CurrentCultureIgnoreCase));

            if (factory == null)
            {
                throw new InvalidOperationException($"Unable to find factory class of type {typeof(T)} with the type \"{name}\".");
            }

            return factory;
        }

        /// <summary>
        /// Determines whether the specified version is a prerelease version.
        /// </summary>
        /// <param name="version">The version number to test.</param>
        /// <returns><c>true</c> if the specified version is prerelease version number; otherwise, <c>false</c>.</returns>
        public static bool IsPrerelease(this string version)
        {
            return !preReleaseRegex.IsMatch(version);
        }
    }
}