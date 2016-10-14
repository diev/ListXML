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

namespace Lib
{
    public static class BankLS
    {
        /// <summary>
        /// Последние 3 цифры БИК банка
        /// </summary>
        public static string BIC3 = null;

        /// <summary>
        /// Первые цифры (конто и валюта) счета
        /// </summary>
        public static string Conto = "40702810";

        /// <summary>
        /// Возвращает лицевой счет с правильно посчитанным ключом
        /// </summary>
        /// <param name="xLS">Последние цифры счета</param>
        /// <returns>20-значный лицевой счет</returns>
        public static string Keyed(int xLS)
        {
            return Keyed(xLS.ToString());
        }

        /// <summary>
        /// Возвращает лицевой счет с правильно посчитанным ключом
        /// </summary>
        /// <param name="xLS">Последние цифры счета</param>
        /// <returns>20-значный лицевой счет</returns>
        public static string Keyed(string xLS)
        {
            if (string.IsNullOrEmpty(BIC3))
            {
                throw new Exception("Не задан БИК банка для расчета ключа");
            }

            string ls11 = xLS.PadLeft(11, '0');
            string stmp = string.Format(" {0}{1}0{2}", BIC3, Conto, ls11);

            char[] tmp = stmp.ToCharArray();
            int sum = 0;

            for (int i = 1; i < tmp.Length; i++)
            {
                switch (i % 3)
                {
                    case 0:
                        sum += Convert.ToInt32(tmp[i]) * 3 % 10;
                        break;
                    case 1:
                        sum += Convert.ToInt32(tmp[i]) * 7 % 10;
                        break;
                    case 2:
                        sum += Convert.ToInt32(tmp[i]) % 10;
                        break;
                }
            }

            return string.Format("{0}{1}{2}", Conto, sum * 3 % 10, ls11);
        }
    }
}
