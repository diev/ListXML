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
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace ListXML
{
    /// <summary>
    /// Хранилище файлов
    /// </summary>
    public static class EDStorage
    {
        static EDStorage()
        {
            PathChk = Settings.PathChk;
            AppTrace.Verbose("Путь к исходным файлам: \"{0}\"", PathChk);

            PathXML = Settings.PathXML;
            AppTrace.Verbose("Путь к хранилищу обрабатываемых файлов: \"{0}\"", PathXML);

            PathABS = Settings.PathABS;
            AppTrace.Verbose("Путь к накопителю для загрузки в АБС: \"{0}\"", PathABS);
        }

        #region Date
        /// <summary>
        /// Текущая дата обработки в формате DateTime
        /// </summary>
        public static DateTime Date { get; set; } = DateTime.Now;

        /// <summary>
        /// Текущая дата обработки в формате "YYYY-MM-DD"
        /// </summary>
        public static string EDDate
        {
            get
            {
                return Date.ToString("yyyy-MM-dd");
            }
            set
            {
                Date = DateTime.Parse(value);
            }
        }

        /// <summary>
        /// Обработка текущей календарной даты
        /// </summary>
        /// <returns>Обработка текущей календарной даты</returns>
        private static bool IsCurrentDate
        {
            get
            {
                return EDDate.Equals(DateTime.Now.ToString("yyyy-MM-dd"));
            }
        }

        /// <summary>
        /// Найти предыдущий день обмена и установить его для обработки
        /// </summary>
        public static void SetLastEDDate(DateTime date)
        {
            for (int i = 1; i <= 20; i++)
            {
                Date = date.AddDays(-i);
                AppTrace.Verbose("Отступ на {0} ({1}).", EDDate, -i);

                string path = Path.Combine(PathXML, "chk", EDDate);
                if (Directory.Exists(path) && Directory.GetFiles(path).Length > 0)
                {
                    AppTrace.Verbose("Работа за день {0}.", EDDate);
                    return;
                }
            }
            AppExit.Error("Нет исходных файлов.");
        }
        #endregion Date

        #region Paths
        /// <summary>
        /// Путь к исходным файлам
        /// </summary>
        public static string PathChk { get; }

        /// <summary>
        /// Путь к хранилищу обработанных файлов
        /// </summary>
        public static string PathXML { get; }

        /// <summary>
        /// Путь общего накопителя для загрузки в АБС
        /// </summary>
        public static string PathABS { get; }

        /// <summary>
        /// Путь к хранилищу читабельных копий файлов в формате XML
        /// </summary>
        public static string PathIn
        {
            get
            {
                return Path.Combine(PathXML, "in", EDDate);
            }
        }

        /// <summary>
        /// Путь к хранилищу копий исходный файлов
        /// </summary>
        public static string PathRcv
        {
            get
            {
                return Path.Combine(PathXML, "chk", EDDate);
            }
        }
        #endregion Paths

        #region GetPathFiles
        /// <summary>
        /// Возвращает файл в хранилище копий файлов, полученных из АРМ КБР за указанную дату
        /// </summary>
        /// <param name="fi">Имя файла</param>
        /// <param name="eddate">Дата в формате "YYYY-MM-DD"</param>
        /// <returns>Имя файла в папке хранилища</returns>
        public static FileInfo GetPathRcvFile(FileInfo fi, string eddate = null)
        {
            string path = IOChecks.CheckDirectory(Path.Combine(PathXML, "chk", eddate ?? EDDate));
            return new FileInfo(Path.Combine(path, fi.Name));
        }

        /// <summary>
        /// Возвращает имя для файла в хранилище читабельных копий файлов в формате XML, создавая, если его не было
        /// </summary>
        /// <param name="file">Имя файла</param>
        /// <param name="ext">Расширение файла</param>
        /// <returns>Имя файла в папке хранилища</returns>
        public static string GetPathInFile(string file, string ext = ".xml")
        {
            string path = IOChecks.CheckDirectory(PathIn);
            return Path.Combine(path, file + ext);
        }

        /// <summary>
        /// Возвращает файл в хранилище читабельных копий файлов в формате XML
        /// </summary>
        /// <param name="fi">Файл</param>
        /// <param name="ext">Расширение файла</param>
        /// <returns>Файл в папке хранилища</returns>
        public static FileInfo GetPathInFile(FileInfo fi, string ext = ".xml")
        {
            return new FileInfo(GetPathInFile(fi.Name, ext));
        }

        /// <summary>
        /// Возвращает имя файла в накопителе для передачи в ГИС ГМП (СМЭВ)
        /// </summary>
        /// <param name="file">Имя файла</param>
        /// <returns>Имя файла в папке хранилища</returns>
        public static string GetPathSmevFile(string file)
        {
            string path = IOChecks.CheckDirectory(Path.Combine(PathXML, "smev"));
            return Path.Combine(path, file);
        }

        /// <summary>
        /// Возвращает имя файла в общем накопителе для загрузки в АБС
        /// </summary>
        /// <param name="file">Имя файла</param>
        /// <returns>Имя файла в накопителе</returns>
        public static string GetPathABSFile(string file)
        {
            string path = IOChecks.CheckDirectory(PathABS);
            return Path.Combine(path, file);
        }

        /// <summary>
        /// Возвращает имя файла в подпапке накопителя для загрузки в АБС по номеру списка
        /// </summary>
        /// <param name="list">Номер списка</param>
        /// <returns>Имя файла в накопителе</returns>
        public static string GetPathABSFile(int list)
        {
            string format = Settings.FileABS;
            string file = Path.Combine(PathABS, string.Format(format, list, EDDate.Substring(5)));
            IOChecks.CheckFileDirectory(file);
            return file;
        }
        #endregion GetPathFiles

        //private static string m_XslTransform = Properties.Settings.Default.XSLTransform; 
        //http://astrasoft.su/AstraKBR.htm

        #region Masks
        /// <summary>
        /// Маски обрабатываемых исходных файлов на текущий день DD
        /// </summary>
        //private readonly static string[] AllMasks =
        //    {
        //    "ED???{0:dd}???.EDS", //Извещения и т.п. системы БЭСП
        //    "ED???{0:dd}???.SF",  //Извещения и т.п. системы передачи финансовых сообщений Банка России
        //    UICBank + "ED???{0:dd}???.SFD", //Информация Справочника пользователей системы передачи финансовых сообщений Банка России
        //    UICBank + "000000000000ED????{0:dd}?????.ED", //Отдельное ЭС или т.п.
        //    UICBank + "00000000PacketEID?{0:dd}?????.ED", //Пакет информационных ЭС
        //    UICBank + "00000000PacketEPD?{0:dd}?????.ED", //Пакет ЭПС
        //    UICBank + "0000000PacketESID?{0:dd}?????.ED"  //Пакет ЭСИС (электронных служебно - информационных сообщений)
        //    };
        #endregion Masks

        #region EPD
        /// <summary>
        /// Было ли пополнение списка платежных файлов
        /// </summary>
        private static bool NewPayments = false;

        /// <summary>
        /// Сумма баланса по выписке
        /// </summary>
        public static long CreditSum = 0L;

        /// <summary>
        /// Получена ли окончательная выписка
        /// </summary>
        public static bool CreditSumFinal = false;

        public struct RePacketEPD
        {
            /// <summary>
            /// Число документов в перепакованном пакете
            /// </summary>
            public int Num;

            /// <summary>
            /// Сумма документов в перепакованном пакете
            /// </summary>
            public long Sum;

            /// <summary>
            /// Файл перепакованного пакета в папке АБС
            /// </summary>
            public string File;
        }

        /// <summary>
        /// Общее число списков, включая 0-й
        /// </summary>
        public const int NumLists = 3;

        /// <summary>
        /// Перепакованный по спискам пакет ЭПС
        /// </summary>
        public static RePacketEPD[] Pack = new RePacketEPD[NumLists]; //0,1,2

        /// <summary>
        /// Сумма срочных документов (вне пакетов)
        /// </summary>
        public static long FastSum = 0L;

        /// <summary>
        /// Сумма пополнений на кассовый счет
        /// </summary>
        public static long CashSum = 0L;

        /// <summary>
        /// Сумма отложенных документов
        /// </summary>
        public static long ExtraSum = 0L;
        #endregion EPD


        /// <summary>
        /// Предварительная проверка прав доступа
        /// </summary>
        public static void PreCheck()
        {
            IOChecks.TestRights(PathChk, "Ошибка тестового удаления в папке исходных файлов");
            IOChecks.TestRights(PathXML, "Ошибка тестовой записи в хранилище");
            IOChecks.TestRights(PathABS, "Ошибка тестовой записи в папку АБС");
        }

        /// <summary>
        /// Забрать новые поступления из АРМ КБР и разложить их в хранилище исходных файлов по датам
        /// </summary>
        public static void PreProcessChk()
        {
            DirectoryInfo chkInfo = new DirectoryInfo(PathChk);
            foreach (FileInfo fi in chkInfo.GetFiles())
            {
                MoveChkEDDated(fi);
            }
        }

        /// <summary>
        /// Читать хранилище исходных файлов
        /// </summary>
        public static void ReadFiles()
        {
            if (!Directory.Exists(PathRcv)) //Выходные? - не было поступлений...
            {
                SetLastEDDate(DateTime.Now); //или лучше выход из программы?
            }

            //Обработка хранилища
            if (Program.Options.Force)
            {
                //Удаляем всю папку с XML, чтобы заставить все обновить начисто
                string path = PathIn;
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }

            DirectoryInfo rcvInfo = new DirectoryInfo(PathRcv);
            foreach (FileInfo fi in rcvInfo.GetFiles())
            {
                FileInfo xmlFi = GetPathInFile(fi); //+ ".xml" by default!

                if (!xmlFi.Exists)
                {
                    if (ExtractSenObject(fi, xmlFi))
                    {
                        ParseXml(xmlFi);
                    }
                    else
                    {
                        AppTrace.Error("{0} не создан", xmlFi.FullName);
                    }
                }
            }

            //Перепаковать платежи
            if (NewPayments || CreditSumFinal || Program.Options.Payments)
            {
                for (int i = 0; i < Pack.Length; i++)
                {
                    Pack[i].Num = 0;
                    Pack[i].Sum = 0L;
                    Pack[i].File = GetPathABSFile(i);
                }

                PacketEPD.Repack(rcvInfo);
                MergePaymentDocs();
            }
        }

        /// <summary>
        /// Перепаковать накопленные пакеты и отдельные документы ЭПС
        /// </summary>
        private static void MergePaymentDocs()
        {
            #region Статистика
            StringBuilder sp = new StringBuilder(128);
            int totalNum = 0;
            long totalSum = 0L;

            for (int i = 0; i < Pack.Length; i++)
            {
                if (Pack[i].Num > 0)
                {
                    sp.AppendFormat("({0}:{1}) {2}  ", i, Pack[i].Num, BaseConvert.FromKopeek(Pack[i].Sum));
                    totalNum += Pack[i].Num;
                    totalSum += Pack[i].Sum;
                }
            }

            if (totalNum > 0)
            {
                sp.AppendFormat("(={0}) {1}", totalNum, BaseConvert.FromKopeek(totalSum));
                if (ExtraSum > 0L)
                {
                    sp.Append("*");
                }
                AppTrace.Information(sp.ToString());
            }
            #endregion Статистика

            #region Конец дня
            if (CreditSumFinal || Program.Options.Force || Program.Options.Ignore)
            {
                StringBuilder sb = new StringBuilder(512);

                sb.AppendFormat("За дату {0}{1}\n\n", EDDate, (CreditSumFinal ? string.Empty : " предварительно"));
                sb.AppendFormat("{0,18} - приход по {1} выписке\n", BaseConvert.FromKopeek(CreditSum),
                    Program.Options.Ignore ? "последней(?)"
                    : CreditSumFinal ? "окончательной"
                    : "промежуточной");

                if (FastSum > 0L)
                {
                    sb.AppendFormat("{0,18} - приход в срочных документах\n", BaseConvert.FromKopeek(FastSum));
                }

                if (CashSum > 0L)
                {
                    sb.AppendFormat("{0,18} - приход в кассовых документах\n", BaseConvert.FromKopeek(CashSum));
                }

                sb.AppendFormat("{0,18} - приход в поступивших {1} док.:\n\n", BaseConvert.FromKopeek(totalSum), totalNum);
                for (int i = 0; i < Pack.Length; i++)
                {
                    if (Pack[i].Num > 0)
                    {
                        sb.AppendFormat("{0,18} -{1,4} док. - List{2}\n", BaseConvert.FromKopeek(Pack[i].Sum), Pack[i].Num, i);
                    }
                }
                sb.AppendLine();

                if (ExtraSum > 0L)
                {
                    sb.AppendFormat("{0,18} - отложено\n", BaseConvert.FromKopeek(ExtraSum));
                }

                long sum = CreditSum - FastSum - CashSum - ExtraSum - totalSum;
                if (sum != 0L && CreditSumFinal)
                {
                    sb.AppendFormat("{0,18} - недостача в пакетах.\n", BaseConvert.FromKopeek(sum));
                }

                string subj = CreditSumFinal ? (sum == 0L ? "Успешное" : "Странное") + " завершение дня " + EDDate
                    : "Предварительно за " + EDDate;

                if (AppTrace.ErrorCount > 0)
                {
                    sb.AppendLine("ОБРАТИТЕ ВНИМАНИЕ: ПРИ ВЫПОЛНЕНИИ ПРОГРАММЫ БЫЛИ ОШИБКИ!!!");
                    subj += "? (см. ОШИБКИ В ЛОГЕ!)";
                }
                else if (AppTrace.WarningCount > 0)
                {
                    sb.AppendLine("Обратите внимание: при выполнении программы были предупреждения!");
                    subj += "? (см. лог!)";
                }

                string msg = sb.ToString();
                AppTrace.Information(msg);

                Mailer.Send(Settings.Email, subj, msg);
            }
            #endregion Конец дня
        }

        /// <summary>
        /// Обработка каждого исходного файла из АРМ КБР
        /// </summary>
        /// <param name="fi">Файл для обработки</param>
        private static void MoveChkEDDated(FileInfo fi)
        {
            string eddate = GetEDDateFromFile(fi);
            if (!string.IsNullOrEmpty(eddate))
            {
                FileInfo rcvFi = GetPathRcvFile(fi, eddate); //в архив полученного as is в конвертах
                if (rcvFi.Exists)
                {
                    rcvFi.Delete();
                }

                AppTrace.Information("{0} > {1}", fi.Name, eddate);
                fi.MoveTo(rcvFi.FullName);
            }
        }

        /// <summary>
        /// Получение EDDate из объекта в sen:Object
        /// </summary>
        /// <param name="fi">Файл для обработки</param>
        /// <returns>Строка из атрибута EDDate</returns>
        private static string GetEDDateFromFile(FileInfo fi)
        {
            const int bufferSize = 1024;
            string eddate = null;
            try
            {
                using (FileStream fs = new FileStream(fi.FullName,
                    FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan))
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.IgnoreWhitespace = true;

                    XmlReader senreader = XmlReader.Create(fs, settings);
                    senreader.MoveToContent();

                    ////Конверт ЭЦП (КА)
                    //if (!senreader.LocalName.Equals("SigEnvelope"))
                    //{
                    //    throw new XmlException("В конверте нет SigEnvelope");
                    //}

                    //Контейнер для подписываемого объекта
                    if (!senreader.ReadToFollowing("sen:Object"))
                    {
                        throw new XmlException("В конверте нет sen:Object");
                    }

                    //Бинарные данные. Контейнер для подписываемого объекта
                    byte[] buffer = new byte[bufferSize];
                    if (senreader.ReadElementContentAsBase64(buffer, 0, bufferSize) == 0)
                    {
                        throw new XmlException("В конверте нет данных в sen:Object");
                    }

                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        XmlReader reader = XmlReader.Create(ms, settings);
                        reader.MoveToContent();

                        //Дата составления ЭС
                        eddate = reader.GetAttribute("EDDate");
                    }
                }
            }
            catch (XmlException ex)
            {
                AppTrace.Warning("{0} не XML файл: {1}", fi.FullName, ex.Message);
            }
            catch (Exception ex)
            {
                AppTrace.Error("{0} ошибка чтения: {1}", fi.FullName, ex.Message);
            }

            return eddate;
        }

        /// <summary>
        /// Извлекает sen:Object в отдельный файл XML
        /// </summary>
        /// <param name="fi">Исходный файл с sen:Object</param>
        /// <param name="xmlFi">Результирующий файл XML</param>
        /// <returns>Создан ли результирующий файл</returns>
        private static bool ExtractSenObject(FileInfo fi, FileInfo xmlFi)
        {
            const int bufferSize = 4096;
            try
            {
                using (FileStream fs = new FileStream(fi.FullName, 
                    FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan))
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.IgnoreWhitespace = true;

                    XmlReader senreader = XmlReader.Create(fs, settings);
                    senreader.MoveToContent();

                    //Контейнер для подписываемого объекта
                    senreader.ReadToFollowing("sen:Object");

                    //Бинарные данные. Контейнер для подписываемого объекта
                    byte[] buffer = new byte[bufferSize];
                    int readBytes = senreader.ReadElementContentAsBase64(buffer, 0, bufferSize);

                    using (FileStream writer = new FileStream(xmlFi.FullName, 
                        FileMode.Create, FileAccess.Write, FileShare.Write))
                    {
                        while (readBytes > 0)
                        {
                            writer.Write(buffer, 0, readBytes);
                            readBytes = senreader.ReadElementContentAsBase64(buffer, 0, bufferSize);
                        }
                        writer.Flush(true);
                        writer.Close();
                    }
                }
            }

            catch (XmlException ex)
            {
                AppTrace.Warning("{0} не XML файл: {1}", fi.FullName, ex.Message);
            }
            catch (Exception ex)
            {
                AppTrace.Error("{0} ошибка чтения: {1}", fi.FullName, ex.Message);
            }

            //Обновить состояние!
            xmlFi.Refresh();

            bool ok = false;
            try
            {
                ok = xmlFi.Length > 0;
            }
            catch (Exception ex)
            {
                //TODO: UnauthorizedAccessException
                AppTrace.Error("{0} ошибка доступа: {1}", fi.FullName, ex.Message);
            }
            return ok;
        }

        /// <summary>
        /// Обработка файла в хранилище XML
        /// </summary>
        /// <param name="fileFullName">Файл для обработки</param>
        private static void ParseXml(FileInfo fi)
        {
            string subscribers;

            XPathDocument packet = new XPathDocument(fi.FullName);
            XPathNavigator navigator = packet.CreateNavigator();
            try
            {
                navigator.MoveToFirstChild(); //Root -> Packet || ED
            }
            catch (XmlException ex)
            {
                AppTrace.Warning("{0} не XML файл: {1}", fi.FullName, ex.Message);
                return;
            }

            #region Пакет или ЭПД
            string node = navigator.LocalName;
            XmlReader ed = navigator.ReadSubtree();
            ed.Read();
            string edno = ed.GetAttribute("EDNo");
            AppTrace.Information("{0} > {1} #{2}", fi.Name, node, edno);

            switch (node)
            {
                //Центральный Банк Российской Федерации.
                //Унифицированные форматы электронных банковских сообщений.
                //Схемы с описанием прикладных частей электроных сообщений, используемых в расчетной сети Банка России.
                //http://www.cbr.ru/analytics/Formats

                //Платежные пакеты и отдельные сообщения
                case "PacketEPD": //КБР: КЦОИ: Пакет ЭПС
                                  //а ED101 вне пакета - БЭСП
                case "ED101": //КБР: КЦОИ: Платежное поручение

                case "ED103": //КБР: КЦОИ: Платежное требование
                case "ED104": //КБР: КЦОИ: Инкассовое поручение
                case "ED105": //КБР: КЦОИ: Платежный ордер

                case "ED107": //КБР: КЦОИ: Поручение банка
                case "ED108": //КБР: КЦОИ: Платежное поручение на общую сумму с реестром
                case "ED109": //КБР: КЦОИ: Банковский ордер //2017.4.2

                case "ED110": //КБР: КЦОИ: ЭПС сокращенного формата
                case "ED111": //КБР: КЦОИ: Мемориальный ордер в электронном виде
                    if (!NewPayments)
                    {
                        if (AppConfig.IsSet(node, out subscribers))
                        {
                            string msg = string.Format("Получен {0} #{1}", node, edno);
                            Mailer.Send(subscribers, msg);
                        }
                        NewPayments = true;
                    }
                    return; //Платежный файл обрабатываем отдельно

                //Прочие пакеты
                case "PacketEID":  //КБР: КЦОИ: Пакет информационных ЭС для информационного обмена между Участниками
                case "PacketESID": //КБР: КЦОИ: Пакет ЭСИС
                case "PacketCash": //КБР: КЦОИ: Пакет ЭС для операции с наличными деньгами //2017.1
                case "PacketCtrl": //КБР: КЦОИ: Пакет управляющих ЭС //2017.4.2
                    navigator.MoveToFirstChild(); //Packet -> SigValue || ED || InitialPacketED || Session
                    break;

                //Прочие отдельные информационные ЭС (разбор ниже)
                default:
                    break;
            }
            #endregion Пакет или ЭПД

            #region ЭС
            string smevFile = GetPathSmevFile(fi.Name);  //для ГИС ГМП через СМЭВ
            string absFile = GetPathABSFile(fi.Name);    //для АБС банка

            fi.CopyTo(absFile, true); //копия в общий "поток сознания" для АБС

            //Разбор прочих отдельных информационных ЭС в пакетах
            do
            {
                node = navigator.LocalName;
                ed = navigator.ReadSubtree();
                ed.Read();
                edno = ed.GetAttribute("EDNo");
                bool defaultMail = true;

                switch (node)
                {
                    //Центральный Банк Российской Федерации.
                    //Унифицированные форматы электронных банковских сообщений.
                    //Схемы с описанием прикладных частей электроных сообщений, используемых в расчетной сети Банка России.
                    //http://www.cbr.ru/analytics/Formats

                    case "SigValue": //Бинарные данные. Значение ЗК
                        break;

                    case "InitialPacketED": //Идентификаторы исходного ЭС. Заполняются при формировании ответа на ЭС
                        fi.CopyTo(smevFile, true);
                        break;

                    case "Session": //Тип и номер рейса в котором произошло исполнение распоряжения (или время исполнения) (если распоряжение исполнено в рейсе)
                        break;

                    case "ED113": //КБР: КЦОИ: Выставляемое на оплату платежное требование //PacketEID
                    case "ED114": //КБР: КЦОИ: Выставляемое на оплату инкассовое поручение //PacketEID

                    case "ED201": //КБР: КЦОИ: ЦОС: СЭД: АСЭКР: НСПК: Извещение о результатах контроля ЭС (пакета ЭС)
                    case "ED202": //КБР: КЦОИ: Запрос по ЭПС (пакету ЭПС)
                    case "ED203": //КБР: КЦОИ: Запрос по группе ЭПС
                    case "ED204": //КБР: КЦОИ: Запрос об отзыве ЭПС (пакета ЭПС)
                    case "ED205": //КБР: КЦОИ: Извещение о состоянии ЭПС (пакета ЭПС)
                    case "ED206": //КБР: КЦОИ: Подтверждение дебета/кредита
                    case "ED207": //КБР: КЦОИ: Извещение о группе ЭПС
                    case "ED208": //КБР: КЦОИ: Информация о состоянии ЭС
                    case "ED209": //КБР: КЦОИ: Извещение о режиме обмена/работы счета
                    case "ED210": //КБР: КЦОИ: Запрос выписки из лицевого счета
                        break;

                    case "ED211": //КБР: КЦОИ: Выписка из лицевого счета
                        #region ED211
                        {
                            fi.CopyTo(smevFile, true);

                            //Номер счета, по которому формируется ЭСИС
                            string ks = ed.GetAttribute("Acc");
                            if (ks.Equals(Settings.KS)) //наш корсчет
                            {
                                //Общая сумма документов по кредиту счета участников (если больше нуля). Не заполняется для выписки, содержащей текущий остаток на счете
                                string creditSum = ed.GetAttribute("CreditSum");
                                long crSum = 0L;
                                if (long.TryParse(creditSum, out crSum))
                                {
                                    if (crSum > CreditSum)
                                    {
                                        CreditSum = crSum;
                                    }
                                }
                                //Тип выписки
                                string kind = ed.GetAttribute("AbstractKind");
                                if (kind.Equals("0")) //окончательная выписка
                                {
                                    CreditSumFinal = true;
                                    AppTrace.Information("Окончательная выписка получена #{0}", edno);
                                }

                                if (AppConfig.IsSet(node, out subscribers))
                                {
                                    string repName = string.Format("{0}-{1}-{2}", node, EDDate, edno);
                                    string repFile = GetPathInFile(repName, ".txt");
                                    XSLT2File(fi.FullName, repFile);

                                    defaultMail = false;
                                    string subj = string.Format("{0} выписка за {1} #{2}", (CreditSumFinal ? "Окончательная" : "Промежуточная"), EDDate, edno);
                                    if (File.Exists(repFile))
                                    {
                                        Mailer.Send(subscribers, subj,
                                            "Файл " + node + " за " + EDDate + " прилагается.", repFile);
                                    }
                                    else
                                    {
                                        Mailer.Send(subscribers, subj,
                                            "Файл приложить не удалось - смотрите лог!");
                                    }
                                }
                            }
                        }
                        break;
                        #endregion ED211

                    case "ED213": //КБР: Запрос акцепта
                    case "ED214": //КБР: КЦОИ: Извещение об акцепте
                    case "ED215": //КБР: КЦОИ: ЭСИС с копией полей ЭПС

                    case "ED217": //КБР: КЦОИ: Извещение о задолженности по внутридневному кредиту
                    case "ED218": //КБР: КЦОИ: Запрос выходной формы
                        break;

                    case "ED219": //КБР: КЦОИ: Выходная форма
                        #region ED219
                        {
                            //Идентификатор (номер) формы. Заполняется при наличии
                            string reportID = ed.GetAttribute("ReportID");
                            switch (reportID)
                            {
                                case "0401103": //final confirmation
                                    //NewPayments = true;
                                    break;

                                case "0401317": //daily costs
                                    if (AppConfig.IsSet(reportID, out subscribers))
                                    {
                                        //Дата, за которую запрашивается информация
                                        string repDate = ed.GetAttribute("ReportDate");
                                        string repName = string.Format("{0}-{1}-{2}", node, EDDate, edno);
                                        string repFile = GetPathInFile(repName, ".txt");

                                        //Текст: содержание формы
                                        ed.ReadToFollowing("ReportContent");
                                        string report = ed.ReadInnerXml();
                                        if (!report.Contains(Environment.NewLine))
                                        {
                                            report = report.Replace("\n", Environment.NewLine);
                                        }
                                        File.WriteAllText(repFile, report, Encoding.GetEncoding(1251));

                                        defaultMail = false;
                                        string subj = string.Format("Ведомость услуг ЦБ за {0} #{1}", EDDate, edno);
                                        if (File.Exists(repFile))
                                        {
                                            Mailer.Send(subscribers, subj,
                                                "Файл " + node + " за один день " + repDate + " прилагается.", repFile);
                                            //File.Delete(repFile); //don't delete because this file is still sending!
                                        }
                                        else
                                        {
                                            Mailer.Send(subscribers, subj,
                                                "Файл приложить не удалось - смотрите лог!");
                                        }
                                    }
                                    break;

                                case "0401318": //monthly costs
                                    if (AppConfig.IsSet(reportID, out subscribers))
                                    {
                                        //Дата, за которую запрашивается информация
                                        string repDate = ed.GetAttribute("ReportDate").Substring(0, 7);
                                        string repName = string.Format("{0}-{1}-{2}", node, EDDate, edno);
                                        string repFile = GetPathInFile(repName, ".txt");

                                        //Текст: содержание формы
                                        ed.ReadToFollowing("ReportContent");
                                        string report = ed.ReadInnerXml();
                                        if (!report.Contains(Environment.NewLine))
                                        {
                                            report = report.Replace("\n", Environment.NewLine);
                                        }
                                        File.WriteAllText(repFile, report, Encoding.GetEncoding(1251));

                                        defaultMail = false;
                                        string subj = string.Format("Ведомость услуг ЦБ за {0} #{1}", EDDate, edno);
                                        if (File.Exists(repFile))
                                        {
                                            Mailer.Send(subscribers, subj,
                                                "Файл " + node + " за весь месяц " + repDate + " прилагается.", repFile);
                                            //File.Delete(repFile); //don't delete because this file is still sending!
                                        }
                                        else
                                        {
                                            Mailer.Send(subscribers, subj,
                                                "Файл приложить не удалось - смотрите лог!");
                                        }
                                    }
                                    break;

                                default:
                                    string msg2 = string.Format("{0} содержит неизвестный ReportID {1} в ED219 #{2}.", fi.FullName, node, edno);
                                    AppTrace.Warning(msg2);
                                    defaultMail = false;
                                    Mailer.Send(Settings.Email, "Получен неизвестный ReportID " + node, msg2);
                                    break;
                            }
                        }
                        break;
                        #endregion ED219

                    case "ED220": //КБР: КЦОИ: Запрос на получение ответного документооборота
                    case "ED221": //КБР: КЦОИ: Отчет об операциях по счету для выверки документов дня участников
                        break;

                    case "ED222": //КБР: КЦОИ: Извещение о дебете/кредите для кассовых документов
                        #region ED222
                        //Money code moved to PacketEPD.Repack()
                        //{
                        //    XmlReader ed = navigator.ReadSubtree();
                        //    ed.Read();

                        //    //Признак дебета/кредита по отношению к лицевому счету кассы
                        //    string cashDC = ed.GetAttribute("CashDC");

                        //    //Общая сумма документа
                        //    long sum = long.Parse(ed.GetAttribute("Sum"));

                        //    CashSum += cashDC.Equals("1") ? sum : -sum;
                        //}
                        break;
                        #endregion ED222

                    case "ED230": //НСПК: Реестр клиринговых позиций
                    case "ED231": //НСПК: Реестр результатов обработки клиринговых позиций

                    //2017.4.2
                    case "ED234": //Извещение о задолженности по внутридневному кредиту
                    case "ED238": //Извещение об операциях по счету

                    case "ED240": //КБР: КЦОИ: Запрос информации о переданных/полученных ЭС
                    case "ED241": //КБР: КЦОИ: Информация о переданных/полученных ЭС
                    case "ED242": //КБР: КЦОИ: Запрос на повторное получение сообщения
                    case "ED243": //КБР: КЦОИ: Запрос на получение информации по ЭПС участника
                    case "ED244": //КБР: КЦОИ: Ответ на запрос участника
                    case "ED245": //КБР: КЦОИ: Отчет для выверки информационных ЭСИС

                    //case "ED273": //КБР: КЦОИ: Сообщение для передачи выставляемых на оплату инкассовых поручений и выставляемых на оплату платежных требований, предъявленных получателем средств
                    case "ED274": //КБР: КЦОИ: Уведомление о результатах приема к исполнению выставляемого на оплату инкассового поручения, выставляемого на оплату платежного требования
                    case "ED275": //КБР: КЦОИ: Запрос на отзыв выставленного на оплату платежного требования / инкассового поручения
                    case "ED276": //КБР: КЦОИ: Уведомление о результатах отзыва выставленного на оплату платежного требования / инкассового поручения
                    case "ED277": //Извещение о привлечении Банком России денежных средств в депозит овернайт

                    //2017.1
                    case "ED280": //КБР: КЦОИ: Извещение о получении ЭС
                    case "ED283": //КБР: КЦОИ: Заявка на получение/сдачу наличных денег Банка России, заявка на выдачу/прием наличных денег Банка России в соответствии с предоставленным Банком России разрешением
                    case "ED284": //КБР: КЦОИ: Разрешение на совершение операций с наличными деньгами Банка России
                    case "ED285": //КБР: КЦОИ: Сообщение о проведенной операции с наличными деньгами Банка России

                    case "ED301": //КЦОИ: Распоряжение для управления ликвидностью

                    case "ED306": //КЦОИ: Подтверждение исполнения распоряжения для управления позицией ликвидности

                    case "ED318": //КБР: КЦОИ: Распоряжение для управления реквизитами получателя
                    case "ED319": //КБР: КЦОИ: Подтверждение исполнения распоряжения для управления реквизитами получателя

                    case "ED330": //КБР: КЦОИ: Информация о корректировке временного регламента функционирования ЦОиР БЭСП, расчетной системы ТУ
                    //case "ED331": //Запрос оперативной информации о состоянии ликвидности ПУР в Банке России
                    //case "ED332": //Оперативная информация о состоянии ликвидности ПУР в Банке России
                    case "ED333": //КБР: КЦОИ: Извещение о выполнении регламента системы БЭСП

                    case "ED373": //КБР: КЦОИ: Запрос информации об участниках расчетов в системе БЭСП
                    case "ED374": //КБР: КЦОИ: Информация об участниках системы БЭСП
                    //case "ED375": //Извещение об отмене установленного лимита ПУР в системе БЭСП

                    //case "ED378": //Распоряжение на установку и изменение лимита в системе БЭСП
                    //case "ED379": //Подтверждение распоряжения на установку и изменение лимита в системе БЭСП
                    //case "ED380": //Запрос о лимитах в системе БЭСП
                    //case "ED381": //Информация о лимитах в системе БЭСП
                    //case "ED382": //Распоряжение на изменение приоритета платежа
                    //case "ED383": //Распоряжение на изменение порядка расположения платежа БЭСП

                    //case "ED385": //Подтверждение обработки распоряжений на управление очередью платежей БЭСП

                    case "ED393": //Распоряжение для изменения признака основного канала взаимодействия

                    case "ED408": //СЭД: Информация о результатах приема/обработки ЭС

                    case "ED421": //СЭД: Заявка на участие в кредитном аукционе Банка России / Заявление на получение кредита Банка России по фиксированной процентной ставке
                    case "ED422": //СЭД: Встречная заявка/заявление Банка России на предоставление кредита Банка России

                    //2017.2.1
                    case "ED425": //СЭД: Сведения о ранее направленной заявке

                    case "ED428": //СЭД: Уведомление о досрочном погашении обязательств по кредиту Банка России
                    case "ED429": //СЭД: Заявление на перевод ценных бумаг из раздела "Блокировано Банком России" в Основной раздел или другой раздел "Блокировано Банком России" счета депо кредитной организации

                    case "ED431": //СЭД: Заявка на участие в депозитном аукционе Банка России / на размещение депозита в Банке России по фиксированной процентной ставке
                    case "ED432": //СЭД: Встречная заявка Банка России на привлечение депозита
                    case "ED433": //СЭД: Заявка  на востребование депозита до востребования/ на досрочный возврат депозита, размещенного в Банке России на определенный срок
                    case "ED434": //СЭД: Встречная заявка Банка России на возврат депозита, размещенного до востребования/ на досрочный возврат депозита, размещенного в Банке России на определенный срок
                    case "ED435": //СЭД: Обращение о снятии заявки, направленной на депозитный аукцион/ на размещение депозита в Банке России по фиксированной процентной ставке

                    //2017.3.2
                    //case "ED460": //--АСЭКР: Электронная опись упаковок
                    //case "ED461": //--АСЭКР: Уведомление (Дополнительная информация о состоянии объекта данных)
                    case "ED462": //АСЭКР: Заявка на получение или сдачу денежной наличности
                    case "ED463": //АСЭКР: Извещение о результатах промежуточного контроля ЭС (пакета ЭС)
                    case "ED464": //АСЭКР: Запрос об отзыве/аннулировании ЭСИС (пакета ЭСИС)
                    case "ED465": //АСЭКР: Извещение о состоянии ЭПС (пакета ЭПС)

                    case "ED499": //СЭД: АСЭКР: Запрос-зонд

                    case "ED501": //ЦОС: Конверт для ЭС в собственных форматах участников электронного обмена
                        
                    case "ED503": //ЦОС: Конверт для передачи финансовых сообщений

                    case "ED508": //ЦОС: Информация о состоянии ЭС

                    case "ED510": //ЦОС: Информация об услугах, предоставленных Банком России пользователю СПФС
                    case "ED511": //ЦОС: Счет за услуги, предоставленные Банком России пользователю СПФС
                    case "ED512": //ЦОС: Распоряжение для управления реквизитами пользователей системы передачи финансовых сообщений Банка России

                    case "ED530": //ЦОС: Информация о корректировке временного регламента функционирования ЦОС

                    case "ED540": //ЦОС: Запрос информации о переданных/полученных ЭС
                    case "ED541": //ЦОС: Информация о переданных/полученных ЭС
                    case "ED542": //ЦОС: Запрос на повторное получение сообщения

                    case "ED573": //ЦОС: Запрос Справочника пользователей системы передачи финансовых сообщений Банка России
                    case "ED574": //ЦОС: Информация Справочника пользователей системы передачи финансовых сообщений Банка России

                    case "ED599": //ЦОС: Запрос-зонд

                    //2017.4.2
                    case "ED801": //Запрос информации о ликвидности (полной или краткой)
                    case "ED802": //Информация о ликвидности

                    case "ED804": //Запрос перечня платежных распоряжений, помещенных в очереди
                    case "ED805": //Перечень ЭПС, помещенных в очереди
                    case "ED806": //Запрос изменений в справочнике БИК
                    case "ED807": //Изменения, внесенные в Справочник БИК (Полный Справочник БИК)
                    case "ED808": //Профиль участника
                    case "ED809": //Уведомление об изменении графика работы
                    case "ED810": //Распоряжение для управления ликвидностью
                    case "ED811": //Подтверждение исполнения распоряжения для управления ликвидностью

                    case "ED813": //Распоряжение об изменении порядка расположения распоряжения во внутридневной очереди
                    case "ED814": //Подтверждение обработки распоряжений на управление внутридневной очередью

                    case "ED816": //Сообщение о подтверждении первоначального условного распоряжения

                    case "ED818": //Выходная форма Ведомость предоставленных Банком России услуг в платежной системе Банка России, cчет за предоставленные Банком России услуги в платежной системе Банка России
                    case "ED819": //Уведомление Участников
                    case "ED820": //Уведомление о приеме к исполнению несрочного управляющего ЭС (информационного запроса)
                    case "ED821": //Уведомление об аннулировании управляющего ЭС

                    case "ED823": //Конверт для передачи ЭС из РАБИС-НП участнику

                    case "ED999": //КБР: КЦОИ: Запрос-зонд
                        break;

                    default:
                        string msg = string.Format("{0} содержит неизвестный документ {1} #{2}.", fi.FullName, node, edno);
                        AppTrace.Warning(msg);
                        defaultMail = false;
                        Mailer.Send(Settings.Email, string.Format("Получен неизвестный {0} #{1}", node, edno), msg);
                        break;
                }

                if (AppConfig.IsSet(node, out subscribers) && defaultMail)
                {
                    string msg = string.Format("Получен {0} #{1}", node, edno);
                    Mailer.Send(subscribers, msg);
                }
            }
            while (navigator.MoveToNext());
            #endregion ЭС
        }

        //private static string NameForPrint(string name)
        //{
        //    if (name.StartsWith(Bank.UICBank))
        //    {
        //        string p1 = name.Substring(10, 17);
        //        string p2 = name.Substring(27, 1);
        //        string p3 = name.Substring(28, 2);
        //        string p4 = name.Substring(30, 2);
        //        string p5 = name.Substring(32, 3);
        //        while (p1.StartsWith("0"))
        //        {
        //            p1 = p1.Remove(0, 1);
        //        }
        //        return string.Format("{0,12} {1} {2} {3} {4}", p1, p2, p3, p4, p5);
        //    }
        //    else
        //    {
        //        string p1 = name.Substring(0, 5);
        //        string p2 = name.Substring(5, 2);
        //        string p3 = name.Substring(7, 3);
        //        return string.Format("{0} {1} {2}", p1, p2, p3);
        //    }
        //}

        private static void XSLT2File(string xmlFile, string txtFile)
        {
            //string xsltPath = Settings.XSLT;
            //string xsltFile = Path.Combine(xsltPath, "MCI_UFEBS.xslt");
            string xsltFile = "MCI_UFEBS.xslt";

            if (!File.Exists(xsltFile))
            {
                AppTrace.Warning("No file " + xsltFile + " found for XSLTransform!");
                return;
            }

            XPathDocument xpdoc = new XPathDocument(xmlFile);
            XPathNavigator nav = xpdoc.CreateNavigator();

            XsltSettings settings = new XsltSettings();
            settings.EnableScript = true;

            XsltArgumentList arguments = new XsltArgumentList();
            arguments.AddParam("InfoBANK", string.Empty, Settings.Bank);
            arguments.AddParam("InfoBIC", string.Empty, Settings.BIC);
            arguments.AddParam("InfoRKC", string.Empty, Settings.RKC);

            //XmlUrlResolver resolver = new XmlUrlResolver(); //TODO: include xslt
            //resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;

            XslCompiledTransform xslt = new XslCompiledTransform();
            //xslt.Load(xsltFile, settings, resolver);
            xslt.Load(xsltFile, settings, new XmlUrlResolver());

            string outText = string.Empty;
            using (StringWriter output = new StringWriter())
            {
                try
                {
                    xslt.Transform(nav, arguments, output);
                    outText = output.ToString();
                }
                catch (Exception ex) //возникает при ошибке трансформации
                {
                    AppTrace.Warning(ex.Message);
                }
            }

            if (outText.Length > 0)
            {
                try
                {
                    AppTrace.Verbose("Экспорт {0}", txtFile);
                    File.WriteAllText(txtFile, outText, Encoding.GetEncoding(1251));
                }
                catch (Exception ex) //возникает при затирании файла, когда с тем же именем еще не отправлен (исправлено добавлением уникального EDNo)
                {
                    //Процесс не может получить доступ к файлу "...\ED211-2016-12-29.txt", так как этот файл используется другим процессом.)
                    AppTrace.Warning(ex.Message);
                }
            }
        }
    }
}
