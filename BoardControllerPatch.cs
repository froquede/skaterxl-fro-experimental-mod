using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fro_mod
{
    [HarmonyPatch(typeof(BoardController), nameof(BoardController.CatchRotation), new Type[] { typeof(float) })]
    class BoardControllerPatch
    {
        static bool Prefix(float p_mag) {
            if(Main.settings.catch_acc_enabled)
            {
                return Main.controller.forced_caught && Main.controller.forced_caught_count >= Main.settings.bounce_delay;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(BoardController), nameof(BoardController.CatchRotation), new Type[] { })]
    class BoardControllerPatch2
    {
        static bool Prefix()
        {
            if (Main.settings.catch_acc_enabled)
            {
                return Main.controller.forced_caught && Main.controller.forced_caught_count >= Main.settings.bounce_delay;
            }
            return true;
        }
    }
}
