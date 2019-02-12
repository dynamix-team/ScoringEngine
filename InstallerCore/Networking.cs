using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.Threading.Tasks;
using System.IO;

namespace Engine.Installer.Core
{
    /// <summary>
    /// All networking interfaces and components that require networking
    /// </summary>
    internal static class Networking
    {
        private const string GitUsername = "dynamix-team";
        private const string GitRepoName = "ScoringEngine";
        private const string GitRepoURLFormat = "https://github.com/{0}/{1}/archive/master.zip"; //Username->0, Repo->1
        private const string ZipName = "engine.zip";
        


        internal static string ProjectURL
        {
            get
            {
                return string.Format(GitRepoURLFormat, GitUsername, GitRepoName);
            }
        }

        /// <summary>
        /// Download the engine to the path specified
        /// </summary>
        /// <param name="path">The path to download the engine to</param>
        /// <returns></returns>
        internal static async Task DownloadEngine(string path)
        {
            try
            {
                while(Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    await Task.Delay(100);
                }
                Directory.CreateDirectory(path);
            }
            catch
            {
                //todo: kill windows explorer when we start the installer
            }
            try
            {
                byte[] FileData;
                using (var client = new System.Net.Http.HttpClient())
                {
                    FileData = await client.GetByteArrayAsync(ProjectURL);
                }
                File.WriteAllBytes(Path.Combine(path, ZipName), FileData);

                ZipFile.ExtractToDirectory(Path.Combine(path, ZipName), path);
            }
            catch
            {
                //report an error to the installer, this is CRITICAL
            }
        }

    }
}
