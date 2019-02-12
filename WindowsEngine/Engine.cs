#if DEBUG
#undef ONLINE
#else
//?installer.online
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Engine.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;


namespace WindowsEngine
{
    /// <summary>
    /// Auto generated class by the installer [DO NOT EDIT]
    /// </summary>
#if DEBUG
    public class Engine : EngineFrame
#else
    internal class Engine : EngineFrame
#endif
    {

        private FileVersionTemplate__0 c_0;

private uint c_0_s;
private FileVersionTemplate__1 c_1;

private uint c_1_s;
private FileVersionTemplate__2 c_2;

private uint c_2_s;
private FileVersionTemplate__3 c_3;

private uint c_3_s;
private FileVersionTemplate__4 c_4;

private uint c_4_s;
private FileVersionTemplate__5 c_5;

private uint c_5_s;
private FileVersionTemplate__6 c_6;

private uint c_6_s;
private FileVersionTemplate__7 c_7;

private uint c_7_s;


#if DEBUG
        public Engine()
#else
        internal Engine()
#endif
        {
            c_0 = new FileVersionTemplate__0(@"C:\Test\vmplayer.exe", @"15.0.0.38213");
c_1 = new FileVersionTemplate__1(@"C:\Test\vmplayer.exe", @"15.0.0.38213");
c_2 = new FileVersionTemplate__2(@"C:\Test\vmplayer.exe", @"15.0.0.38213");
c_3 = new FileVersionTemplate__3(@"C:\Test\vmplayer.exe", @"15.0.0.38213");
c_4 = new FileVersionTemplate__4(@"C:\Test\vmplayer.exe", @"15.0.0.38213");
c_5 = new FileVersionTemplate__5(@"C:\Test\vmplayer.exe", @"15.0.0.38213");
c_6 = new FileVersionTemplate__6(@"C:\Test\vmplayer.exe", @"15.0.0.38213");
c_7 = new FileVersionTemplate__7(@"C:\Test\vmplayer.exe", @"15.0.0.38213");

        }

        protected override async Task Tick()
        {
            if(c_0?.Enabled ?? false){ c_0_s = await c_0.GetCheckValue(); RegisterCheck((ushort)1|((uint)1<< 16),c_0_s);}

if(c_1?.Enabled ?? false){ c_1_s = await c_1.GetCheckValue(); RegisterCheck((ushort)2|((uint)1<< 16),c_1_s);}

if(c_2?.Enabled ?? false){ c_2_s = await c_2.GetCheckValue(); RegisterCheck((ushort)3|((uint)1<< 16),c_2_s);}

if(c_3?.Enabled ?? false){ c_3_s = await c_3.GetCheckValue(); RegisterCheck((ushort)4|((uint)1<< 16),c_3_s);}

if(c_4?.Enabled ?? false){ c_4_s = await c_4.GetCheckValue(); RegisterCheck((ushort)5|((uint)1<< 16),c_4_s);}

if(c_5?.Enabled ?? false){ c_5_s = await c_5.GetCheckValue(); RegisterCheck((ushort)6|((uint)1<< 16),c_5_s);}

if(c_6?.Enabled ?? false){ c_6_s = await c_6.GetCheckValue(); RegisterCheck((ushort)7|((uint)1<< 16),c_6_s);}

if(c_7?.Enabled ?? false){ c_7_s = await c_7.GetCheckValue(); RegisterCheck((ushort)8|((uint)1<< 16),c_7_s);}


        }

#if ONLINE
        public override bool IsOnline()
        {
            return true;
        }
#else
        public override bool IsOnline()
        {
            return false;
        }
#endif

        //cst
internal sealed class FileVersionTemplate__0 : CheckTemplate
{
private readonly SafeString FilePath;
/// <summary>
/// A file version check template
/// </summary>
/// <param name="args">[0:FilePath],[1:FileVersion]</param>
internal FileVersionTemplate__0(params string[] args)
{
if (args.Length < 2)
Enabled = false;
FilePath = args[0];
}
/// <summary>
/// Get the check value
/// </summary>
/// <returns></returns>
internal override async Task<uint> GetCheckValue()
{
if (!File.Exists(FilePath))
{
return PrepareState("");
}
var version = await Task.FromResult<uint>(PrepareState(FileVersionInfo.GetVersionInfo(FilePath).FileVersion));
return version;
}
}

//cst
internal sealed class FileVersionTemplate__1 : CheckTemplate
{
private readonly SafeString FilePath;
/// <summary>
/// A file version check template
/// </summary>
/// <param name="args">[0:FilePath],[1:FileVersion]</param>
internal FileVersionTemplate__1(params string[] args)
{
if (args.Length < 2)
Enabled = false;
FilePath = args[0];
}
/// <summary>
/// Get the check value
/// </summary>
/// <returns></returns>
internal override async Task<uint> GetCheckValue()
{
if (!File.Exists(FilePath))
{
return PrepareState("");
}
var version = await Task.FromResult<uint>(PrepareState(FileVersionInfo.GetVersionInfo(FilePath).FileVersion));
return version;
}
}

//cst
internal sealed class FileVersionTemplate__2 : CheckTemplate
{
private readonly SafeString FilePath;
/// <summary>
/// A file version check template
/// </summary>
/// <param name="args">[0:FilePath],[1:FileVersion]</param>
internal FileVersionTemplate__2(params string[] args)
{
if (args.Length < 2)
Enabled = false;
FilePath = args[0];
}
/// <summary>
/// Get the check value
/// </summary>
/// <returns></returns>
internal override async Task<uint> GetCheckValue()
{
if (!File.Exists(FilePath))
{
return PrepareState("");
}
var version = await Task.FromResult<uint>(PrepareState(FileVersionInfo.GetVersionInfo(FilePath).FileVersion));
return version;
}
}

//cst
internal sealed class FileVersionTemplate__3 : CheckTemplate
{
private readonly SafeString FilePath;
/// <summary>
/// A file version check template
/// </summary>
/// <param name="args">[0:FilePath],[1:FileVersion]</param>
internal FileVersionTemplate__3(params string[] args)
{
if (args.Length < 2)
Enabled = false;
FilePath = args[0];
}
/// <summary>
/// Get the check value
/// </summary>
/// <returns></returns>
internal override async Task<uint> GetCheckValue()
{
if (!File.Exists(FilePath))
{
return PrepareState("");
}
var version = await Task.FromResult<uint>(PrepareState(FileVersionInfo.GetVersionInfo(FilePath).FileVersion));
return version;
}
}

//cst
internal sealed class FileVersionTemplate__4 : CheckTemplate
{
private readonly SafeString FilePath;
/// <summary>
/// A file version check template
/// </summary>
/// <param name="args">[0:FilePath],[1:FileVersion]</param>
internal FileVersionTemplate__4(params string[] args)
{
if (args.Length < 2)
Enabled = false;
FilePath = args[0];
}
/// <summary>
/// Get the check value
/// </summary>
/// <returns></returns>
internal override async Task<uint> GetCheckValue()
{
if (!File.Exists(FilePath))
{
return PrepareState("");
}
var version = await Task.FromResult<uint>(PrepareState(FileVersionInfo.GetVersionInfo(FilePath).FileVersion));
return version;
}
}

//cst
internal sealed class FileVersionTemplate__5 : CheckTemplate
{
private readonly SafeString FilePath;
/// <summary>
/// A file version check template
/// </summary>
/// <param name="args">[0:FilePath],[1:FileVersion]</param>
internal FileVersionTemplate__5(params string[] args)
{
if (args.Length < 2)
Enabled = false;
FilePath = args[0];
}
/// <summary>
/// Get the check value
/// </summary>
/// <returns></returns>
internal override async Task<uint> GetCheckValue()
{
if (!File.Exists(FilePath))
{
return PrepareState("");
}
var version = await Task.FromResult<uint>(PrepareState(FileVersionInfo.GetVersionInfo(FilePath).FileVersion));
return version;
}
}

//cst
internal sealed class FileVersionTemplate__6 : CheckTemplate
{
private readonly SafeString FilePath;
/// <summary>
/// A file version check template
/// </summary>
/// <param name="args">[0:FilePath],[1:FileVersion]</param>
internal FileVersionTemplate__6(params string[] args)
{
if (args.Length < 2)
Enabled = false;
FilePath = args[0];
}
/// <summary>
/// Get the check value
/// </summary>
/// <returns></returns>
internal override async Task<uint> GetCheckValue()
{
if (!File.Exists(FilePath))
{
return PrepareState("");
}
var version = await Task.FromResult<uint>(PrepareState(FileVersionInfo.GetVersionInfo(FilePath).FileVersion));
return version;
}
}

//cst
internal sealed class FileVersionTemplate__7 : CheckTemplate
{
private readonly SafeString FilePath;
/// <summary>
/// A file version check template
/// </summary>
/// <param name="args">[0:FilePath],[1:FileVersion]</param>
internal FileVersionTemplate__7(params string[] args)
{
if (args.Length < 2)
Enabled = false;
FilePath = args[0];
}
/// <summary>
/// Get the check value
/// </summary>
/// <returns></returns>
internal override async Task<uint> GetCheckValue()
{
if (!File.Exists(FilePath))
{
return PrepareState("");
}
var version = await Task.FromResult<uint>(PrepareState(FileVersionInfo.GetVersionInfo(FilePath).FileVersion));
return version;
}
}

//cst
//NOTE: This file is sourced from ScoringEngine/InstallerCore/CheckTemplate.cs
//      Changes made outside of the source file will not be applied!
/// <summary>
/// A check template to be used to define a new system check
/// </summary>
internal abstract class CheckTemplate
{
#region REQUIRED
/// <summary>
/// Get the state of the check as an unsigned integer (4 byte solution, little endian)
/// </summary>
/// <returns>The state of the check</returns>
internal abstract Task<uint> GetCheckValue();
/// <summary>
/// Create the check
/// </summary>
/// <param name="args">Arguments to pass to the check</param>
internal protected CheckTemplate(params string[] args)
{
}
#endregion
#region OPTIONAL
/// <summary>
/// Speed at which this value updates
/// </summary>
internal virtual uint TickDelay => 1000;
#endregion
#region PRIVATE
/// <summary>
/// Timer for internal ticking
/// </summary>
private System.Diagnostics.Stopwatch Timer = new System.Diagnostics.Stopwatch();
/// <summary>
/// State of current check
/// </summary>
private uint CachedState = 0;
/// <summary>
/// Used to lock the state check to only allow one async state to run
/// </summary>
private bool STATE_LOCK;
#endregion
//Specifications (since i literally cant think rn)
//GetState() -> Offline? -> Score += (Failed? -score : State = expected ? score : 0)
//              Online?  -> NetComm(CID,STATEVALUE)
//ONLINE SPEC: CANNOT TICK MORE THAN 1 TIME PER SECOND, MUST GET BATCHED (bandwith limiting)
//ONLINE SPEC: Consolidate data. IE: Hash strings using a PJW Hash, everything else is a number at or below 4 bytes. All states are reported as unsigned integers.
//ONLINE SPEC: Batch sizes should be less than [1024] byte returns
//ONLINE SPEC: Online batch ticking should be equal to or slower than internal ticking. Ticks are only called when the value is being batched for sending
//OFFLINE SPEC: Min tickrate = 1ms, default = 1000ms, max = 300000ms
//OFFLINE SPEC: Still hash strings with PJW Hash
//OFFLINE SPEC: Must return a score
//Implementation Specifications
//Returns the value of the check to communicate between networks
//internal abstract uint GetOnlineState()
//Returns the score of the state
//internal abstract int GetOfflineState()
/// <summary>
/// Can this check be ticked at this current point in time
/// </summary>
/// <returns></returns>
internal bool CanTick()
{
if (STATE_LOCK)
return false;
//Already ticking
if(!Timer.IsRunning)
{
Timer.Start();
return true;
}
return Timer.ElapsedMilliseconds >= TickDelay;
}
/// <summary>
/// Return the state of this check
/// </summary>
/// <returns></returns>
internal async Task<uint> CheckState()
{
if(CanTick())
{
STATE_LOCK = true;
CachedState = await GetCheckValue();
STATE_LOCK = false;
Timer.Restart();
}
return CachedState;
}
/// <summary>
/// Call to force the check to tick nice time checkstate is requested
/// </summary>
internal void ForceTick()
{
Timer.Stop();
Timer.Reset();
}
/// <summary>
/// Convert a string into a state
/// </summary>
/// <param name="content">The string to convert</param>
/// <returns>A state version of a string</returns>
private uint PrepareString(string content) //PJW hash
{
uint hash = 0, high;
foreach(char s in content)
{
hash = (hash << 4) + (uint)s;
if ((high = hash & 0xF0000000) > 0)
hash ^= high >> 24;
hash &= ~high;
}
return hash;
}
/// <summary>
/// Prepare a state for replication
/// </summary>
/// <param name="o_state">The state to be prepared</param>
/// <returns>A UINT version of the state</returns>
internal uint PrepareState(object o_state)
{
if (o_state == null)
return 0u;
return PrepareString(o_state.ToString());
}
/// <summary>
/// Is this check enabled for evaluation?
/// </summary>
internal bool Enabled = true;
}
internal sealed class SafeString
{
private static readonly byte[] __key__ = new byte[] /*?installer.key*/{ 0x00, 0xc5, 0x6c, 0xdd, 0x38, 0x8d, 0xa7, 0x02, 0x43, 0x92, 0x96, 0xae, 0x31, 0x99, 0x8f, 0x79 };
private byte[] data;
/// <summary>
/// Create a string from a safe string
/// </summary>
/// <param name="s"></param>
public static implicit operator string(SafeString s)
{
return D(s.data);
}
/// <summary>
/// Create a safe string from a normal string
/// </summary>
/// <param name="s"></param>
public static implicit operator SafeString(string s)
{
SafeString st = new SafeString
{
data = E(s)
};
return st;
}
private static byte[] E(string str)
{
if (str == null)
str = "";
byte[] encrypted;
byte[] IV;
using (Aes aesAlg = Aes.Create())
{
aesAlg.Key = __key__;
aesAlg.GenerateIV();
IV = aesAlg.IV;
aesAlg.Mode = CipherMode.CBC;
var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
// Create the streams used for encryption.
using (var msEncrypt = new MemoryStream())
{
using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
{
using (var swEncrypt = new StreamWriter(csEncrypt))
{
//Write all data to the stream.
swEncrypt.Write(str);
}
encrypted = msEncrypt.ToArray();
}
}
}
var combinedIvCt = new byte[IV.Length + encrypted.Length];
Array.Copy(IV, 0, combinedIvCt, 0, IV.Length);
Array.Copy(encrypted, 0, combinedIvCt, IV.Length, encrypted.Length);
// Return the encrypted bytes from the memory stream.
return combinedIvCt;
}
private static string D(byte[] str)
{
// Declare the string used to hold
// the decrypted text.
string plaintext = null;
// Create an Aes object
// with the specified key and IV.
using (Aes aesAlg = Aes.Create())
{
aesAlg.Key = __key__;
byte[] IV = new byte[aesAlg.BlockSize / 8];
byte[] cipherText = new byte[str.Length - IV.Length];
Array.Copy(str, IV, IV.Length);
Array.Copy(str, IV.Length, cipherText, 0, cipherText.Length);
aesAlg.IV = IV;
aesAlg.Mode = CipherMode.CBC;
// Create a decrytor to perform the stream transform.
ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
// Create the streams used for decryption.
using (var msDecrypt = new MemoryStream(cipherText))
{
using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
{
using (var srDecrypt = new StreamReader(csDecrypt))
{
// Read the decrypted bytes from the decrypting stream
// and place them in a string.
plaintext = srDecrypt.ReadToEnd();
}
}
}
}
return plaintext;
}
}


    }
}
