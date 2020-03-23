#region header

// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="UtilityExtensionTest.cs">
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

#endregion header

using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using SynchroFeed.Library.Factory;
using SynchroFeed.Library.Settings;
using System;
using System.Net;
using System.Web;
using Xunit;

namespace SynchroFeed.Library.Test.Application
{
    public class UtilityExtensionTest
    {
        [Fact]
        public void Test_Combine_Uri_With_Path()
        {
            const string rootUrl = "https://www.mycompany.com/somepath";
            const string path = "AnotherPath";
            var uri = new Uri(rootUrl);
            var sut = uri.Combine(path);

            Assert.Equal(sut.AbsoluteUri, $"{rootUrl}/{path}");
        }

        [Fact]
        public void Test_Combine_Uri_Null()
        {
            const string rootUrl = "https://www.mycompany.com/somepath";
            const string path = null;
            var uri = new Uri(rootUrl);
            var sut = uri.Combine(path);

            Assert.Null(sut);
        }

        [Fact]
        public void Test_Convert_Boolean()
        {
            string value = "true";

            var sut = value.Convert<bool>();

            Assert.True(sut);
        }

        [Fact]
        public void Test_Convert_Boolean_Negative_Throws_Exception()
        {
            string value = "somevalue";

            Assert.Throws<FormatException>(() => value.Convert<bool>(true));
        }

        [Fact]
        public void Test_Convert_Int()
        {
            string value = "825";

            var sut = value.Convert<int>();

            Assert.Equal(825, sut);
        }

        [Fact]
        public void Test_Convert_Int_Negative_Throws_Exception()
        {
            string value = "somevalue";

            Assert.Throws<FormatException>(() => value.Convert<int>(true));
        }

        [Fact]
        public void Test_Convert_Int_Negative_Returns_Default()
        {
            string value = "somevalue";

            var sut = value.Convert<int>();

            Assert.Equal(default(int), sut);
        }

        [Fact]
        public void Test_FormatWith_Properties_Found()
        {
            string template = "Name: {Name}, Capital: {Capital}, GdpPerCapita: {GdpPerCapita:F2}";
            var model = new FormatWithModel { Name = "Malawi", Capital = "Lilongwe", GdpPerCapita = 226.50 };

            var sut = template.FormatWith(model);

            Assert.Equal("Name: Malawi, Capital: Lilongwe, GdpPerCapita: 226.50", sut);
        }

        [Fact]
        public void Test_FormatWith_Property_Not_Found()
        {
            string template = "Name: {Name}, Capital: {Capital}, GdpPerCapita: {GdpPerCapita:F2}, NotFound: {NotFound}";
            var model = new FormatWithModel { Name = "Malawi", Capital = "Lilongwe", GdpPerCapita = 226.50 };

            var sut = template.FormatWith(model);

            Assert.Equal("Name: Malawi, Capital: Lilongwe, GdpPerCapita: 226.50, NotFound: {NotFound}", sut);
        }

        [Fact]
        public void Test_FormatWith_Property_Not_Found_Throws_Exception()
        {
            string template = "Name: {Name}, Capital: {Capital}, GdpPerCapita: {GdpPerCapita:F2}, NotFound: {NotFound}";
            var model = new FormatWithModel { Name = "Malawi", Capital = "Lilongwe", GdpPerCapita = 226.50 };

            Assert.Throws<HttpException>(() => template.FormatWith(model, true));
        }

        [Fact]
        public void Test_GetCustomSetting_Found_String()
        {
            const string key = "Test Key";
            const string value = "Test Value";
            var fixture = new Fixture();
            var settings = new SettingsCollection();
            fixture.AddManyTo(settings, 10);
            settings.Add(key, value);
            fixture.AddManyTo(settings, 8);

            var sut = settings.GetCustomSetting<string>(key);

            Assert.Equal(value, sut);
        }

        [Fact]
        public void Test_GetCustomSetting_Not_Found_String()
        {
            const string key = "Test Key";
            var fixture = new Fixture();
            var settings = new SettingsCollection();
            fixture.AddManyTo(settings, 10);
            fixture.AddManyTo(settings, 8);

            var sut = settings.GetCustomSetting<string>(key);

            Assert.Null(sut);
        }

        [Fact]
        public void Test_GetCustomSetting_Found_Boolean()
        {
            const string key = "Test Key";
            const string value = "true";
            var fixture = new Fixture();
            var settings = new SettingsCollection();
            fixture.AddManyTo(settings, 10);
            settings.Add(key, value);
            fixture.AddManyTo(settings, 8);

            var sut = settings.GetCustomSetting<bool>(key);

            Assert.True(sut);
        }

        [Fact]
        public void Test_GetCustomSetting_Not_Found_Boolean()
        {
            const string key = "Test Key";
            var fixture = new Fixture();
            var settings = new SettingsCollection();
            fixture.AddManyTo(settings, 10);
            fixture.AddManyTo(settings, 8);

            var sut = settings.GetCustomSetting<bool>(key);

            Assert.False(sut);
        }

        [Fact]
        public void Test_GetNamedFactory_Factory_Found()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IActionFactory>(sp => new DummyActionFactory());
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            var sut = serviceProvider.GetNamedFactory<IActionFactory>("DummyAction");

            Assert.NotNull(sut);
        }

        [Fact]
        public void Test_GetNamedFactory_Factory_Not_Found()
        {
            var serviceCollection = new ServiceCollection();
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(() => serviceProvider.GetNamedFactory<IActionFactory>("DummyAction"));
        }

        [Fact]
        public void Test_IsPrerelease()
        {
            Assert.True("1.6.0-pre023".IsPrerelease());
            Assert.False("1.6.0".IsPrerelease());
        }
    }

    // ReSharper disable UnusedAutoPropertyAccessor.Local
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class FormatWithModel
    {
        public string Name { get; set; }
        public string Capital { get; set; }
        public double GdpPerCapita { get; set; }
    }
}