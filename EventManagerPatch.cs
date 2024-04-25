using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fro_mod
{
    [HarmonyPatch(typeof(EventManager), nameof(EventManager.OnRespawn), new Type[] { })]
    class EventManagerPatchOnRespawn
    {
        static bool Prefix()
        {
            Main.controller.OnRespawn();
            return true;
        }
    }
}