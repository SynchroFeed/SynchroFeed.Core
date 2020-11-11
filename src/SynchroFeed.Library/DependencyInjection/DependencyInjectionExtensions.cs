#region header

// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="DependencyInjectionExtensions.cs">
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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SynchroFeed.Library.Action.Observer;
using SynchroFeed.Library.DomainLoader;
using SynchroFeed.Library.Factory;
using SynchroFeed.Library.Processor;
using SynchroFeed.Library.Settings;
using System;

namespace SynchroFeed.Library.DependencyInjection
{
    /// <summary>The DependencyInjectionExtensions class is an extension class for implementing helper functions to assist with dependency injection.</summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Registers a configuration instance which TOptions will bind against.
        /// </summary>
        /// <param name="services">The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> to add the services to.</param>
        /// <returns>The <see cref="T:Microsoft.Extensions.DependencyInjection.IServiceCollection" /> so that additional calls can be chained.</returns>
        /// <exception cref="ArgumentNullException">services
        /// or
        /// config</exception>
        public static IServiceCollection AddSynchroFeed(this IServiceCollection services)
        {
            return services
                .AddSingleton(provider => provider.GetService<IOptions<ApplicationSettings>>()?.Value)
                .AddTransient<ActionObserverManager>()
                .Scan(scan => scan
                    .FromAssemblies(AssemblyLoader.AssemblyLoaderFunc("*.dll"))
                    .AddClasses(classes => classes.AssignableTo<IActionProcessor>())
                    .AsImplementedInterfaces()
                    .WithTransientLifetime()
                    .AddClasses(classes => classes.AssignableTo<IRepositoryFactory>())
                    .AsImplementedInterfaces()
                    .WithTransientLifetime()
                    .AddClasses(classes => classes.AssignableTo<IActionFactory>())
                    .AsImplementedInterfaces()
                    .WithTransientLifetime()
                    .AddClasses(classes => classes.AssignableTo<ICommandFactory>())
                    .AsImplementedInterfaces()
                    .WithTransientLifetime()
                    .AddClasses(classes => classes.AssignableTo<IActionObserverFactory>())
                    .AsImplementedInterfaces()
                    .WithTransientLifetime()
                );
        }
    }
}