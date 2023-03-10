using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameManagement;
using RapidGUI;
using ReplayEditor;
using SkaterXL.Gameplay;
using UnityEngine;
using UnityModManagerNet;

namespace xlperimental_mod_old
{

    public class UIController : MonoBehaviour
    {
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
                        foreach (var state in Enum.GetValues(typeof(PlayerStateEnum)))
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
                //LeanWallrideSection();
                HippieSection();
                GrindsSection();
                SkateSection();
                //ReallyExperimental();
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

                GUILayout.Space(12);
            }

            //GUI.EndScrollView();
        }
    }
}
