using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using UnityModManagerNet;

namespace fro_mod
{
    [HarmonyPatch(typeof(BoardController), nameof(BoardController.CatchRotation), new Type[] { typeof(float) })]
    class BoardControllerPatch
    {
        static bool Prefix(float p_mag)
        {
            if (Main.settings.catch_acc_enabled)
            {
                Traverse.Create(PlayerController.Instance.boardController).Field("_catchRotateSpeed").SetValue(0f);
                return true;
                //return Main.controller.forced_caught && Main.controller.forced_caught_count >= Main.settings.bounce_delay;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(BoardController), nameof(BoardController.LeaveFlipMode), new Type[] { })]
    class BoardControllerLeaveFlipModePatch
    {
        static bool Prefix()
        {
            if (Main.settings.catch_acc_enabled)
            {
                //PlayerController.Instance.boardController.boardRigidbody.angularVelocity = SkaterXL.Core.Mathd.GlobalAngularVelocityFromLocal(PlayerController.Instance.boardController.boardRigidbody, new Vector3(PlayerController.Instance.boardController.firstVel * PlayerController.Instance.boardController.angVelMult, PlayerController.Instance.boardController.secondVel * PlayerController.Instance.boardController.angVelMult, 10f));
                return false;
            }
            return true;
        }
    }

    

    [HarmonyPatch(typeof(BoardController), nameof(BoardController.SetCatchForwardRotation), new Type[] { })]
    class BoardControllerSetCatchForwardRotationPatch
    {
        static bool Prefix()
        {
            if (Main.settings.catch_acc_enabled)
            {
                return true;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(BoardController), nameof(BoardController.CatchRotation), new Type[] { })]
    class BoardControllerPatch2
    {
        static bool Prefix(BoardController __instance, ref Transform ____catchForwardRotation, ref float ____catchSignedAngle, ref Transform ___catchRotation)
        {
            if (Main.settings.catch_acc_enabled)
            {
                //if (Main.settings.experimental_dynamic_catch) return true;

                if (!Main.settings.snappy_catch)
                {
                    return Main.controller.forced_caught && Main.controller.forced_caught_count >= Main.settings.bounce_delay;
                }
                else
                {
                    if (Main.controller.forced_caught_count >= Main.settings.bounce_delay)
                    {
                        Vector3 from = Vector3.ProjectOnPlane(PlayerController.Instance.skaterController.transform.up, ____catchForwardRotation.forward);
                        ____catchSignedAngle = Vector3.SignedAngle(from, PlayerController.Instance.skaterController.skaterTransform.up, PlayerController.Instance.skaterController.skaterTransform.right);
                        PlayerController.Instance.AnimSetCatchAngle(____catchSignedAngle);
                        ___catchRotation.rotation = ____catchForwardRotation.rotation;
                        InvokeMethod(__instance, "PIDRotation", new object[] { ___catchRotation.rotation });
                    }
                    else
                    {
                        PlayerController.Instance.animationController.ForceUpdateAnimators();
                    }

                    return false;
                }
            }
            return true;
        }

        public static object InvokeMethod(object obj, string methodName, params object[] methodParams)
        {
            var methodParamTypes = methodParams?.Select(p => p.GetType()).ToArray() ?? new Type[] { };
            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            MethodInfo method = null;
            var type = obj.GetType();
            while (method == null && type != null)
            {
                method = type.GetMethod(methodName, bindingFlags, Type.DefaultBinder, methodParamTypes, null);
                type = type.BaseType;
            }

            return method?.Invoke(obj, methodParams);
        }
    }
}
