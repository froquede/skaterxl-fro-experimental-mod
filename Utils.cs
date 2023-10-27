using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityModManagerNet;

namespace fro_mod
{
    public static class Utils
    {
        public static float map01(float value, float min, float max)
        {
            return (value - min) * 1f / (max - min);
        }

        public static float map(float value, float leftMin, float leftMax, float rightMin, float rightMax)
        {
            return rightMin + (value - leftMin) * (rightMax - rightMin) / (leftMax - leftMin);
        }

        public static Quaternion GetLocalRotationRelativeToRootParent(Transform transform)
        {
            if (transform.parent == null)
            {
                return transform.localRotation;
            }
            else
            {
                Quaternion rootParentRotation = Quaternion.identity;
                Transform currentParent = transform.parent;
                while (currentParent.parent != null)
                {
                    rootParentRotation = currentParent.rotation * rootParentRotation;
                    currentParent = currentParent.parent;
                }
                return Quaternion.Inverse(rootParentRotation) * transform.rotation;
            }
        }

        public static float WrapAngle(float angle)
        {
            angle %= 360;
            if (angle > 180)
                return angle - 360;

            return angle;
        }

        public static float UnwrapAngle(float angle)
        {
            if (angle >= 0)
                return angle;

            angle = -angle % 360;

            return 360 - angle;
        }

        public static Vector3 TranslateWithRotation(Vector3 input, Vector3 translation, Quaternion rotation)
        {
            Vector3 rotatedTranslation = rotation * translation;
            Vector3 output = input + rotatedTranslation;
            return output;
        }

        public static Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, ref Vector3 currentVelocity, float smoothTime)
        {
            Vector3 c = current.eulerAngles;
            Vector3 t = target.eulerAngles;
            return Quaternion.Euler(
              Mathf.SmoothDampAngle(c.x, t.x, ref currentVelocity.x, smoothTime),
              Mathf.SmoothDampAngle(c.y, t.y, ref currentVelocity.y, smoothTime),
              Mathf.SmoothDampAngle(c.z, t.z, ref currentVelocity.z, smoothTime)
            );
        }

        public static bool AlmostEquals(this float double1, float double2, float precision)
        {
            return (Mathf.Abs(double1 - double2) <= precision);
        }

        public static bool IsGrabbing()
        {
            return PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Grabs || EventManager.Instance.IsGrabbing;
        }

        public static void Log(object arg)
        {
            UnityModManager.Logger.Log(arg.ToString());
        }

        public static bool isOllie()
        {
            if (PlayerController.Instance.boardController.firstVel <= -1.7f || PlayerController.Instance.boardController.firstVel >= 1.7f) return false;
            if (PlayerController.Instance.boardController.thirdVel <= -1f || PlayerController.Instance.boardController.thirdVel >= 1f) return false;

            if (Main.controller.target_left <= .01f && Main.controller.target_right <= .01f) return true;
            else return false;
        }

        public static bool isOneFootOllie()
        {
            bool flip = PlayerController.Instance.boardController.thirdVel <= -1f || PlayerController.Instance.boardController.thirdVel >= 1f;
            bool oneFootOff = (Main.controller.target_left <= .01f && Main.controller.target_right >= .99f) || (Main.controller.target_left >= .99f && Main.controller.target_right <= .01f);
            return !flip && oneFootOff;
        }

        public static Vector3 getDeltas()
        {
            float first = PlayerController.Instance.boardController.firstVel;
            float second = PlayerController.Instance.boardController.secondVel;
            float third = PlayerController.Instance.boardController.thirdVel;

            return new Vector3(first, second, third);
        }
    }
}
