using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fro_mod
{
    [HarmonyPatch(typeof(StopGameWhileEnabled), "OnEnable", new Type[] { typeof(void) })]
    class StopWhileEnabledPatch
    {
        static bool Prefix()
        {
            return false;
        }
    }
}