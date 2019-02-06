using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Installer.Core
{
    /// <summary>
    /// A definition for a check
    /// </summary>
    public sealed class CheckDefinition
    {
        /*
               #CheckDef typedef
               uint16 CheckKey; //The key identifier for a vuln
               uint16 CheckID; //The identifier of this specific check for online scoring
               int NumPoints;
               byte NumArgs;
               string[NumArgs] Arguments;
       */

        private List<byte> RawData;

        public CheckDefinition(ref List<byte> OwningPackage, ushort pointer)
        {
            RawData = OwningPackage;
        }
    }
}
