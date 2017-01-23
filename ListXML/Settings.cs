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

using Lib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Reflection;

namespace ListXML
{
    public static class Settings
    {
        static Settings()
        {
            // Default values for app.config

            // Название банка
            AppConfig.AddDefault("Bank", "АО &quot;Сити Инвест Банк&quot;");
            // БИК банка
            AppConfig.AddDefault("BIC", "044030702");
            // Корсчет банка
            AppConfig.AddDefault("KS", "30101810600000000702");
            // Название ТУ Банка России
            AppConfig.AddDefault("RKC", "Северо-Западное ГУ Банка России");
            // Файл XSLT форматирования
            AppConfig.AddDefault("XSLT", "UFEBS.xslt");

            // Путь к исходным файлам из АРМ КБР (UARM.cfg:MachineConfig\Gates\ChkOut1Dir)
            AppConfig.AddDefault("PathChk", @"c:\uarm3\exg\chk\");
            // Путь к хранилищу обрабатываемых файлов
            AppConfig.AddDefault("PathXML", @"%TEMP%\{1}\xml\");
            // Путь к накопителю для загрузки в АБС
            AppConfig.AddDefault("PathABS", @"%TEMP%\{1}\in\");
            // Файл в накопителе АБС для загрузки (0: List, 1:EDDate.Substring(5))
            AppConfig.AddDefault("FileABS", @"LIST{0}\List{0}-{1}.xml");

            // UIC банка в АРМ КБР
            AppConfig.AddDefault("UICBank", "4030702000");
            // UIC ТУ Банка России в АРМ КБР
            AppConfig.AddDefault("UICRKC", "4030001999");

            // Файл (имя после @) или построчный перечень счетов для списка 1
            AppConfig.AddDefault("List1", "@list1.txt");
            // Файл (имя после @) или построчный перечень конто для списка 2
            AppConfig.AddDefault("List2", "@list2.txt");
        }

        #region Get

        // Get()

        /// <summary>
        /// Подписчики для сообщений администратору
        /// </summary>
        public static string Email { get; } = AppConfig.Get("Email");

        /// <summary>
        /// Название банка
        /// </summary>
        public static string Bank { get; } = AppConfig.Get("Bank");

        /// <summary>
        /// БИК банка
        /// </summary>
        public static string BIC { get; } = AppConfig.Get("BIC");

        /// <summary>
        /// Корсчет банка
        /// </summary>
        public static string KS { get; } = AppConfig.Get("KS");

        /// <summary>
        /// Название ТУ Банка России
        /// </summary>
        public static string RKC { get; } = AppConfig.Get("RKC");

        /// <summary>
        /// UIC банка в АРМ КБР
        /// </summary>
        public static string UICBank { get; } = AppConfig.Get("UICBank");

        /// <summary>
        /// UIC ТУ Банка России в АРМ КБР
        /// </summary>
        public static string UICRKC { get; } = AppConfig.Get("UICRKC");

        /// <summary>
        /// Файл в накопителе АБС для загрузки (0: List, 1:EDDate.Substring(5))
        /// </summary>
        public static string FileABS { get; } = AppConfig.Get("FileABS");

        #endregion Get

        #region GetPath
        // GetPath() // %Now%, %App%, %переменные_среды%

        /// <summary>
        /// Файл (имя после @) или построчный перечень счетов для списка 1
        /// </summary>
        public static string List1 { get; } = AppConfig.GetPath("List1");

        /// <summary>
        /// Файл (имя после @) или построчный перечень конто для списка 2
        /// </summary>
        public static string List2 { get; } = AppConfig.GetPath("List2");

        /// <summary>
        /// Файл XSLT форматирования
        /// </summary>
        public static string XSLT { get; } = AppConfig.GetPath("XSLT");

        /// <summary>
        /// Путь к исходным файлам из АРМ КБР (UARM.cfg:MachineConfig\Gates\ChkOut1Dir)
        /// </summary>
        public static string PathChk { get; } = AppConfig.GetPath("PathChk");

        /// <summary>
        /// Путь к хранилищу обрабатываемых файлов
        /// </summary>
        public static string PathXML { get; } = AppConfig.GetPath("PathXML");

        /// <summary>
        /// Путь к накопителю для загрузки в АБС
        /// </summary>
        public static string PathABS { get; } = AppConfig.GetPath("PathABS");

        #endregion GetPath
    }
}
