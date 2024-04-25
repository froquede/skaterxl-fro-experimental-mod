using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using UnityModManagerNet;
using SkaterXL.Core;
using Dreamteck.Splines;

namespace fro_mod
{
    [HarmonyPatch(typeof(SkaterController), "InAirRotation", new Type[] { typeof(float) })]
    class InAirRotationPatch
    {
        static bool Prefix(float p_slerp, SkaterController __instance)
        {
            return Main.controller.InAirRotationPatch(p_slerp, __instance);
        }
    }

    [HarmonyPatch(typeof(SkaterController), "AddTurnTorque", new Type[] { typeof(float) })]
    class AddTurnTorquePatch
    {
        static bool Prefix(float p_value, SkaterController __instance)
        {
            if (Main.controller.enabled && Main.settings.customTurn)
            {
                p_value *= (Main.settings.customTurnMultiplier / 10f);
                __instance.skaterRigidbody.AddTorque(__instance.skaterTransform.up * p_value * Time.deltaTime * Main.settings.customTurnSpeed, ForceMode.VelocityChange);
                __instance.leanProxy.AddTorque(__instance.skaterTransform.up * p_value * Time.deltaTime * Main.settings.customTurnSpeed, ForceMode.VelocityChange);

                return false;
            }
            else return true;
        }
    }
}
