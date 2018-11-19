#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="FeedTest.cs">
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
using SynchroFeed.Library.Settings;
using Xunit;

namespace SynchroFeed.Library.Test.Settings
{
    public class FeedTest
    {
        [Fact]
        public void Test_Feed_Constructor_With_Params()
        {
            var sut = new Feed("TestName", "TestType", "TestSettingsGroup");

            Assert.Equal("TestName", sut.Name);
            Assert.Equal("TestType", sut.Type);
            Assert.Equal("TestSettingsGroup", sut.SettingsGroup);
            Assert.True(sut.Settings.Count == 0);
        }

        [Fact]
        public void Test_Feed_Initialize_Properties()
        {
            var sut = new Feed
            {
                Name = "TestName",
                Type = "TestType",
                SettingsGroup = "TestSettingsGroup"
            };
            Assert.Equal("TestName", sut.Name);
            Assert.Equal("TestType", sut.Type);
            Assert.Equal("TestSettingsGroup", sut.SettingsGroup);
            Assert.True(sut.Settings.Count == 0);
        }

        [Fact]
        public void Test_Feed_Settings_Collection()
        {
            var sut = new Feed
            {
                Name = "TestName",
                Type = "TestType",
                SettingsGroup = "TestSettingsGroup",
                Settings = { {"Key1", "Value1"}, {"Key2", "Value2"}, {"Key3", "Value3"} }
            };

            Assert.True(sut.Settings.Count == 3);
            var expectedSettings = new SettingsCollection { {"Key1", "Value1"}, {"Key3", "Value3"}, {"Key2", "Value2"} };
           Assert.Equal(expectedSettings, sut.Settings );
        }


        [Fact]
        public void Test_Feed_Settings_Collection_Merge_SettingsGroup()
        {
            var sut = new Feed
            {
                Name = "TestName",
                Type = "TestType",
                SettingsGroup = "TestSettingsGroup",
                Settings = { { "Key1", "Value1" }, { "Key2", "Value2" }, { "Key3", "Value3" } }
            };

            var settingsGroup = new SettingsCollection { { "Key2", "Value2-new" }, { "Key3", "Value3-new" }, { "Key4", "Value4-new" } };
            sut.Settings.Combine(settingsGroup);

            var expectedSettings = new SettingsCollection { { "Key1", "Value1" }, { "Key3", "Value3" }, { "Key2", "Value2" }, { "Key4", "Value4-new" } };
            Assert.True(sut.Settings.Count == 4);
           Assert.Equal(expectedSettings, sut.Settings);
        }

        [Fact]
        public void Test_Feed_Clone()
        {
            var feed = new Feed
            {
                Name = "TestName",
                Type = "TestType",
                SettingsGroup = "TestSettingsGroup",
                Settings = { { "Key1", "Value1" }, { "Key2", "Value2" }, { "Key3", "Value3" } }
            };

            var sut = feed.Clone();

            Assert.Equal(feed.Name, sut.Name);
            Assert.Equal(feed.Type, sut.Type);
            Assert.Equal(feed.SettingsGroup, sut.SettingsGroup);
           Assert.Equal(feed.Settings, sut.Settings);
        }

        [Fact]
        public void Test_Feed_CloneAndMergeSettings()
        {
            var feed = new Feed
            {
                Name = "TestName",
                Type = "TestType",
                SettingsGroup = "TestSettingsGroup",
                Settings = { { "Key1", "Value1" }, { "Key2", "Value2" }, { "Key3", "Value3" } }
            };
            var expectedSettings = new SettingsCollection { { "Key1", "Value1" }, { "Key3", "Value3" }, { "Key2", "Value2" }, { "Key4", "Value4-new" } };
            var settingsGroup = new SettingsCollection { { "Key2", "Value2-new" }, { "Key3", "Value3-new" }, { "Key4", "Value4-new" } };

            var sut = feed.CloneAndMergeSettings(settingsGroup);

            Assert.Equal(feed.Name, sut.Name);
            Assert.Equal(feed.Type, sut.Type);
            Assert.Equal(feed.SettingsGroup, sut.SettingsGroup);
           Assert.Equal(expectedSettings, sut.Settings);
        }

        [Fact]
        public void Test_FeedCollection_Constructor_FeedIEnumerable()
        {
            var expectedFeedCollection = new List<Feed>
            {
                new Feed("TestName1", "TestType1", "TestSettingsGroup1"),
                new Feed("TestName2", "TestType2", "TestSettingsGroup2"),
                new Feed("TestName3", "TestType3", "TestSettingsGroup3"),
                new Feed("TestName4", "TestType4", "TestSettingsGroup4")
            };
            var newFeed = new Feed("TestName5", "TestType5", "TestSettingsGroup5");

            var feedCollection = new FeedCollection(expectedFeedCollection);

            Assert.True(feedCollection.Count == 4);
            foreach (var feed in expectedFeedCollection)
            {
                Assert.Contains(feed, feedCollection);
            }
            Assert.DoesNotContain(newFeed, feedCollection);
        }

        [Fact]
        public void Test_FeedCollection_Constructor_Feed_Int()
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            var sut = new FeedCollection(20);
            Assert.True(sut.Count == 0);
        }

        [Fact]
        public void Test_FeedCollection_Constructor_Feed()
        {
            var sut = new FeedCollection();
            var expectedFeedCollection = new List<Feed>
            {
                new Feed("TestName1", "TestType1", "TestSettingsGroup1"),
                new Feed("TestName2", "TestType2", "TestSettingsGroup2"),
                new Feed("TestName3", "TestType3", "TestSettingsGroup3"),
                new Feed("TestName4", "TestType4", "TestSettingsGroup4")
            };
            var newFeed = new Feed("TestName5", "TestType5", "TestSettingsGroup5");

            foreach (var feed in expectedFeedCollection)
            {
                sut.Add(feed);
            }
            sut.Add(newFeed);
            sut.Remove(newFeed);

            Assert.True(sut.Count == 4);
            foreach (var feed in expectedFeedCollection)
            {
                Assert.Contains(feed, sut);
            }
            Assert.DoesNotContain(newFeed, sut);
        }
    }
}
