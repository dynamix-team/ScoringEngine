using System;
using System.IO;
using System.Threading.Tasks;

//References for later in case engine builds run into issues
//https://docs.microsoft.com/en-us/visualstudio/install/workload-component-id-vs-build-tools?view=vs-2017
//https://docs.microsoft.com/en-us/nuget/tools/nuget-exe-cli-reference
//https://docs.microsoft.com/en-us/visualstudio/msbuild/common-msbuild-project-properties?view=vs-2017
//https://docs.microsoft.com/en-us/visualstudio/install/use-command-line-parameters-to-install-visual-studio?view=vs-2017


namespace Engine.Installer.Core
{//TODO: proper error levels, ie: enumerate them
    /// <summary>
    /// The global installation singleton
    /// </summary>
    public static class Installation
    {
        private const string WinEnginePath =    @"C:\Scoring";
        private const string LinEnginePath =    @"/Scoring";
        public const string InstallDir =        @"Installation";
        public const string EnginesrcDir =      @"Engine";
        private const string RepoName =         @"ScoringEngine-master";
        private const string CoreName =         @"InstallerCore";
        private const string TemplateFilename = @"CheckTemplate.cs";
        private const string MSBuildInstaller = @"ms_build.exe";
#if DEBUG
        private const string MSBuildPath =      @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Preview\MSBuild\Current\Bin\MSBuild.exe";
#else
        private const string MSBuildPath =      @"C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe";
#endif
        private const string SolutionName =     @"ScoringEngine.sln";
        private const string NugetURL =         @"https://dist.nuget.org/win-x86-commandline/v4.9.3/nuget.exe";


        public static string EnginePath
        {
            get
            {
                return (CurrentInstallation?.HasFlag(InstallationPackage.InstallFlags.Linux) ?? false) ? LinEnginePath : WinEnginePath;
            }
        }

        public static string NugetPath
        {
            get
            {
                return Path.Combine(RepoPath, "nuget.exe");
            }
        }

        public static string ConfigurationName
        {
            get
            {
#if DEBUG
                string mode = "Debug" + ((CurrentInstallation?.HasFlag(InstallationPackage.InstallFlags.Offline) ?? false) ? "Offline" : "Online");
#else
                string mode = "Release" + ((CurrentInstallation?.HasFlag(InstallationPackage.InstallFlags.Offline) ?? false) ? " -p:DefineConstants=OFFLINE" : "");
#endif
                return mode;
            }
        }

        public static string SolutionPath
        {
            get
            {
                return Path.Combine(RepoPath, SolutionName);
            }
        }

        public static string InstallPath
        {
            get
            {
                return Path.Combine(EnginePath, InstallDir);
            }
        }

        public static string SourcePath
        {
            get
            {
                return Path.Combine(EnginePath, InstallDir, EnginesrcDir);
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

        public static string MSBuildInstallerPath
        {
            get
            {
                return Path.Combine(RepoPath, MSBuildInstaller);
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
            await PatchCheckKey();
            bool result = true;
            //Step 3. Install the c# compiler for the respective platform (only if not debug)
#if DEBUG
#else
            
            if(CurrentInstallation.HasFlag(InstallationPackage.InstallFlags.Linux))
            {
                return new InstallationResult() { Message = "Linux installation is not currently implemented!", ErrorLevel = -1 };
            }
            else
            {
                result = await InstallMSBuild();

            }
            if(!result)
                return new InstallationResult() { Message = "There was error creating the engine", ErrorLevel = -1 };
#endif
            //Step 4. Compile the engine using the correct compiler for the platform, including the target build mode for the platform
            //need to target Release configuration, define or not define OFFLINE constant, clean solution, and call rebuild on the whole solution
            //the target release package will be in a const folder with the filename "install.bin"
            if (CurrentInstallation.HasFlag(InstallationPackage.InstallFlags.Linux))
            {
                return new InstallationResult() { Message = "Linux installation is not currently implemented!", ErrorLevel = -1 };
            }
            else
            {
                result = await BuildWindowsEngine();

            }

            if(!result)
                return new InstallationResult() { Message = "The engine failed to build.", ErrorLevel = -1 };

            //Step 5. Copy the required assemblies into the installation directory


            return InstallationResult.Success;
        }

        private static async Task PatchCheckKey()
        {
            try
            {
                string text = File.ReadAllText(CheckTemplatePath);
                byte[] newkey = new byte[16];
                Random r = new Random((int)DateTime.Now.Ticks);
                await Task.Delay(r.Next(5) * 10); //some entropy to offset actual random time seeding
                r.NextBytes(newkey);
                string keytext = "{";
                for(int i = 0; i < 16; i++)
                {
                    keytext += newkey[i] + (i != 15 ? "," : "};//");
                }
                text = text.Replace("/*?installer.key*/", keytext);
                File.WriteAllText(CheckTemplatePath, text);
            }
            catch
            {
                //Report as a fatal error. this is a security compromise and cannot be waivered. TODO
            }
        }

#pragma warning disable IDE0051 // Remove unused private members
        /// <summary>
                               /// Installs MSBuild
                               /// </summary>
                               /// <returns></returns>
        private static async Task<bool> InstallMSBuild()
#pragma warning restore IDE0051 // Remove unused private members
        {
            if (!File.Exists(MSBuildInstallerPath))
                return false;
            //vs_buildtools.exe --add Microsoft.VisualStudio.Workload.MSBuildTools --quiet
            //MSBuildPath
            try
            {
                int result = await Extensions.StartProcess(MSBuildInstallerPath, "--add Microsoft.VisualStudio.Workload.MSBuildTools --add Microsoft.VisualStudio.Component.NuGet --add Microsoft.VisualStudio.Component.NuGet.BuildTools --add Microsoft.Net.Component.4.7.2.SDK -add Microsoft.Net.Component.4.7.2.TargetingPack --add Microsoft.VisualStudio.Component.Roslyn.Compiler --add Microsoft.VisualStudio.Workload.NetCoreBuildTools --quiet", null, null, Console.Out, Console.Out);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Build the windows engine using MSBuild
        /// </summary>
        /// <returns></returns>
        private static async Task<bool> BuildWindowsEngine()
        {
            //msbuild CommunityServer.Sync.sln /p:Configuration=Release
            try
            {
                int result = 0;
                if(!File.Exists(NugetPath))
                {
                    bool gotNuget = await Networking.DownloadResource(NugetURL, RepoPath, "nuget.exe");
                    if (!gotNuget)
                        return false;
                }

                result = await Extensions.StartProcess(NugetPath, "restore \"" + SolutionPath + "\"", RepoPath, null, Console.Out, Console.Out);
                result = await Extensions.StartProcess(MSBuildPath, "\"" + SolutionPath + "\" -p:Configuration=" + ConfigurationName, RepoPath, null, Console.Out, Console.Out);

                return result == 0;
            }
            catch
            {
                return false;
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
