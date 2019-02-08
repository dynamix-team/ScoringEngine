using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
/* TODO:
 * Call commence
 * Commence does a backend load with reporting (can use background manager with error reporting, i think thats the safest bet
 * Meanwhile, do the windows flashy install thing with random quote messages and color changes
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */ 

namespace WindowsInstaller
{
    public partial class Installer : Form
    {
        private static readonly bool StandaloneInstaller = true;
        private string Key;
        private byte phase;
        private int count;
        private Image LoadIcon = Properties.Resources.win_load;

        private List<Color> RandomColors = new List<Color>()
        {
            Color.FromArgb(255,32,32,32),
            Color.FromArgb(255,142,0,0),
            Color.FromArgb(255,35,94,188),
            Color.FromArgb(255,0,112,26),
            Color.FromArgb(255,255,143,0),
            Color.FromArgb(255,86,0,39),
            Color.FromArgb(255,0,77,64),
            Color.FromArgb(255,26,35,126),
            Color.FromArgb(255,183,28,28),
            Color.FromArgb(255,191,54,12),
            Color.FromArgb(255,120,0,46),
        };

        private int colorindex = 0;
        private Color CurrentColor
        {
            get
            {
                return RandomColors[colorindex];
            }
        }

        private Color NextColor
        {
            get
            {
                if (colorindex + 1 >= RandomColors.Count)
                    return RandomColors[0];
                return RandomColors[colorindex + 1];
            }
        }

        public Installer()
        {
            InitializeComponent();
            UniqueID.Location = new Point(0, (int)(this.Height * .6));
            StatusMessage.Location = new Point(0, UniqueID.Location.Y + 67);
            UniqueID.TextChanged += UniqueID_TextChanged;
            IntroTimer.Tick += IntroTimer_Tick;
            IntroTimer.Start();
        }

        private void UniqueID_TextChanged(object sender, EventArgs e)
        {
            if(UniqueID.MaskFull)
            {
                bool result = KeyMeetsCriteria(UniqueID.Text);
                if (result)
                {
                    StatusMessage.ForeColor = Color.White;
                    StatusMessage.Text = "";
                    UniqueID.ForeColor = Color.LightGreen;
                    UniqueID.ReadOnly = true;
                    StatusMessage.Focus();
                    phase = 3;
                    IntroTimer.Start();
                }
                else
                {
                    StatusMessage.ForeColor = Color.LightCoral;
                    StatusMessage.Text = "Invalid Unique Identifier.";
                }
            }
            else
            {
                StatusMessage.ForeColor = Color.White;
                StatusMessage.Text = "";
            }
        }

        /// <summary>
        /// Precheck for key validation
        /// </summary>
        /// <param name="key">The key to validate</param>
        /// <returns></returns>
        private bool KeyMeetsCriteria(string key)
        {
            int result = 0;
            foreach(char c in key)
            {
                result ^= c;
            }
            return (result % 16) == 0;
        }

        private void IntroTimer_Tick(object sender, EventArgs e)
        {
            switch(phase)
            {
                case 0:
                    IntroTimer.Stop();
                    IntroTimer.Interval = 10;
                    phase = 1;
                    IntroTimer.Start();
                    break;
                case 1:
                    count++;
                    WelcomeLabel.ForeColor = Color.FromArgb(255, (int)FloatLerp(255, 32, (float)count / 50), (int)FloatLerp(255, 32, (float)count / 50), (int)FloatLerp(255, 32, (float)count / 50));
                    if(count >= 50)
                    {
                        count = 0;
                        WelcomeLabel.Text = "Please enter your Unique ID";
                        UniqueID.Visible = true;
                        UniqueID.Focus();
                        phase = 2;
                    }
                    break;
                case 2:
                    count++;
                    WelcomeLabel.ForeColor = Color.FromArgb(255, (int)FloatLerp(32, 255, (float)count / 50), (int)FloatLerp(32, 255, (float)count / 50), (int)FloatLerp(32, 255, (float)count / 50));
                    UniqueID.ForeColor = WelcomeLabel.ForeColor;
                    if (count >= 50)
                    {
                        phase = 3;
                        count = 0;
                        IntroTimer.Stop();
                    }
                    break;
                case 3:
                    count++;
                    WelcomeLabel.ForeColor = Color.FromArgb(255, (int)FloatLerp(255, 32, (float)count / 50), (int)FloatLerp(255, 32, (float)count / 50), (int)FloatLerp(255, 32, (float)count / 50));
                    if (count >= 50)
                    {
                        count = 0;
                        WelcomeLabel.Text = "Just a second...";
                        phase = 4;
                    }
                    break;
                case 4:
                    count++;
                    WelcomeLabel.ForeColor = Color.FromArgb(255, (int)FloatLerp(32, 255, (float)count / 50), (int)FloatLerp(32, 255, (float)count / 50), (int)FloatLerp(32, 255, (float)count / 50));
                    if (count >= 50)
                    {
                        phase = 5;
                        count = 0;
                        IntroTimer.Stop();
                        ValidateID();
                    }
                    break;
                case 5:
                    count++;
                    WelcomeLabel.ForeColor = Color.FromArgb(255, (int)FloatLerp(255, 32, (float)count / 50), (int)FloatLerp(255, 32, (float)count / 50), (int)FloatLerp(255, 32, (float)count / 50));
                    UniqueID.ForeColor = Color.FromArgb(255, (int)FloatLerp(Color.LightGreen.R, 32, (float)count / 50), (int)FloatLerp(Color.LightGreen.G, 32, (float)count / 50), (int)FloatLerp(Color.LightGreen.B, 32, (float)count / 50));
                    if (count >= 50)
                    {
                        phase = 6;
                        WelcomeLabel.Text = "Starting Installation...";
                        Controls.Remove(UniqueID);
                        count = 0;
                    }
                    break;
                case 6:
                    count++;
                    WelcomeLabel.ForeColor = Color.FromArgb(255, (int)FloatLerp(32, 255, (float)count / 50), (int)FloatLerp(32, 255, (float)count / 50), (int)FloatLerp(32, 255, (float)count / 50));
                    if (count >= 50)
                    {
                        phase = 7;
                        Controls.Remove(UniqueID);
                        StatusMessage.Visible = false;
                        count = 0;
                        Commence();
                    }
                    break;
                case 7:
                    count++;
                    if (count < 50)
                    {
                        BackColor = Color.FromArgb(255, (int)FloatLerp(CurrentColor.R, NextColor.R, (float)count / 50), (int)FloatLerp(CurrentColor.G, NextColor.G, (float)count / 50), (int)FloatLerp(CurrentColor.B, NextColor.B, (float)count / 50));
                    }
                    else
                    {
                        phase = 8;
                    }
                    break;
                case 8:
                    count++;
                    if (count >= 250)
                    {
                        count = 0;
                        colorindex++;
                        if (colorindex >= RandomColors.Count)
                            colorindex = 0;
                        phase = 7;
                    }
                    break;
            }
        }

        private void ValidateID()
        {
            //for current, we are going to accept the key using offline installer mode
            if(StandaloneInstaller)
            {
                Key = UniqueID.Text;
                phase = 5;
                IntroTimer.Start();
            }

            //if id is good, start the installation
            //else, tell em its bad, clear it, and restart the process
        }

        /// <summary>
        /// Time to start installing
        /// </summary>
        private void Commence()
        {
            if(StandaloneInstaller)
            {
                byte[] IData = System.IO.File.ReadAllBytes(@"testing\install.bin");
                Engine.Installer.Core.Installation.LoadInstallationInformation(IData);
                Engine.Installer.Core.Installation.CollectTemplates(@"Templates");
                //BackgroundWorker bgw = new BackgroundWorker();

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Linear float interpolation
        /// </summary>
        /// <param name="from">Starting Point</param>
        /// <param name="to">End Point</param>
        /// <param name="alpha">Alpha</param>
        /// <returns></returns>
        private float FloatLerp(float from, float to, float alpha)
        {
            return from * (1 - alpha) + to * alpha;
        }

        private void UniqueID_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
        }
    }
}
