using GameManagement;
using SkaterXL.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityModManagerNet;

namespace xlperimental_mod
{
    public class TrickCustomizer : MonoBehaviour
    {
        public int air_frame = 0;
        string actual_stance;
        List<string> last_input = new List<string>(0);
        Vector3 origin;
        GameObject copy;
        public void FixedUpdate()
        {
            if (!Main.settings.trick_customization) return;

            bool run = GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Pop || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Release || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.InAir || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Impact;
            run = !run ? GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Grinding && Main.controller.last_state != PlayerStateEnum.Grinding.ToString() : run;
            run = !run ? GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Manual && Main.controller.last_state != PlayerStateEnum.Manual.ToString() : run;

            List<string> type_of_input = getTypeOfInput();

            if (type_of_input.Count == 0) run = false;

            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.BeginPop) actual_stance = getActualStance();

            if (run)
            {
                for (int i = 0; i < type_of_input.Count; i++)
                {
                    string input = type_of_input[i];
                    float multiplier = getMultiplier(input);
                    Quaternion lerpedRotation;
                    Vector3 offset = getCustomRotation(actual_stance, input) * multiplier;
                    offset /= 12;

                    Vector3 offset_position = (getCustomPosition(actual_stance, input) / 10f);

                    float anim = Controller.map01(air_frame, 0, getAnimationlength(actual_stance, input));
                    anim = anim > 1 ? 1 : anim;

                    Quaternion origin = GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.transform.rotation;
                    if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.GetBoardBackwards())
                    {
                        lerpedRotation = Quaternion.Slerp(origin, Quaternion.Euler(origin.eulerAngles.x - offset.x, origin.eulerAngles.y + offset.y, origin.eulerAngles.z - offset.z), anim);
                    }
                    else
                    {
                        lerpedRotation = Quaternion.Slerp(origin, Quaternion.Euler(origin.eulerAngles.x + offset.x, origin.eulerAngles.y + offset.y, origin.eulerAngles.z + offset.z), anim);
                    }

                    GameStateMachine.Instance.MainPlayer.gameplay.boardController.gameObject.transform.rotation = lerpedRotation;
                    if (Main.settings.trick_customization_mode > 0)
                    {
                        offset_position = new Vector3(offset_position.x * multiplier, offset_position.y, offset_position.y * multiplier);
                        if (Main.settings.trick_customization_mode == 1 && GameStateMachine.Instance.MainPlayer.gameplay.playerData.GetBoardBackwards()) offset_position = new Vector3(offset_position.x * -1, offset_position.y, offset_position.y * -1);
                        Vector3 target = Main.TranslateWithRotation(GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.transform.position, offset_position, Main.settings.trick_customization_mode == 1 ? GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.transform.rotation : Quaternion.LookRotation(new Vector3(GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.velocity.x, 0, GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.velocity.z)));
                        if ((Main.settings.trick_customization_mode == 2 && GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.velocity != Vector3.zero) || Main.settings.trick_customization_mode == 1)
                        {
                            GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.transform.position = Vector3.Lerp(GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.transform.position, target, anim);
                            GameStateMachine.Instance.MainPlayer.gameplay.playerData.board.CurrentPositionTarget = GameStateMachine.Instance.MainPlayer.gameplay.boardController.gameObject.transform.position;
                            GameStateMachine.Instance.MainPlayer.gameplay.playerData.skater.skaterRigidbody.position = GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.transform.position;
                        }
                    }

                    GameStateMachine.Instance.MainPlayer.gameplay.playerData.board.CurrentRotationTarget = GameStateMachine.Instance.MainPlayer.gameplay.boardController.gameObject.transform.rotation;

                    air_frame++;

                    if (!last_input.SequenceEqual(type_of_input)) air_frame = 0;
                    last_input = type_of_input;
                }
            }
            else
            {
                origin = GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.transform.rotation.eulerAngles;
                air_frame = 0;
            }

            if (Main.settings.force_stick_backwards && (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.InAir || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Pop || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Release))
            {
                for (int i = 0; i < type_of_input.Count; i++)
                {
                    string input = type_of_input[i];
                    if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Regular && input == "right-backwards" || GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Goofy && input == "left-backwards")
                    {
                        if (!copy) copy = new GameObject();
                        copy.transform.rotation = GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRoot.rotation;
                        copy.transform.position = GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRoot.position;
                        copy.transform.Translate(-2f, 0, 0, Space.Self);
                        GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRigidbody.AddForceAtPosition(-copy.transform.up * Main.settings.force_stick_backwards_multiplier, copy.transform.position, ForceMode.Impulse);
                    }
                }
            }
        }

        float last_press = 0;
        float limit_press = .5f;
        public void LateUpdate()
        {
            if (GameStateMachine.Instance.CurrentState.GetType() == typeof(PlayState))
            {
                if ((GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Pushing || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Riding || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Impact))
                {
                    if (GameStateMachine.Instance.MainPlayer.gameplay.inputController.rewiredPlayer.GetButtonSinglePressHold("Right Stick Button") && Time.unscaledTime - last_press >= limit_press)
                    {
                        Main.settings.trick_customization_mode += 1;
                        if (Main.settings.trick_customization_mode > 2) Main.settings.trick_customization_mode = 0;

                        if (Main.settings.trick_customization_mode == 0) NotificationManager.Instance.ShowNotification($"only rotation", limit_press, false, NotificationManager.NotificationType.Normal, TextAlignmentOptions.TopRight, 0.1f);
                        if (Main.settings.trick_customization_mode == 1) NotificationManager.Instance.ShowNotification($"skate mode", limit_press, false, NotificationManager.NotificationType.Normal, TextAlignmentOptions.TopRight, 0.1f);
                        if (Main.settings.trick_customization_mode == 2) NotificationManager.Instance.ShowNotification($"velocity mode", limit_press, false, NotificationManager.NotificationType.Normal, TextAlignmentOptions.TopRight, 0.1f);

                        last_press = Time.unscaledTime;
                    }
                    if (GameStateMachine.Instance.MainPlayer.gameplay.inputController.rewiredPlayer.GetButtonDoublePressDown("Right Stick Button"))
                    {
                        Main.settings.trick_customization = !Main.settings.trick_customization;
                        NotificationManager.Instance.ShowNotification($"Trick customization { (Main.settings.trick_customization ? "enabled" : "disabled") }", 1f, false, NotificationManager.NotificationType.Normal, TextAlignmentOptions.TopRight, 0.1f);
                    }
                }
                else last_press = Time.unscaledTime;
            }
        }

        public Vector3 getCustomRotation(string stance, string input)
        {
            int id = getStanceId(stance);
            if (input == "both-forward") return Main.settings.ollie_customization_rotation[id];
            if (input == "both-backwards") return Main.settings.ollie_customization_rotation_backwards[id];
            if (input == "left-forward") return Main.settings.ollie_customization_rotation_left_stick[id];
            if (input == "right-forward") return Main.settings.ollie_customization_rotation_right_stick[id];
            if (input == "left-backwards") return Main.settings.ollie_customization_rotation_left_stick_backwards[id];
            if (input == "right-backwards") return Main.settings.ollie_customization_rotation_right_stick_backwards[id];
            if (input == "both-outside") return Main.settings.ollie_customization_rotation_both_outside[id];
            if (input == "both-inside") return Main.settings.ollie_customization_rotation_both_inside[id];
            if (input == "left-left") return Main.settings.ollie_customization_rotation_left2left[id];
            if (input == "left-right") return Main.settings.ollie_customization_rotation_left2right[id];
            if (input == "right-left") return Main.settings.ollie_customization_rotation_right2left[id];
            if (input == "right-right") return Main.settings.ollie_customization_rotation_right2right[id];
            if (input == "both-left") return Main.settings.ollie_customization_rotation_both2left[id];
            if (input == "both-right") return Main.settings.ollie_customization_rotation_both2right[id];
            return Vector3.zero;
        }

        public Vector3 getCustomPosition(string stance, string input)
        {
            int id = getStanceId(stance);
            if (input == "both-forward") return Main.settings.ollie_customization_position[id];
            if (input == "both-backwards") return Main.settings.ollie_customization_position_backwards[id];
            if (input == "left-forward") return Main.settings.ollie_customization_position_left_stick[id];
            if (input == "right-forward") return Main.settings.ollie_customization_position_right_stick[id];
            if (input == "left-backwards") return Main.settings.ollie_customization_position_left_stick_backwards[id];
            if (input == "right-backwards") return Main.settings.ollie_customization_position_right_stick_backwards[id];
            if (input == "both-outside") return Main.settings.ollie_customization_position_both_outside[id];
            if (input == "both-inside") return Main.settings.ollie_customization_position_both_inside[id];
            if (input == "left-left") return Main.settings.ollie_customization_position_left2left[id];
            if (input == "left-right") return Main.settings.ollie_customization_position_left2right[id];
            if (input == "right-left") return Main.settings.ollie_customization_position_right2left[id];
            if (input == "right-right") return Main.settings.ollie_customization_position_right2right[id];
            if (input == "both-left") return Main.settings.ollie_customization_position_both2left[id];
            if (input == "both-right") return Main.settings.ollie_customization_position_both2right[id];
            return Vector3.zero;
        }

        public float getAnimationlength(string stance, string input)
        {
            int id = getStanceId(stance);
            if (input == "both-forward") return Main.settings.ollie_customization_length[id];
            if (input == "both-backwards") return Main.settings.ollie_customization_length_backwards[id];
            if (input == "left-forward") return Main.settings.ollie_customization_length_left_stick[id];
            if (input == "right-forward") return Main.settings.ollie_customization_length_right_stick[id];
            if (input == "left-backwards") return Main.settings.ollie_customization_length_left_stick_backwards[id];
            if (input == "right-backwards") return Main.settings.ollie_customization_length_right_stick_backwards[id];
            if (input == "both-outside") return Main.settings.ollie_customization_length_both_outside[id];
            if (input == "both-inside") return Main.settings.ollie_customization_length_both_inside[id];
            if (input == "left-left") return Main.settings.ollie_customization_length_left2left[id];
            if (input == "left-right") return Main.settings.ollie_customization_length_left2right[id];
            if (input == "right-left") return Main.settings.ollie_customization_length_right2left[id];
            if (input == "right-right") return Main.settings.ollie_customization_length_right2right[id];
            if (input == "both-left") return Main.settings.ollie_customization_length_both2left[id];
            if (input == "both-right") return Main.settings.ollie_customization_length_both2right[id];
            return 24;
        }

        int getStanceId(string stance)
        {
            stance = stance == null ? getActualStance() : stance;
            int index = 0;

            for (int i = 0; i < EnumHelper.StancesCustomizer.Length; i++)
            {
                if (EnumHelper.StancesCustomizer[i] == stance) index = i;
            }
            return index;
        }
        public string getActualStance()
        {
            string stance = "Regular";
            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.IsSwitch)
            {
                stance = "Switch";
                if (GameStateMachine.Instance.MainPlayer.gameplay.animationController.skaterAnim.GetFloat("Nollie") == 0f)
                {
                    stance = "Fakie";
                }
            }
            else if (GameStateMachine.Instance.MainPlayer.gameplay.animationController.skaterAnim.GetFloat("Nollie") == 1f)
            {
                stance = "Nollie";
            }

            return stance;
        }

        public List<string> getTypeOfInput()
        {
            List<string> type_of_input = new List<string>(0);

            double angle_left = calcAngleDegrees(GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.x, GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.y);
            double angle_right = calcAngleDegrees(GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.x, GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.y);

            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.y >= .05f || GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.x >= .05f || GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.y <= -.1f || GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.x <= -.1f)
            {
                if (angle_left <= 45 && angle_left >= -44) type_of_input.Add("left-right");
                if (angle_left <= 135 && angle_left >= 46) type_of_input.Add("left-forward");
                if (angle_left >= -135 && angle_left <= -45) type_of_input.Add("left-backwards");
                if ((angle_left >= -180 && angle_left <= -136) || (angle_left <= 180 && angle_left >= 136)) type_of_input.Add("left-left");
            }

            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.y >= .05f || GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.x >= .05f || GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.y <= -.1f || GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.x <= -.1f)
            {
                if (angle_right <= 45 && angle_right >= -44) type_of_input.Add("right-right");
                if (angle_right <= 135 && angle_right >= 46) type_of_input.Add("right-forward");
                if (angle_right >= -135 && angle_right <= -45) type_of_input.Add("right-backwards");
                if ((angle_right >= -180 && angle_right <= -136) || (angle_right <= 180 && angle_right >= 136)) type_of_input.Add("right-left");
            }

            if (type_of_input.Contains("right-right") && type_of_input.Contains("left-right")) type_of_input.Add("both-right");
            if (type_of_input.Contains("right-left") && type_of_input.Contains("left-left")) type_of_input.Add("both-left");
            if (type_of_input.Contains("right-backwards") && type_of_input.Contains("left-backwards")) type_of_input.Add("both-backwards");
            if (type_of_input.Contains("right-forward") && type_of_input.Contains("left-forward")) type_of_input.Add("both-forward");
            if (type_of_input.Contains("right-left") && type_of_input.Contains("left-right")) type_of_input.Add("both-inside");
            if (type_of_input.Contains("right-right") && type_of_input.Contains("left-left")) type_of_input.Add("both-outside");

            if (Main.settings.debug)
            {
                for (int i = 0; i < type_of_input.Count; i++)
                {
                    UnityModManager.Logger.Log(type_of_input[i]);
                }
            }

            return type_of_input;
        }

        double calcAngleDegrees(float x, float y)
        {
            return Math.Atan2(y, x) * 180f / Math.PI;
        }

        float getMultiplier(string input)
        {
            float multi = 0;

            if (input == "both-forward") multi = Controller.map01((GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.y + GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.y) / 2, .05f, 1);
            if (input == "both-backwards") multi = Controller.map01((GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.y + GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.y) / 2 * -1, .05f, 1);
            if (input == "left-forward") multi = Controller.map01(GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.y, .05f, 1);
            if (input == "right-forward") multi = Controller.map01(GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.y, .05f, 1);
            if (input == "left-backwards") multi = Controller.map01(GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.y * -1, .05f, 1);
            if (input == "right-backwards") multi = Controller.map01(GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.y * -1, .05f, 1);
            if (input == "both-outside") multi = Controller.map01((GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.x + -GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.x) / 2, .05f, 1);
            if (input == "both-inside") multi = Controller.map01((-GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.x + GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.x) / 2, .05f, 1);
            if (input == "left-left") multi = Controller.map01(GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.x * -1, .05f, 1);
            if (input == "left-right") multi = Controller.map01(GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.x, .05f, 1);
            if (input == "right-left") multi = Controller.map01(GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.x * -1, .05f, 1);
            if (input == "right-right") multi = Controller.map01(GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.x, .05f, 1);
            if (input == "both-left") multi = Controller.map01((GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.x + GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.x) / 2 * -1, .05f, 1);
            if (input == "both-right") multi = Controller.map01((GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.x + GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.x) / 2, .05f, 1);

            return multi;
        }
    }
}
