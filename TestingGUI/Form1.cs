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

        public Form1()
        {
            InitializeComponent();

            WindowsEngine.Engine engine = new WindowsEngine.Engine();

            Engine.Core.Scoring.StartEngine(engine);

            MouseDown += Form1_MouseDown1;
            Location = new Point(0,0);
            TestTimer.Start();
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
