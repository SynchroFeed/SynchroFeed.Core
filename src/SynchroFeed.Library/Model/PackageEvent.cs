﻿#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="PackageEvent.cs">
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

namespace SynchroFeed.Library.Model
{
    /// <summary>
    /// The types of events associated with a package.
    /// </summary>
    public enum PackageEvent
    {
        /// <summary>The event for when a package was added.</summary>
        Added,
        /// <summary>The event for when a package was deleted.</summary>
        Deleted,
        /// <summary>The event for when a package was deployed.</summary>
        Deployed,
        /// <summary>The event for when a package was promoted.</summary>
        Promoted,
        /// <summary>The event for when a package was processed.</summary>
        Processed
    }
}