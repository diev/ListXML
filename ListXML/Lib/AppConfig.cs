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
using System.Reflection;

namespace Lib
{
    public static class AppConfig
    {
        static Dictionary<string, string> baseDictionary = new Dictionary<string, string>();

        static AppConfig()
        {
            NameValueCollection appSettings = ConfigurationManager.AppSettings;
            foreach (string key in appSettings.AllKeys)
            {
                string value = appSettings[key];
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }
                Add(key, value);
            }
        }

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
            if (baseDictionary.ContainsKey(key))
            {
                baseDictionary[key] = value;
            }
            else
            {
                baseDictionary.Add(key, value);
            }
        }

        public static void AddDefault(string key, string value)
        {
            if (baseDictionary.ContainsKey(key))
            {
                return;
            }
            baseDictionary.Add(key, value);
        }

        public static string Get(string key)
        {
            //return baseDictionary[key]; // Exception if key does not exist!
            string value = null;
            baseDictionary.TryGetValue(key, out value);
            return value;
        }

        public static void Set(string key, string value)
        {
            baseDictionary[key] = value;
        }

        /// <summary>
        /// Expands the stored value with %Now%, %App% и %переменные_среды%
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetPath(string key)
        {
            string value = baseDictionary[key];
            if (value.Contains("{"))
            {
                string format = value.Replace("%Now%", "0").Replace("%App%", "1");
                value = string.Format(format,
                    DateTime.Now,
                    Assembly.GetCallingAssembly().GetName().Name);
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
    }
}
