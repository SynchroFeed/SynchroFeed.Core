#region header
// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Robert Vandehey" file="TestLogger.cs">
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;

namespace SynchroFeed.Command.ApplicationIs64bit.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class TestLogger : ILogger
    {
        private class Scope : IDisposable
        {
            public void Dispose()
            {
            }
        }

        public class LoggedMessage
        {
            public LoggedMessage(LogLevel logLevel, EventId eventId, object state, Exception exception)
            {
                LogLevel = logLevel;
                EventId = eventId;
                State = state;
                Exception = exception;
            }

            public LogLevel LogLevel { get; }
            public EventId EventId { get; }
            public object State { get; }
            public Exception Exception { get; }
        }

        public List<LoggedMessage> LoggedMessages { get; } = new List<LoggedMessage>();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var logValues = state as FormattedLogValues;
            if (logValues != null)
            {
                foreach (var logValue in logValues)
                {
                    Console.WriteLine(logValue.Value);
                }
            }
            LoggedMessages.Add(new LoggedMessage(logLevel, eventId, state, exception));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new Scope();
        }
    }
}
