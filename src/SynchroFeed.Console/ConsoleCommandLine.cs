#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="ConsoleCommandLine.cs">
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
using Microsoft.Extensions.CommandLineUtils;

namespace SynchroFeed.Console
{
    /// <summary>
    /// The ConsoleCommandLine class is an implementation of the Microsoft.Extensions.CommandLineUtils.CommandLineApplication
    /// class that implements command line handling for the application.
    /// </summary>
    /// <seealso cref="CommandLineApplication" />
    public class ConsoleCommandLine : CommandLineApplication
    {
        private const string DefaultConfigFile = "app.json";

        private readonly CommandOption configFile;
        private readonly CommandArgument actions;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleCommandLine"/> class.
        /// </summary>
        public ConsoleCommandLine()
        {
            // SynchroFeed.Console.exe [-c|--config <configFilename>] [action...] [-?|-h|--help]
            configFile = Option("-c |--config <config>",
                                     "The config file to use. If blank, the app.json file will be used.",
                                     CommandOptionType.SingleValue);
            actions = Argument("action", 
                                    "The action name(s) within the config file to run. If blank, all enabled actions are run.", 
                                    true);
            HelpOption("-? | -h | --help");
        }

        /// <summary>
        /// Gets the name of the configuration file.
        /// </summary>
        /// <value>The name of the configuration file.</value>
        public string ConfigFile {
            get
            {
                if (configFile.HasValue())
                {
                    return configFile.Value();
                }
                else
                {
                    return DefaultConfigFile;
                }
            }
        }

        /// <summary>
        /// Gets the list of actions to execute in the ConfigFile. If the list is empty, all enabled actions are executed.
        /// </summary>
        /// <value>The actions to execute in the ConfigFile.</value>
        public List<string> Actions
        {
            get { return actions.Values; }
        }
    }
}
