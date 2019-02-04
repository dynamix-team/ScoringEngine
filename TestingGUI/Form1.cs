using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CPVulnerabilityFramework.Core;
using System.IO;
using System.Xml;
using System.Reflection;

//Link dump
// kernel32 dep policy https://docs.microsoft.com/en-us/windows/desktop/api/winbase/nf-winbase-getsystemdeppolicy
//https://retep998.github.io/doc/kernel32/fn.GetSystemDEPPolicy.html
//https://en.wikipedia.org/wiki/Service_Control_Manager


namespace TestingGUI
{
    public partial class Form1 : Form
    {
        private List<Vulnerability> Vulnerabilities;
        public Form1()
        {
            InitializeComponent();
            TestTimer.Tick += TestTimer_Tick;
            Vulnerability.AddTypeAlias("customuseralias", typeof(UA_AccountAction));

            string XMLData = "";
            XmlDocument ScoringData = new XmlDocument();
            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TestingGUI.config.xml"))
                using (StreamReader reader = new StreamReader(stream))
                {
                    XMLData = reader.ReadToEnd();
                }
                ScoringData.LoadXml(XMLData);
            }
            catch
            {
                MessageBox.Show("Failed to load scoring data! XML file may be formatted incorrectly...", "Scoring Data Corrupted", MessageBoxButtons.OK);
                Application.Exit();
            }

            Vulnerabilities = Vulnerability.ParseVulnerabilities(ScoringData);
            foreach(var vuln in Vulnerabilities)
            {
                vuln.OnCompleted += VulnMessageUpdate;
                vuln.OnRegressed += VulnMessageUpdate;
                vuln.OnFailed += VulnMessageUpdate;
            }

            Vulnerability.LockAssembly(); //Finalize vulnerabilities from any changes

            MouseDown += Form1_MouseDown1;
            Location = new Point(0,0);
            TestTimer.Start();
        }

        /// <summary>
        /// Fired on vuln state changed
        /// </summary>
        /// <param name="v"></param>
        /// <param name="e"></param>
        private void VulnMessageUpdate(Vulnerability caller, VulnerabilityEventArgs e)
        {
            //Could be optimized but might be no point
            VulnMessages.Clear();
            int i = 0;
            int count = 0;
            foreach (Vulnerability v in Vulnerabilities)
            {
                if (!v?.Enabled ?? true)
                    continue;
                if (v != 0)
                {
                    i++;
                    VulnMessages.Add((int)v + " points: " + v.Message);
                }
                count++;
            }
            groupBox1.Text = "Vulnerabilities Found: " + i + "/" + count;

            VulnInfo.Lines = VulnMessages.ToArray();
        }

        List<string> VulnMessages = new List<string>();
        private void TestTimer_Tick(object sender, EventArgs e)
        {
            label1.Text = "Score: " + GetScore();
            //GC.Collect(); //keeps memory low :)
        }

        private int GetScore()
        {
            int result = 0;
            foreach (Vulnerability v in Vulnerabilities)
            {
                result += v;
            }
            return result;
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void Form1_MouseDown1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
