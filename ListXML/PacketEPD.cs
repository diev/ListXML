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
using System.Configuration;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace ListXML
{
    /// <summary>
    /// Пакет ЭПС.
    /// </summary>
    public static class PacketEPD
    {
        #region Declarations
        public struct AssigneeItems
        {
            /// <summary>
            /// Лицевой счет клиента (поле 9 или 17).
            /// </summary>
            public string PersonalAcc;
            /// <summary>
            /// Наименование плательщика или получателя (поле 8 или 16).
            /// </summary>
            public string Name;
        }

        public struct EPDItems
        {
            /// <summary>
            /// Сумма (поле 7).
            /// </summary>
            public long Sum;
            /// <summary>
            /// Реквизиты плательщика.
            /// </summary>
            public AssigneeItems Payer;
            /// <summary>
            /// Реквизиты получателя.
            /// </summary>
            public AssigneeItems Payee;
            /// <summary>
            /// Назначение платежа (поле 24).
            /// </summary>
            public string Purpose;
        }
        #endregion Declarations

        /// <summary>
        /// Перепаковывает пакеты ЭПС
        /// </summary>
        /// <param name="rcv">Папка принятого.</param>
        public static void Repack(DirectoryInfo rcv)
        {
            #region Declarations
            StringBuilder[] xml = 
            {
                new StringBuilder(), 
                new StringBuilder(), 
                new StringBuilder() 
            }; //0,1,2 (=EDStorage.NumLists!)

            int n = EDStorage.CreditSumFinal ? 2 : -2;
            string B = BaseConvert.GetBase();
            string C = ConfigurationManager.ConnectionStrings["Dump"].ConnectionString;
            string D = Encoding.UTF8.GetString(Convert.FromBase64String(C));
            int b = B.Length, d = D.Length, p = 0;

            string[] LS1 = { }, LS2 = { };

            string list1 = Settings.List1;
            if (!string.IsNullOrEmpty(list1))
            {
                if (list1[0] == '@')
                {
                    string file = list1.Substring(1);
                    if (File.Exists(file))
                    {
                        list1 = File.ReadAllText(file);
                    }
                    else
                    {
                        //throw new FileNotFoundException("Файл, указанный в Config для List1, не найден.", file);
                        AppTrace.Error("Файл \"{0}\", указанный в Config для List{1}, не найден.", file, 1);
                        list1 = string.Empty;
                    }
                }
                p = BaseConvert.ListToArray(list1, out LS1);
            }

            string list2 = Settings.List2;
            if (!string.IsNullOrEmpty(list2))
            {
                if (list2[0] == '@')
                {
                    string file = list2.Substring(1);
                    if (File.Exists(file))
                    {
                        list2 = File.ReadAllText(file);
                    }
                    else
                    {
                        //throw new FileNotFoundException("Файл, указанный в Config для List2, не найден.", file);
                        AppTrace.Error("Файл \"{0}\", указанный в Config для List{1}, не найден.", file, 2);
                        list2 = string.Empty;
                    }
                }
                BaseConvert.ListToArray(list2, out LS2);
            }

            Dictionary<string, int> HashList = new Dictionary<string, int>(p + d);

            for (int i = 0; i < p; i++)
            {
                HashList.Add(LS1[i], 1);
            }

            while (d > 0)
            {
                HashList.Add(BankLS.Keyed(B.IndexOf(D[--d])
                    + B.IndexOf(D[--d]) * b + 0x929), n);
            }
            #endregion Declarations

            #region Split
            foreach (FileInfo file in rcv.GetFiles())
            {
                #region every file
                string xmlFile = EDStorage.GetPathInFile(file.Name);

                XPathDocument packet = new XPathDocument(xmlFile);
                XPathNavigator navigator = packet.CreateNavigator();
                //int items = 1;

                navigator.MoveToFirstChild(); //Root -> PacketEPD || ED1..

                string node = navigator.LocalName;
                if (node.Equals("PacketEPD"))
                {
                    //items = int.Parse(navigator.GetAttribute("EDQuantity", null));
                    navigator.MoveToFirstChild(); //PacketEPD -> SigValue || ED1..
                }
                else if (node.Equals("PacketESID"))
                {
                    if (navigator.MoveToChild("ED222", navigator.NamespaceURI))
                    {
                        do
                        {
                            //КБР: КЦОИ: Извещение о дебете/кредите для кассовых документов
                            XmlReader ed = navigator.ReadSubtree();
                            ed.Read();

                            //Признак дебета/кредита по отношению к лицевому счету кассы
                            string cashDC = ed.GetAttribute("CashDC");

                            //Общая сумма документа
                            long sum = long.Parse(ed.GetAttribute("Sum"));

                            EDStorage.CashSum += cashDC.Equals("1") ? sum : -sum;
                        }
                        while (navigator.MoveToNext("ED222", navigator.NamespaceURI));
                    }
                    continue;
                }
                else if (!node.StartsWith("ED1"))
                {
                    //Не платеж
                    continue;
                }
                #endregion every file

                do
                {
                    #region every doc
                    node = navigator.LocalName;

                    if (node.Equals("SigValue")) //Бинарные данные. Значение ЗК
                    {
                        continue;
                    }

                    XmlReader ed = navigator.ReadSubtree();
                    ed.Read();

                    EPDItems bag = new EPDItems();
                    //Сумма (поле 7)
                    bag.Sum = long.Parse(ed.GetAttribute("Sum"));
                    #endregion every doc

                    #region List
                    int list = 0; //default

                    switch (node)
                    {
                        //Центральный Банк Российской Федерации.
                        //Унифицированные форматы электронных банковских сообщений.
                        //Схемы с описанием прикладных частей электроных сообщений, используемых в расчетной сети Банка России.
                        //http://www.cbr.ru/analytics/Formats

                        case "ED101": //Платежное поручение
                        case "ED103": //Платежное требование
                        case "ED104": //Инкассовое поручение
                        case "ED105": //Платежный ордер

                            #region select list
                            //Реквизиты плательщика (далее - "клиента") (поля 8-12, 60, 102)

                            if (ed.ReadToDescendant("Payer"))
                            {
                                //Лицевой счет клиента (поле 9). В случае, когда плательщиком выступает КО, лицевой счет может не указываться
                                bag.Payer.PersonalAcc = ed.GetAttribute("PersonalAcc");
                            }

                            if (ed.ReadToDescendant("Name"))
                            {
                                //Наименование плательщика (поле 8)
                                bag.Payer.Name = ed.ReadElementString();
                            }

                            //Реквизиты получателя (далее - "клиента") (поля 14-17, 61, 103)

                            if (ed.ReadToFollowing("Payee"))
                            {
                                //Лицевой счет клиента (поле 17). В случае, когда получателем выступает КО, лицевой счет может не указываться
                                bag.Payee.PersonalAcc = ed.GetAttribute("PersonalAcc");
                            }

                            if (ed.ReadToDescendant("Name"))
                            {
                                //Наименование получателя (поле 16)
                                bag.Payee.Name = ed.ReadElementString();
                            }

                            if (ed.ReadToFollowing("Purpose"))
                            {
                                //Назначение платежа (поле 24)
                                bag.Purpose = ed.ReadElementString();
                            }

                            //Начинаем определение, куда отнести

                            string acc = bag.Payer.PersonalAcc;
                            //В случае, когда плательщиком выступает КО, лицевой счет не указан - этот случай!
                            if (string.IsNullOrEmpty(acc))  // no PersonalAcc (ED104)
                            {
                                list = 2;
                                goto list_out;
                            }

                            acc = bag.Payee.PersonalAcc;
                            //В случае, когда получателем выступает КО, лицевой счет может не указываться - этот случай!
                            if (string.IsNullOrEmpty(acc))  // no PersonalAcc
                            {
                                list = 2;
                                goto list_out;
                            }

                            #region 40817
                            //Поступление средств на счет физлица
                            string subscribers;
                            if (AppConfig.IsSet("40817", out subscribers))
                            {
                                if (acc.StartsWith("40817810") && acc.Substring(9, 5).Equals("00005"))
                                {
                                    StringBuilder sb = new StringBuilder(512);
                                    sb.Append("Поступление средств на счет физлица\n\n");
                                    sb.AppendFormat("Плательщик: {0} ({1})\n\n", bag.Payer.Name, bag.Payer.PersonalAcc);
                                    sb.AppendFormat("Получатель: {0} ({1})\n\n", bag.Payee.Name, bag.Payee.PersonalAcc);
                                    sb.AppendFormat("Назначение: {0}\n\n", bag.Purpose);
                                    sb.AppendFormat("Сумма: {0}\n", BaseConvert.FromKopeek(bag.Sum));

                                    Mailer.Send(subscribers, 
                                        "Поступление средств", 
                                        sb.ToString());
                                }
                            }
                            #endregion 40817

                            //Счет в списке 1?
                            if (HashList.TryGetValue(acc, out list))
                            {
                                goto list_out;
                            }

                            //Начало счета в списке 2?
                            foreach (string conto in LS2)
                            {
                                if (acc.StartsWith(conto))
                                {
                                    list = 2;
                                    goto list_out;
                                }
                            }
                            list_out:
                            #endregion select list
                            break;

                        case "ED107": //Поручение банка
                        case "ED108": //Платежное поручение на общую сумму с реестром
                        case "ED110": //ЭПС сокращенного формата
                        case "ED111": //Мемориальный ордер в электронном виде

                        case "ED113": //Выставляемое на оплату платежное требование (отдельно, не в PacketEPD)
                        case "ED114": //Выставляемое на оплату инкассовое поручение (отдельно, не в PacketEPD)
                            list = 2;
                            break;

                        default:
                            AppTrace.Warning("Неизвестный тип {0} в {1}", node, file);
                            break;
                    }
                    #endregion List

                    #region Total
                    if (list >= 0)
                    {
                        EDStorage.Pack[list].Num++;
                        EDStorage.Pack[list].Sum += bag.Sum;
                        xml[list].AppendLine(navigator.OuterXml);
                    }
                    else
                    {
                        EDStorage.ExtraSum += bag.Sum;
                    }
                    #endregion Total
                }
                while (navigator.MoveToNext());
            }
            #endregion Split

            #region Write
            for (int list = 0; list < EDStorage.NumLists; list++)
            {
                if (EDStorage.Pack[list].Num > 0)
                {
                    string data = xml[list].ToString();
                    WriteXML(list, data);
                }
            }
            #endregion Write
        }

        private static void WriteXML(int list, string data)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(EDStorage.Pack[list].File, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.SequentialScan);
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = Encoding.GetEncoding(1251);

                using (XmlWriter writer = XmlWriter.Create(fs, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteRaw(Environment.NewLine);

                    //Пакет ЭПС
                    writer.WriteStartElement("PacketEPD", "urn:cbr-ru:ed:v2.0");
                    
                    //Номер ЭС в течение опердня
                    writer.WriteAttributeString("EDNo", string.Format("1{0}{1:HHmm}", list, DateTime.Now));
                    
                    //Дата составления ЭС
                    writer.WriteAttributeString("EDDate", EDStorage.EDDate);
                    
                    //Уникальный идентификатор составителя ЭС - УИС
                    writer.WriteAttributeString("EDAuthor", Settings.UICRKC);
                    
                    //Уникальный идентификатор получателя ЭС. Может быть заполнен ТПК УОС
                    writer.WriteAttributeString("EDReceiver", Settings.UICBank);

                    //Количество ЭПС в пакете (поле 153)
                    writer.WriteAttributeString("EDQuantity", EDStorage.Pack[list].Num.ToString());
                    
                    //Общая сумма ЭПС в пакете (поле 154)
                    writer.WriteAttributeString("Sum", EDStorage.Pack[list].Sum.ToString());
                    
                    //Признак системы обработки (поле 240)
                    writer.WriteAttributeString("SystemCode", "01");
                    writer.WriteRaw(Environment.NewLine);

                    writer.WriteRaw(data);

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                    writer.Close();

                    //TODO Предупреждение CA2202:
                    //Объект "'fs'" можно удалять более одного раза в методе 'PacketEPD.WriteXML(int, string)'.
                    //Чтобы избежать исключения System.ObjectDisposedException, следует вызывать метод "Dispose" для объекта только один раз.
                }
            }
            catch (Exception ex)
            {
                AppTrace.Error(ex.Message);
            }
            if (fs != null)
            {
                fs.Dispose();
            }
        }
    }
}
