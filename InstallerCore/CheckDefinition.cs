using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                uint16 NumPoints;
                byte Flags;
                byte NumArgs;
                ushort CheckSize;
                ushort ArgsPtr;
                string[NumArgs] Arguments;
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

        private CheckDefinition()
        {
            RawData.AddRange(new byte[0x10]);
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
            private set
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
            set
            {
                if (ArgsPtr == 0)
                    return;
                foreach (string s in Arguments)
                {
                    RawData.RemoveRange(ArgsPtr, s.Length + 1);
                }
                NumArgs = (byte)value.Length;

                for (int i = value.Length - 1; i > -1; i--)
                {
                    RawData.Insert(ArgsPtr, 0);
                    RawData.InsertRange(ArgsPtr, Encoding.ASCII.GetBytes(value[i]));
                }

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
                check.RawData = Source.GetBytes(fileoffset, 0xA).ToList(); //Force feed the 10 byte header
                check.RawData.AddRange(Source.GetBytes(fileoffset + 0xA, check.CheckSize - 0xA)); //Pipe in the rest, accounting for the 10 bytes we force fed before
                return check;
            }
            catch
            {
            }
            return null;
        }

        /// <summary>
        /// Add an argument to the check definition
        /// </summary>
        /// <param name="c">The check to add an argument to</param>
        /// <param name="argument">The argument to add</param>
        /// <returns></returns>
        public static CheckDefinition operator +(CheckDefinition c, string argument)
        {
            if (c != null)
                c.AddArgument(argument);
            return c;
        }

        public static CheckDefinition operator -(CheckDefinition c, string argument)
        {
            if (c != null)
                c.RemoveArgument(argument);
            return c;
        }

        /// <summary>
        /// Add an argument to the list of args
        /// </summary>
        /// <param name="arg"></param>
        internal void AddArgument(string arg)
        {
            if (arg == null)
                return;
            List<string> args = new List<string>(Arguments);
            args.Add(arg);
            Arguments = args.ToArray();
        }

        /// <summary>
        /// Remove an argument from the list of args
        /// </summary>
        /// <param name="arg">The argument to remove</param>
        internal void RemoveArgument(string arg)
        {
            if (arg == null)
                return;
            List<string> args = new List<string>(Arguments);
            args.Remove(arg);
            Arguments = args.ToArray();
        }

#if DEBUG
        /// <summary>
        /// Create a debug check. Only exists in a debug build
        /// </summary>
        /// <param name="type">Type of check to implement</param>
        /// <param name="ID">The ID of the check</param>
        /// <param name="Points">The amount of points</param>
        /// <param name="Flags">The flags of the check (can be 0)</param>
        /// <param name="args">Aruments to pass to the check</param>
        /// <returns></returns>
        public static CheckDefinition DebugCheck(CheckTypes type, ushort ID, short Points, byte Flags, params string[] args)
        {
            CheckDefinition c = new CheckDefinition();
            c.CheckKey = (ushort)type;
            c.CheckID = ID;
            c.NumPoints = Points;
            c.Flags = Flags;
            c.CheckSize = 0;
            c.ArgsPtr = (ushort)c.RawData.Count; //super weird bug, this overwrites the arg ptr if the byte array is smaller before it adds the bytes.
            c.Arguments = args;
            c.CheckSize = (ushort)c.RawData.Count;
            return c;
        }
#endif
    }
}
