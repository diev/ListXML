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
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Lib
{
    /// <summary>
    /// Properties of application
    /// </summary>
    public static class App
    {
        #region Info
        //http://msdn.microsoft.com/en-us/library/system.configuration.configuration.aspx
        //Name = Environment.GetCommandLineArgs()[0] + ".exe"

        //Config = System.Reflection.Assembly.GetExecutingAssembly().Location + ".config"
        // or AppDomain.CurrentDomain.SetupInformation.ConfigurationFile

        //public static readonly string Name = Assembly.GetCallingAssembly().GetName().Name;
        //public static readonly string Version = Assembly.GetCallingAssembly().GetName().Version.ToString();
        /// <summary>
        /// Файл приложения
        /// </summary>
        public static readonly string Exe = Assembly.GetCallingAssembly().Location;
        /// <summary>
        /// Путь к размещению файла приложения
        /// </summary>
        public static readonly string Dir = AppDomain.CurrentDomain.BaseDirectory;

        //public static readonly string Name = Assembly.GetExecutingAssembly().GetName().Name;
        //public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        static Assembly assembly = Assembly.GetCallingAssembly();
        static AssemblyName assemblyName = assembly.GetName();
        public static readonly string Name = assemblyName.Name;

        /// <summary>
        /// Версия приложения
        /// </summary>
        public static readonly Version Ver = assemblyName.Version.Build > 60101
            ? assemblyName.Version
            : new Version(1, 0, 60101, 0);
        /// <summary>
        /// Дата версии приложения
        /// </summary>
        // 1.0.0.0 (by default)
        // 1.0.2016.815 (yyyy.[M]Mdd, where Build = yyyy, Revision = Mdd)
        //public static readonly DateTime Dated = DateTime.Parse(string.Format("{0}-{1}-{2}", Ver.Build, Ver.Revision / 100, Ver.Revision % 100));
        // 1.0.60815.0 (yMMdd, where Build = yMMdd)
        public static readonly string SDated = string.Format("201{0}-{1}-{2}", Ver.Build / 10000, Ver.Build % 10000 / 100, Ver.Build % 100);
        public static readonly DateTime Dated = DateTime.Parse(SDated);
        /// <summary>
        /// Строка с версией и датой версии приложения
        /// </summary>
        public static readonly string Version = string.Format("{0} v{1}", Name, Ver); // Ver.ToString(2), Ver.Build, Ver.Revision

        //public static readonly string attribute = (attribute == null) ? string.Empty : attribute;
        static AssemblyDescriptionAttribute descriptionAttribute = Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute)) as AssemblyDescriptionAttribute;
        public static readonly string Description = descriptionAttribute.Description;

        static AssemblyCompanyAttribute companyAttribute = Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;
        /// <summary>
        /// Компания разработчика приложения
        /// </summary>
        public static readonly string Company = companyAttribute.Company;

        static AssemblyCopyrightAttribute copyrightAttribute = Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute;
        /// <summary>
        /// Авторские права на приложение
        /// </summary>
        public static readonly string Copyright = copyrightAttribute.Copyright;

        /// <summary>
        /// Файл записи лога приложения
        /// </summary>
        //public static string Log = Path.ChangeExtension(App.Exe, string.Format("{0:yyyyMMdd}.log", DateTime.Now));
        //public static string Log = Path.Combine("log", string.Format("{0:yyyyMMdd}_{1}.log", DateTime.Now, Name));
        #endregion Info

        #region Config
        // https://msdn.microsoft.com/en-us/library/system.configuration.configurationmanager(v=vs.100).aspx
        //<configuration>
        //    <appSettings>
        //        <add key="" value=""/>
        //    </appSettings>
        //    <connectionStrings>
        //        <add name = "" connectionString=""/>
        //    </connectionStrings>
        //</configuration>

        public static NameValueCollection Settings = ConfigurationManager.AppSettings;
        public static ConnectionStringSettingsCollection Connections = ConfigurationManager.ConnectionStrings;

        public static readonly string Log = Environment.ExpandEnvironmentVariables(string.Format(Settings["Log"] ?? "{0:yyyyMMdd}_{1}.log", DateTime.Now, Name));
        public static readonly string Email = Settings["Email"];

        public static bool IsSet(string key, out string value)
        {
            value = Settings[key];
            return !string.IsNullOrWhiteSpace(value);
        }

#endregion Config

#region Paths
        /// <summary>
        /// Checks if a directory exists. If not, tries to create.
        /// </summary>
        /// <param name="dir">A directory to check or create.</param>
        /// <returns>Returns the checked or created directory.</returns>
        public static string CheckDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                try
                {
                    Directory.CreateDirectory(dir);
                }
                catch (DirectoryNotFoundException)
                {
                    ExitError("Ошибка создания директории " + dir);
                    //If wrong, creates in the App directory.
                    //Trace.TraceError("Директорию не создать:" + ex.Message);
                    //dir = Path.Combine(App.Dir, "_Recovery", Path.GetDirectoryName(dir));
                    //Directory.CreateDirectory(dir);
                    //Trace.TraceWarning("Recovery directory {0} created.", dir);
                }
                catch (IOException)
                {
                    ExitError("Нет диска для директории " + dir);
                    //Trace.TraceError("Сетевой диск не доступен: " + ex.Message);
                    //dir = Path.Combine(App.Dir, "_Recovery", Path.GetDirectoryName(dir));
                    //Directory.CreateDirectory(dir);
                    //Trace.TraceWarning("Recovery directory {0} created.", dir);
                }
                catch (Exception ex)
                {
                    ExitError(string.Concat("Другая ошибка создания директории ", dir, "\n", ex.Message));
                }
            }
            return dir;
        }
#endregion Paths

#region Exit
        /// <summary>
        /// Завершить приложение с указанными кодом [0] и информационным сообщением
        /// </summary>
        /// <param name="code">Код завершения [0]</param>
        /// <param name="msg">Текст информации</param>
        public static void ExitInformation(string msg = null, int code = 0)
        {
            Trace.TraceInformation(ExitMessage(msg, code));
            Exit(code);
        }

        /// <summary>
        /// Завершить приложение с указанными кодом [1] и предупреждающим сообщением
        /// </summary>
        /// <param name="code">Код завершения [1]</param>
        /// <param name="msg">Текст предупреждения</param>
        public static void ExitWarning(string msg = null, int code = 1)
        {
            Trace.TraceWarning(ExitMessage(msg, code));
            Exit(code);
        }

        /// <summary>
        /// Завершить приложение с указанными кодом [2] и сообщением об ошибке
        /// </summary>
        /// <param name="code">Код завершения [2]</param>
        /// <param name="msg">Текст ошибки</param>
        public static void ExitError(string msg = null, int code = 2)
        {
            Trace.TraceError(ExitMessage(msg, code));
            Exit(code);
        }

        /// <summary>
        /// Завершить приложение с указанными кодом [0]
        /// </summary>
        /// <param name="code">Код завершения</param>
        public static void Exit(int code = 0)
        {
            Trace.Close();
            Environment.Exit(code);
        }

        public static string ExitMessage(string msg = null, int code = 0)
        {
            StringBuilder sb = new StringBuilder();
            if (code > 0)
            {
                sb.Append("Exit ");
                sb.Append(code);
            }
            if (msg != null)
            {
                sb.Append(": ");
                sb.Append(msg);
            }
            return sb.ToString();
        }
#endregion Exit
    }
}