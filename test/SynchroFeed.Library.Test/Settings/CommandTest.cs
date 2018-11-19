#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="CommandTest.cs">
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
using SynchroFeed.Library.Command;
using SynchroFeed.Library.Settings;
using Xunit;

namespace SynchroFeed.Library.Test.Settings
{
    public class CommandTest
    {
        [Fact]
        public void Test_Command_Properties()
        {
            var fixture = new Fixture();
            var sut = new Library.Settings.Command
            {
                Type = "TestType",
                SettingsGroup = "TestSettingsGroup",
                FailureAction = CommandFailureAction.FailAction
            };
            fixture.AddManyTo(sut.Settings, 10);

            Assert.Equal("TestType", sut.Type);
            Assert.Equal("TestSettingsGroup", sut.SettingsGroup);
            Assert.Equal(CommandFailureAction.FailAction, sut.FailureAction);
            Assert.True(sut.Settings.Count == 10);
        }

        [Fact]
        public void Test_CommandCollection_Constructor_Int()
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            var c = new CommandCollection(20);
            Assert.True(c.Count == 0);
        }

        [Fact]
        public void Test_Command_Clone()
        {
            var fixture = new Fixture();
            var command = fixture.Create<Library.Settings.Command>();
            fixture.AddManyTo(command.Settings);
            command.FailureAction = CommandFailureAction.FailPackage;

            var sut = command.Clone();

            Assert.Equal(command.SettingsGroup, sut.SettingsGroup);
            Assert.Equal(command.Type, sut.Type);
            Assert.Equal(command.FailureAction, sut.FailureAction);
            Assert.Equal(command.Settings, sut.Settings);
        }

        [Fact]
        public void Test_Command_CloneAndMergeSettings()
        {
            var fixture = new Fixture();
            var command = fixture.Create<Library.Settings.Command>();
            command.FailureAction = CommandFailureAction.FailPackage;
            fixture.AddManyTo(command.Settings);
            var settingsGroup = fixture.Create<SettingsCollection>();
            fixture.AddManyTo(settingsGroup);

            var sut = command.CloneAndMergeSettings(settingsGroup);

            Assert.Equal(command.SettingsGroup, sut.SettingsGroup);
            Assert.Equal(command.Type, sut.Type);
            Assert.Equal(command.FailureAction, sut.FailureAction);
            Assert.NotEqual(command.Settings, sut.Settings);
        }
    }
}
