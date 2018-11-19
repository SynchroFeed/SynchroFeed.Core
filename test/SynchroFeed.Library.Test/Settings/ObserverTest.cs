#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="ObserverTest.cs">
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
    public class ObserverTest
    {
        private Fixture Fixture { get; } = new Fixture();

        [Fact]
        public void Test_Observer_Properties()
        {
            var action = Fixture.Create<Observer>();
            var sut = new Observer();

            sut.Name = action.Name;
            sut.SettingsGroup = action.SettingsGroup;

            Assert.True(action.Name == sut.Name);
            Assert.True(action.SettingsGroup == sut.SettingsGroup);
        }

        [Fact]
        public void Test_Observer_Constructor()
        {
            var sut = new Observer("ObserverName");

            Assert.Equal("ObserverName", sut.Name);
        }

        [Fact]
        public void Test_Observer_Settings_Collection()
        {
            var sut = Fixture.Create<Observer>();

            Fixture.AddManyTo(sut.Settings, 8);

            Assert.True(sut.Settings.Count == 8);
        }

        [Fact]
        public void Test_Observer_Clone()
        {
            var observer = Fixture.Create<Observer>();
            Fixture.AddManyTo(observer.Settings, 15);

            var sut = observer.Clone();

            Assert.Equal(observer.SettingsGroup, sut.SettingsGroup);
            Assert.Equal(observer.Name, sut.Name);
            Assert.Equal(observer.Settings, sut.Settings);
        }

        [Fact]
        public void Test_Action_CloneAndMergeSettings()
        {
            var observer = Fixture.Create<Observer>();
            Fixture.AddManyTo(observer.Settings, 15);

            var settingsGroup = Fixture.Create<SettingsCollection>();
            Fixture.AddManyTo(settingsGroup, 18);

            var sut = observer.CloneAndMergeSettings(settingsGroup);

            Assert.Equal(observer.SettingsGroup, sut.SettingsGroup);
            Assert.Equal(observer.Name, sut.Name);
            Assert.NotEqual(observer.Settings, sut.Settings);
        }

        [Fact]
        public void Test_Implicit_Conversion_From_String_To_Observer()
        {
            Observer observer = "TestStringObserver";

            Assert.Equal("TestStringObserver", observer.Name);
        }

        [Fact]
        public void Test_Observer_Collection_Constructor_Int()
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            var sut = new ObserverCollection(20);
            Assert.True(sut.Count == 0);
        }
    }
}
