using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Engine.Installer.Core
{
    /// <summary>
    /// The global installation singleton
    /// </summary>
    public static class Installation
    {
        /// <summary>
        /// The current installation data
        /// </summary>
        internal static InstallationPackage CurrentInstallation;

        /// <summary>
        /// Try to parse installation info from the provided IData
        /// </summary>
        /// <param name="IData">The Installation data to parse</param>
        public static void LoadInstallationInformation(byte[] IData)
        {
            CurrentInstallation = new InstallationPackage(IData);
        }

        /// <summary>
        /// Collect the templates from the current installation
        /// <paramref name="templatesdirectory">The directory to load templates from</paramref>
        /// </summary>
        public static void CollectTemplates(string templatesdirectory)
        {
            if (CurrentInstallation == null)
                return;
            try
            {
                DirectoryInfo TemplateDir = new DirectoryInfo(templatesdirectory);
                foreach (FileInfo template in TemplateDir.GetFiles("*.cst", SearchOption.AllDirectories))
                {
                    string templatename = template.Name.ToLower().Replace(".cst", "");
                    if(Enum.TryParse(templatename, true, out CheckTypes checktype))
                    {
                        CurrentInstallation.RuntimeTemplates[checktype] = template.FullName;
                    }
                }
            }
            catch
            {
                //todo report the security error to the installer
            }
        }

        /// <summary>
        /// Try to parse installation info from the provided install.bin
        /// </summary>
        /// <param name="FilePath">The file path to install.bin</param>
        /// <exception cref="FileNotFoundException"></exception>
        public static void LoadInstallationInformation(string FilePath)
        {
            LoadInstallationInformation(File.ReadAllBytes(FilePath));
        }

        /// <summary>
        /// Begin the installation
        /// </summary>
        public static void Install()
        {

        }
    }
}
