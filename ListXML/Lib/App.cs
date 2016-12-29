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
using System.Reflection;

namespace Lib
{
    /// <summary>
    /// Properties of application
    /// </summary>
    public static class App
    {
        static App()
        {
            Exe = Assembly.GetCallingAssembly().Location;
            Dir = AppDomain.CurrentDomain.BaseDirectory;
            Assembly assembly = Assembly.GetCallingAssembly();
            AssemblyName assemblyName = assembly.GetName();
            //Name = Environment.GetCommandLineArgs()[0] + ".exe"
            Name = assemblyName.Name;

            // Major.Minor.Build.Revision
            Ver = assemblyName.Version.Build > 60101
                ? assemblyName.Version
                : new Version(1, 0, 60101, 0);

            // 1.0.0.0 (by default)
            // 1.0.2016.815 (yyyy.[M]Mdd, where Build = yyyy, Revision = Mdd)
            //public static readonly DateTime Dated = DateTime.Parse(string.Format("{0}-{1}-{2}", Ver.Build, Ver.Revision / 100, Ver.Revision % 100));
            // 1.0.60815.0 (yMMdd, where Build = yMMdd)
            SDated = string.Format("201{0}-{1}-{2}", Ver.Build / 10000, Ver.Build % 10000 / 100, Ver.Build % 100);
            Dated = DateTime.Parse(SDated);

            Version = string.Format("{0} v{1}", Name, Ver); // Ver.ToString(2), Ver.Build, Ver.Revision

            AssemblyDescriptionAttribute descriptionAttribute = Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute)) as AssemblyDescriptionAttribute;
            Description = descriptionAttribute.Description;

            AssemblyCompanyAttribute companyAttribute = Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;
            Company = companyAttribute.Company;

            AssemblyCopyrightAttribute copyrightAttribute = Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute;
            Copyright = copyrightAttribute.Copyright;
        }

        /// <summary>
        /// Файл приложения
        /// </summary>
        public static string Exe { get; }

        /// <summary>
        /// Путь к размещению файла приложения
        /// </summary>
        public static string Dir { get; }

        /// <summary>
        /// Название приложения
        /// </summary>
        public static string Name { get; }

        /// <summary>
        /// Версия приложения
        /// </summary>
        public static Version Ver { get; }

        /// <summary>
        /// Дата версии приложения
        /// </summary>
        // 1.0.0.0 (by default)
        // 1.0.2016.815 (yyyy.[M]Mdd, where Build = yyyy, Revision = Mdd)
        //public static readonly DateTime Dated = DateTime.Parse(string.Format("{0}-{1}-{2}", Ver.Build, Ver.Revision / 100, Ver.Revision % 100));
        // 1.0.60815.0 (yMMdd, where Build = yMMdd)
        public static string SDated { get; }
        public static DateTime Dated { get; }

        /// <summary>
        /// Строка с версией и датой версии приложения
        /// </summary>
        public static string Version { get; }

        /// <summary>
        /// Краткое описание приложения
        /// </summary>
        public static string Description { get; }

        /// <summary>
        /// Компания разработчика приложения
        /// </summary>
        public static string Company { get; }

        /// <summary>
        /// Авторские права на приложение
        /// </summary>
        public static string Copyright { get; }
    }
}