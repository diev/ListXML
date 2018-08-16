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
// using CommandLine v1.9, MIT
// Copyright 2015-2013 Giacomo Stelluti Scala
// https://github.com/gsscoder/commandline
//------------------------------------------------------------------------------
#endregion

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
            try
            {
                #region Options
                Parser.Default.ParseArgumentsStrict(args, Options);

                #region Verbose
                if (Options.Verbose)
                {
                    AppTrace.TraceSource.Switch.Level = SourceLevels.All;
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
                AppTrace.Information(sb);
                #endregion Info

                #region TestSettings
                if (Options.Test)
                {
                    AppConfig.Display();
                    Environment.Exit(0); //for AppVeyor
                }
                #endregion TestSettings

                #region TestMail
                if (Options.TestMail != null)
                {
                    string test = Options.GetUsage();

                    Console.WriteLine("Sending a test mail to {0}...", Options.TestMail);

                    Mailer.Send(Options.TestMail, "тест отправки почты!", test);

                    Console.WriteLine("Press Esc to exit");
                    Console.ReadKey();

                    AppExit.Information("Тест почты завершен.");
                }
                #endregion TestMail

                EDStorage.PreCheck();
                EDStorage.PreProcessChk();

                #region Date
                if (Options.Date != null)
                {
                    DateTime date;
                    if (DateTime.TryParse(Options.Date, out date))
                    {
                        AppTrace.Verbose("Работа за день {0:yyyy-MM-dd}.", date);
                        EDStorage.Date = date;
                    }
                    else
                    {
                        //throw new ArgumentException("Ошибка чтения даты.", "Options.Date");
                        AppExit.Error("Ошибка чтения даты из параметров.");
                    }
                }
                else if (DateTime.Now.Hour < Options.Hour)
                {
                    AppTrace.Verbose("Время работать предыдущим днем.");
                    EDStorage.SetLastEDDate(DateTime.Now);
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
                //Читаем файлы
                EDStorage.ReadFiles();
                #endregion Run

                #region Finish
                //Подождем окончания рассылки
                Mailer.FinalDelivery(2);
                #endregion Finish

                //Выход
                AppExit.Information(); //"Программа завершена.");
            }
            catch (Exception ex)
            {
                AppExit.Error(ex.Message);
            }
        }
    }
}
