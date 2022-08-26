using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;

namespace fro_mod
{
    public class TrickCustomizer : MonoBehaviour
    {
        public int air_frame = 0;
        string actual_stance;
        string last_input;
        Vector3 origin;
        public void FixedUpdate()
        {
            bool run = PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Pop || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Release || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.InAir;
            string type_of_input = getTypeOfInput();

            if (type_of_input == null) run = false;

            if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.BeginPop) actual_stance = getActualStance();

            if (run)
            {
                float multiplier = getMultiplier(type_of_input);
                Vector3 lerpedRotation;
                Vector3 offset = getCustomRotation(actual_stance, type_of_input) * multiplier;
                offset /= 12;

                float anim = Controller.map01(air_frame, 0, getAnimationlength(actual_stance, type_of_input));
                anim = anim > 1 ? 1 : anim;

                Vector3 origin = PlayerController.Instance.boardController.boardRigidbody.transform.rotation.eulerAngles;
                if (PlayerController.Instance.GetBoardBackwards())
                {
                    lerpedRotation = Vector3.Lerp(origin, origin + new Vector3(-offset.x, offset.y, -offset.z), anim);
                }
                else
                {
                    lerpedRotation = Vector3.Lerp(origin, origin + new Vector3(offset.x, offset.y, offset.z), anim);
                }

                PlayerController.Instance.boardController.gameObject.transform.rotation = Quaternion.Euler(lerpedRotation);
                PlayerController.Instance.boardController.UpdateBoardPosition();
                air_frame++;
                if (last_input != type_of_input) air_frame = 0;
                last_input = type_of_input;
            }
            else
            {
                origin = PlayerController.Instance.boardController.boardRigidbody.transform.rotation.eulerAngles;
                air_frame = 0;
            }

            if (Main.settings.force_stick_backwards && (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.InAir || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Pop || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Release))
            {
                if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular && type_of_input == "right-backwards" || SettingsManager.Instance.stance == SkaterXL.Core.Stance.Goofy && type_of_input == "left-backwards")
                {
                    GameObject copy = new GameObject();
                    copy.transform.rotation = PlayerController.Instance.skaterController.skaterTransform.rotation;
                    copy.transform.position = PlayerController.Instance.skaterController.skaterTransform.position;
                    copy.transform.Translate(-2f, 0, 0, Space.Self);
                    PlayerController.Instance.skaterController.skaterRigidbody.AddForceAtPosition(-copy.transform.up * Main.settings.force_stick_backwards_multiplier, copy.transform.position, ForceMode.Impulse);
                    Destroy(copy);
                }
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
            return 12;
        }

        int getStanceId(string stance)
        {
            stance = stance == null ? getActualStance() : stance;
            int index = 0;

            for (int i = 0; i < Main.ui.StancesCustomizer.Length; i++)
            {
                if (Main.ui.StancesCustomizer[i] == stance) index = i;
            }
            return index;
        }
        public string getActualStance()
        {
            string stance = "Regular";
            if (PlayerController.Instance.IsSwitch)
            {
                stance = "Switch";
                if (PlayerController.Instance.animationController.skaterAnim.GetFloat("Nollie") == 0f)
                {
                    stance = "Fakie";
                }
            }
            else if (PlayerController.Instance.animationController.skaterAnim.GetFloat("Nollie") == 1f)
            {
                stance = "Nollie";
            }

            return stance;
        }

        string getTypeOfInput()
        {
            string type_of_input = null;
            if (PlayerController.Instance.inputController.LeftStick.rawInput.pos.y >= .25f) type_of_input = "left-forward";
            if (PlayerController.Instance.inputController.RightStick.rawInput.pos.y >= .25f) type_of_input = "right-forward";
            if (PlayerController.Instance.inputController.RightStick.rawInput.pos.y >= .25f && PlayerController.Instance.inputController.LeftStick.rawInput.pos.y >= .25f) type_of_input = "both-forward";

            if (PlayerController.Instance.inputController.LeftStick.rawInput.pos.y <= -.25f) type_of_input = "left-backwards";
            if (PlayerController.Instance.inputController.RightStick.rawInput.pos.y <= -.25f) type_of_input = "right-backwards";
            if (PlayerController.Instance.inputController.RightStick.rawInput.pos.y <= -.25f && PlayerController.Instance.inputController.LeftStick.rawInput.pos.y <= -.25f) type_of_input = "both-backwards";

            if (PlayerController.Instance.inputController.LeftStick.rawInput.pos.y <= .25f && PlayerController.Instance.inputController.RightStick.rawInput.pos.y <= .25f && PlayerController.Instance.inputController.LeftStick.rawInput.pos.y >= -.25f && PlayerController.Instance.inputController.RightStick.rawInput.pos.y >= -.25f)
            {
                if (PlayerController.Instance.inputController.LeftStick.rawInput.pos.x <= -.25f && PlayerController.Instance.inputController.RightStick.rawInput.pos.x >= .25f) type_of_input = "both-outside";
                if (PlayerController.Instance.inputController.LeftStick.rawInput.pos.x >= .25f && PlayerController.Instance.inputController.RightStick.rawInput.pos.x <= -.25f) type_of_input = "both-inside";
            }

            return type_of_input;
        }

        float getMultiplier(string input)
        {
            float multi = 0;
            if (input == "both-forward") multi = Controller.map01((PlayerController.Instance.inputController.RightStick.rawInput.pos.y + PlayerController.Instance.inputController.LeftStick.rawInput.pos.y) / 2, .25f, 1);
            if (input == "both-backwards") multi = Controller.map01((PlayerController.Instance.inputController.RightStick.rawInput.pos.y + PlayerController.Instance.inputController.LeftStick.rawInput.pos.y) / 2 * -1, .25f, 1);
            if (input == "left-forward") multi = Controller.map01(PlayerController.Instance.inputController.LeftStick.rawInput.pos.y, .25f, 1);
            if (input == "right-forward") multi = Controller.map01(PlayerController.Instance.inputController.RightStick.rawInput.pos.y, .25f, 1);
            if (input == "left-backwards") multi = Controller.map01(PlayerController.Instance.inputController.LeftStick.rawInput.pos.y * -1, .25f, 1);
            if (input == "right-backwards") multi = Controller.map01(PlayerController.Instance.inputController.RightStick.rawInput.pos.y * -1, .25f, 1);
            if (input == "both-outside") multi = Controller.map01((PlayerController.Instance.inputController.RightStick.rawInput.pos.x + -PlayerController.Instance.inputController.LeftStick.rawInput.pos.x) / 2, .25f, 1);
            if (input == "both-inside") multi = Controller.map01((-PlayerController.Instance.inputController.RightStick.rawInput.pos.x + PlayerController.Instance.inputController.LeftStick.rawInput.pos.x) / 2, .25f, 1);

            return multi;
        }
    }
}
