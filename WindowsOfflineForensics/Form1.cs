using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsOfflineForensics
{
    public partial class Form1 : Form
    {

        private string location;
        private string answer;
        Microsoft.Win32.RegistryKey key;
        public Form1(string[] args)
        {
            InitializeComponent();
            location = args[0];
            Addtext();
        }

        private void RichTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        public void Addtext()
        {
            try
            {
                richTextBox1.AppendText("Forensics Question " + GetLine(location, 1));
                richTextBox1.AppendText(Environment.NewLine + Environment.NewLine + GetLine(location, 2));
            }
            catch
            {
                MessageBox.Show("Invalid Arguments (Most likely location)!");
            }
        }

        string GetLine(string fileName, int line)
        {
            using (var sr = new StreamReader(fileName))
            {
                for (int i = 1; i < line; i++)
                    sr.ReadLine();
                return sr.ReadLine();
            }
        }

        

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            answer = textBox1.Text;
            try
            {
                key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Forensics");
                key.SetValue("Forensics" + GetLine(location, 1), answer);
                key.Close();
            }
            catch
            {
                MessageBox.Show("Invalid Arguments (Most likely location)!");
            }
            
        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
