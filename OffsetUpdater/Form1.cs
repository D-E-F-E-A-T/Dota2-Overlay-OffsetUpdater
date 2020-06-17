using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OffsetUpdater
{
    public partial class Form1 : Form
    {
        //Config Directory
        public string confPath = Environment.CurrentDirectory + @"\offs.conf";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            getFileInfo();
        }

        // get config file last modified date
        private void getFileInfo()
        {
            if (File.Exists(confPath))
            {
                DateTime modification = File.GetLastWriteTime(confPath);
                lbl_modDate.Text = modification.Date.ToString("MM/dd/yyyy");
            }
            else
            {
                MessageBox.Show("Config file not found , Press enter to create new config");
                File.Create(confPath);
                Application.Restart();
            }
        }

        // Write the offsets to our config file.
        private void writeOffsets()
        {
            string[] temp = rawOffsets.Split();
            string offset = "";
            for (int i = 0; i < temp.Length; i++)
            {
                offset = offset + " " + temp[i];
            }
            temp = null;
            File.WriteAllText(confPath, offset);
            lbl_stats.Text = "Done.";
            getFileInfo();
        }

        //Start updating offset from dataserver.
        private async void button1_Click(object sender, EventArgs e)
        {
            lbl_stats.Text = "Fetching Latest Offsets , Please Wait";
            Thread.Sleep(100);
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.Navigate("https://pastebin.com/Gf8T1DDn");
            //wait for page loading
            await PageLoad(10);
            string urlOffs;
            HtmlElement htmlelement = webBrowser1.Document.GetElementById("paste_code");
            if (htmlelement != null)
            {
                urlOffs = webBrowser1.Document.GetElementById("paste_code").OuterText;
                rawOffsets = urlOffs;
                htmlelement = null;
                webBrowser1.Dispose();
                writeOffsets();
            }
            else
            {
                MessageBox.Show("Loading data site failed.");
            }
        }

        private string _rawOffs;
        public string rawOffsets
        {
            get { return _rawOffs; }
            set { _rawOffs = value; } 
        }

        private async Task PageLoad(int TimeOut)
        {
            TaskCompletionSource<bool> PageLoaded = null;
            PageLoaded = new TaskCompletionSource<bool>();
            int TimeElapsed = 0;
            webBrowser1.DocumentCompleted += (s, e) =>
            {
                if (webBrowser1.ReadyState != WebBrowserReadyState.Complete) return;
                if (PageLoaded.Task.IsCompleted) return; PageLoaded.SetResult(true);
            };
            
            while (PageLoaded.Task.Status != TaskStatus.RanToCompletion)
            {
                await Task.Delay(10);
                TimeElapsed++;
                if (TimeElapsed >= TimeOut * 100) PageLoaded.TrySetResult(true);
            }
        }

    }
}
