#region License
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
// using CommandLine
// Copyright 2015-2013 Giacomo Stelluti Scala
// https://github.com/gsscoder/commandline
//------------------------------------------------------------------------------
#endregion

using CommandLine;
using CommandLine.Text;

namespace Lib
{
    class Options
    {
        [Option('t', "test",
            HelpText = "Test display of Settings and exit.")]
        public bool Test { get; set; }

        [Option('m', "mail", MetaValue = "admin@bank.ru",
            HelpText = "Send a test mail to this address and exit.")]
        public string TestMail { get; set; }

        [Option('f', "force",
            HelpText = "Force the rebuild of all files.")]
        public bool Force { get; set; }

        [Option('i', "ignore",
            HelpText = "Ignore the statement exists.")]
        public bool Ignore { get; set; }

        [Option('p', "payments",
            HelpText = "Extract payments only.")]
        public bool Payments { get; set; }

        [Option('d', "date", MetaValue = "YYYY-MM-DD",
            HelpText = "Specify a date to process those files.")]
        public string Date { get; set; }

        [Option('s', "switch", MetaValue = "HH", DefaultValue = 11,
            HelpText = "Specify a hour to switch between previous and current days.")]
        public int Hour { get; set; }

        [Option('v', "verbose",
            HelpText = "Print verbose details during execution.")]
        public bool Verbose { get; set; }

        //[Option('h', "help",
        //    HelpText = "Display this help screen.")]
        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
