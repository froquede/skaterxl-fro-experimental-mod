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

        public string[] StatesReal = new string[] {
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

        public string[] Stances = new string[] {
            "Fakie",
            "Switch"
        };

        public string[] StancesCustomizer = new string[] {
            "Regular",
            "Fakie",
            "Nollie",
            "Switch"
        };

        public string[] GrindType = new string[] {
            "BsFiftyFifty",
            "FsFiftyFifty",
            "FsFiveO",
            "BsFiveO",
            "BsNoseGrind",
            "FsNoseGrind",
            "BsCrook",
            "FsOverCrook",
            "FsCrook",
            "BsOverCrook",
            "FsNoseSlide",
            "BsNoseSlide",
            "FsNoseBluntSlide",
            "BsNoseBluntSlide",
            "BsBoardSlide",
            "FsBoardSlide",
            "FsLipSlide",
            "BsLipSlide",
            "FsTailSlide",
            "BsBluntSlide",
            "BsTailSlide",
            "FsBluntSlide",
            "BsFeeble",
            "FsSmith",
            "FsFeeble",
            "BsSmith",
            "BsSuski",
            "FsSuski",
            "BsSalad",
            "FsSalad",
            "BsWilly",
            "FsWilly",
            "FsLosi",
            "BsLosi"
        };

        string[] special_patreons = new string[]
        {
            "Fury XL",
            "doobiedoober",
            "Kai Down",
            "Tyler W",
            "Eric Mcgrady",
            "Slabs",
            "Marcel Mink",
            "etcyj",
            "Theo Wegner",
            "helio",
            "Kyle Gherman",
            "Max Crowe",
            "Lonelycupid",
            "Nick Morocco",
            "Merrick Dougherty",
            "flipadip",
            "Nick Hanslip",
            "Bubble Gun",
            "Matt B",
            "Ricky Alonso",
            "Brody Jackson",
            "adri montes",
            "Euan",
            "Clips And Footage",
            "silentry",
            "loganhuntfilmm",
            "Foolie Surfin",
            "Nowak",
            "OG",
            "khellr",
            "Alex Tagg",
            "slade.",
            "Sir_Cheeba",
            "Dominic Galbavy",
            "Jeffery Depriest",
            "Alan Nairn",
            "Trav Wright",
            "Nati Adams",
            "Nicki Mouhs",
            "Krinsher",
            "DeVion Bailey",
            "Lucas Jaehn",
            "heartsick",
            "b00ph",
            "dustin fuston",
            "Dali",
            "peeeeeee poooooooooo",
            "Nick Duncan",
            "Rogue Bond",
            "Malleik",
            "StillAManChild",
            "Ted Budd",
            "Drew Muscolo",
            "Ardell Manning",
        };

        public void Start()
        {
            style.margin = new RectOffset(20, 0, 0, 0);
        }

        public void Update()
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(Main.settings.Hotkey.keyCode))
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

            if (showMainMenu)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
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
                MainMenuRect = GUILayout.Window(666, MainMenuRect, MainMenu, "<b>Fro's Experimental Mod v1.13.1</b>");
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

        void Fold(FoldObj obj, string color = "#fdcb6e")
        {
            if (GUILayout.Button($"<b><size=14><color={color}>" + (obj.reference ? "▶" : "▼") + "</color>" + obj.text + "</size></b>", "Label"))
            {
                obj.reference = !obj.reference;
                MainMenuRect.height = 20;
                MainMenuRect.width = Screen.width / 6;
            }
        }

        FoldObj about_fold = new FoldObj(true, "About");
        FoldObj patreons_fold = new FoldObj(true, "Special thanks <3");
        void AboutSection()
        {
            Fold(about_fold);
            if (!about_fold.reference)
            {
                GUILayout.BeginVertical("Box");
                GUILayout.Label("<b>fro's experimental mod v1.13.1 (31/08/2022)</b>");
                GUILayout.Label("Disclaimer: I'm not related to Easy Days Studios and i'm not responsible for any of your actions, use this mod at your own risk.");
                GUILayout.Label("This software is distributed 'as is', with no warranty expressed or implied, and no guarantee for accuracy or applicability to any purpose.");
                GUILayout.Label("This mod is not intended to harm the game or its respective developer in any purposeful way, its online functionality, or the game economy.");
                GUILayout.Label("I, the author of the mod, repudiate any type of practice or conduct that involves or promotes racism or any type of discrimination.");

                Fold(patreons_fold);
                if (!patreons_fold.reference)
                {
                    GUILayout.BeginVertical("Box", GUILayout.Width(Screen.width / 2));
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    for (int i = 0; i < special_patreons.Length; i++)
                    {
                        if (i % 6 == 0)
                        {
                            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
                        }
                        GUILayout.Label($"<color=#FFC312>• <b>{special_patreons[i]}</b></color>");
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                }
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
                    Main.settings.input_threshold = RGUI.SliderFloat(Main.settings.input_threshold, 0f, 100f, 20f, "Valid stick vertical area (%)");

                    if (RGUI.Button(Main.settings.swap_lean, "Invert input"))
                    {
                        Main.settings.swap_lean = !Main.settings.swap_lean;
                    }
                }
                GUILayout.EndVertical();
            }
        }

        FoldObj feet_fold = new FoldObj(true, "Dynamic feet");
        FoldObj feet_activation = new FoldObj(true, "Activation states");
        void FeetSection()
        {
            Fold(feet_fold);

            if (!feet_fold.reference)
            {
                GUILayout.BeginVertical("Box");
                GUILayout.Label("<b><color=#f9ca24>This feature tries to place your feet on board dynamically so no stances are needed for floaty feet</color></b>", GUILayout.Width(420));
                GUILayout.Label("<b><color=#f9ca24>If you already use a stance you can try combining it with rotation / position separately</color></b>", GUILayout.Width(420));
                GUILayout.Label("v1.1.0");
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

                if (Main.settings.feet_rotation || Main.settings.feet_offset)
                {
                    Fold(feet_activation, "#6ab04c");

                    if (!feet_activation.reference)
                    {
                        int count = 0;
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        foreach (var state in Enum.GetValues(typeof(PlayerController.CurrentState)))
                        {
                            if (count % 4 == 0 && count != 0)
                            {
                                GUILayout.FlexibleSpace();
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                GUILayout.FlexibleSpace();
                            }

                            if (Main.settings.dynamic_feet_states[count])
                            {
                                Texture2D flatButtonTex = new Texture2D(1, 1);
                                flatButtonTex.SetPixels(new[] { new Color(1, 1, 1, 1) });
                                flatButtonTex.Apply();
                                RGUIStyle.flatButton.active.background = flatButtonTex;
                                RGUIStyle.flatButton.normal.background = flatButtonTex;
                                GUI.backgroundColor = new Color32(106, 176, 76, 255);
                            }
                            else
                            {
                                GUI.backgroundColor = new Color32(1, 1, 1, 50);
                            }

                            if (GUILayout.Button("<b>" + state.ToString() + "</b>", RGUIStyle.flatButton, GUILayout.Width(92f), GUILayout.Height(26)))
                            {
                                Main.settings.dynamic_feet_states[count] = !Main.settings.dynamic_feet_states[count];
                            }
                            count++;
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
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

        FoldObj filmer_fold = new FoldObj(true, "Filmer mode");
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

                    GUILayout.Space(6);

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
                }
                GUILayout.EndVertical();
            }
        }

        FoldObj multi_fold = new FoldObj(true, "Settings");
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
                if (RGUI.Button(Main.settings.multiplayer_collision, "Enable player collision - WIP"))
                {
                    Main.settings.multiplayer_collision = !Main.settings.multiplayer_collision;
                }
                if (Main.settings.multiplayer_collision)
                {
                    if (RGUI.Button(Main.settings.show_colliders, "Show colliders"))
                    {
                        Main.settings.show_colliders = !Main.settings.show_colliders;
                    }
                }


                Main.settings.multiplayer_lobby_size = (int)RGUI.SliderFloat(Main.settings.multiplayer_lobby_size, 1f, 35f, 20f, "Multiplayer lobby size");
                if (Main.settings.multiplayer_lobby_size > 35) Main.settings.multiplayer_lobby_size = 35;
                if (Main.settings.multiplayer_lobby_size < 1) Main.settings.multiplayer_lobby_size = 1;

                // Main.settings.RoomIDlength = (int)RGUI.SliderFloat(Main.settings.RoomIDlength, 1f, 5f, 5f, "Multiplayer code size");

#if DEBUG
                if (GUILayout.Button("Create multi room"))
                {
                    Main.multi.CreateRoom();
                }
#endif

                GUILayout.EndVertical();
            }
        }

        FoldObj multichat_fold = new FoldObj(true, "Chat");
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


        public string[] Keyframe_States = new string[] {
            "Head",
            "Left Hand",
            "Right Hand",
            //"Filmer Object"
        };

        FoldObj camera_fold = new FoldObj(true, "Camera");
        FoldObj keyframe_fold = new FoldObj(true, "Keyframe creator");
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

                if (RGUI.Button(Main.settings.camera_shake, "Camera shake and FOV change"))
                {
                    Main.settings.camera_shake = !Main.settings.camera_shake;
                }

                if (Main.settings.camera_shake)
                {
                    Main.settings.camera_shake_offset = RGUI.SliderFloat(Main.settings.camera_shake_offset, 0f, 16f, 7f, "Camera shake minimum velocity");
                    Main.settings.camera_shake_multiplier = RGUI.SliderFloat(Main.settings.camera_shake_multiplier, 0f, 10f, 1f, "Camera shake multiplier");
                    Main.settings.camera_shake_fov_multiplier = RGUI.SliderFloat(Main.settings.camera_shake_fov_multiplier, 0f, 5f, 1f, "Camera shake FOV multiplier");
                }

                if (RGUI.Button(Main.settings.filmer_object, "Filmer object"))
                {
                    Main.settings.filmer_object = !Main.settings.filmer_object;
                    Main.controller.DisableCameraCollider();
                }

                if (Main.settings.filmer_object)
                {
                    GUILayout.BeginHorizontal();
                    Main.settings.filmer_object_target = GUILayout.TextField(Main.settings.filmer_object_target, 666, GUILayout.Height(21f));

                    if (GUILayout.Button("Scan object", RGUIStyle.button, GUILayout.Width(86)))
                    {
                        Main.controller.scanObject();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Label("Object " + (Main.controller.object_found != null ? "found" : "not found, check the object name"));
                }

                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label("<b>Force LOD 0 in all objects</b>");
                GUILayout.Label("<color=#f9ca24>After toggling this feature your game will freeze for some seconds</color>");
                GUILayout.Label("<color=#f9ca24>Enabling this feature will <b>for sure</b> cap some frames</color>");
                GUILayout.EndVertical();
                GUILayout.BeginVertical(GUILayout.Width(64f));
                GUILayout.Space(8);
                if (GUILayout.Button("Enable", RGUIStyle.button, GUILayout.Width(64f)))
                {
                    Main.controller.ForceLODs();
                }

                if (GUILayout.Button("Disable", RGUIStyle.button, GUILayout.Width(64f)))
                {
                    Main.controller.ResetLODs();
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                GUILayout.Space(6);
                Fold(keyframe_fold);
                if (!keyframe_fold.reference)
                {
                    GUILayout.Label("<b>Use this feature for creating filmer mode or first person keyframes on the replay editor</b>", GUILayout.Width(420));

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("<b>Target:</b>");
                    Main.settings.keyframe_target = RGUI.SelectionPopup(Main.settings.keyframe_target, Keyframe_States);
                    GUILayout.EndHorizontal();

                    if (RGUI.Button(Main.settings.keyframe_start_of_clip, "Generate from beginning"))
                    {
                        Main.settings.keyframe_start_of_clip = !Main.settings.keyframe_start_of_clip;
                    }
                    Main.settings.keyframe_sample = (int)RGUI.SliderFloat(Main.settings.keyframe_sample, 2f, 200f, 40f, "Number of keyframes to create");
                    Main.settings.keyframe_fov = (int)RGUI.SliderFloat(Main.settings.keyframe_fov, 1f, 180f, 120f, "Keyframe field of view");
                    Main.settings.time_offset = RGUI.SliderFloat(Main.settings.time_offset, -1f, 1f, 0f, "Time offset");

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

        FoldObj grinds_fold = new FoldObj(true, "Grinds");
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

        FoldObj body_fold = new FoldObj(true, "Body customization");
        FoldObj muscle_fold = new FoldObj(true, "Body parts scale");
        void BodySection()
        {
            Fold(body_fold);

            if (!body_fold.reference)
            {
                GUILayout.BeginVertical("Box");

                Main.settings.custom_scale.x = RGUI.SliderFloat(Main.settings.custom_scale.x, 0f, 2f, 1f, "Scale body x");
                Main.settings.custom_scale.y = RGUI.SliderFloat(Main.settings.custom_scale.y, 0f, 2f, 1f, "Scale body y");
                Main.settings.custom_scale.z = RGUI.SliderFloat(Main.settings.custom_scale.z, 0f, 2f, 1f, "Scale body z");
                GUILayout.Space(6);

                Fold(muscle_fold);
                if (!muscle_fold.reference)
                {
                    Main.settings.custom_scale_pelvis = RGUI.SliderFloat(Main.settings.custom_scale_pelvis, 0f, 4f, 1f, "Pelvis scale");
                    Main.settings.custom_scale_spine = RGUI.SliderFloat(Main.settings.custom_scale_spine, 0f, 4f, 1f, "Spine scale");
                    Main.settings.custom_scale_spine2 = RGUI.SliderFloat(Main.settings.custom_scale_spine2, 0f, 4f, 1f, "Spine2 scale");

                    Main.settings.custom_scale_head = RGUI.SliderFloat(Main.settings.custom_scale_head, 0f, 4f, 1f, "Head scale");
                    Main.settings.custom_scale_neck = RGUI.SliderFloat(Main.settings.custom_scale_neck, 0f, 4f, 1f, "Neck scale");

                    Main.settings.custom_scale_arm_l = RGUI.SliderFloat(Main.settings.custom_scale_arm_l, 0f, 4f, 1f, "Left arm scale");
                    Main.settings.custom_scale_forearm_l = RGUI.SliderFloat(Main.settings.custom_scale_forearm_l, 0f, 4f, 1f, "Left forearm scale");

                    Main.settings.custom_scale_hand_l = RGUI.SliderFloat(Main.settings.custom_scale_hand_l, 0f, 4f, 1f, "Left hand scale");

                    Main.settings.custom_scale_arm_r = RGUI.SliderFloat(Main.settings.custom_scale_arm_r, 0f, 4f, 1f, "Right arm scale");
                    Main.settings.custom_scale_forearm_r = RGUI.SliderFloat(Main.settings.custom_scale_forearm_r, 0f, 4f, 1f, "Right forearm scale");

                    Main.settings.custom_scale_hand_r = RGUI.SliderFloat(Main.settings.custom_scale_hand_r, 0f, 4f, 1f, "Right hand scale");

                    Main.settings.custom_scale_upleg_l = RGUI.SliderFloat(Main.settings.custom_scale_upleg_l, 0f, 4f, 1f, "Left upleg scale");
                    Main.settings.custom_scale_leg_l = RGUI.SliderFloat(Main.settings.custom_scale_leg_l, 0f, 4f, 1f, "Left leg scale");

                    Main.settings.custom_scale_foot_l = RGUI.SliderFloat(Main.settings.custom_scale_foot_l, 0f, 4f, 1f, "Left foot scale");

                    Main.settings.custom_scale_upleg_r = RGUI.SliderFloat(Main.settings.custom_scale_upleg_r, 0f, 4f, 1f, "Right upleg scale");
                    Main.settings.custom_scale_leg_r = RGUI.SliderFloat(Main.settings.custom_scale_leg_r, 0f, 4f, 1f, "Right leg scale");

                    Main.settings.custom_scale_foot_r = RGUI.SliderFloat(Main.settings.custom_scale_foot_r, 0f, 4f, 1f, "Right foot scale");
                }

                GUILayout.EndVertical();

                GUILayout.BeginVertical("Box");
                if (RGUI.Button(Main.settings.alternative_arms, "Alternative setup arms"))
                {
                    Main.settings.alternative_arms = !Main.settings.alternative_arms;
                }
                if (Main.settings.alternative_arms)
                {
                    if (RGUI.Button(Main.settings.alternative_arms_damping, "Change setup arms damping"))
                    {
                        Main.settings.alternative_arms_damping = !Main.settings.alternative_arms_damping;
                    }
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("Box");
                Main.settings.left_hand_weight = RGUI.SliderFloat(Main.settings.left_hand_weight, 0.01f, 5f, 1f, "Left hand weight");
                Main.settings.right_hand_weight = RGUI.SliderFloat(Main.settings.right_hand_weight, 0.01f, 5f, 1f, "Right hand weight");
                GUILayout.EndVertical();
            }

        }

        FoldObj head_fold = new FoldObj(true, "Head customization");
        FoldObj lookforwars_fold = new FoldObj(true, "Activation states");
        FoldObj lookforward_stance_fold = new FoldObj(true, "Neck rotation");
        int selected_state = 1;
        int selected_grind = 0;
        string selected_stance = "Switch";
        void HeadSection()
        {
            Fold(head_fold);
            if (!head_fold.reference)
            {
                GUILayout.BeginVertical("Box");
                if (RGUI.Button(Main.settings.look_forward, "Look forward (switch and fakie)"))
                {
                    Main.settings.look_forward = !Main.settings.look_forward;
                }

                if (Main.settings.look_forward)
                {
                    Main.settings.look_forward_delay = (int)RGUI.SliderFloat(Main.settings.look_forward_delay, 0f, 60f, 0f, "Delay (frames)");
                    Main.settings.look_forward_length = (int)RGUI.SliderFloat(Main.settings.look_forward_length, 0f, 60f, 18f, "Animation length (frames)");
                    Fold(lookforwars_fold, "#6ab04c");
                    if (!lookforwars_fold.reference)
                    {
                        int count = 0;
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        foreach (var state in Enum.GetValues(typeof(PlayerController.CurrentState)))
                        {
                            if (count % 4 == 0 && count != 0)
                            {
                                GUILayout.FlexibleSpace();
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                GUILayout.FlexibleSpace();
                            }

                            if (Main.settings.look_forward_states[count])
                            {
                                Texture2D flatButtonTex = new Texture2D(1, 1);
                                flatButtonTex.SetPixels(new[] { new Color(1, 1, 1, 1) });
                                flatButtonTex.Apply();
                                RGUIStyle.flatButton.active.background = flatButtonTex;
                                RGUIStyle.flatButton.normal.background = flatButtonTex;
                                GUI.backgroundColor = new Color32(106, 176, 76, 255);
                            }
                            else
                            {
                                GUI.backgroundColor = new Color32(1, 1, 1, 50);
                            }

                            if (GUILayout.Button("<b>" + state.ToString() + "</b>", RGUIStyle.flatButton, GUILayout.Width(92f), GUILayout.Height(26)))
                            {
                                Main.settings.look_forward_states[count] = !Main.settings.look_forward_states[count];
                            }
                            count++;
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }

                    Fold(lookforward_stance_fold, "#6ab04c");
                    if (!lookforward_stance_fold.reference)
                    {
                        if (!Main.settings.look_forward_states[selected_state]) RGUI.WarningLabel($"{StatesReal[selected_state]} is not enabled");
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("State:");
                        selected_state = RGUI.SelectionPopup(selected_state, StatesReal);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Stance:");
                        selected_stance = RGUI.SelectionPopup(selected_stance, Stances);
                        GUILayout.EndHorizontal();

                        if (selected_state == 9)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Grind:");
                            selected_grind = RGUI.SelectionPopup(selected_grind, GrindType);
                            GUILayout.EndHorizontal();

                            GUILayout.Label("Current detected grind: <b><color=green>" + Main.controller.getStance() + " " + PlayerController.Instance.boardController.triggerManager.grindDetection.grindType + "</color></b>");
                            Vector3 temp_vector_rot = selected_stance == "Fakie" ? Main.settings.head_rotation_grinds_fakie[selected_grind] : Main.settings.head_rotation_grinds_switch[selected_grind];
                            temp_vector_rot.x = RGUI.SliderFloat(temp_vector_rot.x, -120f, 120f, 0f, "X");
                            temp_vector_rot.y = RGUI.SliderFloat(temp_vector_rot.y, -120f, 120f, 0f, "Y");
                            temp_vector_rot.z = RGUI.SliderFloat(temp_vector_rot.z, -120f, 120f, 0f, "Z");
                            if (selected_stance == "Fakie") Main.settings.head_rotation_grinds_fakie[selected_grind] = temp_vector_rot;
                            else Main.settings.head_rotation_grinds_switch[selected_grind] = temp_vector_rot;

                        }
                        else
                        {
                            Vector3 temp_vector_rot = selected_stance == "Fakie" ? Main.settings.head_rotation_fakie[selected_state] : Main.settings.head_rotation_switch[selected_state];
                            temp_vector_rot.x = RGUI.SliderFloat(temp_vector_rot.x, -120f, 120f, 0f, "X");
                            temp_vector_rot.y = RGUI.SliderFloat(temp_vector_rot.y, -120f, 120f, 0f, "Y");
                            temp_vector_rot.z = RGUI.SliderFloat(temp_vector_rot.z, -120f, 120f, 0f, "Z");
                            if (selected_stance == "Fakie") Main.settings.head_rotation_fakie[selected_state] = temp_vector_rot;
                            else Main.settings.head_rotation_switch[selected_state] = temp_vector_rot;
                        }
                    }
                }
                GUILayout.EndVertical();
            }
        }

        FoldObj skate_fold = new FoldObj(true, "Skate");
        FoldObj skate_settings_fold = new FoldObj(true, "Settings");
        FoldObj customizer_fold = new FoldObj(true, "Trick customizer");
        // string selected_stance_customizer = "Regular";
        int selected_stance_customizer = 0;

        public string[] tricks_customizer = new string[] {
            "Ollie",
            "Kickflip",
            "Heelflip"
        };

        public string[] input_types = new string[] {
            "Both sticks to the front",
            "Both sticks to the back",
            "Left stick to the front",
            "Right stick to the front",
            "Left stick to the back",
            "Right stick to the back",
            "Both sticks to the outside",
            "Both sticks to the inside",
            "Left stick to the left",
            "Left stick to the right",
            "Right stick to the left",
            "Right stick to the right",
            "Both sticks to the left",
            "Both sticks to the right"
        };

        int selected_input = 0;

        void SkateSection()
        {
            Fold(skate_fold);

            if (!skate_fold.reference)
            {
                GUILayout.BeginVertical("Box");
                Fold(skate_settings_fold);

                if (!skate_settings_fold.reference)
                {
                    if (RGUI.Button(Main.settings.wobble, "Wobble on high speed"))
                    {
                        Main.settings.wobble = !Main.settings.wobble;
                    }

                    if (Main.settings.wobble)
                    {
                        Main.settings.wobble_offset = RGUI.SliderFloat(Main.settings.wobble_offset, 0f, 16f, 4f, "Minimum velocity for wobbling");
                    }

                    GUILayout.Space(6);

                    if (RGUI.Button(Main.settings.BetterDecay, "Better friction / decay"))
                    {
                        Main.settings.BetterDecay = !Main.settings.BetterDecay;
                    }

                    Main.settings.decay = RGUI.SliderFloat(Main.settings.decay, 0f, 10f, 3.25f, "Friction force");

                    GUILayout.Space(6);

                    Main.settings.nose_tail_collider = RGUI.SliderFloat(Main.settings.nose_tail_collider, 0f, 2f, 1f, "Nose and tail collider height");

                    GUILayout.Space(6);

                    if (RGUI.Button(Main.settings.powerslide_force, "Add force to powerslides"))
                    {
                        Main.settings.powerslide_force = !Main.settings.powerslide_force;
                    }

                    if (Main.settings.powerslide_force)
                    {
                        if (RGUI.Button(Main.settings.powerslide_velocitybased, "Velocity based"))
                        {
                            Main.settings.powerslide_velocitybased = !Main.settings.powerslide_velocitybased;
                        }
                    }

                    GUILayout.Space(6);

                    if (RGUI.Button(Main.settings.force_stick_backwards, "Add stomp force on back foot stick to the back"))
                    {
                        Main.settings.force_stick_backwards = !Main.settings.force_stick_backwards;
                    }
                    if (Main.settings.force_stick_backwards)
                    {
                        Main.settings.force_stick_backwards_multiplier = RGUI.SliderFloat(Main.settings.force_stick_backwards_multiplier, 0f, .5f, .125f, "Stomp force");
                    }
                }


                Fold(customizer_fold, "#6ab04c");

                if (!customizer_fold.reference)
                {
                    if (RGUI.Button(Main.settings.trick_customization, "Enabled"))
                    {
                        Main.settings.trick_customization = !Main.settings.trick_customization;
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Stance:");
                    selected_stance_customizer = RGUI.SelectionPopup(selected_stance_customizer, StancesCustomizer);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Input:");
                    selected_input = RGUI.SelectionPopup(selected_input, input_types);
                    GUILayout.EndHorizontal();

                    // this is not ideal but letsgo
                    Vector3 rotation = Main.settings.ollie_customization_rotation[selected_stance_customizer];
                    if (selected_input == 1) rotation = Main.settings.ollie_customization_rotation_backwards[selected_stance_customizer];
                    if (selected_input == 2) rotation = Main.settings.ollie_customization_rotation_left_stick[selected_stance_customizer];
                    if (selected_input == 3) rotation = Main.settings.ollie_customization_rotation_right_stick[selected_stance_customizer];
                    if (selected_input == 4) rotation = Main.settings.ollie_customization_rotation_left_stick_backwards[selected_stance_customizer];
                    if (selected_input == 5) rotation = Main.settings.ollie_customization_rotation_right_stick_backwards[selected_stance_customizer];
                    if (selected_input == 6) rotation = Main.settings.ollie_customization_rotation_both_outside[selected_stance_customizer];
                    if (selected_input == 7) rotation = Main.settings.ollie_customization_rotation_both_inside[selected_stance_customizer];
                    if (selected_input == 8) rotation = Main.settings.ollie_customization_rotation_left2left[selected_stance_customizer];
                    if (selected_input == 9) rotation = Main.settings.ollie_customization_rotation_left2right[selected_stance_customizer];
                    if (selected_input == 10) rotation = Main.settings.ollie_customization_rotation_right2left[selected_stance_customizer];
                    if (selected_input == 11) rotation = Main.settings.ollie_customization_rotation_right2right[selected_stance_customizer];
                    if (selected_input == 12) rotation = Main.settings.ollie_customization_rotation_both2left[selected_stance_customizer];
                    if (selected_input == 13) rotation = Main.settings.ollie_customization_rotation_both2right[selected_stance_customizer];

                    rotation.z = RGUI.SliderFloat(rotation.z, -180f, 180f, 0f, "Roll");
                    rotation.x = RGUI.SliderFloat(rotation.x, -180f, 180f, 0f, "Pitch");
                    rotation.y = RGUI.SliderFloat(rotation.y, -180f, 180f, 0f, "Yaw");

                    if (selected_input == 0) Main.settings.ollie_customization_rotation[selected_stance_customizer] = rotation;
                    if (selected_input == 1) Main.settings.ollie_customization_rotation_backwards[selected_stance_customizer] = rotation;
                    if (selected_input == 2) Main.settings.ollie_customization_rotation_left_stick[selected_stance_customizer] = rotation;
                    if (selected_input == 3) Main.settings.ollie_customization_rotation_right_stick[selected_stance_customizer] = rotation;
                    if (selected_input == 4) Main.settings.ollie_customization_rotation_left_stick_backwards[selected_stance_customizer] = rotation;
                    if (selected_input == 5) Main.settings.ollie_customization_rotation_right_stick_backwards[selected_stance_customizer] = rotation;
                    if (selected_input == 6) Main.settings.ollie_customization_rotation_both_outside[selected_stance_customizer] = rotation;
                    if (selected_input == 7) Main.settings.ollie_customization_rotation_both_inside[selected_stance_customizer] = rotation;
                    if (selected_input == 8) Main.settings.ollie_customization_rotation_left2left[selected_stance_customizer] = rotation;
                    if (selected_input == 9) Main.settings.ollie_customization_rotation_left2right[selected_stance_customizer] = rotation;
                    if (selected_input == 10) Main.settings.ollie_customization_rotation_right2left[selected_stance_customizer] = rotation;
                    if (selected_input == 11) Main.settings.ollie_customization_rotation_right2right[selected_stance_customizer] = rotation;
                    if (selected_input == 12) Main.settings.ollie_customization_rotation_both2left[selected_stance_customizer] = rotation;
                    if (selected_input == 13) Main.settings.ollie_customization_rotation_both2right[selected_stance_customizer] = rotation;

                    if (selected_input == 0) Main.settings.ollie_customization_length[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 1) Main.settings.ollie_customization_length_backwards[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_backwards[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 2) Main.settings.ollie_customization_length_left_stick[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_left_stick[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 3) Main.settings.ollie_customization_length_right_stick[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_right_stick[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 4) Main.settings.ollie_customization_length_left_stick_backwards[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_left_stick_backwards[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 5) Main.settings.ollie_customization_length_right_stick_backwards[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_right_stick_backwards[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 6) Main.settings.ollie_customization_length_both_outside[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_both_outside[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 7) Main.settings.ollie_customization_length_both_inside[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_both_inside[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 8) Main.settings.ollie_customization_length_left2left[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_left2left[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 9) Main.settings.ollie_customization_length_left2right[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_left2right[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 10) Main.settings.ollie_customization_length_right2left[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_right2left[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 11) Main.settings.ollie_customization_length_right2right[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_right2right[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 12) Main.settings.ollie_customization_length_both2left[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_both2left[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                    if (selected_input == 13) Main.settings.ollie_customization_length_both2right[selected_stance_customizer] = RGUI.SliderFloat(Main.settings.ollie_customization_length_both2right[selected_stance_customizer], 0, 60f, 24f, "Animation length");
                }

                GUILayout.EndVertical();
            }
        }

        FoldObj gameplay_fold = new FoldObj(true, "Gameplay");
        FoldObj multi_all_fold = new FoldObj(true, "Multiplayer");
        GUIStyle style = new GUIStyle();
        private void MainMenu(int windowID)
        {
            GUI.backgroundColor = Color.red;
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            MainSection();
            if (!Main.settings.enabled) return;

            AboutSection();

            AnimationAndPushingSection();

            BodySection();

            HeadSection();

            CameraSection();

            Fold(gameplay_fold);
            if (!gameplay_fold.reference)
            {
                GUILayout.BeginVertical(style);
                FeetSection();
                LeanWallrideSection();
                HippieSection();
                GrindsSection();
                SkateSection();
                GUILayout.EndVertical();
            }


            Fold(multi_all_fold);
            if (!multi_all_fold.reference)
            {
                GUILayout.BeginVertical(style);
                MultiSection();
                ChatSection();
                FilmerSection();
                GUILayout.EndVertical();
            }
        }
    }
}
