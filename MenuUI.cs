using GameManagement;
using RapidGUI;
using ReplayEditor;
using SkaterXL.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace xlperimental_mod
{
    public class MenuUI : MonoBehaviour
    {
        bool show = false;
        private Rect MainMenuRect = new Rect(20, 20, Screen.width / 6, 20);
        string green = "#b8e994", white = "#ecf0f1", red = "#b71540";
        GUIStyle style = new GUIStyle();

        public void updateMainWindow()
        {
            MainMenuRect.height = 0;
            MainMenuRect.width = Screen.width / 6;
        }

        public void Start()
        {
            style.margin = new RectOffset(20, 0, 0, 0);
        }

        public void Update()
        {
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.F))
            {
                if (show) Close();
                else Open();
            }

            if (show)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        void OnGUI()
        {
            if (show)
            {
                GUI.backgroundColor = new Color32(87, 75, 144, 255);
                MainMenuRect = GUILayout.Window(666, MainMenuRect, MainMenu, "<b>Fro's XLperimental Mod</b>");
            }
        }

        UIFold about = new UIFold("About");
        UIFold patreons = new UIFold("Special thanks <3");
        UIFold animations = new UIFold("Animations");
        UIFold camera = new UIFold("Camera");
        UIFold map = new UIFold("Map");
        UIFold misc = new UIFold("Misc");
        UIFold multiplayer = new UIFold("Multiplayer");
        UIFold skate = new UIFold("Skate");
        UIFold skater = new UIFold("Skater");
        UIFold trick_customizer = new UIFold("Trick customizer");
        void MainMenu(int windowID)
        {
            GUI.backgroundColor = new Color32(87, 75, 144, 255);
            GUI.DragWindow(new Rect(0, 0, Screen.width / 6, 20));

            if (RGUI.Button(Main.settings.enabled, "Enabled"))
            {
                Main.settings.enabled = !Main.settings.enabled;
            }

            if (!Main.settings.enabled) return;

            about.Fold();
            AboutSection();

            animations.Fold();
            AnimationSection();

            camera.Fold();
            CameraSection();

            map.Fold();
            MapSection();

            misc.Fold();
            MiscSection();

            multiplayer.Fold();
            MultiplayerSection();

            skate.Fold();
            SkateSection();

            skater.Fold();
            SkaterSection();

            trick_customizer.Fold();
            TrickCustomizerUI();
        }

        void Open()
        {
            show = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            UISounds.Instance.PlayOneShotSelectMajor();
        }

        void Close()
        {
            show = false;
            Cursor.visible = false;
            Main.settings.Save(Main.modEntry);
            UISounds.Instance.PlayOneShotExit();
        }

        void AboutSection()
        {
            if (about.active)
            {
                GUILayout.BeginVertical("Box");
                GUILayout.Label("<b>fro's xlperimental mod v2.0.1 for XL v1.2.6.X (09/03/2023)</b>");
                GUILayout.Label("Disclaimer: I'm not related to Easy Days Studios and i'm not responsible for any of your actions, use this mod at your own risk.");
                GUILayout.Label("This software is distributed 'as is', with no warranty expressed or implied, and no guarantee for accuracy or applicability to any purpose.");
                GUILayout.Label("This mod is not intended to harm the game, the respective developer, the online functionality, or the game economy in any purposeful way.");
                GUILayout.Label("I, the author of the mod, repudiate any type of practice or conduct that involves or promotes racism or any type of discrimination.");

                patreons.Fold();
                if (patreons.active)
                {
                    GUILayout.BeginVertical("Box", GUILayout.Width(Screen.width / 2));
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    for (int i = 0; i < Patreons.names.Length; i++)
                    {
                        if (i % 6 == 0)
                        {
                            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal(); GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
                        }
                        GUILayout.Label($"<color=#FFC312>• <b>{Patreons.names[i]}</b></color>");
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
            }
        }

        void AnimationSection()
        {
            if (animations.active)
            {
                //Main.settings.steez_anim_speed = RGUI.SliderFloat(Main.settings.steez_anim_speed, 0f, 1f, 1f, "Steez animation weight");

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


                /*if (RGUI.Button(Main.settings.bails, "Alternative bails"))
                {
                    Main.settings.bails = !Main.settings.bails;
                }*/

                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>Automatically wave on:</b>");
                Main.settings.wave_on = RGUI.SelectionPopup(Main.settings.wave_on, EnumHelper.States);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("<b>Automatically celebrate on:</b>");
                Main.settings.celebrate_on = RGUI.SelectionPopup(Main.settings.celebrate_on, EnumHelper.States);
                GUILayout.EndHorizontal();

                GUILayout.Space(6);

                /*if (RGUI.Button(Main.settings.bump_anim, "Alternative bumps"))
                {
                    Main.settings.bump_anim = !Main.settings.bump_anim;
                }
                if (Main.settings.bump_anim)
                {
                    if (RGUI.Button(Main.settings.bump_anim_pop, "Play animation"))
                    {
                        Main.settings.bump_anim_pop = !Main.settings.bump_anim_pop;
                    }

                    if (Main.settings.bump_anim_pop) Main.settings.bump_pop_delay = RGUI.SliderFloat(Main.settings.bump_pop_delay, 0f, 1f, .15f, "Animation delay");
                }

                GUILayout.Space(6);*/

                /*if (RGUI.Button(Main.settings.catch_acc_enabled, "Catch fast forward"))
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
                }*/

                GUILayout.EndVertical();
            }
        }

        UIFold camera_shake = new UIFold("Camera shake");
        void CameraSection()
        {
            if (camera.active)
            {
                GUILayout.BeginVertical("Box");
                if (RGUI.Button(Main.settings.camera_avoidance, "v1.2.x.x obstacle avoidance"))
                {
                    Main.settings.camera_avoidance = !Main.settings.camera_avoidance;
                    Main.controller.DisableCameraCollider(Main.settings.camera_avoidance);
                }

                camera_shake.Fold();
                if (camera_shake.active)
                {
                    if (RGUI.Button(Main.settings.camera_shake, "Velocity based shake and field of view"))
                    {
                        Main.settings.camera_shake = !Main.settings.camera_shake;
                    }

                    if (Main.settings.camera_shake)
                    {
                        Main.settings.camera_shake_offset = RGUI.SliderFloat(Main.settings.camera_shake_offset, 0f, 16f, 7f, "Shake minimum velocity");
                        Main.settings.camera_shake_multiplier = RGUI.SliderFloat(Main.settings.camera_shake_multiplier, 0f, 10f, 3f, "Shake multiplier");
                        Main.settings.camera_shake_length = (int)RGUI.SliderFloat(Main.settings.camera_shake_length, 1f, 10f, 4f, "Shake animation length");
                        GUILayout.Space(8);
                        Main.settings.camera_fov_offset = RGUI.SliderFloat(Main.settings.camera_fov_offset, 0f, 16f, 4f, "FOV minimum velocity");
                        Main.settings.camera_shake_fov_multiplier = RGUI.SliderFloat(Main.settings.camera_shake_fov_multiplier, 0f, 5f, 1.5f, "FOV multiplier");
                    }
                }

                GUILayout.EndVertical();
            }
        }

        void MapSection()
        {
            if (map.active)
            {
                GUILayout.BeginVertical("Box");
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label("<b>Force best detail (LOD0) in all objects</b>");
                GUILayout.Label("<color=#f9ca24>After toggling this feature your game can freeze for some seconds</color>");
                GUILayout.Label("<color=#f9ca24>Enabling this feature will <b>for sure</b> make your game run slower since this feature is removing optimizations</color>");
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

                GUILayout.Label("<b>Map scale</b>");
                GUILayout.Label("Use negative values on one axis for mirroring maps");
                Main.settings.map_scale.x = RGUI.SliderFloat(Main.settings.map_scale.x, -2f, 2f, 1f, "X");
                Main.settings.map_scale.y = RGUI.SliderFloat(Main.settings.map_scale.y, -2f, 2f, 1f, "Y");
                Main.settings.map_scale.z = RGUI.SliderFloat(Main.settings.map_scale.z, -2f, 2f, 1f, "Z");
                GUILayout.Space(4);

                if (GUILayout.Button("Set scale", GUILayout.Height(34)))
                {
                    Main.controller.ScaleMap();
                }
                GUILayout.EndVertical();
            }
        }

        UIFold keyframe_creator = new UIFold("Keyframe creator");
        void MiscSection()
        {
            if (misc.active)
            {
                GUILayout.BeginVertical("Box");
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
                };
                GUILayout.Space(6);

                keyframe_creator.Fold();
                if (keyframe_creator.active)
                {
                    GUILayout.Label("<b><color=#f9ca24>Use this feature for creating filmer mode or first person keyframes on the replay editor</color></b>", GUILayout.Width(420));

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("<b>Target:</b>");
                    Main.settings.keyframe_target = RGUI.SelectionPopup(Main.settings.keyframe_target, EnumHelper.Keyframe_States);
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

        void MultiplayerSection()
        {
            if (multiplayer.active)
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
                if (RGUI.Button(Main.settings.multiplayer_collision, "Enable player collision"))
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


                Main.settings.multiplayer_lobby_size = (int)RGUI.SliderFloat(Main.settings.multiplayer_lobby_size, 1f, 35f, 10f, "Multiplayer lobby size");
                if (Main.settings.multiplayer_lobby_size > 35) Main.settings.multiplayer_lobby_size = 35;
                if (Main.settings.multiplayer_lobby_size < 1) Main.settings.multiplayer_lobby_size = 1;

                GUILayout.EndVertical();
            }
        }

        int selected_input = 0;
        int selected_stance_customizer = 0;
        UIFold skate_general = new UIFold("General");
        UIFold verticality = new UIFold("Verticality");

        void SkateSection()
        {
            if (skate.active)
            {
                GUILayout.BeginVertical("Box");

                skate_general.Fold();
                if (skate_general.active)
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
                }

                verticality.Fold();
                if (verticality.active)
                {
                    GUILayout.BeginVertical("Box");
                    Main.settings.GrindFlipVerticality = RGUI.SliderFloat(Main.settings.GrindFlipVerticality, -1f, 1f, 0f, "Out of grinds");
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical("Box");
                    Main.settings.ManualFlipVerticality = RGUI.SliderFloat(Main.settings.ManualFlipVerticality, -1f, 1f, 0f, "Out of manuals");
                    GUILayout.EndVertical();
                }

                /*                GUILayout.Space(6);

                                Main.settings.board_size.x = RGUI.SliderFloat(Main.settings.board_size.x, 0f, 2f, 1f, "Board size x");
                                Main.settings.board_size.y = RGUI.SliderFloat(Main.settings.board_size.y, 0f, 2f, 1f, "Board size y");
                                Main.settings.board_size.z = RGUI.SliderFloat(Main.settings.board_size.z, 0f, 2f, 1f, "Board size z");

                                Main.settings.truck_size.x = RGUI.SliderFloat(Main.settings.truck_size.x, 0f, 2f, 1f, "Truck size x");
                                Main.settings.truck_size.y = RGUI.SliderFloat(Main.settings.truck_size.y, 0f, 2f, 1f, "Truck size y");
                                Main.settings.truck_size.z = RGUI.SliderFloat(Main.settings.truck_size.z, 0f, 2f, 1f, "Truck size z");*/

                GUILayout.EndVertical();
            }
        }

        UIFold body_fold = new UIFold("Body customization");
        UIFold muscle_fold = new UIFold("Body parts scale");
        UIFold head_fold = new UIFold("Head customization");
        UIFold lookforwars_fold = new UIFold("Activation states");
        UIFold lookforward_stance_fold = new UIFold("Neck rotation");
        int selected_state = 2;
        int selected_grind = 0;
        string selected_stance = "Switch";
        void SkaterSection()
        {
            if (skater.active)
            {
                GUILayout.BeginVertical("Box");
                BodySection();
                //DynamicFeetSection();
                HeadSection();
                GUILayout.EndVertical();
            }
        }

        UIFold arms = new UIFold("Arms");
        UIFold dynamic_feet = new UIFold("Dynamic feet");
        UIFold com = new UIFold("Center of mass");
        void BodySection()
        {
            arms.Fold();
            if (arms.active)
            {
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
            }

            body_fold.Fold();
            if (body_fold.active)
            {
                Main.settings.custom_scale.x = RGUI.SliderFloat(Main.settings.custom_scale.x, 0.01f, 2f, 1f, "Scale body x");
                Main.settings.custom_scale.y = RGUI.SliderFloat(Main.settings.custom_scale.y, 0.01f, 2f, 1f, "Scale body y");
                Main.settings.custom_scale.z = RGUI.SliderFloat(Main.settings.custom_scale.z, 0.01f, 2f, 1f, "Scale body z");
                GUILayout.Space(6);

                //Main.settings.comOffset_y = RGUI.SliderFloat(Main.settings.comOffset_y, -1f, 1f, 0.07f, "Offset Y \n(use this to compensate the body customization)");
            }

            muscle_fold.Fold();
            if (muscle_fold.active)
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

                GUILayout.Space(6);
            }

            com.Fold();
            if (com.active)
            {
                /*Main.settings.ridingCOM.x = RGUI.SliderFloat(Main.settings.ridingCOM.x, 0f, 10000f, 5000f, "Riding COM P");
                Main.settings.ridingCOM.y = RGUI.SliderFloat(Main.settings.ridingCOM.y, 0f, 100f, 0f, "Riding COM I");
                Main.settings.ridingCOM.z = RGUI.SliderFloat(Main.settings.ridingCOM.z, 0f, 1800f, 900f, "Riding COM D");
                GUILayout.Space(6);*/

                Main.settings.impactCOM.x = RGUI.SliderFloat(Main.settings.impactCOM.x, 0f, 10000f, 5000f, "Impact P");
                Main.settings.impactCOM.y = RGUI.SliderFloat(Main.settings.impactCOM.y, 0f, 100f, 0f, "Impact I");
                Main.settings.impactCOM.z = RGUI.SliderFloat(Main.settings.impactCOM.z, 0f, 2000f, 1000f, "Impact D");
                GUILayout.Space(6);

                Main.settings.impactUpCOM.x = RGUI.SliderFloat(Main.settings.impactUpCOM.x, 0f, 4000f, 2000f, "Impact up P");
                Main.settings.impactUpCOM.y = RGUI.SliderFloat(Main.settings.impactUpCOM.y, 0f, 100f, 0f, "Impact up I");
                Main.settings.impactUpCOM.z = RGUI.SliderFloat(Main.settings.impactUpCOM.z, 0f, 1800f, 900f, "Impact up D");
                GUILayout.Space(6);

                Main.settings.setupCOM.x = RGUI.SliderFloat(Main.settings.setupCOM.x, 0f, 40000f, 20000f, "Setup P");
                Main.settings.setupCOM.y = RGUI.SliderFloat(Main.settings.setupCOM.y, 0f, 100f, 0f, "Setup I");
                Main.settings.setupCOM.z = RGUI.SliderFloat(Main.settings.setupCOM.z, 0f, 3000f, 1500f, "Setup D");
                GUILayout.Space(6);

                /*Main.settings.grindCOM.x = RGUI.SliderFloat(Main.settings.grindCOM.x, 0f, 4000f, 2000f, "Grind COM P");
                Main.settings.grindCOM.y = RGUI.SliderFloat(Main.settings.grindCOM.y, 0f, 100f, 0f, "Grind COM I");
                Main.settings.grindCOM.z = RGUI.SliderFloat(Main.settings.grindCOM.z, 0f, 1800f, 900f, "Grind COM D");
                GUILayout.Space(6);*/
            }
        }

        UIFold dynamic_feet_states = new UIFold("Activation states");
        void DynamicFeetSection()
        {
            dynamic_feet.Fold();
            if (dynamic_feet.active)
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
                    dynamic_feet_states.Fold();
                    if (dynamic_feet_states.active) ActivationStates(Main.settings.dynamic_feet_states);
                }
                GUILayout.EndVertical();
            }
        }

        void HeadSection()
        {
            head_fold.Fold();
            if (head_fold.active)
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

                    lookforwars_fold.Fold();
                    if (lookforwars_fold.active)
                    {
                        ActivationStates(Main.settings.look_forward_states);
                    }

                    lookforward_stance_fold.Fold();
                    if (lookforward_stance_fold.active)
                    {
                        if (!Main.settings.look_forward_states[selected_state]) RGUI.WarningLabel($"{EnumHelper.StatesReal[selected_state]} is not enabled");
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("State:");
                        selected_state = RGUI.SelectionPopup(selected_state, EnumHelper.StatesReal);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Stance:");
                        selected_stance = RGUI.SelectionPopup(selected_stance, EnumHelper.Stances);
                        GUILayout.EndHorizontal();

                        if (selected_state == 12)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Grind:");
                            selected_grind = RGUI.SelectionPopup(selected_grind, EnumHelper.GrindType);
                            GUILayout.EndHorizontal();

                            GUILayout.Label("Current detected grind: <b><color=green>" + Main.controller.getStance() + " " + GameStateMachine.Instance.MainPlayer.gameplay.playerData.grind.type + "</color></b>");
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

        void ActivationStates(List<bool> reference)
        {
            int count = 0;
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            foreach (var state in EnumHelper.StatesReal)
            {
                if (count % 4 == 0 && count != 0)
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                }

                if (reference.Count - 1 < count) reference.Add(false);

                if (reference[count])
                {
                    GUI.backgroundColor = new Color32(106, 176, 76, 255);
                }
                else
                {
                    GUI.backgroundColor = new Color32(1, 1, 1, 50);
                }

                if (GUILayout.Button("<b>" + state + "</b>", GUILayout.Width(92f), GUILayout.Height(26)))
                {
                    reference[count] = !reference[count];
                }

                GUI.backgroundColor = new Color32(1, 1, 1, 50);

                count++;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void TrickCustomizerUI()
        {
            if (trick_customizer.active)
            {
                GUILayout.BeginVertical("Box");
                if (RGUI.Button(Main.settings.trick_customization, "Enabled"))
                {
                    Main.settings.trick_customization = !Main.settings.trick_customization;
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Stance:");
                selected_stance_customizer = RGUI.SelectionPopup(selected_stance_customizer, EnumHelper.StancesCustomizer);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Input:");
                selected_input = RGUI.SelectionPopup(selected_input, EnumHelper.input_types);
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

                GUILayout.Space(6);

                Vector3 position = Main.settings.ollie_customization_position[selected_stance_customizer];
                if (selected_input == 1) position = Main.settings.ollie_customization_position_backwards[selected_stance_customizer];
                if (selected_input == 2) position = Main.settings.ollie_customization_position_left_stick[selected_stance_customizer];
                if (selected_input == 3) position = Main.settings.ollie_customization_position_right_stick[selected_stance_customizer];
                if (selected_input == 4) position = Main.settings.ollie_customization_position_left_stick_backwards[selected_stance_customizer];
                if (selected_input == 5) position = Main.settings.ollie_customization_position_right_stick_backwards[selected_stance_customizer];
                if (selected_input == 6) position = Main.settings.ollie_customization_position_both_outside[selected_stance_customizer];
                if (selected_input == 7) position = Main.settings.ollie_customization_position_both_inside[selected_stance_customizer];
                if (selected_input == 8) position = Main.settings.ollie_customization_position_left2left[selected_stance_customizer];
                if (selected_input == 9) position = Main.settings.ollie_customization_position_left2right[selected_stance_customizer];
                if (selected_input == 10) position = Main.settings.ollie_customization_position_right2left[selected_stance_customizer];
                if (selected_input == 11) position = Main.settings.ollie_customization_position_right2right[selected_stance_customizer];
                if (selected_input == 12) position = Main.settings.ollie_customization_position_both2left[selected_stance_customizer];
                if (selected_input == 13) position = Main.settings.ollie_customization_position_both2right[selected_stance_customizer];

                position.x = RGUI.SliderFloat(position.x, -1f, 1f, 0f, "X");
                position.y = RGUI.SliderFloat(position.y, -1f, 1f, 0f, "Y");
                position.z = RGUI.SliderFloat(position.z, -1f, 1f, 0f, "Z");

                if (selected_input == 0) Main.settings.ollie_customization_position[selected_stance_customizer] = position;
                if (selected_input == 1) Main.settings.ollie_customization_position_backwards[selected_stance_customizer] = position;
                if (selected_input == 2) Main.settings.ollie_customization_position_left_stick[selected_stance_customizer] = position;
                if (selected_input == 3) Main.settings.ollie_customization_position_right_stick[selected_stance_customizer] = position;
                if (selected_input == 4) Main.settings.ollie_customization_position_left_stick_backwards[selected_stance_customizer] = position;
                if (selected_input == 5) Main.settings.ollie_customization_position_right_stick_backwards[selected_stance_customizer] = position;
                if (selected_input == 6) Main.settings.ollie_customization_position_both_outside[selected_stance_customizer] = position;
                if (selected_input == 7) Main.settings.ollie_customization_position_both_inside[selected_stance_customizer] = position;
                if (selected_input == 8) Main.settings.ollie_customization_position_left2left[selected_stance_customizer] = position;
                if (selected_input == 9) Main.settings.ollie_customization_position_left2right[selected_stance_customizer] = position;
                if (selected_input == 10) Main.settings.ollie_customization_position_right2left[selected_stance_customizer] = position;
                if (selected_input == 11) Main.settings.ollie_customization_position_right2right[selected_stance_customizer] = position;
                if (selected_input == 12) Main.settings.ollie_customization_position_both2left[selected_stance_customizer] = position;
                if (selected_input == 13) Main.settings.ollie_customization_position_both2right[selected_stance_customizer] = position;

                GUILayout.Space(6);

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
    }
}
