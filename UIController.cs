using System;
using System.Collections.Generic;
using System.IO;
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
        string green = "#b8e994";
        string white = "#ecf0f1";
        string red = "#b71540";
        Texture2D inputbg, buttonbg, knob;

        public void LoadBG()
        {
            buttonbg = new Texture2D(128, 128);
            var bytes = File.ReadAllBytes(Main.modEntry.Path + "Background.png");
            ImageConversion.LoadImage(buttonbg, bytes, false);
            buttonbg.filterMode = FilterMode.Point;
        }

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
            "doobiedoober",
            "Eric Mcgrady",
            "Slabs",
            "Marcel Mink",
            "etcyj",
            "helio",
            "Kyle Gherman",
            "SKSixtySeven",
            "Max Crowe",
            "Nick Morocco",
            "flipadip",
            "Nick Hanslip",
            "Bubble Gun",
            "Matt B",
            "Euan",
            "J'vonte Johnson",
            "loganhuntfilmm",
            "Foolie Surfin",
            "Nowak",
            "OG",
            "Alex Tagg",
            "slade.",
            "Jeffery Depriest",
            "Trav Wright",
            "Nati Adams",
            "Nicki Mouhs",
            "Lucas Jaehn",
            "heartsick",
            "dustin fuston",
            "Dali",
            "Rogue Bond",
            "Ardell Manning",
            "Nathaniel Gardner",
            "Corey Populus",
            "countinsequence",
            "JdFilthyFree",
            "Ayden",
            "Roko Kvesic",
            "nahkel skylar",
            "Justin S Reynolds",
            "Ross Hill",
            "Joel de Roll",
            "Temp",
            "Seth Bates",
            "etown sk8r",
            "Alex Baker",
            "JMCAutomatic",
            "bluewalgreens",
            "Christian Joel",
            "Zaheer",
            "Mink",
            "Wouter van Huis"
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
            showMainMenu = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            UISounds.Instance.PlayOneShotSelectMajor();
        }

        private void Close()
        {
            showMainMenu = false;
            Cursor.visible = false;
            Main.settings.Save(Main.modEntry);
            UISounds.Instance.PlayOneShotExit();
        }

        bool style_applied = false, loaded = false;
        private void OnGUI()
        {
            Debug.Log(MainMenuRect.width);
            if (showMainMenu)
            {
                GUI.backgroundColor = new Color32(87, 75, 144, 255);
                MainMenuRect = GUILayout.Window(666, MainMenuRect, MainMenu, "<b>Fro's Experimental Mod</b>");

                if (!style_applied)
                {
                    Texture2D flatButtonTex = new Texture2D(1, 1);
                    flatButtonTex.SetPixels(new[] { new Color(1, 1, 1, 1) });
                    flatButtonTex.Apply();
                    RGUIStyle.flatButton.active.background = flatButtonTex;
                    RGUIStyle.flatButton.normal.background = flatButtonTex;
                    RGUIStyle.flatButton.focused.background = flatButtonTex;
                    RGUIStyle.flatButton.hover.background = flatButtonTex;
                    style_applied = true;
                }
            }
        }

        void MainSection()
        {
            //GUILayout.BeginVertical("Box");

            if (RGUI.Button(Main.settings.enabled, "Enabled"))
            {
                Main.settings.enabled = !Main.settings.enabled;
            }

            //GUILayout.EndVertical();
        }

        void Fold(FoldObj obj, string color = "#fad390")
        {
            if (GUILayout.Button($"<b><size=14><color={color}>" + (obj.reference ? "▶" : "▼") + "</color>" + obj.text + "</size></b>", "Label"))
            {
                obj.reference = !obj.reference;

                if (!obj.reference) UISounds.Instance.PlayOneShotSelectionChange();
                else UISounds.Instance.PlayOneShotSelectMinor();

                MainMenuRect.height = 20;
                MainMenuRect.width = Screen.width / 6;
            }
        }

        FoldObj about_fold = new FoldObj(true, "About");
        FoldObj patreons_fold = new FoldObj(true, "Special thanks <3");
        void AboutSection()
        {
            Fold(about_fold, white);
            if (!about_fold.reference)
            {
                GUILayout.BeginVertical("Box");
                GUILayout.Label("<b>fro's experimental mod v1.15.3 for XL v1.2.X.X (29/11/2022)</b>");
                GUILayout.Label("Disclaimer: I'm not related to Easy Days Studios and i'm not responsible for any of your actions, use this mod at your own risk.");
                GUILayout.Label("This software is distributed 'as is', with no warranty expressed or implied, and no guarantee for accuracy or applicability to any purpose.");
                GUILayout.Label("This mod is not intended to harm the game or the respective developer, the online functionality, or the game economy in any purposeful way.");
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
                    //Main.settings.input_threshold = RGUI.SliderFloat(Main.settings.input_threshold, 0f, 100f, 20f, "Valid stick vertical area (%)");

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
            Fold(feet_fold, green);

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
                    Main.settings.left_foot_offset = RGUI.SliderFloat(Main.settings.left_foot_offset, 0.01f, 2f, 1f, "Left shoe height offset");
                    Main.settings.right_foot_offset = RGUI.SliderFloat(Main.settings.right_foot_offset, 0.01f, 2f, 1f, "Right shoe height offset");
                }

                if (Main.settings.feet_rotation || Main.settings.feet_offset)
                {
                    Fold(feet_activation);

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

                            GUI.backgroundColor = Color.black;
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
            Fold(hippie_fold, green);

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
        public string[] Animations = new string[] {
            "Waving",
            "Celebrating",
            "Clapping"
        };
        void AnimationAndPushingSection()
        {
            Fold(animpush_fold, green);

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


                if (RGUI.Button(Main.settings.bails, "Alternative bails"))
                {
                    Main.settings.bails = !Main.settings.bails;
                }

                /*GUILayout.Label("<b>Automatically play animation on state</b>");
                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>Animation:</b>");
                Main.settings.wave_on = RGUI.SelectionPopup(Main.settings.wave_on, States);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>State:</b>");
                Main.settings.wave_on = RGUI.SelectionPopup(Main.settings.wave_on, States);
                GUILayout.EndHorizontal();*/


                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>Automatically wave on:</b>");
                Main.settings.wave_on = RGUI.SelectionPopup(Main.settings.wave_on, States);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>Automatically celebrate on:</b>");
                Main.settings.celebrate_on = RGUI.SelectionPopup(Main.settings.celebrate_on, States);
                GUILayout.EndHorizontal();

                GUILayout.Space(6);

                if (RGUI.Button(Main.settings.bump_anim, "Alternative bumps"))
                {
                    Main.settings.bump_anim = !Main.settings.bump_anim;
                }
                if (Main.settings.bump_anim)
                {
                    if (RGUI.Button(Main.settings.bump_anim_pop, "Play animation"))
                    {
                        Main.settings.bump_anim_pop = !Main.settings.bump_anim_pop;
                    }

                    if(Main.settings.bump_anim_pop) Main.settings.bump_pop_delay = RGUI.SliderFloat(Main.settings.bump_pop_delay, 0f, 1f, .15f, "Animation delay");
                }

                GUILayout.Space(6);

                if (RGUI.Button(Main.settings.catch_acc_enabled, "Catch fast forward"))
                {
                    Main.settings.catch_acc_enabled = !Main.settings.catch_acc_enabled;
                }

                if (Main.settings.catch_acc_enabled)
                {
                    Main.settings.catch_acc = RGUI.SliderFloat(Main.settings.catch_acc, 0f, 10f, 10f, "Animation speed");
                    Main.settings.catch_lerp_speed = RGUI.SliderFloat(Main.settings.catch_lerp_speed, 10f, 30f, 30f, "Feet lerp speed");
                    Main.settings.bounce_delay = (int)RGUI.SliderFloat(Main.settings.bounce_delay, 0f, 12f, 4f, "Catch correction delay");

                    if (RGUI.Button(Main.settings.snappy_catch, "Disable XXL3 catch correction"))
                    {
                        Main.settings.snappy_catch = !Main.settings.snappy_catch;
                    }
                    if (RGUI.Button(Main.settings.catch_acc_onflick, "Flick to catch support"))
                    {
                        Main.settings.catch_acc_onflick = !Main.settings.catch_acc_onflick;
                    }
                    if (Main.settings.catch_acc_onflick)
                    {
                        Main.settings.FlickThreshold = RGUI.SliderFloat(Main.settings.FlickThreshold, 0f, 1f, .6f, "Flick threshold");
                    }
                }

                GUILayout.EndVertical();
            }
        }

        FoldObj filmer_fold = new FoldObj(true, "Filmer mode");
        FoldObj instructions_fold = new FoldObj(true, "Instructions");
        string[] FilmerActivation = new string[] { "On pump input", "Always on" };
        void FilmerSection()
        {
            Fold(filmer_fold);

            if (!filmer_fold.reference)
            {
                GUILayout.BeginVertical("Box");

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
                    GUILayout.Label("<b>Activation</b>");
                    Main.settings.multi_filmer_activation = RGUI.SelectionPopup(Main.settings.multi_filmer_activation, FilmerActivation);
                    GUILayout.EndHorizontal();

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
            Fold(multi_fold, green);

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
            Fold(multichat_fold, green);

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

        void ClothColliders()
        {
            Cloth[] dynamic = FindObjectsOfType<Cloth>();
            List<CapsuleCollider> colliders = new List<CapsuleCollider>();

            for (int i = 0; i < PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles.Length; i++)
            {
                if (i != 4 && i != 5 && i != 7 && i != 8)
                {
                    if (PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[i].transform.gameObject.GetComponent<CapsuleCollider>() != null)
                    {
                        colliders.Add(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[i].transform.gameObject.GetComponent<CapsuleCollider>());
                    }
                }
            }

            /*colliders.Add(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].transform.gameObject.GetComponent<CapsuleCollider>());
            colliders.Add(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].transform.gameObject.GetComponent<CapsuleCollider>());*/

            foreach (Cloth c in dynamic)
            {
                c.stretchingStiffness = 1;
                c.enableContinuousCollision = true;
                c.stiffnessFrequency = 1;
                c.capsuleColliders = colliders.ToArray();
            }
        }


        public string[] Keyframe_States = new string[] {
            "Head",
            "Left Hand",
            "Right Hand",
            //"Filmer Object"
        };

        FoldObj camera_fold = new FoldObj(true, "Camera");
        FoldObj camera_settings_fold = new FoldObj(true, "Settings");
        FoldObj camera_shake_fold = new FoldObj(true, "Camera shake");
        FoldObj keyframe_fold = new FoldObj(true, "Keyframe creator");
        void CameraSection()
        {
            Fold(camera_fold, green);

            if (!camera_fold.reference)
            {
                GUILayout.BeginVertical("Box");
                Fold(camera_settings_fold, green);
                if (!camera_settings_fold.reference)
                {

                    if (RGUI.Button(Main.settings.camera_avoidance, "v1.2.x.x obstacle avoidance"))
                    {
                        Main.settings.camera_avoidance = !Main.settings.camera_avoidance;
                        Main.controller.DisableCameraCollider(Main.settings.camera_avoidance);
                    }

                    if (RGUI.Button(Main.settings.filmer_object, "Filmer object"))
                    {
                        Main.settings.filmer_object = !Main.settings.filmer_object;
                        Main.controller.DisableCameraCollider(Main.settings.camera_avoidance);
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
                    GUILayout.Label("<color=#f9ca24>After toggling this feature your game can freeze for some seconds</color>");
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
                }

                Fold(camera_shake_fold, green);
                if (!camera_shake_fold.reference)
                {
                    if (RGUI.Button(Main.settings.camera_shake, "Velocity based shake and field of view"))
                    {
                        Main.settings.camera_shake = !Main.settings.camera_shake;
                    }

                    if (Main.settings.camera_shake)
                    {
                        Main.settings.camera_shake_offset = RGUI.SliderFloat(Main.settings.camera_shake_offset, 0f, 16f, 7f, "Shake minimum velocity");
                        Main.settings.camera_shake_multiplier = RGUI.SliderFloat(Main.settings.camera_shake_multiplier, 0f, 10f, 3f, "Shake multiplier");
                        /*Main.settings.camera_shake_range = RGUI.SliderFloat(Main.settings.camera_shake_range, 0f, 1f, .2f, "Camera shake range");*/
                        Main.settings.camera_shake_length = (int)RGUI.SliderFloat(Main.settings.camera_shake_length, 1f, 10f, 4f, "Shake animation length");
                        GUILayout.Space(8);
                        Main.settings.camera_fov_offset = RGUI.SliderFloat(Main.settings.camera_fov_offset, 0f, 16f, 4f, "FOV minimum velocity");
                        Main.settings.camera_shake_fov_multiplier = RGUI.SliderFloat(Main.settings.camera_shake_fov_multiplier, 0f, 5f, 1.5f, "FOV multiplier");
                    }
                }

                Fold(keyframe_fold);
                if (!keyframe_fold.reference)
                {
                    GUILayout.Label("<b><color=#f9ca24>Use this feature for creating filmer mode or first person keyframes on the replay editor</color></b>", GUILayout.Width(420));

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

        FoldObj grinds_fold = new FoldObj(true, "Verticality");
        void GrindsSection()
        {
            Fold(grinds_fold, green);

            if (!grinds_fold.reference)
            {
                GUILayout.BeginVertical("Box");
                Main.settings.GrindFlipVerticality = RGUI.SliderFloat(Main.settings.GrindFlipVerticality, -1f, 1f, 0f, "Out of grinds");
                GUILayout.EndVertical();

                GUILayout.BeginVertical("Box");
                Main.settings.ManualFlipVerticality = RGUI.SliderFloat(Main.settings.ManualFlipVerticality, -1f, 1f, 0f, "Out of manuals");
                GUILayout.EndVertical();
            }
        }

        FoldObj body_fold = new FoldObj(true, "Body customization");
        FoldObj muscle_fold = new FoldObj(true, "Body parts scale");
        void BodySection()
        {
            Fold(body_fold, green);

            if (!body_fold.reference)
            {
                GUILayout.BeginVertical("Box");

                Main.settings.custom_scale.x = RGUI.SliderFloat(Main.settings.custom_scale.x, 0.01f, 2f, 1f, "Scale body x");
                Main.settings.custom_scale.y = RGUI.SliderFloat(Main.settings.custom_scale.y, 0.01f, 2f, 1f, "Scale body y");
                Main.settings.custom_scale.z = RGUI.SliderFloat(Main.settings.custom_scale.z, 0.01f, 2f, 1f, "Scale body z");
                GUILayout.Space(6);

                Main.settings.comOffset_y = RGUI.SliderFloat(Main.settings.comOffset_y, -1f, 1f, 0.07f, "Offset Y \n(use this to compensate the body customization)");

                GUILayout.Space(6);

                Fold(muscle_fold, green);
                if (!muscle_fold.reference)
                {
                    Main.settings.custom_scale_pelvis = RGUI.SliderFloat(Main.settings.custom_scale_pelvis, 0.01f, 4f, 1f, "Pelvis scale");
                    Main.settings.custom_scale_spine = RGUI.SliderFloat(Main.settings.custom_scale_spine, 0.01f, 4f, 1f, "Spine scale");
                    Main.settings.custom_scale_spine2 = RGUI.SliderFloat(Main.settings.custom_scale_spine2, 0.01f, 4f, 1f, "Spine2 scale");

                    Main.settings.custom_scale_head = RGUI.SliderFloat(Main.settings.custom_scale_head, 0.01f, 4f, 1f, "Head scale");
                    Main.settings.custom_scale_neck = RGUI.SliderFloat(Main.settings.custom_scale_neck, 0.01f, 4f, 1f, "Neck scale");

                    Main.settings.custom_scale_arm_l = RGUI.SliderFloat(Main.settings.custom_scale_arm_l, 0.01f, 4f, 1f, "Left arm scale");
                    Main.settings.custom_scale_forearm_l = RGUI.SliderFloat(Main.settings.custom_scale_forearm_l, 0.01f, 4f, 1f, "Left forearm scale");

                    Main.settings.custom_scale_hand_l = RGUI.SliderFloat(Main.settings.custom_scale_hand_l, 0.01f, 4f, 1f, "Left hand scale");

                    Main.settings.custom_scale_arm_r = RGUI.SliderFloat(Main.settings.custom_scale_arm_r, 0.01f, 4f, 1f, "Right arm scale");
                    Main.settings.custom_scale_forearm_r = RGUI.SliderFloat(Main.settings.custom_scale_forearm_r, 0.01f, 4f, 1f, "Right forearm scale");

                    Main.settings.custom_scale_hand_r = RGUI.SliderFloat(Main.settings.custom_scale_hand_r, 0.01f, 4f, 1f, "Right hand scale");

                    Main.settings.custom_scale_upleg_l = RGUI.SliderFloat(Main.settings.custom_scale_upleg_l, 0.01f, 4f, 1f, "Left upleg scale");
                    Main.settings.custom_scale_leg_l = RGUI.SliderFloat(Main.settings.custom_scale_leg_l, 0.01f, 4f, 1f, "Left leg scale");

                    Main.settings.custom_scale_foot_l = RGUI.SliderFloat(Main.settings.custom_scale_foot_l, 0.01f, 4f, 1f, "Left foot scale");

                    Main.settings.custom_scale_upleg_r = RGUI.SliderFloat(Main.settings.custom_scale_upleg_r, 0.01f, 4f, 1f, "Right upleg scale");
                    Main.settings.custom_scale_leg_r = RGUI.SliderFloat(Main.settings.custom_scale_leg_r, 0.01f, 4f, 1f, "Right leg scale");

                    Main.settings.custom_scale_foot_r = RGUI.SliderFloat(Main.settings.custom_scale_foot_r, 0.01f, 4f, 1f, "Right foot scale");
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
            Fold(head_fold, green);
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
                    Fold(lookforwars_fold);
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

                            GUI.backgroundColor = Color.black;
                            count++;
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }

                    Fold(lookforward_stance_fold, red);
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

        FoldObj experimental_fold = new FoldObj(true, "Skater center of mass");

        void ReallyExperimental()
        {
            Fold(experimental_fold, red);

            if (!experimental_fold.reference)
            {
                GUILayout.BeginVertical("Box");
                Main.settings.Kp = RGUI.SliderFloat(Main.settings.Kp, 0f, 10000f, 5000f, "Kp");
                Main.settings.Ki = RGUI.SliderFloat(Main.settings.Ki, 0f, 10000f, 0f, "Ki");
                Main.settings.Kd = RGUI.SliderFloat(Main.settings.Kd, 0f, 2000f, 900f, "Kd");
                Main.settings.KpImpact = RGUI.SliderFloat(Main.settings.KpImpact, 0f, 10000f, 5000f, "KpImpact");
                Main.settings.KdImpact = RGUI.SliderFloat(Main.settings.KdImpact, 0f, 2000f, 1000f, "KdImpact");
                Main.settings.KpSetup = RGUI.SliderFloat(Main.settings.KpSetup, 0f, 40000f, 20000f, "KpSetup");
                Main.settings.KdSetup = RGUI.SliderFloat(Main.settings.KdSetup, 0f, 3000f, 1500f, "KdSetup");
                /*Main.settings.KpGrind = RGUI.SliderFloat(Main.settings.KpGrind, 0f, 4000f, 2000f, "KpGrind");
                Main.settings.KdGrind = RGUI.SliderFloat(Main.settings.KdGrind, 0f, 2000f, 900f, "KdGrind");*/
                GUILayout.Space(8);
                Main.settings.comHeightRiding = RGUI.SliderFloat(Main.settings.comHeightRiding, -1f, 2f, 1.06f, "Height Riding");
                Main.settings.maxLegForce = RGUI.SliderFloat(Main.settings.maxLegForce, 0f, 10000f, 5000f, "Max Leg Force");

                GUILayout.Space(8);

                /*PlayerController.Instance.boardController.Kp = RGUI.SliderFloat(PlayerController.Instance.boardController.Kp, 0f, 10000f, 5000f, "Board KP");
                PlayerController.Instance.boardController.Ki = RGUI.SliderFloat(PlayerController.Instance.boardController.Ki, 0f, 10000f, 5000f, "Board KI");
                PlayerController.Instance.boardController.Kd = RGUI.SliderFloat(PlayerController.Instance.boardController.Kd, 0f, 10000f, 5000f, "Board KD");*/

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
            Fold(skate_fold, green);

            if (!skate_fold.reference)
            {
                GUILayout.BeginVertical("Box");
                /*Fold(skate_settings_fold, green);

                if (!skate_settings_fold.reference)
                {*/
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

                Main.settings.decay = RGUI.SliderFloat(Main.settings.decay, 0f, 4f, 1.5f, "Friction force");

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

                GUILayout.Space(6);

                if (RGUI.Button(Main.settings.forward_force_onpop, "Add forward force on pop"))
                {
                    Main.settings.forward_force_onpop = !Main.settings.forward_force_onpop;
                }
                if (Main.settings.forward_force_onpop)
                {
                    Main.settings.forward_force = RGUI.SliderFloat(Main.settings.forward_force, -1f, 2f, .35f, "Forward force");
                }

                /*GUILayout.Space(6);

                PlayerController.Instance.boardController.minManualAngle = RGUI.SliderFloat(PlayerController.Instance.boardController.minManualAngle, 0, 40f, 10f, "Min manual angle");
                PlayerController.Instance.boardController.maxManualAngle = RGUI.SliderFloat(PlayerController.Instance.boardController.maxManualAngle, 0, 40f, 10f, "Max manual angle");*/

                GUILayout.Space(6);

                if (RGUI.Button(Main.settings.custom_board_correction, "Custom board correction"))
                {
                    Main.settings.custom_board_correction = !Main.settings.custom_board_correction;
                }

                if (Main.settings.custom_board_correction)
                {
                    Main.settings.board_p = RGUI.SliderFloat(Main.settings.board_p, 0f, 10000f, 5000, "Board P");
                    Main.settings.board_i = RGUI.SliderFloat(Main.settings.board_i, 0f, 2f, 0, "Board I");
                    Main.settings.board_d = RGUI.SliderFloat(Main.settings.board_d, 0f, 2f, 1, "Board D");
                }


                GUILayout.EndVertical();
            }

            Fold(customizer_fold, red);

            if (!customizer_fold.reference)
            {
                GUILayout.BeginVertical("Box");
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

                GUILayout.EndVertical();
            }
        }

        FoldObj gameplay_fold = new FoldObj(true, "Gameplay");
        FoldObj multi_all_fold = new FoldObj(true, "Multiplayer");
        FoldObj exp_fold = new FoldObj(true, "Experimental");
        FoldObj misc_fold = new FoldObj(true, "Misc");
        GUIStyle style = new GUIStyle();
        public Vector2 scrollPosition = Vector2.zero;
        string kickplayer = "";
        private void MainMenu(int windowID)
        {
            GUI.backgroundColor = Color.red;
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            //GUI.BeginScrollView(new Rect(0, 0, 0, Screen.height), scrollPosition, MainMenuRect, false, true);
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
                ReallyExperimental();
                GUILayout.EndVertical();
            }


            Fold(multi_all_fold, green);
            if (!multi_all_fold.reference)
            {
                GUILayout.BeginVertical(style);
                MultiSection();
                ChatSection();
                FilmerSection();
                GUILayout.EndVertical();
            }

            Fold(misc_fold, green);
            if (!misc_fold.reference)
            {
                if (RGUI.Button(Main.settings.walk_after_bail, "Ghost walking after bail"))
                {
                    Main.settings.walk_after_bail = !Main.settings.walk_after_bail;
                }

                if (Main.settings.walk_after_bail)
                {
                    GUILayout.BeginVertical("Box");
                    GUILayout.Label("Circle / B for magnetizing the skate");
                    GUILayout.Label("Square / X for dropping the skate");
                    GUILayout.Label("Left stick click to toggle between walking / running");
                    GUILayout.Label("If you jump into the skate the skater will respawn at that position");
                    GUILayout.EndVertical();

                    Main.settings.bails = true;
                }

                GUILayout.Space(12);

                if (RGUI.Button(Main.settings.debug, "Debug"))
                {
                    Main.settings.debug = !Main.settings.debug;
                    Main.controller.checkDebug();
                    Main.controller.getDeck();
                }

                GUILayout.Space(12);

                GUILayout.Label("Use this button to reload custom gear textures while the game is open");
                if (GUILayout.Button("Update gear texture files", GUILayout.Height(28)))
                {
                    SaveManagerFocusPatch.HandleCustomGearChanges();
                }
            }

            Fold(exp_fold, red);
            if (!exp_fold.reference)
            {
                /*if(Main.settings.walk_after_bail)
                {
                    if (RGUI.Button(Main.settings.haunting_arms, "Haunting arms"))
                    {
                        Main.settings.haunting_arms = !Main.settings.haunting_arms;
                    }
                }*/

                GUILayout.Space(6);

                if (RGUI.Button(Main.settings.alternative_powerslide, "Alternative powerslides"))
                {
                    Main.settings.alternative_powerslide = !Main.settings.alternative_powerslide;
                }

                if (Main.settings.alternative_powerslide)
                {
                    Main.settings.powerslide_animation_length = RGUI.SliderFloat(Main.settings.powerslide_animation_length, 0f, 64f, 24f, "Powerslide animation length");
                    Main.settings.powerslide_minimum_velocity = RGUI.SliderFloat(Main.settings.powerslide_minimum_velocity, 0f, 20f, 0f, "Powerslide min velocity");
                    Main.settings.powerslide_max_velocity = RGUI.SliderFloat(Main.settings.powerslide_max_velocity, 0f, 20f, 15f, "Powerslide max velocity");
                    Main.settings.powerslide_maxangle = RGUI.SliderFloat(Main.settings.powerslide_maxangle, 0f, 45f, 20f, "Powerslide max angle");
                }


                /*if (GUILayout.Button("Change Cloth Colliders to legs, torso, and hands", GUILayout.Height(34)))
                {
                    ClothColliders();
                }*/

                GUILayout.Space(12);

                if (RGUI.Button(Main.settings.jiggle_on_setup, "Jiggle feet on setup"))
                {
                    Main.settings.jiggle_on_setup = !Main.settings.jiggle_on_setup;
                }

                if (Main.settings.jiggle_on_setup)
                {
                    if (!Main.settings.feet_rotation) RGUI.WarningLabel("You need to enable dynamic feet rotation for this feature to work (Gameplay > Dynamic feet)");
                    Main.settings.jiggle_delay = RGUI.SliderFloat(Main.settings.jiggle_delay, 0f, 60f, 24f, "Jiggle delay");
                    Main.settings.jiggle_limit = RGUI.SliderFloat(Main.settings.jiggle_limit, 0f, 90f, 40f, "Jiggle angle limit");
                    /*Main.settings.jiggle_randommax = RGUI.SliderFloat(Main.settings.jiggle_randommax, 0f, 30f, 10f, "Jiggle max random angle");*/
                }

                GUILayout.Space(12);

                if (RGUI.Button(Main.settings.partial_gear, "Multiplayer load partial gear"))
                {
                    Main.settings.partial_gear = !Main.settings.partial_gear;
                }

                //GUILayout.Space(12);

                /*GUILayout.BeginHorizontal();
                GUILayout.Label("<b>Player username</b>");
                kickplayer = RGUI.SelectionPopup(kickplayer, Main.controller.getListOfPlayers());
                GUILayout.EndHorizontal();
                if (Main.multi.isMaster())
                {
                    if (GUILayout.Button("Kick " + kickplayer, GUILayout.Height(34)))
                    {
                        Main.multi.KickPlayer(kickplayer);
                    }
                }

                if (GUILayout.Button("Steal main", GUILayout.Height(34)))
                {
                    Main.multi.setMeAsMaster();
                }

                if (GUILayout.Button("Restore main", GUILayout.Height(34)))
                {
                    Main.multi.restoreMaster();
                }

                if (GUILayout.Button("Master to " + kickplayer, GUILayout.Height(34)))
                {
                    Main.multi.MasterPlayer(kickplayer);
                }*/
            }

            //GUI.EndScrollView();
        }
    }
}
