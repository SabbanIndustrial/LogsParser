using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LogsParser
{
    class LogDocument
    {
        private string documentText;
        private List<LogBlock> documentBlocks;

        public DateTime LogDate { get; private set; }
        public string LogFileName { get; private set; }
        public string LogSource { get; private set; }
        public double ParseProgress { get; private set; }

        public int BlocksCount
        {
            get
            {
                return documentBlocks.Count;
            }
        }
        public List<LogBlock> Blocks
        {
            get
            {
                return documentBlocks;
            }
        }

        public LogDocument(string documentText, string documentName)
        {
            this.documentText = documentText;
            string date = documentName.Substring(documentName.Length - 8);
            LogSource = documentName.Substring(0, documentName.Length - 8);
            LogFileName = documentName;
            LogDate = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture);
            documentBlocks = new List<LogBlock>();
            ParseProgress = 0;



        }
        public event Action<LogDocument> ProgressUpdated;
        public void ParseDocument()
        {
            List<string> documentBlocksStrings = ParseBlocks(documentText);

            foreach (var item in documentBlocksStrings)
            {
                documentBlocks.Add(new LogBlock(item, LogDate));
                ParseProgress = documentBlocks.Count / (double)documentBlocksStrings.Count * 100;
                if (ParseProgress % 5 == 0)
                {
                    ProgressUpdated(this);
                }
            }
            string stop = "228";
        }

        private List<string> ParseBlocks(string text)
        {
            ProgressUpdated(this);

            text += "\n99:99:99.00000000 5d5b0cf277943->";
            List<string> toReturn = new List<string>();
            Regex regex = new Regex("(\\d{2}:\\d{2}:\\d{2}.\\d{8} \\w{13}->)(.|\\n)*?(?=\\d{2}:\\d{2}:\\d{2}.\\d{8} \\w{13}->)",RegexOptions.Compiled);
            MatchCollection matches = regex.Matches(text);
            foreach (var item in matches)
            {
                toReturn.Add(item.ToString());
            }
            return toReturn;
        }

    }

    public struct LogBlock : IComparable<LogBlock>
    {
        public LogBlock(string blockText, DateTime logDate) : this()
        {
            Text = blockText;
            Time = logDate;
            ParseBlock();
        }

        private void ParseBlock()
        {
            Regex regex = new Regex("(\\d{2}:\\d{2}:\\d{2}.\\d{8} \\w{13})");

            string[] blockInfo = regex.Match(Text).ToString().Split(' ');
            TimeSpan temp = DateTime.ParseExact(blockInfo[0].Substring(0, 15), "HH:mm:ss.ffffff", CultureInfo.InvariantCulture) - DateTime.Today;
            Time += temp;
            Guid = blockInfo[1];
            Text = Text.Substring(34);
            RRN = "";
            ID_Plat_Klienta = "";
            OrderId = "";
            OperId = "";
            Action = "";
            IdSiteUser = "";
            ParseParameters();
            regex = new Regex("(?<=\\W)(\\d{12})(?=\\W)");

            if (regex.Match(Text).Success)
            {
                RRN = regex.Match(Text).ToString();
            }

            if (Text.Length < 50)
            {
                Action = Text;
            }
            if (Text.Contains("DATA JSON->"))
            {//Response from Bank
                Text = Text.Substring(0, 20);
                Action = "DATA JSON(XML)";
            }
            if (Text.Contains("PaRes") || Text.Contains("PARES") || Text.Contains("paRes"))
            {//Response from Bank
                Text = Text.Substring(0, 20);
                Action = "Запрос банку";
            }
            if (Text.Contains("Response from Bank"))
            {
                Action = "Ответ банка";
            }
            if (Text.Contains("Распарсенный ответ"))
            {
                Action = "Распарсенный ответ банка";
            }
        }

        public void ParseParameters()
        {
            Regex regex = new Regex("(\\[\\S{3,16}\\] => )(\\S{0,16})");
            foreach (var item in regex.Matches(Text))
            {
                if (item.ToString().Contains("[oper_id]"))
                {
                    OperId = item.ToString().Split('>')[1].Trim();
                }
                if (item.ToString().Contains("[action]"))
                {
                    Action = item.ToString().Split('>')[1].Trim();
                }
                if (item.ToString().Contains("[order_id]"))
                {
                    OrderId = item.ToString().Split('>')[1].Trim();
                }
                if (item.ToString().Contains("[ORDER]") || item.ToString().Contains("[payment_id]"))
                {
                    ID_Plat_Klienta = item.ToString().Split('>')[1].Trim();
                }
                //if (item.ToString().Contains("[RRN]"))
                //{
                //    RRN = item.ToString().Split('>')[1].Trim();
                //}
            }
            if (Text.Contains("Получен ID операции-> "))
            {
                OperId = Text.Replace("Получен ID операции-> ", "");
                OperId = OperId.Replace("\n", "");
            }
            if (Text.Contains('\''))
            {
                regex = new Regex("(\\'\\S{3,16}\' => )(\\S{0,16})");
                foreach (var item in regex.Matches(Text))
                {
                    if (item.ToString().Contains("'ID_PLAT_KLIENT'"))
                    {
                        ID_Plat_Klienta = item.ToString().Split('>')[1].Trim().Replace(",", "").Replace("'", "");
                    }
                    if (item.ToString().Contains("'ID_SITE_USER'"))
                    {
                        IdSiteUser = item.ToString().Split('>')[1].Trim().Replace(",", "").Replace("'", "");
                        IdSiteUser = IdSiteUser.Replace("NULL", "");
                    }
                }
            }


        }


        public int CompareTo(LogBlock other)
        {
            if (other.Text == Text && other.Time == Time)
            {
                return 0;
            }
            return Time.CompareTo(other.Time);
        }
        public string IdSiteUser { get; set; }
        public string ID_Plat_Klienta { get; set; }
        public string OrderId { get; set; }//внутренний id
        public string OperId { get; set; }//внутренний id
        public string RRN { get; set; }
        public string Text { get; set; }
        public string Guid { get; set; }
        public string Action { get; set; }
        public DateTime Time { get; set; }
    }


}
