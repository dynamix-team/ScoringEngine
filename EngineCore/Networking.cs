using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Core
{
    /// <summary>
    /// Networking control center
    /// </summary>
    internal static class Networking
    {
        /// <summary>
        /// The size of check batches to send to the server
        /// </summary>
        internal static byte BatchSize = 128;
    }
}
