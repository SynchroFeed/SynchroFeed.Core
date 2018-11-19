#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="ApplicationSettingsTest.cs">
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
using AutoFixture;
using SynchroFeed.Library.Settings;
using Xunit;

namespace SynchroFeed.Library.Test.Settings
{
    public class ApplicationSettingsTest
    {
        [Fact]
        public void Test_Application_Settings_Constructor()
        {
            var fixture = new Fixture();
            var sut = new ApplicationSettings();
            fixture.AddManyTo(sut.Actions, 15);
            fixture.AddManyTo(sut.Feeds, 20);
            fixture.AddManyTo(sut.SettingsGroups, 5);

            Assert.True(sut.Actions.Count == 15);
            Assert.True(sut.Feeds.Count == 20);
            Assert.True(sut.SettingsGroups.Count == 5);
        }
    }
}
