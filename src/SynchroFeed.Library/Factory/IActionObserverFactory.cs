#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="IActionObserverFactory.cs">
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
using SynchroFeed.Library.Action.Observer;
using SynchroFeed.Library.Settings;

namespace SynchroFeed.Library.Factory
{
    /// <summary>
    /// The IActionObserverFactory interface is a factory interface
    /// for creating instances of an <see cref="IActionObserver"/>,
    /// </summary>
    /// <seealso cref="SynchroFeed.Library.Factory.INamedFactory" />
    public interface IActionObserverFactory : INamedFactory
    {
        /// <summary>
        /// A factory method that creates the specified action observer.
        /// </summary>
        /// <param name="observer">The settings associated with this action observer.</param>
        /// <returns>Returns an instance of IActionObserver for the specified action type.</returns>
        IActionObserver Create(Observer observer);
    }
}