using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
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

            Engine.Core.Engine engine = new Engine.Core.Engine();

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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox2.Text = PrepareString(textBox1.Text).ToString("X");
        }

        private uint PrepareString(string content) //PJW hash
        {
            uint hash = 0, high;
            foreach (char s in content)
            {
                hash = (hash << 4) + (uint)s;
                if ((high = hash & 0xF0000000) > 0)
                    hash ^= high >> 24;
                hash &= ~high;
            }
            return hash;
        }
    }
}
