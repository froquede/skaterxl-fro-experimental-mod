using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fro_mod
{
    [HarmonyPatch(typeof(HeadIK), "IsAllowedAnimation", new Type[] { })]
    class HeadIKPatch
    {
        static void Postfix(ref bool __result)
        {
            __result = !Main.controller.inState && Main.controller.head_frame == 0 && Main.controller.step_head <= .01f;
        }
    }
}
