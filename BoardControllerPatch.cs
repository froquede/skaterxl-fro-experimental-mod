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

namespace fro_mod
{
    [HarmonyPatch(typeof(EventManager), nameof(EventManager.OnCatched), new Type[] { typeof(bool), typeof(bool) })]
    class OnCatchedPatch
    {
        static bool Prefix(bool caughtRight, bool caughtLeft)
        {
            return Main.controller.shouldRunCatch();
        }
    }

    [HarmonyPatch(typeof(StickInput), nameof(StickInput.OnStickPressed), new Type[] { typeof(bool) })]
    class OnStickPressedPatch
    {
        static bool Prefix(bool p_right)
        {
            return Main.controller.shouldRunCatch();
        }
    }

    [HarmonyPatch(typeof(IKController), nameof(IKController.SetIK), new Type[] { })]
    class SetIKPatch
    {
        static bool Prefix()
        {
            return Main.controller.shouldRunCatch() && Main.controller.IsGrounded();
        }
    }

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.SetTargetToMaster), new Type[] { })]
    class SetTargetToMasterPatch
    {
        static bool Prefix()
        {
            PlayerController.Instance.skaterController.skaterRigidbody.angularVelocity = PlayerController.Instance.boardController.boardRigidbody.angularVelocity;
            PlayerController.Instance.skaterController.skaterRigidbody.velocity = PlayerController.Instance.boardController.boardRigidbody.velocity;
            if (!Main.controller.IsGrabbing() && !Main.controller.was_leaning) PlayerController.Instance.boardController.boardRigidbody.angularVelocity = Vector3.zero;
            PlayerController.Instance.movementMaster = PlayerController.MovementMaster.Target;

            return false;
        }
    }
}
