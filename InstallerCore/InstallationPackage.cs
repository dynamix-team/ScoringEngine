using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Engine.Installer.Core
{
    /// <summary>
    /// A wrapper for an installation package
    /// </summary>
    public class InstallationPackage
    {
        private const string TargetFileName = "install.bin";
        private const string FileMagic = "SEI";

        /// <summary>
        /// A dictionary of types to templates, based entirely on filenames
        /// </summary>
        public Dictionary<CheckTypes, string> RuntimeTemplates;

        /// <summary>
        /// Installation flags
        /// </summary>
        public enum InstallFlags : uint
        {
            /// <summary>
            /// Is this an offline installation (no scoring hook)
            /// </summary>
            Offline = 1,
            /// <summary>
            /// Disables image patching (configuration of vulnerabilities to presets)
            /// </summary>
            NoPatch = 2,
            /// <summary>
            /// Is this a debug package? (Offline & NoPatch)
            /// </summary>
            Debug = 3,
            /// <summary>
            /// Is this a linux installation
            /// </summary>
            Linux = 4,
            /// <summary>
            /// Is this a ranked competition
            /// </summary>
            Ranked = 8,
        }

        private List<byte> RawData;

        /* Temp cause idk where to put this while designing

            #install.bin typedef
            byte[4] Magic = { (byte)'S', (byte)'E', (byte)'I', 0x0 };
            uint16 NumCheckDefs;
            uint16* CheckDefPtr;
            uint Flags;
            uint Reserved;
            byte[16] UniqueID; //Used for verification
            CheckDefinition[NumVulnDefs]
        */
        private enum PackageFields : int
        {
            Magic = 0x0,
            NumCheckDefs = 0x4,
            CheckDefPtr = 0x6,
            Flags = 0x8,
            Reserved = 0xC,
            UniqueID = 0x10,
        }

        /// <summary>
        /// Magic of the file
        /// </summary>
        internal byte[] Magic
        {
            get
            {
                try
                {
                    return RawData.GetBytes((int)PackageFields.Magic, 4);
                }
                catch
                {
                    return new byte[4];
                }
            }
            set
            {
                RawData.SetBytes((int)PackageFields.Magic, value);
            }
            
        }

        /// <summary>
        /// Number of check definitions
        /// </summary>
        internal ushort NumCheckDefs
        {
            get
            {
                return BitConverter.ToUInt16(RawData.GetBytes((int)PackageFields.NumCheckDefs, sizeof(ushort)), 0);
            }
            set
            {
                RawData.SetBytes((int)PackageFields.NumCheckDefs, BitConverter.GetBytes(value));
            }
        }

        /// <summary>
        /// Ptr to CheckDefs
        /// </summary>
        private ushort CheckDefsPtr
        {
            get
            {
                return BitConverter.ToUInt16(RawData.GetBytes((int)PackageFields.CheckDefPtr, sizeof(ushort)), 0);
            }
            set
            {
                RawData.SetBytes((int)PackageFields.CheckDefPtr, BitConverter.GetBytes(value));
            }
        }

        /// <summary>
        /// Flags for the installation
        /// </summary>
        internal uint Flags
        {
            get
            {
                return BitConverter.ToUInt32(RawData.GetBytes((int)PackageFields.Flags, sizeof(uint)), 0);
            }
            set
            {
                RawData.SetBytes((int)PackageFields.Flags, BitConverter.GetBytes(value));
            }
        }

        /// <summary>
        /// Just some reserved space. Probably a crc32 eventually
        /// </summary>
        private uint Reserved
        {
            get
            {
                return BitConverter.ToUInt32(RawData.GetBytes((int)PackageFields.Reserved, sizeof(uint)), 0);
            }
            set
            {
                RawData.SetBytes((int)PackageFields.Reserved, BitConverter.GetBytes(value));
            }
        }

        private byte[] UniqueID
        {
            get
            {
                try
                {
                    return RawData.GetBytes((int)PackageFields.UniqueID, 16);
                }
                catch
                {
                    return new byte[16]; //should be impossible
                }
            }
            set
            {
                RawData.SetBytes((int)PackageFields.UniqueID, value);
            }
        }

        /// <summary>
        /// All check pointers loaded into this installation
        /// </summary>
        private ushort[] CheckPointers
        {
            get
            {
                if (CheckDefsPtr == 0 || NumCheckDefs == 0)
                {
                    return new ushort[0];
                }
                ushort[] pointers = new ushort[NumCheckDefs];
                for(int i = 0; i < NumCheckDefs; i++)
                {
                    pointers[i] = BitConverter.ToUInt16(RawData.ToArray(), CheckDefsPtr + sizeof(ushort) * i);
                }
                return pointers;
            }
            set
            {
                for(int i = 0; i < value.Length; i++)
                {
                    RawData.SetBytes(CheckDefsPtr + i * sizeof(ushort), BitConverter.GetBytes(value[i]));
                }
            }
        }

        /// <summary>
        /// All checks contained in the installer
        /// </summary>
        internal CheckDefinition[] Checks
        {
            get
            {
                CheckDefinition[] checks = new CheckDefinition[CheckPointers.Length];
                for(int i = 0; i < checks.Length; i++)
                {
                    checks[i] = (RawData, CheckPointers[i]);
                }
                return checks;
            }
            set
            {
                CheckDefinition[] checks = Checks;
                ushort[] checkptrs = CheckPointers;
                for (int i = checks.Length - 1; i > -1; i--)
                {
                    RawData.RemoveRange(checkptrs[i], checks[i].CheckSize);
                }
                for(int i = 0; i < checkptrs.Length; i++)
                {
                    RawData.RemoveRange(CheckDefsPtr, 2);
                }
                for(int i = 0; i < value.Length; i++)
                {
                    RawData.InsertRange(CheckDefsPtr, new byte[2]);
                }
                checkptrs = new ushort[value.Length];
                for (int i = 0; i < value.Length; i++)
                {
                    checkptrs[i] = (ushort)RawData.Count;
                    RawData.AddRange((byte[])value[i]);
                }
                CheckPointers = checkptrs;
                NumCheckDefs = (ushort)checkptrs.Length;
            }
        }

        /// <summary>
        /// An installation configuration
        /// </summary>
        /// <param name="data">The data to parse into an installation package</param>
        public InstallationPackage(byte[] data)
        {
            RawData = data.ToList();
            RuntimeTemplates = new Dictionary<CheckTypes, string>();
            if (Encoding.ASCII.GetString(Magic, 0, 3) != FileMagic)
                throw new FormatException("Installation package is an unrecognized format");
        }

        /// <summary>
        /// Get the raw byte data of an installation package
        /// </summary>
        /// <param name="p">the package to get data from</param>
        public static implicit operator byte[](InstallationPackage p)
        {
            return p.RawData.ToArray();
        }

        private InstallationPackage()
        {

        }
#if DEBUG
        /// <summary>
        /// Create a debug installation package using an xml input
        /// </summary>
        /// <returns></returns>
        public static InstallationPackage MakeDebugInstall(string XMLInput)
        {
            try
            {
                InstallationPackage package = new InstallationPackage();
                List<CheckDefinition> checks = new List<CheckDefinition>();
                package.RawData = new List<byte>();
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(XMLInput);
                package.RawData.AddRange(new byte[32]);
                package.Flags |= (uint)InstallFlags.Debug;
                package.Magic = Encoding.ASCII.GetBytes(FileMagic);

                foreach (XmlNode node in doc.GetElementsByTagName("check"))
                {
                    try
                    {
                        Enum.TryParse(node["key"].InnerText, true, out CheckTypes checktype);
                        string[] args = new string[node["arguments"].ChildNodes.Count];
                        for (int i = 0; i < args.Length; i++)
                        {
                            args[i] = node["arguments"].ChildNodes[i].InnerText;
                        }
                        CheckDefinition d = CheckDefinition.DebugCheck(checktype, Convert.ToUInt16(node["id"].InnerText), Convert.ToInt16(node["points"].InnerText), Convert.ToByte(node["flags"].InnerText), args);
                        if (d != null)
                            checks.Add(d);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.StackTrace.ToString());
                        Console.WriteLine(e.Message);
                    }
                }
                package.CheckDefsPtr = (ushort)package.RawData.Count;
                package.Checks = checks.ToArray();
                return package;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace.ToString());
                Console.WriteLine(e.Message);
                return null;
            }
        }
#endif
        /// <summary>
        /// Does this installation have the specified flag
        /// </summary>
        /// <param name="flag">The flag to check</param>
        /// <returns></returns>
        public bool HasFlag(InstallFlags flag)
        {
            return (Flags & (uint)flag) > 0;
        }
    }
}
