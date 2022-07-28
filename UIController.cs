using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidGUI;
using ReplayEditor;
using UnityEngine;
using UnityModManagerNet;

namespace fro_mod
{
    class FoldObj
    {
        public bool reference;
        public string text;

        public FoldObj(bool reference, string text)
        {
            this.reference = reference;
            this.text = text;
        }
    }

    public class UIController : MonoBehaviour
    {
        bool showMainMenu = false;
        private Rect MainMenuRect = new Rect(20, 20, Screen.width / 6, 20);
        public string[] States = new string[] {
            "Disabled",
            "Riding",
            "Setup",
            "BeginPop",
            "Pop",
            "InAir",
            "Release",
            "Impact",
            "Powerslide",
            "Manual",
            "Grinding",
            "EnterCoping",
            "ExitCoping",
            "Grabs",
            "Bailed",
            "Pushing",
            "Braking"
        };

        public void Update()
        {
            if (Input.GetKeyDown(Main.settings.Hotkey.keyCode))
            {
                if (showMainMenu)
                {
                    Close();
                }
                else
                {
                    Open();
                }
            }
        }

        private void Open()
        {
            MainMenuRect.height = 20;
            MainMenuRect.width = Screen.width / 6;
            showMainMenu = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void Close()
        {
            MainMenuRect.height = 20;
            MainMenuRect.width = Screen.width / 6;
            showMainMenu = false;
            Cursor.visible = false;
            Main.settings.Save(Main.modEntry);
        }

        private void OnGUI()
        {
            if (showMainMenu)
            {
                MainMenuRect = GUILayout.Window(666, MainMenuRect, MainMenu, "<b>Fro's Experimental Mod v1.9.0</b>");
            }
        }

        void MainSection()
        {
            //GUILayout.BeginVertical("Box");

            if (RGUI.Button(Main.settings.enabled, "Enabled"))
            {
                Main.settings.enabled = !Main.settings.enabled;
            }

            if (RGUI.Button(Main.settings.debug, "Debug"))
            {
                Main.settings.debug = !Main.settings.debug;
                Main.controller.checkDebug();
                Main.controller.getDeck();
            }

            //GUILayout.EndVertical();
        }

        void Fold(FoldObj obj)
        {
            if (GUILayout.Button("<b><size=14><color=#fdcb6e>" + (obj.reference ? "▶" : "▼") + "</color>" + obj.text + "</size></b>", "Label"))
            {
                obj.reference = !obj.reference;
                MainMenuRect.height = 20;
                MainMenuRect.width = Screen.width / 6;
            }
        }

        FoldObj about_fold = new FoldObj(true, "About");
        void AboutSection()
        {
            Fold(about_fold);
            if (!about_fold.reference)
            {
                GUILayout.BeginVertical("Box");
                GUILayout.Label("<b>fro's experimental mod v1.9.0 (28/07/2022)</b>");
                GUILayout.Label("Disclaimer: I'm not related to Easy Days Studios and i'm not responsible for any of your actions, use this mod at your own risk.");
                GUILayout.Label("This software is distributed 'as is', with no warranty expressed or implied, and no guarantee for accuracy or applicability to any purpose.");
                GUILayout.Label("This mod is not intended to harm the game or its respective developer in any purposeful way, its online functionality, or the game economy.");
                GUILayout.Label("I, the author of the mod, repudiate any type of practice or conduct that involves or promotes racism or any type of discrimination.");
                GUILayout.EndVertical();
            }
        }

        FoldObj lean_fold = new FoldObj(true, "Leaning / Wallrides");
        void LeanWallrideSection()
        {
            Fold(lean_fold);

            if (!lean_fold.reference)
            {
                GUILayout.BeginVertical("Box");
                if (RGUI.Button(Main.settings.lean, "Lean on double stick to the side"))
                {
                    Main.settings.lean = !Main.settings.lean;
                }

                if (Main.settings.lean)
                {
                    Main.settings.speed = RGUI.SliderFloat(Main.settings.speed, 0f, 1000f, 300f, "In air leaning speed");
                    // Main.settings.grind_speed = RGUI.SliderFloat(Main.settings.grind_speed, 0f, 240f, 40f, "Grinding Speed");
                    Main.settings.wallride_downforce = RGUI.SliderFloat(Main.settings.wallride_downforce, 0f, 200f, 80f, "Wallride downforce");
                    Main.settings.wait_threshold = (int)RGUI.SliderFloat(Main.settings.wait_threshold, 0f, 60f, 10f, "Hold X frames to activate");
                    Main.settings.input_threshold = RGUI.SliderFloat(Main.settings.input_threshold, 0f, 100f, 20f, "Valid stick vertical area");

                    if (RGUI.Button(Main.settings.swap_lean, "Invert input"))
                    {
                        Main.settings.swap_lean = !Main.settings.swap_lean;
                    }
                }
                GUILayout.EndVertical();
            }
        }

        FoldObj feet_fold = new FoldObj(true, "Dynamic feet - WIP");
        FoldObj feet_activation = new FoldObj(true, "Activation states");
        void FeetSection()
        {
            Fold(feet_fold);

            if (!feet_fold.reference)
            {
                GUILayout.BeginVertical("Box");
                RGUI.WarningLabel("This feature tries to place your feet on board dynamically so no stances are needed for floaty feet");
                RGUI.WarningLabel("If you already use a stance you can try combining it with rotation / position separately");
                if (RGUI.Button(Main.settings.feet_rotation, "Follow board rotation"))
                {
                    Main.settings.feet_rotation = !Main.settings.feet_rotation;
                }

                if (RGUI.Button(Main.settings.feet_offset, "Follow board position"))
                {
                    Main.settings.feet_offset = !Main.settings.feet_offset;
                }

                if (Main.settings.feet_offset)
                {
                    Main.settings.left_foot_offset = RGUI.SliderFloat(Main.settings.left_foot_offset, 0f, 2f, 1f, "Left shoe height offset");
                    Main.settings.right_foot_offset = RGUI.SliderFloat(Main.settings.right_foot_offset, 0f, 2f, 1f, "Right shoe height offset");
                }

                Fold(feet_activation);

                if (!feet_activation.reference)
                {
                    int count = 0;
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    foreach (var state in Enum.GetValues(typeof(PlayerController.CurrentState)))
                    {
                        if (count % 4 == 0)
                        {
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                        }

                        if (RGUI.Button(Main.settings.dynamic_feet_states[count], "     " + state.ToString(), GUILayout.Width(144)))
                        {
                            Main.settings.dynamic_feet_states[count] = !Main.settings.dynamic_feet_states[count];
                        }
                        count++;
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("     ");
                    if (GUILayout.Button("<b>Reset all</b>", "Button", GUILayout.Width(120f))) 
                    {
                        Main.checkLists(Main.modEntry, true);
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
            }
        }

        FoldObj hippie_fold = new FoldObj(true, "Hippie jump downforce");
        void HippieSection()
        {
            Fold(hippie_fold);

            if (!hippie_fold.reference)
            {
                GUILayout.BeginVertical("Box");
                if (RGUI.Button(Main.settings.hippie, "Hippie jump (B)"))
                {
                    Main.settings.hippie = !Main.settings.hippie;
                }
                if (Main.settings.hippie)
                {
                    Main.settings.HippieForce = RGUI.SliderFloat(Main.settings.HippieForce, 0f, 2f, 1f, "Hippie jump downforce");
                    Main.settings.HippieTime = RGUI.SliderFloat(Main.settings.HippieTime, 0.01f, 4f, 0.3f, "Hippie jump animation time");
                }
                GUILayout.EndVertical();
            }
        }

        FoldObj animpush_fold = new FoldObj(true, "Animations");
        void AnimationAndPushingSection()
        {
            Fold(animpush_fold);

            if (!animpush_fold.reference)
            {
                GUILayout.BeginVertical("Box");
                if (RGUI.Button(Main.settings.displacement_curve, "\"Realistic\" animation curve on pop"))
                {
                    Main.settings.displacement_curve = !Main.settings.displacement_curve;
                }
                RGUI.WarningLabel("This feature changes pop time and will affect your pop height");

                if (RGUI.Button(Main.settings.push_by_velocity, "Velocity based pushing"))
                {
                    Main.settings.push_by_velocity = !Main.settings.push_by_velocity;
                    if (Main.settings.push_by_velocity) Main.settings.sonic_mode = false;
                }
                if (RGUI.Button(Main.settings.sonic_mode, "Sonic© pushing"))
                {
                    Main.settings.sonic_mode = !Main.settings.sonic_mode;
                    if (Main.settings.sonic_mode) Main.settings.push_by_velocity = false;
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>Automatically wave on:</b>");
                Main.settings.wave_on = RGUI.SelectionPopup(Main.settings.wave_on, States);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>Automatically celebrate on:</b>");
                Main.settings.celebrate_on = RGUI.SelectionPopup(Main.settings.celebrate_on, States);
                GUILayout.EndHorizontal();

                if (RGUI.Button(Main.settings.bails, "Alternative bails"))
                {
                    Main.settings.bails = !Main.settings.bails;
                }

                GUILayout.EndVertical();
            }
        }

        FoldObj filmer_fold = new FoldObj(true, "Multiplayer filmer");
        FoldObj instructions_fold = new FoldObj(true, "Instructions");
        void FilmerSection()
        {
            Fold(filmer_fold);

            if (!filmer_fold.reference)
            {
                GUILayout.BeginVertical("Box");

                RGUI.WarningLabel("<b>Activated on pump input</b>");

                if (RGUI.Button(Main.settings.follow_mode_left, "Follow player with left hand"))
                {
                    Main.settings.follow_mode_left = !Main.settings.follow_mode_left;
                }

                if (RGUI.Button(Main.settings.follow_mode_right, "Follow player with right hand"))
                {
                    Main.settings.follow_mode_right = !Main.settings.follow_mode_right;
                }

                if (RGUI.Button(Main.settings.follow_mode_head, "Follow player with head"))
                {
                    Main.settings.follow_mode_head = !Main.settings.follow_mode_head;
                }

                if (Main.settings.follow_mode_left || Main.settings.follow_mode_right || Main.settings.follow_mode_head)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("<b>Player username</b>");
                    Main.settings.selected_player = RGUI.SelectionPopup(Main.settings.selected_player, Main.controller.getListOfPlayers());
                    GUILayout.EndHorizontal();

                    Main.settings.follow_target_offset = RGUI.SliderFloat(Main.settings.follow_target_offset, -1f, 1f, -.4f, "Camera target angle");
                    Main.settings.filmer_arm_angle = RGUI.SliderFloat(Main.settings.filmer_arm_angle, 0f, 90f, 37.5f, "Camera arm angle");
                    //Main.settings.lookat_speed = RGUI.SliderFloat(Main.settings.lookat_speed, 0f, 1f, 1f, "Speed");

                    if (RGUI.Button(Main.settings.camera_feet, "Put feet on the ground when pumping"))
                    {
                        Main.settings.camera_feet = !Main.settings.camera_feet;
                    }

                    GUILayout.Label("");

                    if (RGUI.Button(Main.settings.filmer_light, "Camera Light"))
                    {
                        Main.settings.filmer_light = !Main.settings.filmer_light;
                    }

                    if (Main.settings.filmer_light)
                    {
                        Main.settings.filmer_light_intensity = RGUI.SliderFloat(Main.settings.filmer_light_intensity, 0f, 10000f, 6000f, "Light intensity");
                        Main.settings.filmer_light_spotangle = RGUI.SliderFloat(Main.settings.filmer_light_spotangle, 0f, 360f, 120f, "Light angle");
                        Main.settings.filmer_light_range = RGUI.SliderFloat(Main.settings.filmer_light_range, 0f, 20f, 5f, "Light range");
                    }

                    GUILayout.Label("");
                    GUILayout.Label("<b>Keyframe creation</b>");
                    Fold(instructions_fold);
                    if(!instructions_fold.reference)
                    {
                        GUILayout.BeginHorizontal("Box");
                        GUILayout.Label("When this button is pressed this feature will delete all actual keyframes and create new ones based on the selected left / right camera hand / head, you can run it as many times as you want; I recommend you to cut the clip before creating the keyframes, you can watch the result in realtime enabling the keyframes.");
                        GUILayout.Label("Less keyframes will result in a smoother but less precise output");
                        GUILayout.EndHorizontal();
                    }

                    Main.settings.keyframe_sample = (int)RGUI.SliderFloat(Main.settings.keyframe_sample, 2f, 1000f, 50f, "Number of keyframes to create");
                    Main.settings.keyframe_fov = (int)RGUI.SliderFloat(Main.settings.keyframe_fov, 1f, 180f, 120f, "Keyframe field of view");
                    if (Main.controller.keyframe_state == true)
                    {
                        if (GUILayout.Button("Cancel creation", GUILayout.Height(32)))
                        {
                            Main.controller.keyframe_state = false;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Create keyframes", GUILayout.Height(32)))
                        {
                            ReplayEditorController.Instance.cameraController.DeleteAllKeyFrames();
                            Main.controller.keyframe_state = true;
                        }
                    }

                }
                GUILayout.EndVertical();
            }
        }

        FoldObj multi_fold = new FoldObj(true, "Multiplayer options");
        void MultiSection()
        {
            Fold(multi_fold);

            if (!multi_fold.reference)
            {
                GUILayout.BeginVertical("Box");
                if (RGUI.Button(Main.settings.reset_inactive, "Disable multiplayer AFK timeout"))
                {
                    Main.settings.reset_inactive = !Main.settings.reset_inactive;
                }
                if (RGUI.Button(Main.settings.disable_popup, "Disable multiplayer popup messages"))
                {
                    Main.settings.disable_popup = !Main.settings.disable_popup;
                    Main.controller.DisableMultiPopup(Main.settings.disable_popup);
                }
                Main.settings.multiplayer_lobby_size = (int)RGUI.SliderFloat(Main.settings.multiplayer_lobby_size, 1f, 35f, 20f, "Multiplayer lobby size");
                if (Main.settings.multiplayer_lobby_size > 35) Main.settings.multiplayer_lobby_size = 35;
                if (Main.settings.multiplayer_lobby_size < 1) Main.settings.multiplayer_lobby_size = 1;
                GUILayout.EndVertical();
            }
        }

        FoldObj multichat_fold = new FoldObj(true, "Multiplayer chat messages");
        void ChatSection()
        {
            Fold(multichat_fold);

            if (!multichat_fold.reference)
            {
                GUILayout.BeginVertical("Box");
                if (RGUI.Button(Main.settings.chat_messages, "Enable chat messages"))
                {
                    Main.settings.chat_messages = !Main.settings.chat_messages;
                }
                if (Main.settings.chat_messages)
                {
                    RGUI.WarningLabel("There are in total 11 pages of predefined messages you can use");
                    Main.settings.left_page = (int)RGUI.SliderFloat(Main.settings.left_page, 0f, 10f, 0f, "Left d-pad page");
                    Main.settings.right_page = (int)RGUI.SliderFloat(Main.settings.right_page, 0f, 10f, 1f, "Right d-pad page");
                    Main.settings.up_page = (int)RGUI.SliderFloat(Main.settings.up_page, 0f, 10f, 2f, "Up d-pad page");
                    Main.settings.down_page = (int)RGUI.SliderFloat(Main.settings.down_page, 0f, 10f, 3f, "Down d-pad page");
                }

                GUILayout.EndVertical();
            }
        }

        FoldObj camera_fold = new FoldObj(true, "Camera options");
        void CameraSection()
        {
            Fold(camera_fold);

            if (!camera_fold.reference)
            {
                GUILayout.BeginVertical("Box");
                if (RGUI.Button(Main.settings.camera_avoidance, "Alpha camera obstacle avoidance"))
                {
                    Main.settings.camera_avoidance = !Main.settings.camera_avoidance;
                    Main.controller.DisableCameraCollider();
                }
                GUILayout.EndVertical();
            }
        }

        FoldObj grinds_fold = new FoldObj(true, "Grinds options");
        void GrindsSection()
        {
            Fold(grinds_fold);

            if (!grinds_fold.reference)
            {
                GUILayout.BeginVertical("Box");
                Main.settings.GrindFlipVerticality = RGUI.SliderFloat(Main.settings.GrindFlipVerticality, -1f, 1f, 0f, "Flip verticality out of grinds");
                GUILayout.EndVertical();
            }
        }

        FoldObj body_fold = new FoldObj(true, "Player body");
        void BodySection()
        {
            Fold(body_fold);

            if (!body_fold.reference)
            {
                GUILayout.BeginVertical("Box");
                Main.settings.custom_scale.x = RGUI.SliderFloat(Main.settings.custom_scale.x, 0f, 2f, 1f, "Scale body x");
                Main.settings.custom_scale.y = RGUI.SliderFloat(Main.settings.custom_scale.y, 0f, 2f, 1f, "Scale body y");
                Main.settings.custom_scale.z = RGUI.SliderFloat(Main.settings.custom_scale.z, 0f, 2f, 1f, "Scale body z");
                Main.settings.custom_scale_head = RGUI.SliderFloat(Main.settings.custom_scale_head, 0f, 4f, 1f, "Head scale");
                Main.settings.custom_scale_hand_l = RGUI.SliderFloat(Main.settings.custom_scale_hand_l, 0f, 4f, 1f, "Left hand scale");
                Main.settings.custom_scale_hand_r = RGUI.SliderFloat(Main.settings.custom_scale_hand_r, 0f, 4f, 1f, "Right hand scale");
                Main.settings.custom_scale_foot_l = RGUI.SliderFloat(Main.settings.custom_scale_foot_l, 0f, 4f, 1f, "Left foot scale");
                Main.settings.custom_scale_foot_r = RGUI.SliderFloat(Main.settings.custom_scale_foot_r, 0f, 4f, 1f, "Right foot scale");
                GUILayout.EndVertical();

                GUILayout.BeginVertical("Box");
                Main.settings.left_hand_weight = RGUI.SliderFloat(Main.settings.left_hand_weight, 0f, 5f, 1f, "Left hand weight");
                Main.settings.right_hand_weight = RGUI.SliderFloat(Main.settings.right_hand_weight, 0f, 5f, 1f, "Right hand weight");
                GUILayout.EndVertical();
            }

        }

        FoldObj skate_fold = new FoldObj(true, "Skate options");
        void SkateSection()
        {
            Fold(skate_fold);

            if (!skate_fold.reference)
            {
                GUILayout.BeginVertical("Box");

                if (RGUI.Button(Main.settings.wobble, "Wobble on high speed"))
                {
                    Main.settings.wobble = !Main.settings.wobble;
                }

                if (Main.settings.wobble)
                {
                    Main.settings.wobble_offset = RGUI.SliderFloat(Main.settings.wobble_offset, 0f, 16f, 4f, "Minimum velocity for wobbling");
                }

                GUILayout.Label("");

                if (RGUI.Button(Main.settings.BetterDecay, "Better friction / decay"))
                {
                    Main.settings.BetterDecay = !Main.settings.BetterDecay;
                }

                Main.settings.decay = RGUI.SliderFloat(Main.settings.decay, 0f, 10f, 3.25f, "Friction force");

                GUILayout.Label("");

                Main.settings.nose_tail_collider = RGUI.SliderFloat(Main.settings.nose_tail_collider, 0f, 2f, 1f, "Nose and tail collider height");

                GUILayout.EndVertical();
            }
        }

        private void MainMenu(int windowID)
        {
            GUI.backgroundColor = Color.red;
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            MainSection();
            if (!Main.settings.enabled) return;

            AboutSection();
            LeanWallrideSection();
            FeetSection();
            HippieSection();
            AnimationAndPushingSection();
            MultiSection();
            ChatSection();
            FilmerSection();
            CameraSection();
            GrindsSection();
            BodySection();
            SkateSection();
        }
    }
}
