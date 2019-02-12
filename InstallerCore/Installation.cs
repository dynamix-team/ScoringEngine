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
        private const string WinEnginePath =    @"C:\Scoring";
        private const string LinEnginePath =    @"/Scoring";
        public const string InstallPath =       @"Installation";
        public const string EnginesrcPath =     @"Engine";
        private const string RepoName =         @"ScoringEngine-master";
        private const string CoreName =         @"InstallerCore";
        private const string TemplateFilename = @"CheckTemplate.cs";
        private const string MSBuildInstaller = @"ms_build.exe";
        private const string MSBuildURL =       @"";

        public static string EnginePath
        {
            get
            {
                return (CurrentInstallation?.HasFlag(InstallationPackage.InstallFlags.Linux) ?? false) ? LinEnginePath : WinEnginePath;
            }
        }

        public static string SourcePath
        {
            get
            {
                return Path.Combine(EnginePath, InstallPath, EnginesrcPath);
            }
        }

        public static string RepoPath
        {
            get
            {
                return Path.Combine(SourcePath, RepoName);
            }
        }

        public static string CheckTemplatePath
        {
            get
            {
                return Path.Combine(RepoPath, CoreName, TemplateFilename);
            }
        }

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
        public static void CollectTemplates(string templatesdirectory) //Potentially obsolete because of the build process
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
        public static async System.Threading.Tasks.Task<InstallationResult> Install()
        {
            //Step 0. Pre-Setup
#if DEBUG
#else
            try { foreach (var process in System.Diagnostics.Process.GetProcessesByName("explorer")) process.Kill(); } catch { } //Kill windows explorer so installation ops are un-interrupted
#endif

            //Step 1. Get the engine source
            bool GotEngine = await Networking.DownloadEngine(SourcePath);

            if(!GotEngine)
            {
                return new InstallationResult() { Message = "Failed to download the engine source. Please verify that you are connected to the internet and have access to github.com", ErrorLevel = -1 };
            }

            //Step 2. Patch the engine encryption key
            PatchCheckKey();
            return InstallationResult.Success;
            //Step 3. Download the c# compiler for the respective platform
            //ms_build for windows
            bool GotCompiler = await Networking.DownloadResource(MSBuildURL, InstallPath, MSBuildInstaller);

            if(!GotCompiler)
            {
                return new InstallationResult() { Message = "Failed to download a critical resource. Please verify that you are connected to the internet.", ErrorLevel = -1 };
            }

            return InstallationResult.Success;
        }

        private static void PatchCheckKey()
        {
            //*?installer.key*/
            try
            {
                string text = File.ReadAllText(CheckTemplatePath);
                byte[] newkey = new byte[16];
                new Random((int)DateTime.Now.Ticks).NextBytes(newkey);
                string keytext = "{";
                for(int i = 0; i < 16; i++)
                {
                    keytext += newkey[i] + (i == 15 ? "," : "};//");
                }
                text = text.Replace("/*?installer.key*/", keytext);
                File.WriteAllText(CheckTemplatePath, text);
            }
            catch
            {
                //Report as a fatal error. this is a security compromise and cannot be waivered.
            }
        }

        /// <summary>
        /// The result of an installation procedure
        /// </summary>
        public sealed class InstallationResult
        {
            /// <summary>
            /// The error level for the installation
            /// </summary>
            public int ErrorLevel = 0;

            /// <summary>
            /// The message of the installation
            /// </summary>
            public string Message = "An unknown error occurred in the installation";

            /// <summary>
            /// The installation finished successfully
            /// </summary>
            public static InstallationResult Success
            {
                get
                {
                    return new InstallationResult() { Message = "The installation completed successfully" };
                }
            }
        }
    }
}
