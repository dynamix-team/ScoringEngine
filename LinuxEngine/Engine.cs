//?installer.online
#if DEBUG
//#define ONLINE
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Engine.Core;

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
#if DEBUG
        public Engine()
#else
        internal Engine()
#endif
        {
            //?installer.init
        }

#if DEBUG

#endif

        protected override async Task Tick()
        {

#if DEBUG
            //?debug.tick
#else
            //?installer.tick
#endif
        }

#if ONLINE
        protected override bool IsOnline()
        {
            return true;
        }
#else
        protected override bool IsOnline()
        {
            return false;
        }
#endif

#if DEBUG
        //?debug.classes
#else
        //?installer.classes
#endif
    }
}
