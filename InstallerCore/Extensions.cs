using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Installer.Core
{
    /// <summary>
    /// Extensions for internal use
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Safely set bytes in a list
        /// </summary>
        /// <param name="RawData">The list to set bytes in</param>
        /// <param name="index">The index of the bytes</param>
        /// <param name="bytes">The byte array to write</param>
        public static void SetBytes(this List<byte> RawData, int index, byte[] bytes)
        {
            while (index + bytes.Length >= RawData.Count)
                RawData.Add(0x0);
            for (int i = 0; i < bytes.Length; i++)
            {
                RawData[i + index] = bytes[i];
            }
            return;
        }

        /// <summary>
        /// Safely get bytes from a list of bytes
        /// </summary>
        /// <param name="RawData">The list to read from</param>
        /// <param name="index">The index to read at</param>
        /// <param name="count">The amount of bytes to read</param>
        /// <returns></returns>
        public static byte[] GetBytes(this List<byte> RawData, int index, int count)
        {
            while (index + count > RawData.Count)
                RawData.Add(0x0);
            byte[] bytes = new byte[count];
            for (int i = 0; i < count; i++)
            {
                bytes[i] = RawData[i + index];
            }
            return bytes;
        }

        /// <summary>
        /// Read a null terminated string from a byte list
        /// </summary>
        /// <param name="RawData">The raw data to parse from</param>
        /// <param name="index">The index of the string. Gets modified to be past the null character</param>
        /// <returns></returns>
        public static string ReadString(this List<byte> RawData, ref int index)
        {
            string result = "";
            while(index < RawData.Count && RawData[index] != 0x0)
            {
                result += (char)RawData[index];
                index++;
            }
            index++;
            return result;
        }
    }
}
