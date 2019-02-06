using System;
using System.Collections.Generic;
using System.Text;
//todo Add/Remove Arguments
namespace Engine.Installer.Core
{
    /// <summary>
    /// A definition for a check
    /// </summary>
    public sealed class CheckDefinition
    {
        //#align(0xF)
        /*
                #CheckDef typedef
                uint16 CheckKey; //The key identifier for a vuln
                uint16 CheckID; //The identifier of this specific check for online scoring
                int NumPoints;
                byte NumArgs;
                string[NumArgs] Arguments;

            todo: patching data
        */
        private enum CheckDefFields
        {
            CheckKey = 0x0,
            CheckID = 0x2,
            NumPoints = 0x4,
            Flags = 0x6,
            NumArgs = 0x7,
            CheckSize = 0x8,
            ArgsPtr = 0xA
        }

        private List<byte> RawData = new List<byte>();

        /// <summary>
        /// The internalized key to translate this check
        /// </summary>
        public ushort CheckKey
        {
            get
            {
                return BitConverter.ToUInt16(RawData.GetBytes((int)CheckDefFields.CheckKey, sizeof(ushort)), 0);
            }
            set
            {
                RawData.SetBytes((int)CheckDefFields.CheckKey, BitConverter.GetBytes(value));
            }
        }
        /// <summary>
        /// The ID of this check for online competitions. You can have multiple Checks with the same ID if you need an extra failure check or something like that
        /// </summary>
        public ushort CheckID
        {
            get
            {
                return BitConverter.ToUInt16(RawData.GetBytes((int)CheckDefFields.CheckID, sizeof(ushort)), 0);
            }
            set
            {
                RawData.SetBytes((int)CheckDefFields.CheckID, BitConverter.GetBytes(value));
            }
        }
        /// <summary>
        /// The amount of points to award when the task is complete, to revoke when regressed, or to remove when failed
        /// </summary>
        public short NumPoints
        {
            get
            {
                return BitConverter.ToInt16(RawData.GetBytes((int)CheckDefFields.NumPoints, sizeof(short)), 0);
            }
            set
            {
                RawData.SetBytes((int)CheckDefFields.NumPoints, BitConverter.GetBytes(value));
            }
        }
        /// <summary>
        /// The flags of this check
        /// </summary>
        public byte Flags
        {
            get
            {
                while (RawData.Count <= (int)CheckDefFields.Flags)
                    RawData.Add(0x0);
                return RawData[(int)CheckDefFields.Flags];
            }
            set
            {
                while (RawData.Count <= (int)CheckDefFields.Flags)
                    RawData.Add(0x0);
                RawData[(int)CheckDefFields.Flags] = value;
            }
        }
        /// <summary>
        /// The number of arguments to parse. All arguments should be strings
        /// </summary>
        public byte NumArgs
        {
            get
            {
                while (RawData.Count <= (int)CheckDefFields.NumArgs)
                    RawData.Add(0x0);
                return RawData[(int)CheckDefFields.NumArgs];
            }
            set
            {
                while (RawData.Count <= (int)CheckDefFields.NumArgs)
                    RawData.Add(0x0);
                RawData[(int)CheckDefFields.NumArgs] = value;
            }
        }
        /// <summary>
        /// The size of the raw check data
        /// </summary>
        public ushort CheckSize
        {
            get
            {
                return BitConverter.ToUInt16(RawData.GetBytes((int)CheckDefFields.CheckSize, sizeof(ushort)), 0);
            }
            set
            {
                RawData.SetBytes((int)CheckDefFields.CheckSize, BitConverter.GetBytes(value));
            }
        }
        /// <summary>
        /// The arguments pointer for the check
        /// </summary>
        public ushort ArgsPtr
        {
            get
            {
                return BitConverter.ToUInt16(RawData.GetBytes((int)CheckDefFields.ArgsPtr, sizeof(ushort)), 0);
            }
            set
            {
                RawData.SetBytes((int)CheckDefFields.ArgsPtr, BitConverter.GetBytes(value));
            }
        }
        /// <summary>
        /// The arguments of this check
        /// </summary>
        public string[] Arguments
        {
            get
            {
                if (ArgsPtr == 0 || NumArgs == 0)
                    return new string[0];
                string[] args = new string[NumArgs];
                int index = ArgsPtr;
                for(int i = 0; i < NumArgs; i++)
                {
                    args[i] = RawData.ReadString(ref index);
                }
                return args;
            }
        }

        /// <summary>
        /// Get the raw data of a check definition
        /// </summary>
        /// <param name="d"></param>
        public static implicit operator byte[](CheckDefinition d)
        {
            return d.RawData.ToArray();
        }

        /// <summary>
        /// Create a check definition from a data source and a pointer
        /// </summary>
        /// <param name="IData">Data, followed by the data pointer</param>
        public static implicit operator CheckDefinition((List<byte>,int) IData)
        {
            List<byte> Source = IData.Item1;
            CheckDefinition check = new CheckDefinition();
            int fileoffset = IData.Item2;
            try
            {
                check.RawData.AddRange(Source.GetBytes(fileoffset, 0xA)); //Force feed up to the file size data
                check.RawData.AddRange(Source.GetBytes(fileoffset + 0xA, check.CheckSize - 0xA)); //Pipe in the rest, accounting for the 10 bytes we force fed before
                return check;
            }
            catch
            {
            }
            return null;
        }
    }
}
