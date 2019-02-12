using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Installer.Core
{
    /*
        WARNING: Strict naming rules apply to all templates! Failing to do so will result in templates being ignored by the engine!

            1. All templates much contain Template as the last word of their name (ex: CheckTemplate, ForensicsTemplate, etc)
            2. All template names are CASE SENSITIVE. They must be named exactly the same as the class they are tied to. (ex: class CheckTemplate -> CheckTypes.CheckTemplate)
            3. All template names much match their filenames (ex: class CheckTemplate -> CheckTypes.CheckTemplate -> CheckTemplate.cs)
    */

    /// <summary>
    /// All types of checks enumerated. Must match online database.
    /// </summary>
    public enum CheckTypes : ushort
    {
        CheckTemplate = 0,
        FileVersionTemplate = 1,
        RegTemplate = 2,
    }
}
