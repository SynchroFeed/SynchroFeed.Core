#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="Program.cs">
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
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SynchroFeed.Library.DependencyInjection;
using SynchroFeed.Library.Processor;
using SynchroFeed.Library.Settings;

namespace SynchroFeed.Console
{
    /// <summary>
    /// The main program that contains the entry point of the SynchroFeed.Console application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The entry point of the SynchroFeed.Console application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            var commandLineApp = new ConsoleCommandLine();
            commandLineApp.OnExecute(() => Execute(commandLineApp));
            commandLineApp.Execute(args);
        }

        /// <summary>
        /// The main setup and execution logic for the SynchroFeed.Console program.
        /// </summary>
        /// <param name="commandLineApp">The command line application containing the parsed command line parameters.</param>
        /// <returns>System.Int32.</returns>
        private static int Execute(ConsoleCommandLine commandLineApp)
        {
            try
            {
                var host = CreateHostBuilder(commandLineApp)
                    .Build();

                host.Services
                    .GetService<ILoggerFactory>();

                NLog.LogManager.LoadConfiguration("nlog.config");

                try
                {
                    var actionProcessor = host.Services.GetRequiredService<IActionProcessor>();
                    actionProcessor.Execute(commandLineApp.Actions);
                }
                finally
                {
                    NLog.LogManager.Shutdown();
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Creates the host builder and initializes the application.
        /// </summary>
        /// <param name="commandLineApp">The parsed command line arguments.</param>
        /// <returns>Returns an instance of IHostBuilder.</returns>
        private static IHostBuilder CreateHostBuilder(ConsoleCommandLine commandLineApp)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile(Path.GetFullPath(commandLineApp.ConfigFile));
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                    services.AddLogging();
                    services.Configure<ApplicationSettings>(hostContext.Configuration.GetSection("FeedSettings"));
                    services.AddSingleton(commandLineApp);
                    services.AddSynchroFeed();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddNLog();
                });

            return builder;
        }
    }
}