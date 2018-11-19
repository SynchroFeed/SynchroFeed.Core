#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="SettingsTest.cs">
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
using AutoFixture;
using SynchroFeed.Library.Settings;
using Xunit;

namespace SynchroFeed.Library.Test.Settings
{
    public class SettingsTest
    {
        private Fixture Fixture { get; } = new Fixture();

        [Fact]
        public void Test_Settings_Collection()
        {
            var sut = new SettingsCollection { { "Key1", "Value1" }, { "Key2", "Value2" }, { "Key3", "Value3" } };
            var expectedSettings = new SettingsCollection { {"Key1", "Value1"}, {"Key3", "Value3"}, {"Key2", "Value2"} };

            Assert.True(sut.Count == 3);
            Assert.Equal(expectedSettings, sut);
        }


        [Fact]
        public void Test_Settings_Collection_Merge_SettingsGroup()
        {
            var sut = new SettingsCollection {{"Key1", "Value1"}, {"Key2", "Value2"}, {"Key3", "Value3"}};
            var settingsGroup = new SettingsCollection { { "Key2", "Value2-new" }, { "Key3", "Value3-new" }, { "Key4", "Value4-new" } };
            var expectedSettings = new SettingsCollection { { "Key1", "Value1" }, { "Key3", "Value3" }, { "Key2", "Value2" }, { "Key4", "Value4-new" } };

            sut.Combine(settingsGroup);

            Assert.True(sut.Count == 4);
            Assert.Equal(expectedSettings, sut);
        }

        [Fact]
        public void Test_SettingsCollection_Constructor_Int()
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            var sut = new SettingsCollection(20);

            Assert.True(sut.Count == 0);
        }

        [Fact]
        public void Test_SettingsCollection_Constructor_SettingsCollection()
        {
            var expectedSettings = new SettingsCollection();
            Fixture.AddManyTo(expectedSettings, 14);

            var sut = new SettingsCollection(expectedSettings);

            Assert.Equal(expectedSettings, sut);
        }

        [Fact]
        public void Test_SettingsCollection_Constructor_KeyValuePair()
        {
            var expectedSettings = new List<KeyValuePair<string, string>>();
            Fixture.AddManyTo(expectedSettings, 11);

            var sut = new SettingsCollection(expectedSettings);

            Assert.Equal(expectedSettings, sut);
        }

        [Fact]
        public void Test_SettingsGroupCollection_Constructor_SettingsGroupCollection()
        {
            var expectedSettings = new SettingsGroupCollection();
            Fixture.AddManyTo(expectedSettings, 23);

            var sut = new SettingsGroupCollection(expectedSettings);

            Assert.Equal(expectedSettings, sut);
        }

        [Fact]
        public void Test_SettingsGroupCollection_Constructor_Int()
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            var sut = new SettingsGroupCollection(20);

            Assert.True(sut.Count == 0);
        }

        [Fact]
        public void Test_SettingsGroupCollection_Find_KeyFound()
        {
            Fixture.RepeatCount = 100;
            var settingsGroups = new SettingsGroupCollection();
            Fixture.AddManyTo(settingsGroups, () =>
                              {
                                  var settings = Fixture.Create<SettingsCollection>();
                                  Fixture.AddManyTo(settings, 10);
                                  var kvp = new KeyValuePair<string, SettingsCollection>(Fixture.Create<string>(), settings);
                                  return kvp;
                              });
            var randomIndex = new Random().Next(0, 99);
            int index = 0;
            KeyValuePair<string, SettingsCollection> randomKvp = new KeyValuePair<string, SettingsCollection>();
            foreach (var kvp in settingsGroups)
            {
                if (index == randomIndex)
                {
                    randomKvp = kvp;
                    break;
                }

                index++;
            }

            var sut = settingsGroups.Find(randomKvp.Key);

            Assert.NotNull(randomKvp.Key);
            Assert.Equal(randomKvp.Value, sut);
        }

        [Fact]
        public void Test_SettingsGroupCollection_Find_KeyNotFound()
        {
            Fixture.RepeatCount = 100;
            var settingsGroups = new SettingsGroupCollection();
            Fixture.AddManyTo(settingsGroups, 100);

            Assert.Throws<InvalidOperationException>(() => settingsGroups.Find(Fixture.Create<string>()));
        }

        [Fact]
        public void Test_SettingsGroupCollection_Find_GroupNameNull()
        {
            Fixture.RepeatCount = 100;
            var settingsGroups = new SettingsGroupCollection();
            Fixture.AddManyTo(settingsGroups, 100);

            var sut = settingsGroups.Find(null);

            Assert.NotNull(sut);
            Assert.True(sut.Count == 0);
        }
    }
}
