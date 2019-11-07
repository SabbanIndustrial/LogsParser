using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogsParser
{
    public partial class Form1 : Form
    {
        List<string>  filePaths;
        LogContextParser context;
        public Form1()
        {
            context = new LogContextParser();
            filePaths = new List<string>();
            InitializeComponent();
        }



        private void LoadFileButtonClick(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    foreach (var filePath in ofd.FileNames)
                    {
                        filePaths.Add(filePath);
                        fileNamesListBox.Items.Add(Path.GetFileNameWithoutExtension(filePath));

                    }
                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }

        }
        private void AddToContextButtonClick(object sender, EventArgs e)
        {
            parsingFilesCount = 0;
            string targetTime = richTextBox2.Text;
            string targetDate = richTextBox4.Text;
            foreach (var filePath in filePaths)
            {
                string date = Path.GetFileNameWithoutExtension(filePath).Substring(Path.GetFileNameWithoutExtension(filePath).Length - 8);
                string source = Path.GetFileNameWithoutExtension(filePath).Substring(0, Path.GetFileNameWithoutExtension(filePath).Length - 8);
                if (date == targetDate)
                {
                    int hours = int.Parse(targetTime.Substring(0, 2));
                    int mins = int.Parse(targetTime.Substring(3));
                    var temp = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture);
                    string filesDirectory = Path.GetDirectoryName(filePath);
                    ParseFile(filePath, targetTime);
                    if (hours == 23 && mins > 40)
                    {//парсим следущий файл
                        string nextFile = Path.Combine(filesDirectory, source + temp.AddDays(1).ToString("yyyyMMdd")) + ".txt";
                        if (filePaths.Contains(Path.GetFileNameWithoutExtension(nextFile)))
                        {
                            ParseFile(nextFile, "00:00");
                        }
                    }
                    if (hours == 00 && mins < 20)
                    {//парсим предыдущий файл
                        string prevFile = Path.Combine(filesDirectory, source + temp.AddDays(-1).ToString("yyyyMMdd")) + ".txt";
                        if (fileNamesListBox.Items.Contains(Path.GetFileNameWithoutExtension(prevFile)))
                        {
                            ParseFile(prevFile, "23:59");
                        }
                    }


                    string stop = "228";

                }

            }

        }
        int parsingFilesCount;
        private void ParseFile(string filePath, string targetTime)
        {
            parsingFilesCount++;
            label10.Text = "Парсится файлов: " + parsingFilesCount;
            ThreadStart starter = delegate { ParseFileTimePeriod(filePath, targetTime); };
            Thread t = new Thread(starter);
            t.Start();
            //NewMethod(item, targetTime);
        }


        private long FindStartWith(StreamReader sr, string value)
        {
            while (!sr.EndOfStream)
            {
                string readString = sr.ReadLine();
                if (readString.StartsWith(value))
                {
                    return sr.BaseStream.Position;
                }
            }
            return 0;
        }
        private int TimeToInt(string time)
        {
            int hours = int.Parse(time.Substring(0, 2));
            int mins = int.Parse(time.Substring(3));
            int convSearchedValue = hours * 60 + mins;
            return convSearchedValue;
        }
        private string TimeToString(int time)
        {
            if (time < 0)
            {
                time += 1439;
            }
            return $"{(time / 60).ToString("D2")}:{(time % 60).ToString("D2")}";
        }



        private void ParseFileTimePeriod(string filePath, string targetTime)
        {
            var sr = new StreamReader(filePath);
            string timeStart = TimeToString(Math.Max(TimeToInt(targetTime) - (int)numericUpDown1.Value, 0));
            string timeEnd = TimeToString(Math.Min(TimeToInt(targetTime) + (int)numericUpDown1.Value, 1339));
            long streamPointer = FindStartWith(sr, timeStart);


            sr.BaseStream.Position = streamPointer;
            List<string> scp = new List<string>();
            while (!sr.EndOfStream)
            {
                string tempRead = sr.ReadLine();
                scp.Add(tempRead);

                if (tempRead.StartsWith(timeEnd))
                {
                    break;
                }
            }
            int ddd = 0;

            string toParse = scp.Aggregate((a, b) => a + "\n" + b);

            LogDocument doc = new LogDocument(toParse, Path.GetFileNameWithoutExtension(filePath));
            doc.ProgressUpdated += Doc_ProgressUpdated;
            context.AddDocument(doc);
            sr.Dispose();
        }

        private void Doc_ProgressUpdated(LogDocument obj)
        {
            double progress = 0;
            foreach (var item in context.Documents)
            {
                progress += item.ParseProgress;
            }
            progress = progress / parsingFilesCount;
            progressBar1.BeginInvoke((MethodInvoker)(() => progressBar1.Value = (int)progress));
        }

        private void SearchButtonClick(object sender, EventArgs e)
        {
            keyValuesListBox.Items.Clear();
            int blocks = 0;
            foreach (var item in context.Documents)
            {
                blocks += item.BlocksCount;
            }
            label7.Text = "Общее кол-во блоков: " + blocks;
            resultTextBox.Text = "";
            blockSummaryTextBox.Text = "";
            List<LogBlock> blockChain = context.CreateBlockChain(rrnTextBox.Text);
            List<string> operIds = new List<string>();
            List<string> orderIds = new List<string>();
            List<string> IdPlatKlients = new List<string>();
            string idSiteUser = "";
            foreach (var item in blockChain)
            {
                resultTextBox.Text += item.Time.ToString()+"."+item.Time.Millisecond + " =>\n" + item.Text + "\n";

                blockSummaryTextBox.Text += item.Action.Replace("\n","") + "\n";
                blockSummaryTextBox.Text += item.Time + "\n";
                if (item.OperId != "")
                {
                    operIds.Add("oper_id: " + item.OperId);
                }
                if (item.OrderId != "")
                {
                    orderIds.Add("order_id: " + item.OrderId);
                }
                if (item.ID_Plat_Klienta != "")
                {
                    IdPlatKlients.Add("id_plat_k: " + item.ID_Plat_Klienta);
                }
                if (item.IdSiteUser != "")
                {
                    idSiteUser = "id_site_user: " + item.IdSiteUser;
                }
            }
            keyValuesListBox.Items.AddRange(operIds.Distinct().ToArray());
            keyValuesListBox.Items.AddRange(orderIds.Distinct().ToArray());
            keyValuesListBox.Items.AddRange(IdPlatKlients.Distinct().ToArray());
            keyValuesListBox.Items.Add(idSiteUser);
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void UnloadAllButtonClick(object sender, EventArgs e)
        {
            filePaths.Clear();
            fileNamesListBox.Items.Clear();
        }

        private void FileNamesListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (fileNamesListBox.SelectedItem != null)
            {
                try
                {
                    string selectedString = fileNamesListBox.SelectedItem.ToString();
                    richTextBox4.Text = selectedString.Substring(selectedString.Length - 8);
                    label2.Text = "Количество блоков: " + context.Documents.Where(d => d.LogFileName == selectedString).First().BlocksCount;

                }
                catch { label2.Text = "Ошибка"; }
            }
        }


    }
}
