using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fro_mod
{
    [HarmonyPatch(typeof(InputController), "UpdateSticks", new Type[] { })]
    class InputControllerPatch
    {
        static bool Prefix(InputController __instance)
        {
            if (Main.settings.catch_acc_enabled && PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Release)
            {
                return true;
                //return Main.controller.forced_caught && Main.controller.forced_caught_count >= Main.settings.bounce_delay;
            }
            return true;
        }
    }
}
