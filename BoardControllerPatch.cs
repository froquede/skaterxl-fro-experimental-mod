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

    [HarmonyPatch(typeof(BoardController), nameof(BoardController.LerpCopingTarget), new Type[] { })]
    class LerpCopingTargetPatch
    {
        static bool Prefix()
        {
            if (Main.settings.enabled && Main.settings.alternative_coping)
            {
                Traverse t = Traverse.Create(PlayerController.Instance.boardController);
                Transform targetLerp = (Transform)t.Field("_copingTargetLerp").GetValue();
                Transform target = (Transform)t.Field("_copingTarget").GetValue();
                targetLerp.position = Vector3.SmoothDamp(targetLerp.position, target.position, ref Main.controller.copingTargetVelocity, Main.settings.coping_part_speed);
                return false;
            }
            else return true;
        }
    }

    [HarmonyPatch(typeof(BoardController), nameof(BoardController.SetCopingTargetLerp), new Type[] { typeof(Vector3) })]
    class SetCopingTargetLerpPatch
    {
        static bool Prefix(Vector3 _pos)
        {
            if (Main.settings.enabled && Main.settings.alternative_coping)
            {
                Traverse t = Traverse.Create(PlayerController.Instance.boardController);
                Transform targetLerp = (Transform)t.Field("_copingTargetLerp").GetValue();

                float num = Main.settings.coping_part_distance;
                for (int j = 0; j < 6; j++)
                {
                    Vector3 closest = PlayerController.Instance.boardController.GetClosestCopingTarget(j);
                    float diff = Vector3.Distance(closest, _pos);
                    if (diff <= num)
                    {
                        num = diff;
                        targetLerp.position = closest;
                    }
                }

                return false;
            }
            else return true;
        }
    }

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.CheckForCopingGrind), new Type[] { typeof(SplineComputer), typeof(Vector2), typeof(Vector2) })]
    class CheckForCopingGrindPatch
    {
        static bool Prefix(SplineComputer _splineComputer, Vector2 _leftStick, Vector2 _rightStick, PlayerController __instance)
        {
            if (Main.settings.enabled && Main.settings.alternative_coping)
            {
                if (_leftStick.magnitude > 0.15f || _rightStick.magnitude > 0.15f)
                {
                    if (_splineComputer == null) return true;
                    Traverse t = Traverse.Create(PlayerController.Instance);
                    double percent = (double)t.Field("percent").GetValue();
                    SplineResult _splineResult = (SplineResult)t.Field("_splineResult").GetValue();

                    percent = _splineComputer.Project(__instance.boardController.boardTransform.position, 6, 0.01f, .99f);
                    _splineResult = _splineComputer.Evaluate(percent);
                    Vector3 direction = _splineResult.direction;

                    if (__instance.boardController.boardRigidbody.velocity.magnitude > 10f)
                    {
                        return false;
                    }

                    float _x = (float)t.Field("_x").GetValue();
                    float _y = (float)t.Field("_y").GetValue();

                    _x = Mathf.Clamp(_leftStick.x - _rightStick.x, -1f, 1f);
                    _y = Mathf.Clamp(_leftStick.y + _rightStick.y, -1f, 1f);

                    float _yAngle = (float)t.Field("_yAngle").GetValue();
                    _yAngle = _x * 90f;

                    Vector3 from = Vector3.ProjectOnPlane(__instance.boardController.GetClosestBoardForwardToVelocity(), _splineResult.normal);

                    float _angleToSpline = (float)t.Field("_angleToSpline").GetValue();
                    _angleToSpline = Vector3.Angle(from, direction);

                    Quaternion _newRot = (Quaternion)t.Field("_newRot").GetValue();
                    _newRot = Quaternion.AngleAxis(_yAngle, _splineResult.normal) * __instance.boardController.boardTransform.rotation;
                    Vector3 from2 = _newRot * Vector3.forward;

                    t.Field("_newAngleToGrindDir").SetValue(Vector3.Angle(from2, direction));
                    if (Vector3.Distance(__instance.boardController.SetCopingTarget(_y, Vector3.Angle(__instance.boardController.boardTransform.forward, direction)), _splineResult.position) < Main.settings.coping_part_distance)
                    {
                        __instance.playerSM.TransitionToEnterCopingStateSM(__instance.boardController._copingTarget, _newRot, _splineComputer, direction);
                    }
                }

                return false;
            }
            else return true;
        }
    }
}
