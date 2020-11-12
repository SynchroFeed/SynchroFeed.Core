#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="DataBinder.cs">
// MIT License
// 
// Copyright(c) 2020 Robert Vandehey
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
using System.Text.RegularExpressions;

namespace SynchroFeed.Library
{
    /// <summary>
    /// The DataBinder class is a simple replacement to the System.Web.UI.DataBinder class that wasn't converted to .NET 5.
    /// </summary>
    public static class DataBinder
    {
        /// <summary>
        /// This method evaluates an expression and returns the value of that expression.
        /// </summary>
        /// <param name="container">The object that contains the values to bind to the expression.</param>
        /// <param name="expression">The expression is expected to be a simple PropertyName that represents a property from the container. This method also supports nested properties.</param>
        /// <returns>Returns the value of the expression</returns>
        /// <exception cref="ArgumentNullException">An ArgumentNullException is thrown when the property expression can't find the related property in the container.</exception>
        public static object Eval(object container, string expression)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            var r = new Regex(@"(?<property>[\w\[\]]+)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            var matches = r.Matches(expression);
            object value = container;
            for (var i = 0; i < matches.Count && value != null; i++)
            {
                var type = value.GetType();
                var propertyName = matches[i].Captures[0].Value;
                value = type.GetProperty(propertyName)?.GetValue(value, null);
            }

            if (value == null)
                throw new ArgumentNullException(expression);

            return value;
        }
    }
}