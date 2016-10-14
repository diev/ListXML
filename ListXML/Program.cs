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
// using CommandLine v1.9, MIT
// Copyright 2015-2013 Giacomo Stelluti Scala
// https://github.com/gsscoder/commandline
//------------------------------------------------------------------------------

#define TRACE

using CommandLine;
using Lib;
using System;
using System.Diagnostics;
using System.Text;

namespace ListXML
{
    class Program
    {
        public static Options Options = new Options();

        static void Main(string[] args)
        {
            Trace.Listeners.Add(new AppTraceListener(App.Log));

            #region Options
            Parser.Default.ParseArgumentsStrict(args, Options);

            #region Verbose
            if (Options.Verbose || AppTraceListener.TraceConsole.TraceVerbose)
            {
                Trace.Listeners.Add((TraceListener)new AppTraceListener());
            }
            #endregion Verbose

            #region Info
            Console.WriteLine("{0} running...", App.Version);

            StringBuilder sb = new StringBuilder(128);
            sb.AppendFormat("{0} on {1}", Environment.UserName, Environment.MachineName.ToLower());
#if DEBUG
            sb.Append(" DEBUG");
#endif
            foreach (string arg in args)
            {
                sb.Append(" ");
                sb.Append(arg);
            }
            Trace.TraceInformation(sb.ToString());
            #endregion Info

            #region TestMail
            if (Options.TestMail != null)
            {
                string test = Options.GetUsage();

                Console.WriteLine("Sending a test mail to {0}...", Options.TestMail);

                Mailer.Send(Options.TestMail, "тест отправки почты!", test);

                Console.WriteLine("Press Esc to exit");
                Console.ReadKey();

                App.ExitInformation("Тест почты завершен.");
            }
            #endregion TestMail

            #region Date
            if (Options.Date != null)
            {
                DateTime date;
                if (DateTime.TryParse(Options.Date, out date))
                {
                    EDStorage.Date = date;
                }
                else
                {
                    //throw new ArgumentException("Ошибка чтения даты.", "Options.Date");
                    App.ExitError("Ошибка чтения даты из параметров.");
                }
            }
            else
            {
                if (DateTime.Now.Hour < 11)
                {
                    //Ищем предыдущий рабочий день
                    EDStorage.SetLastEDDate();
                }
                else
                {
                    //Работа текущим днем 
                    EDStorage.SetToday();
                }
            }
            #endregion Date

            #region Ignore
            if (Options.Ignore)
            {
                //Если уж так вышло, что ну нет окончательной выписки
                EDStorage.CreditSumFinal = true;
            }
            #endregion Ignore
            #endregion Options

            #region Run
            //Инициализация
            string bic = App.Settings["BIC"];
            BankLS.BIC3 = bic.Substring(bic.Length - 3);

            //Читаем файлы
            EDStorage.ReadFiles();
            #endregion Run

            #region Finish
            //Подождем окончания рассылки
            Mailer.FinalDelivery(2);

            //Выход
            App.Exit(); //App.ExitInformation("Программа завершена.");
            #endregion Finish
        }
    }
}
