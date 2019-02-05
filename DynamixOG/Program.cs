using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Xml;
using File = System.IO.File;

namespace ScoringEngine
{
	class Program
	{

		public static int currentVulns = 0;

		static void Main(string[] args)
		{
			AppShortcutToDesktop();
            CreateHTML();
            Run();
		}
		public static void Run()
		{
            ExportLSP();
            //Insert commands here
            ParseScore();


            EditHTML();
            System.Threading.Thread.Sleep(2000); //Sleeps for 20 seconds before running again. This is just a loop.
            CreateHTML();
            Run();
		}


		public static void FileDetection(string location) //Detects if file has been removed.
		{
			bool exists = File.Exists(location);
			if (exists == false)
			{
				currentVulns = currentVulns + 1;
				HtmlScoring("File: " + '"' + location + '"' + " has been removed");
			}
		}


		public static void ForensicsCheck(string location, string answerTemp) //Forensics checker, grabs location and searches for the string "answer"
		{
			{
				string line;
                string answer = "ANSWER: " + answerTemp;

                System.IO.StreamReader file = new System.IO.StreamReader(@location);
				while ((line = file.ReadLine()) != null)
				{
					if (line.Contains(answer))
					{
						string fileName = Path.GetFileNameWithoutExtension(@location); //Gets the filename from location
						currentVulns = currentVulns + 1;
						HtmlScoring(fileName + " has been answered correctly"); //So this would output "Forensics question 1 has been answered correctly" or something similar
					}
				}
			}
		}


		public static void FirewallCheck(string status) //Grabs the status of the firewall. status must be "True" or "False"
		{
			Type FWManagerType = Type.GetTypeFromProgID("HNetCfg.FwMgr");
			dynamic FWManager = Activator.CreateInstance(FWManagerType);
			if (Convert.ToString(FWManager.LocalPolicy.CurrentProfile.FirewallEnabled) == status)
			{
				currentVulns = currentVulns + 1;
				HtmlScoring("Firewall has been enabled");
			}
			else { }
		}


		public static void UserLockout(string user) //Grabs if the user is locked out or not
		{
			SelectQuery query = new SelectQuery("Win32_UserAccount", "Name=" + "'" + user + "'");
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
			foreach (ManagementObject userObj in searcher.Get())
			{
				if (userObj["Lockout"].ToString() == "False") //Added .ToString() so if would stop complaining
				{
					currentVulns = currentVulns + 1;
					HtmlScoring(user + " has been unlocked");
				}
			}
		}
		public static void UserDisabled(string user) //Grabs if the user is disabled or not
		{
			SelectQuery query = new SelectQuery("Win32_UserAccount", "Name=" + "'" + user + "'");
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
			foreach (ManagementObject userObj in searcher.Get())
			{
				if (userObj["Disabled"].ToString() == "False")
				{
					currentVulns = currentVulns + 1;
					HtmlScoring(user + " has been enabled");
				}
			}
		}
		public static void UserPasswordChangeable(string user) //Grabs if the users password is changeable or not.
		{
			SelectQuery query = new SelectQuery("Win32_UserAccount", "Name=" + "'" + user + "'");
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
			foreach (ManagementObject userObj in searcher.Get())
			{
				if (userObj["PasswordChangeable"].ToString() == "True")
				{
					currentVulns = currentVulns + 1;
					HtmlScoring(user + "'s password is changeable");
				}
			}
		}


		public static void ProgramVersionCheck(string location, string desiredVersion) //Checks the version of an exe and compares it to the desiredVersion.
		{
			FileVersionInfo program = FileVersionInfo.GetVersionInfo(@location);
			string programVersion = program.FileVersion;
			string fileName = UppercaseFirst(Path.GetFileNameWithoutExtension(@location));
			var result = programVersion.CompareTo(desiredVersion);
			if (result > 0)
			{
                currentVulns = currentVulns + 1;
				HtmlScoring(fileName + " has been updated to the latest version");
            }
			else if (result < 0) { }
			else
			{
				currentVulns = currentVulns + 1;
				HtmlScoring(fileName + " has been updated to the latest version");
			}
		}


		public static void ShareDetection(string desiredShare) //Detects if a share exists or not
		{
			using (ManagementClass shares = new ManagementClass(@"\\Localhost", "Win32_Share", new ObjectGetOptions()))
			{
				List<string> activeShares = new List<string>();
				foreach (ManagementObject share in shares.GetInstances())
				{
					activeShares.Add(share["Name"].ToString());
				}
				bool inList = activeShares.Contains(desiredShare);
				if (inList == false)
				{
					currentVulns = currentVulns + 1;
					HtmlScoring("Share: " + '"' + desiredShare + '"' + " has been deleted");
				}
				else { }
			}
		}


		public static void ServiceRunning(string service) //Checks if a service is running
		{
			ServiceController sc = new ServiceController(service);
			if (sc.Status.ToString() == "Running")
			{
				currentVulns = currentVulns + 1;
				HtmlScoring(service + " is running");
			}
			else { }
		}
		public static void ServiceStopped(string service) // Checks if a service is stopped
		{
			ServiceController sc = new ServiceController(service);
			if (sc.Status.ToString() == "Stopped")
			{
				currentVulns = currentVulns + 1;
				HtmlScoring(service + " is not running");
			}
			else { }
		}


		public static void LSPMinimumPasswordAge(int minValue, int maxValue) //Checks if the MinimumPasswordAge is between minValue and maxValue
		{
			string[] lines = System.IO.File.ReadAllLines(@"C:\Dynamix\Current-Policy.txt");
			string minimumPassword = Array.Find(lines,
			   element => element.StartsWith("MinimumPasswordAge = ", StringComparison.Ordinal));

			string temp = minimumPassword.Split(new string[] { "MinimumPasswordAge = " }, StringSplitOptions.None).Last();

			int.TryParse(temp, out int minimumPasswordVal);
			if (minimumPasswordVal >= minValue)
			{
				if (maxValue >= minimumPasswordVal)
				{
					currentVulns = currentVulns + 1;
					HtmlScoring("Minimum Password Age has been set");
				}
			}
		}
		public static void LSPMaximumPasswordAge(int minValue, int maxValue) //Checks if the MaximumPasswordAge is between minValue and maxValue
		{
			string[] lines = System.IO.File.ReadAllLines(@"C:\Dynamix\Current-Policy.txt");
			string maximumPassword = Array.Find(lines,
			   element => element.StartsWith("MaximumPasswordAge = ", StringComparison.Ordinal));

			string temp = maximumPassword.Split(new string[] { "MaximumPasswordAge = " }, StringSplitOptions.None).Last();

			int.TryParse(temp, out int maximumPasswordVal);
			if (maximumPasswordVal >= minValue)
			{
				if (maxValue >= maximumPasswordVal)
				{
					currentVulns = currentVulns + 1;
					HtmlScoring("Maximum Password Age has been set");
				}
			}
		}
		public static void LSPPasswordComplexity(int desiredValue) //Checks if PasswordComplexity is set to 1 (Enabled) or 0 (Disabled)
		{
			string[] lines = System.IO.File.ReadAllLines(@"C:\Dynamix\Current-Policy.txt");
			string passwordComplexity = Array.Find(lines,
			   element => element.StartsWith("PasswordComplexity = ", StringComparison.Ordinal));
			string temp = passwordComplexity.Split(new string[] { "PasswordComplexity = " }, StringSplitOptions.None).Last();

			int.TryParse(temp, out int passwordComplexityVal);
			if (passwordComplexityVal == desiredValue)
			{
				currentVulns = currentVulns + 1;
				HtmlScoring("Password Complexity has been set");
			}
		}


		public static void OptionFeatureDisable(string feature) //Detects if an optional feature is disabled
		{
			SelectQuery query = new SelectQuery("Win32_OptionalFeature", "Name='" + feature + "'");
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
			foreach (ManagementObject envVar in searcher.Get())
			{
				if (envVar["InstallState"].ToString() == "2")
				{
					currentVulns = currentVulns + 1;
					HtmlScoring(feature + " has been disabled");
				}
			}
		}
		public static void OptionFeatureEnable(string feature) //Detects if an optional feature is enabled
		{
			SelectQuery query = new SelectQuery("Win32_OptionalFeature", "Name='" + feature + "'");
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
			foreach (ManagementObject envVar in searcher.Get())
			{
				if (envVar["InstallState"].ToString() == "1")
				{
					currentVulns = currentVulns + 1;
					HtmlScoring(feature + " has been enabled");
				}
			}
		}


		public static void IsProgramInstalled(string program) //Guess what, it detects if it's installed
		{
			if (SoftwareName(program) == true)
			{
				currentVulns = currentVulns + 1;
				HtmlScoring(program + " has been installed");
			}
		}
		public static void IsProgramUninstalled(string program) //Detects if it's uninstalled
		{
			if (SoftwareName(program) == false)
			{
				currentVulns = currentVulns + 1;
				HtmlScoring(program + " has been uninstalled");
			}
		}


		public static void GroupMembershipTrue(string user, string groupName) //If a member is apart of a group
		{
			using (DirectoryEntry machine = new DirectoryEntry("WinNT://localhost"))
			{
				using (DirectoryEntry group = machine.Children.Find(groupName, "Group"))
				{
					List<string> groupMembers = new List<string>();
					object members = group.Invoke("Members", null);
					foreach (object member in (IEnumerable)members)
					{
						string accountName = new DirectoryEntry(member).Name;
						groupMembers.Add(accountName);
					}
					if (groupMembers.Contains(user) == true)
					{
						currentVulns = currentVulns + 1;
						HtmlScoring(user + " is in the " + groupName + " group");
					}
				}
			}
		}
		public static void GroupMembershipFalse(string user, string groupName) //If a member is NOT apart of a group
		{
			using (DirectoryEntry machine = new DirectoryEntry("WinNT://localhost"))
			{
				using (DirectoryEntry group = machine.Children.Find(groupName, "Group"))
				{
					List<string> groupMembers = new List<string>();
					object members = group.Invoke("Members", null);
					foreach (object member in (IEnumerable)members)
					{
						string accountName = new DirectoryEntry(member).Name;
						groupMembers.Add(accountName);
					}
					if (groupMembers.Contains(user) == false)
					{
						currentVulns = currentVulns + 1;
						HtmlScoring(user + " is not in the " + groupName + " group");
					}
				}
			}
		}


		public static void GroupExistTrue(string group) //If a group exists
		{
			var machine = Environment.MachineName;
			var server = new DirectoryEntry(string.Format("WinNT://{0},Computer", machine));
			bool exists = server.Children.Cast<DirectoryEntry>().Any(d => d.SchemaClassName.Equals("Group") && d.Name.Equals(group));
			if (exists == true)
			{
				currentVulns = currentVulns + 1;
				HtmlScoring(group + " exists");
			}
		}
		public static void GroupExistFalse(string group) //If a group does not exist
		{
			var machine = Environment.MachineName;
			var server = new DirectoryEntry(string.Format("WinNT://{0},Computer", machine));
			bool exists = server.Children.Cast<DirectoryEntry>().Any(d => d.SchemaClassName.Equals("Group") && d.Name.Equals(group));
			if (exists == false)
			{
				currentVulns = currentVulns + 1;
				HtmlScoring(group + " does not exist");
			}
		}


        public static void IEInternetZone(string desiredValue) //Zones for Internet Explorer
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\3"))
            {
                if (key != null)
                {
                    var value = key.GetValue("CurrentLevel").ToString();
                    if (value == desiredValue)
                    {
                        currentVulns = currentVulns + 1;
                        HtmlScoring("Internet Explorer zone set for Internet");
                    }
                }
            }
        }
        public static void IEIntranetZone(string desiredValue)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\1"))
            {
                if (key != null)
                {
                    var value = key.GetValue("CurrentLevel").ToString();
                    if (value == desiredValue)
                    {
                        currentVulns = currentVulns + 1;
                        HtmlScoring("Internet Explorer zone set for Intranet");
                    }
                }
            }
        }
        public static void IETrustedZone(string desiredValue)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\2"))
            {
                if (key != null)
                {
                    var value = key.GetValue("CurrentLevel").ToString();
                    if (value == desiredValue)
                    {
                        currentVulns = currentVulns + 1;
                        HtmlScoring("Internet Explorer zone set for Trusted Sites");
                    }
                }
            }
        }
        public static void IERestrictedZone(string desiredValue)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\3"))
            {
                if (key != null)
                {
                    var value = key.GetValue("CurrentLevel").ToString();
                    if (value == desiredValue)
                    {
                        currentVulns = currentVulns + 1;
                        HtmlScoring("Internet Explorer zone set for Restricted Sites");
                    }
                }
            }
        }

        public static void IEPopup(string desiredValue)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\New Windows"))
            {
                if (key != null)
                {
                    var value = key.GetValue("PopupMgr").ToString();
                    if (value == desiredValue)
                    {
                        currentVulns = currentVulns + 1;
                        HtmlScoring("Popup blocker in Internet Explorer as been enabled");
                    }
                }
            }
        }



        public static void HtmlScoring(string text) //Outputs input above "</ul>"
		{
            string location = @"C:\\Dynamix\\score_report.html";
            string lineToFind = "			<!--correct-->";

			List<string> lines = File.ReadLines(location).ToList();
			int index = lines.IndexOf(lineToFind);
			lines.Insert(index, "<li>" + text + "</li>");
			File.WriteAllLines(location, lines);

            Console.WriteLine(text); //Just a debug
		}

		public static void AppShortcutToDesktop() //Creates a shortcut to the desktop. 
		{
			string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

			WshShell wsh = new WshShell();
			IWshRuntimeLibrary.IWshShortcut shortcut = wsh.CreateShortcut(
				Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Scoring Report.lnk") as IWshRuntimeLibrary.IWshShortcut;
			shortcut.Arguments = "";
			shortcut.TargetPath = "C:\\Dynamix\\score_report.html";
			shortcut.WindowStyle = 1;
			shortcut.Description = "Windows Scoring Report";
			shortcut.WorkingDirectory = "c:\\Dynamix";
			shortcut.IconLocation = "C:\\Dynamix\\dx-128-icon.ico";
			shortcut.Save();
		}

		public static void CreateHTML()
		{
			File.Delete(@"C:\Dynamix\score_report.html");
			File.Copy(@"C:\Dynamix\base_report.html", @"C:\Dynamix\score_report.html");
		}
        public static void EditHTML()
        {
            string location = @"C:\Dynamix\score_report.html";
            string lineToFind = "			<!--issues-->";

            List<string> lines = File.ReadLines(location).ToList();
            int index = lines.IndexOf(lineToFind);
            lines.Insert(index, "<li class=\"issuesTitle\">Security Issues (" + currentVulns + " out of " + TotalVulns() + " resolved)</li>");
            File.WriteAllLines(location, lines);

            string lineToFind2 = "			<!--scored-->";

            int index2 = lines.IndexOf(lineToFind2);
            lines.Insert(index, "<li>" + currentVulns + " out of " + TotalVulns() + " security issues resolved</li>");
            File.WriteAllLines(location, lines);

            currentVulns = 0;
        }

        public static int TotalVulns()
        {
            int lineCount = File.ReadLines(@"C:\Dynamix\answers.xml").Count();
            return lineCount - 3;
        }

        public static void ExportLSP() //Exports Local Security Policy
		{
			//This has to be run as admin
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
			{
				WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
				FileName = "cmd.exe",
				Arguments = "/C SecEdit /export /cfg c:\\Dynamix\\Current-Policy.txt"
			};
			process.StartInfo = startInfo;
			process.Start();
		}

		private static bool SoftwareName(string softwareName)
		{
			var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall") ??
					  Registry.LocalMachine.OpenSubKey(
						  @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");

			if (key == null)
				return false;

			return key.GetSubKeyNames()
				.Select(keyName => key.OpenSubKey(keyName))
				.Select(subkey => subkey.GetValue("DisplayName") as string)
				.Any(displayName => displayName != null && displayName.Contains(softwareName));
		}

        public static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static void ParseScore()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(@"C:\Dynamix\answers.xml");

            XmlNodeList fileDetection = xmlDoc.GetElementsByTagName("FileDetection");
            for (int i = 0; i < fileDetection.Count; i++)
            {
                string tempVar = fileDetection[i].InnerText;
                FileDetection(tempVar);
            }

            XmlNodeList forensicsDetection = xmlDoc.GetElementsByTagName("ForensicsCheck");
            for (int i = 0; i < forensicsDetection.Count; i++)
            {
                string tempVar = forensicsDetection[i].InnerText;
                string[] words = tempVar.Split('|');
                string location = words[0];
                string answer = words[1];
                ForensicsCheck(location, answer);
            }

            XmlNodeList firewallCheck = xmlDoc.GetElementsByTagName("FirewallCheck");
            for (int i = 0; i < firewallCheck.Count; i++)
            {
                string tempVar = firewallCheck[i].InnerText;
                FirewallCheck(tempVar);
            }

            XmlNodeList userLockout = xmlDoc.GetElementsByTagName("UserLockout");
            for (int i = 0; i < userLockout.Count; i++)
            {
                string tempVar = userLockout[i].InnerText;
                UserLockout(tempVar);
            }

            XmlNodeList userDisabled = xmlDoc.GetElementsByTagName("UserDisabled");
            for (int i = 0; i < userDisabled.Count; i++)
            {
                string tempVar = userDisabled[i].InnerText;
                UserDisabled(tempVar);
            }

            XmlNodeList userChangeable = xmlDoc.GetElementsByTagName("UserPasswordChangeable");
            for (int i = 0; i < userChangeable.Count; i++)
            {
                string tempVar = userChangeable[i].InnerText;
                UserPasswordChangeable(tempVar);
            }

            XmlNodeList programVersion = xmlDoc.GetElementsByTagName("ProgramVersionCheck");
            for (int i = 0; i < programVersion.Count; i++)
            {
                try
                {
                    string tempVar = programVersion[i].InnerText;
                    string[] words = tempVar.Split('|');
                    string location = words[0];
                    string desiredVersion = words[1];
                    ProgramVersionCheck(location, desiredVersion);
                }
                catch { }
            }

            XmlNodeList shareDetection = xmlDoc.GetElementsByTagName("ShareDetection");
            for (int i = 0; i < shareDetection.Count; i++)
            {
                string tempVar = shareDetection[i].InnerText;
                ShareDetection(tempVar);
            }

            XmlNodeList serviceRunning = xmlDoc.GetElementsByTagName("ServiceRunning");
            for (int i = 0; i < serviceRunning.Count; i++)
            {
                string tempVar = serviceRunning[i].InnerText;
                ServiceRunning(tempVar);
            }

            XmlNodeList serviceStopped = xmlDoc.GetElementsByTagName("ServiceStopped");
            for (int i = 0; i < serviceStopped.Count; i++)
            {
                string tempVar = serviceStopped[i].InnerText;
                ServiceStopped(tempVar);
            }

            XmlNodeList LSPMinimum = xmlDoc.GetElementsByTagName("LSPMinimumPasswordAge");
            for (int i = 0; i < LSPMinimum.Count; i++)
            {
                string tempVar = LSPMinimum[i].InnerText;
                string[] words = tempVar.Split('|');
                int min = int.Parse(words[0]);
                int max = int.Parse(words[1]);
                LSPMinimumPasswordAge(min, max);
            }

            XmlNodeList LSPMaximum = xmlDoc.GetElementsByTagName("LSPMaximumPasswordAge");
            for (int i = 0; i < LSPMaximum.Count; i++)
            {
                string tempVar = LSPMaximum[i].InnerText;
                string[] words = tempVar.Split('|');
                int min = int.Parse(words[0]);
                int max = int.Parse(words[1]);
                LSPMaximumPasswordAge(min, max);
            }

            XmlNodeList LSPComp = xmlDoc.GetElementsByTagName("LSPPasswordComplexity");
            for (int i = 0; i < LSPComp.Count; i++)
            {
                int tempVar = int.Parse(LSPComp[i].InnerText);
                LSPPasswordComplexity(tempVar);
            }

            XmlNodeList featureDisable = xmlDoc.GetElementsByTagName("OptionFeatureDisable");
            for (int i = 0; i < featureDisable.Count; i++)
            {
                string tempVar = featureDisable[i].InnerText;
                OptionFeatureDisable(tempVar);
            }

            XmlNodeList featureEnable = xmlDoc.GetElementsByTagName("OptionFeatureEnable");
            for (int i = 0; i < featureEnable.Count; i++)
            {
                string tempVar = featureEnable[i].InnerText;
                OptionFeatureEnable(tempVar);
            }

            XmlNodeList programInstalled = xmlDoc.GetElementsByTagName("IsProgramInstalled");
            for (int i = 0; i < programInstalled.Count; i++)
            {
                string tempVar = programInstalled[i].InnerText;
                IsProgramInstalled(tempVar);
            }

            XmlNodeList programUninstalled = xmlDoc.GetElementsByTagName("IsProgramUninstalled");
            for (int i = 0; i < programUninstalled.Count; i++)
            {
                string tempVar = programUninstalled[i].InnerText;
                IsProgramUninstalled(tempVar);
            }

            XmlNodeList groupMembershipTrue = xmlDoc.GetElementsByTagName("GroupMembershipTrue");
            for (int i = 0; i < groupMembershipTrue.Count; i++)
            {
                string tempVar = groupMembershipTrue[i].InnerText;
                string[] words = tempVar.Split('|');
                string user = words[0];
                string groupName = words[1];
                GroupMembershipTrue(user, groupName);
            }

            XmlNodeList groupMembershipFalse = xmlDoc.GetElementsByTagName("GroupMembershipFalse");
            for (int i = 0; i < groupMembershipFalse.Count; i++)
            {
                string tempVar = groupMembershipFalse[i].InnerText;
                string[] words = tempVar.Split('|');
                string user = words[0];
                string groupName = words[1];
                GroupMembershipFalse(user, groupName);
            }

            XmlNodeList groupExistTrue = xmlDoc.GetElementsByTagName("GroupExistTrue");
            for (int i = 0; i < groupExistTrue.Count; i++)
            {
                string tempVar = groupExistTrue[i].InnerText;
                GroupExistTrue(tempVar);
            }

            XmlNodeList groupExistFalse = xmlDoc.GetElementsByTagName("GroupExistFalse");
            for (int i = 0; i < groupExistFalse.Count; i++)
            {
                string tempVar = groupExistFalse[i].InnerText;
                GroupExistFalse(tempVar);
            }

            XmlNodeList internetZone = xmlDoc.GetElementsByTagName("IEInternetZone");
            for (int i = 0; i < internetZone.Count; i++)
            {
                string tempVar = internetZone[i].InnerText;
                IEInternetZone(tempVar);
            }

            XmlNodeList intranetZone = xmlDoc.GetElementsByTagName("IEIntranetZone");
            for (int i = 0; i < intranetZone.Count; i++)
            {
                string tempVar = intranetZone[i].InnerText;
                IEIntranetZone(tempVar);
            }

            XmlNodeList restrictedZone = xmlDoc.GetElementsByTagName("IERestrictedZone");
            for (int i = 0; i < restrictedZone.Count; i++)
            {
                string tempVar = restrictedZone[i].InnerText;
                IERestrictedZone(tempVar);
            }

            XmlNodeList iePopup = xmlDoc.GetElementsByTagName("IEPopup");
            for (int i = 0; i < iePopup.Count; i++)
            {
                string tempVar = iePopup[i].InnerText;
                IEPopup(tempVar);
            }
        }
    }
}
//UwU
// D Y N A M I X
//All done by TheiMacNoob with mental assistance done Matthew and rainbow did some stuff too. feat. Stack Overflow
//Hi, white did stuff too
//meme.mp4