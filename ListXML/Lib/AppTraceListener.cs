//------------------------------------------------------------------------------
// Copyright (c) Dmitrii Evdokimov
// Source https://github.com/diev/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Lib
{
    public class AppTraceListener : DefaultTraceListener
    {
        public static TraceSwitch TraceConsole = new TraceSwitch("TraceConsole", "Trace level for Console in config", "Warning");
        public static TraceSwitch TraceLog = new TraceSwitch("TraceLog", "Trace level for Log in config", "Warning");
        //public static TraceSwitch TraceEmail = new TraceSwitch("TraceEmail", "Trace level for Email in config", "Error");

        public static int WarningCount = 0;
        public static int ErrorCount = 0;

        public AppTraceListener()
        {
        }

        public AppTraceListener(string file)
        {
            base.LogFileName = file;

            string path = Path.GetDirectoryName(file);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            this.TraceEvent(eventCache, source, eventType, id, string.Format(format, args));
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            DateTime DT = DateTime.Now;

            if (Trace.IndentLevel > 0)
            {
                message = new string(' ', Trace.IndentLevel * Trace.IndentSize) + message;
            }

            if (string.IsNullOrEmpty(base.LogFileName))
            {
                //Console
                if (TraceConsole.TraceVerbose)
                {
                    Console.Write("{0:HH:mm:ss}  ", DT);

                    switch (eventType)
                    {
                        case TraceEventType.Verbose:
                        case TraceEventType.Information:
                            break;

                        case TraceEventType.Warning:
                            WarningCount++;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;

                        case TraceEventType.Error:
                        case TraceEventType.Critical:
                            ErrorCount++;
                            Console.ForegroundColor = ConsoleColor.Red;
                            break;
                    }

                    Console.WriteLine(message);
                    Console.ResetColor();
                }
            }
            else
            {
                //File
                if (TraceLog.TraceVerbose)
                {
                    string dt = string.Format("{0:dd.MM.yyyy HH:mm:ss} ", DT); //{0:dd.MM.yyyy HH:mm:ss.fff}
                    string sg = " ";

                    switch (eventType)
                    {
                        case TraceEventType.Verbose:
                        case TraceEventType.Information:
                            break;

                        case TraceEventType.Warning:
                            WarningCount++;
                            sg = "{?} ";
                            break;

                        case TraceEventType.Error:
                        case TraceEventType.Critical:
                            ErrorCount++;
                            sg = "[!] ";
                            break;
                    }

                    File.AppendAllText(base.LogFileName, dt + sg + message + Environment.NewLine, Encoding.GetEncoding(1251));
                }
            }
        }
    }
}