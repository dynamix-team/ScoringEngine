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
        /// Try to parse installation info from the provided install.bin
        /// </summary>
        /// <param name="FilePath">The file path to install.bin</param>
        /// <exception cref="FileNotFoundException"></exception>
        public static void LoadInstallationInformation(string FilePath)
        {
            LoadInstallationInformation(File.ReadAllBytes(FilePath));
        }
    }
}
