#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="ActionTest.cs">
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
using AutoFixture;
using SynchroFeed.Library.Settings;
using Xunit;

namespace SynchroFeed.Library.Test.Settings
{
    public class ActionTest
    {
        private Fixture Fixture { get; } = new Fixture();

        [Fact]
        public void Test_Action_Properties()
        {
            var action = Fixture.Create<Library.Settings.Action>();
            var sut = new Library.Settings.Action();

            sut.Enabled = !action.Enabled;
            sut.FailOnError = !action.FailOnError;
            sut.IncludePrerelease = !action.IncludePrerelease;
            sut.OnlyLatestVersion = !action.OnlyLatestVersion;
            sut.Name = action.Name;
            sut.PackagesToIgnore.AddRange(action.PackagesToIgnore);
            sut.SettingsGroup = action.SettingsGroup;
            sut.SourceFeed = action.SourceFeed;
            sut.TargetFeed = action.TargetFeed;
            sut.Type = action.Type;

            Assert.True(!action.Enabled == sut.Enabled);
            Assert.True(!action.FailOnError == sut.FailOnError);
            Assert.True(!action.IncludePrerelease == sut.IncludePrerelease);
            Assert.True(!action.OnlyLatestVersion == sut.OnlyLatestVersion);
            Assert.True(action.Name == sut.Name);
            Assert.True(action.SettingsGroup == sut.SettingsGroup);
            Assert.True(action.SourceFeed == sut.SourceFeed);
            Assert.True(action.TargetFeed == sut.TargetFeed);
            Assert.True(action.Type == sut.Type);
            Assert.Equal(action.PackagesToIgnore, sut.PackagesToIgnore);
        }

        [Fact]
        public void Test_Action_Commands_Collection()
        {
            var sut = Fixture.Create<Library.Settings.Action>();

            Fixture.AddManyTo(sut.Commands, 7);

            Assert.True(sut.Commands.Count == 7);
        }

        [Fact]
        public void Test_Action_Observers_Collection()
        {
            var sut = Fixture.Create<Library.Settings.Action>();

            Fixture.AddManyTo(sut.Observers, 10);

            Assert.True(sut.Observers.Count == 10);
        }

        [Fact]
        public void Test_Action_Settings_Collection()
        {
            var sut = Fixture.Create<Library.Settings.Action>();

            Fixture.AddManyTo(sut.Settings, 5);

            Assert.True(sut.Settings.Count == 5);
        }

        [Fact]
        public void Test_Action_Clone()
        {
            var action = Fixture.Create<Library.Settings.Action>();
            Fixture.AddManyTo(action.Commands);
            Fixture.AddManyTo(action.Observers);
            Fixture.AddManyTo(action.Settings);
            action.Enabled = !action.Enabled;
            action.FailOnError = !action.FailOnError;
            action.IncludePrerelease = !action.IncludePrerelease;
            action.OnlyLatestVersion = !action.OnlyLatestVersion;
            action.OnlyPackagesCreatedInTheLastMinutes += 1;

            var sut = action.Clone();

            Assert.Equal(action.Enabled, sut.Enabled);
            Assert.Equal(action.SettingsGroup, sut.SettingsGroup);
            Assert.Equal(action.TargetFeed, sut.TargetFeed);
            Assert.Equal(action.Type, sut.Type);
            Assert.Equal(action.FailOnError, sut.FailOnError);
            Assert.Equal(action.IncludePrerelease, sut.IncludePrerelease);
            Assert.Equal(action.OnlyLatestVersion, sut.OnlyLatestVersion);
            Assert.Equal(action.Name, sut.Name);
            Assert.Equal(action.SourceFeed, sut.SourceFeed);
            Assert.Equal(action.Commands, sut.Commands);
            Assert.Equal(action.Observers, sut.Observers);
            Assert.Equal(action.PackagesToIgnore, sut.PackagesToIgnore);
            Assert.Equal(action.OnlyPackagesCreatedInTheLastMinutes, sut.OnlyPackagesCreatedInTheLastMinutes);
            Assert.Equal(action.Settings, sut.Settings);
        }

        [Fact]
        public void Test_Action_CloneAndMergeSettings()
        {
            var action = Fixture.Create<Library.Settings.Action>();
            Fixture.AddManyTo(action.Commands);
            Fixture.AddManyTo(action.Observers);
            Fixture.AddManyTo(action.Settings);
            action.Enabled = !action.Enabled;
            action.FailOnError = !action.FailOnError;
            action.IncludePrerelease = !action.IncludePrerelease;
            action.OnlyLatestVersion = !action.OnlyLatestVersion;
            var settingsGroup = Fixture.Create<SettingsCollection>();
            Fixture.AddManyTo(settingsGroup);

            var sut = action.CloneAndMergeSettings(settingsGroup);

            Assert.Equal(action.Enabled, sut.Enabled);
            Assert.Equal(action.SettingsGroup, sut.SettingsGroup);
            Assert.Equal(action.TargetFeed, sut.TargetFeed);
            Assert.Equal(action.Type, sut.Type);
            Assert.Equal(action.FailOnError, sut.FailOnError);
            Assert.Equal(action.IncludePrerelease, sut.IncludePrerelease);
            Assert.Equal(action.OnlyLatestVersion, sut.OnlyLatestVersion);
            Assert.Equal(action.Name, sut.Name);
            Assert.Equal(action.SourceFeed, sut.SourceFeed);
            Assert.Equal(action.Commands, sut.Commands);
            Assert.Equal(action.Observers, sut.Observers);
            Assert.Equal(action.PackagesToIgnore, sut.PackagesToIgnore);
            Assert.NotEqual(action.Settings, sut.Settings);
        }

        [Fact]
        public void Test_Action_Collection_Default()
        {
            var sut = new ActionCollection();
            Fixture.AddManyTo(sut, 19);

            Assert.True(sut.Count == 19);
        }

        [Fact]
        public void Test_Action_Collection_Constructor_Int()
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            var sut = new ActionCollection(20);
            Assert.True(sut.Count == 0);
        }

        [Fact]
        public void Test_Action_Collection_Constructor_IEnumerable()
        {
            var actionCollection = new ActionCollection();
            Fixture.AddManyTo(actionCollection, 13);

            var sut = new ActionCollection(actionCollection);

            Assert.Equal(actionCollection, sut);
        }
    }
}
