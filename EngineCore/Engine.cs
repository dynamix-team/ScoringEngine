﻿//?installer.online
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
    internal class Engine : EngineFrame
    {
        internal Engine()
        {
            //?installer.init
        }

        protected override Task Tick()
        {
            //?installer.tick
            return base.Tick();
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
    }
}
