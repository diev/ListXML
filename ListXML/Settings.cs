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
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;

namespace ListXML
{
    public static class Settings
    {
        static Settings()
        {
            // Default values for app.config
            baseDictionary = new Dictionary<string, string>()
            {
                // Маска файлов для лога (0: DateTime, 1: App.Name), можно использовать %переменные_среды%
                { "Log", @"%TEMP%\{1}\log\{0:yyyyMMdd}_{1}.log" },

                // Название банка
                { "Bank", "АО &quot;Сити Инвест Банк&quot;" },
                // БИК банка
                { "BIC", "044030702" },
                // Корсчет банка
                { "KS", "30101810600000000702" },
                // Название ТУ Банка России
                { "RKC", "Северо-Западное ГУ Банка России" },
                // Файл XSLT форматирования
                { "XSLT", "UFEBS.xslt" },

                // Путь к исходным файлам из АРМ КБР (UARM.cfg:MachineConfig\Gates\ChkOut1Dir)
                { "PathChk", @"c:\uarm3\exg\chk\" },
                // Путь к хранилищу обрабатываемых файлов
                { "PathXML", @"%TEMP%\{1}\xml\" },
                // Путь к накопителю для загрузки в АБС
                { "PathABS", @"%TEMP%\{1}\in\" },
                // Файл в накопителе АБС для загрузки (0: List, 1:EDDate.Substring(5))
                { "FileABS", @"LIST{0}\List{0}-{1}.xml" },

                // UIC банка в АРМ КБР
                { "UICBank", "4030702000" },
                // UIC ТУ Банка России в АРМ КБР
                { "UICRKC", "4030001999" },

                // Файл (имя после @) или построчный перечень счетов для списка 1
                { "List1", "@list1.txt" },
                // Файл (имя после @) или построчный перечень конто для списка 2
                { "List2", "@list2.txt" }
            };

            // Add and override default values by app.config
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            foreach (string key in appSettings.AllKeys)
            {
                string value = appSettings[key];
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (baseDictionary.ContainsKey(key))
                {
                    Trace.TraceInformation("Key \"{0}\" overrided with value \"{1}\".", key, value);
                    baseDictionary[key] = value;
                }
                else
                {
                    baseDictionary.Add(key, value);
                }
            }
        }

        static Dictionary<string, string> baseDictionary;

        public static void Display()
        {
            Console.WriteLine();
            foreach (KeyValuePair<string, string> p in baseDictionary)
            {
                Console.WriteLine("\t{0}\t = {1}", p.Key, p.Value);
            }
            Console.WriteLine();
        }

        public static void Add(string key, string value)
        {
            baseDictionary.Add(key, value);
        }

        public static string Get(string key)
        {
            return baseDictionary[key];
        }

        public static void Set(string key, string value)
        {
            baseDictionary[key] = value;
        }

        public static string GetPath(string key)
        {
            string value = baseDictionary[key];
            if (value.Contains("{"))
            {
                value = string.Format(value, DateTime.Now, Assembly.GetCallingAssembly().GetName().Name);
            }
            if (value.Contains("%"))
            {
                value = Environment.ExpandEnvironmentVariables(value);
            }
            return value;
        }

        public static bool IsSet(string key, out string value)
        {
            return baseDictionary.TryGetValue(key, out value);
        }

        #region Get

        // Get()

        /// <summary>
        /// Подписчики для сообщений администратору
        /// </summary>
        public static string Email { get { return baseDictionary["Email"]; } }

        /// <summary>
        /// Название банка
        /// </summary>
        public static string Bank { get { return baseDictionary["Bank"]; } }

        /// <summary>
        /// БИК банка
        /// </summary>
        public static string BIC { get { return baseDictionary["BIC"]; } }

        /// <summary>
        /// Корсчет банка
        /// </summary>
        public static string KS { get { return baseDictionary["KS"]; } }

        /// <summary>
        /// Название ТУ Банка России
        /// </summary>
        public static string RKC { get { return baseDictionary["RKC"]; } }

        /// <summary>
        /// UIC банка в АРМ КБР
        /// </summary>
        public static string UICBank { get { return baseDictionary["UICBank"]; } }

        /// <summary>
        /// UIC ТУ Банка России в АРМ КБР
        /// </summary>
        public static string UICRKC { get { return baseDictionary["UICRKC"]; } }

        /// <summary>
        /// Файл в накопителе АБС для загрузки (0: List, 1:EDDate.Substring(5))
        /// </summary>
        public static string FileABS { get { return baseDictionary["FileABS"]; } }

        #endregion Get

        #region GetPath
        // GetPath() // (0: DateTime, 1: App.Name), можно использовать %переменные_среды%

        /// <summary>
        /// Маска файлов для лога
        /// </summary>
        public static string Log { get { return GetPath("Log"); } }

        /// <summary>
        /// Файл (имя после @) или построчный перечень счетов для списка 1
        /// </summary>
        public static string List1 { get { return GetPath("List1"); } }

        /// <summary>
        /// Файл (имя после @) или построчный перечень конто для списка 2
        /// </summary>
        public static string List2 { get { return GetPath("List2"); } }

        /// <summary>
        /// Файл XSLT форматирования
        /// </summary>
        public static string XSLT { get { return GetPath("XSLT"); } }

        /// <summary>
        /// Путь к исходным файлам из АРМ КБР (UARM.cfg:MachineConfig\Gates\ChkOut1Dir)
        /// </summary>
        public static string PathChk { get { return GetPath("PathChk"); } }

        /// <summary>
        /// Путь к хранилищу обрабатываемых файлов
        /// </summary>
        public static string PathXML { get { return GetPath("PathXML"); } }

        /// <summary>
        /// Путь к накопителю для загрузки в АБС
        /// </summary>
        public static string PathABS { get { return GetPath("PathABS"); } }

        #endregion GetPath
    }
}
