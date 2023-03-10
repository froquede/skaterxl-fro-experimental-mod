using Cinemachine;
using GameManagement;
using HarmonyLib;
using ModIO.UI;
using Photon.Realtime;
using ReplayEditor;
using RootMotion.Dynamics;
using RootMotion.FinalIK;
using SkaterXL.Core;
using SkaterXL.Data;
using SkaterXL.Gameplay;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace xlperimental_mod
{
    public class Controller : MonoBehaviour
    {
        private RaycastHit _hit;
        RaycastHit right_hit;
        RaycastHit left_hit;
        GameObject debug_cube, debug_cube_2, debug_cube_3, debug_cube_4;
        public Transform left_foot;
        public Transform right_foot;
        public Transform head, neck, head_replay, spine, left_hand_replay, right_hand_replay;
        Transform center_collider;
        Transform tail_collider;
        Transform nose_collider;
        public string last_state;
        int count = 0;
        bool was_leaning = false;
        Cinemachine.CinemachineCollider cinemachine_collider;
        public CinemachineVirtualCamera mainCam;
        bool bailed_puppet = false;
        System.Random rd = new System.Random();
        Transform replay_skater;

        public void Start()
        {

            replay_skater = getReplayEditor();

            DisableMultiPopup(Main.settings.disable_popup);
            DisableCameraCollider(Main.settings.camera_avoidance);

            MultiplayerManager.ROOMSIZE = 10;

            checkDebug();
            getReplayUI();

            GameStateMachine.Instance.MainPlayer.gameplay.respawnController.Respawn();

            deck = GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.transform.Find("Deck").gameObject;
            visible_deck = GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.transform.Find("Visible Deck Mesh").gameObject;
            backtruck = deck.transform.Find("Back Truck").gameObject;
            fronttruck = deck.transform.Find("Front Truck").gameObject;

            deck_replay = replay_transform.FindChildRecursively("Deck").gameObject;
            visible_deck_replay = replay_transform.FindChildRecursively("Visible Deck Mesh").gameObject;
            backtruck_replay = deck_replay.transform.Find("Back Truck").gameObject;
            fronttruck_replay = deck_replay.transform.Find("Front Truck").gameObject;
        }

        void AddColliders()
        {

        }

        public string[] getListOfPlayers()
        {
            string[] names = new string[MultiplayerManager.Instance.networkPlayers.Count];
            int i = 0;
            foreach (KeyValuePair<int, NetworkPlayerController> entry in MultiplayerManager.Instance.networkPlayers)
            {
                if (entry.Value)
                {
                    names[i] = entry.Key + ":" + entry.Value.NickName;
                    i++;
                }
            }

            return names;
        }

        Transform getOnlinePlayer()
        {
            if (Main.settings.selected_player != "")
            {
                int index = int.Parse(Main.settings.selected_player.Split(':')[0]);
                var player = MultiplayerManager.Instance.GetNextPlayer(index - 1);

                if (player != null && !player.IsLocal)
                {
                    Transform body = player.GetBody().transform;
                    body.Translate(0, Main.settings.follow_target_offset, 0);
                    return body;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private void LogState()
        {
            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState.ToString() != last_state)
            {
                last_state = GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState.ToString();
                if (Main.settings.debug) UnityModManager.Logger.Log(last_state);
            }
        }

        public bool IsGrabbing() { return (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Grabs) || GameStateMachine.Instance.MainPlayer.gameplay.trickController.IsGrabbing; }
        int powerslide_anim = 0;

        LayerMask layerMask = ~(1 << LayerMask.NameToLayer("Skateboard"));
        float head_frame = 0, delay_head = 0;
        Quaternion last_head = Quaternion.identity;
        bool lsb_pressed = false, rsb_pressed = false;
        public void Update()
        {
            if (center_collider == null) getDeck();
            if (left_foot == null) getFeet();

            MultiplayerFilmer();

            LookForward();
            LetsGoAnimHead();

            if (keyframe_state == true)
            {
                FilmerKeyframes();
            }

            if (Main.settings.alternative_powerslide && GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Powerslide)
            {
                if (powerslide_anim < 24) powerslide_anim++;
            }
            else
            {
                if (powerslide_anim > 0)
                {
                    powerslide_anim -= 4;
                }
                powerslide_anim = powerslide_anim < 0 ? 0 : powerslide_anim;
            }

            /*if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.InAir)
            {
                GameStateMachine.Instance.MainPlayer.gameplay.ikController.ForceLeftLerpValue(1);
                GameStateMachine.Instance.MainPlayer.gameplay.ikController.ForceRightLerpValue(1);
            }

            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Impact)
            {
                GameStateMachine.Instance.MainPlayer.gameplay.ikController.ForceLeftLerpValue(0);
                GameStateMachine.Instance.MainPlayer.gameplay.ikController.ForceRightLerpValue(0);
            }*/

            bool left = GameStateMachine.Instance.MainPlayer.gameplay.inputController.rewiredPlayer.GetButtonDown("Left Stick Button"), right = GameStateMachine.Instance.MainPlayer.gameplay.inputController.rewiredPlayer.GetButtonDown("Right Stick Button");

            if ((last_state == PlayerStateEnum.Grinding.ToString() || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.ExitCoping) && GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState != PlayerStateEnum.Impact)
            {
                if ((right || left) && !IsBumping)
                {
                    if (Main.settings.bump_anim)
                    {
                        GameStateMachine.Instance.MainPlayer.gameplay.playerData.animation.ResetChangeCheck();
                        GameStateMachine.Instance.MainPlayer.gameplay.playerData.ScaleCOMDisplacementCurve(Vector3.ProjectOnPlane(GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRoot.position - GameStateMachine.Instance.MainPlayer.gameplay.transformReference.boardTransform.position, GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRoot.forward).magnitude * .75f);
                        GameStateMachine.Instance.MainPlayer.gameplay.playerData.ResetIKOffsets();
                        GameStateMachine.Instance.MainPlayer.gameplay.playerData.animation.Grinding = false;
                        GameStateMachine.Instance.MainPlayer.gameplay.playerData.air.ForwardLoad = GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.y > .1f || GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.y >= .1f ? true : false;
                        GameStateMachine.Instance.MainPlayer.gameplay.playerData.animation.Nollie = GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.y > .1f || GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.y >= .1f ? 1f : 0f;
                        GameStateMachine.Instance.MainPlayer.gameplay.animationController.ikAnim.SetFloat("Nollie", GameStateMachine.Instance.MainPlayer.gameplay.playerData.animation.Nollie);
                        GameStateMachine.Instance.MainPlayer.gameplay.animationController.skaterAnim.SetFloat("Nollie", GameStateMachine.Instance.MainPlayer.gameplay.playerData.animation.Nollie);
                        GameStateMachine.Instance.MainPlayer.gameplay.playerData.animation.Released = false;
                        //GameStateMachine.Instance.MainPlayer.gameplay.playerData.ik.ForceKneeIKWeight(0.5f);
                        GameStateMachine.Instance.MainPlayer.gameplay.playerData.ScalePlayerCollider();

                        if (Main.settings.bump_anim_pop)
                        {
                            GameStateMachine.Instance.MainPlayer.gameplay.animationController.skaterAnim.Play("Pop", 0, 0f);
                            GameStateMachine.Instance.MainPlayer.gameplay.animationController.ikAnim.Play("Pop", 0, 0f);
                            //GameStateMachine.Instance.MainPlayer.gameplay.playerData.animation.ForceAnimation("Pop");
                        }
                    }

                    //MonoBehaviourSingleton<EventManager>.Instance.EnterAir(GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.y > .1f || GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.y >= .1f ? GameStateMachine.Instance.MainPlayer.gameplay.playerData.IsSwitch ? GameStateMachine.Instance.MainPlayer.gameplay.animationController.skaterAnim.GetFloat("Nollie") == 0f ? PopType.Fakie : PopType.Switch : PopType.Nollie : PopType.Ollie, 0f);
                    IsBumping = true;
                }

                if (IsGroundedNoGrind()) IsBumping = false;
            }
            else IsBumping = false;

            if ((Main.settings.alternative_powerslide && GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Powerslide) || powerslide_anim > 0)
            {
                CustomPowerSlideBoard();
            }
        }

        RaycastHit rayCastOut;
        void CustomPowerSlide()
        {
            if (!Main.settings.alternative_powerslide) return;

            if (Physics.Raycast(GameStateMachine.Instance.MainPlayer.gameplay.transformReference.boardTransform.position, -GameStateMachine.Instance.MainPlayer.gameplay.transformReference.boardTransform.up, out rayCastOut, 0.4f, LayerUtility.GroundMask))
            {
                int multiplier = GameStateMachine.Instance.MainPlayer.gameplay.playerData.GetBoardBackwards() ? -1 : 1;
                multiplier *= GameStateMachine.Instance.MainPlayer.gameplay.playerData.IsSwitch ? -1 : 1;
                int side_multiplier = Main.tc.getTypeOfInput().Contains("both-inside") ? -1 : 1;

                if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == Stance.Goofy) side_multiplier *= -1;

                float vel_map = map01(GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRigidbody.velocity.magnitude, Main.settings.powerslide_minimum_velocity, Main.settings.powerslide_max_velocity);
                vel_map = vel_map > 1 ? 1 : vel_map;
                float anim_length = map01(powerslide_anim, 0, Main.settings.powerslide_animation_length);
                anim_length = anim_length > 1 ? 1 : anim_length;

                GameStateMachine.Instance.MainPlayer.gameplay.playerData.ik.ForceKneeIKWeight(anim_length);
                GameObject pelvis_copy = new GameObject();
                pelvis_copy.transform.position = pelvis.transform.position;
                pelvis_copy.transform.rotation = pelvis.transform.rotation;
                /*Quaternion rothit = Quaternion.LookRotation(rayCastOut.normal);
                pelvis.transform.rotation = Quaternion.Lerp(pelvis.transform.rotation, Quaternion.Euler(pelvis.transform.rotation.eulerAngles.x, pelvis.transform.rotation.eulerAngles.y, -90 - (270 - rothit.eulerAngles.x)), anim_length);*/
                pelvis_copy.transform.Rotate(0, side_multiplier == -1 ? -Mathf.Lerp(0, (vel_map * 20f), anim_length) : 0, Mathf.Lerp(0, (vel_map * (side_multiplier == -1 ? 10f : 15f)), anim_length) * side_multiplier, Space.Self);
                if (side_multiplier == -1)
                {
                    pelvis_copy.transform.Translate(Mathf.Lerp(0, (vel_map * -.15f), anim_length), Mathf.Lerp(0, (vel_map * .125f), anim_length), Mathf.Lerp(0, (vel_map * -.3f), anim_length), Space.Self);
                }
                else
                {
                    pelvis_copy.transform.Translate(Mathf.Lerp(0, (vel_map * .02f), anim_length) * side_multiplier, -Mathf.Lerp(0, (vel_map * .15f), anim_length) * side_multiplier, 0, Space.Self);
                }

                /*pelvis.transform.position = pelvis_copy.transform.position;
                pelvis.transform.rotation = pelvis_copy.transform.rotation;*/

                ScaleAnimSpeed(anim_length);

                Destroy(pelvis_copy);
            }
        }

        float last_hit_ps = 0;
        GameObject boardCopy;
        void CustomPowerSlideBoard()
        {
            int multiplier = GameStateMachine.Instance.MainPlayer.gameplay.playerData.GetBoardBackwards() ? -1 : 1;
            multiplier *= GameStateMachine.Instance.MainPlayer.gameplay.playerData.IsSwitch ? -1 : 1;
            int side_multiplier = Main.tc.getTypeOfInput().Contains("both-inside") ? -1 : 1;

            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == Stance.Goofy) side_multiplier *= -1;

            float vel_map = map01(GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRigidbody.velocity.magnitude, Main.settings.powerslide_minimum_velocity, Main.settings.powerslide_max_velocity);
            vel_map = vel_map > 1 ? 1 : vel_map;
            float anim_length = map01(powerslide_anim, 0, Main.settings.powerslide_animation_length);
            anim_length = anim_length > 1 ? 1 : anim_length;

            Quaternion rothit = Quaternion.LookRotation(rayCastOut.normal);

            if(!boardCopy) boardCopy = new GameObject();
            boardCopy.transform.position = GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.transform.position;
            boardCopy.transform.rotation = GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.transform.rotation;

            float angle = Vector3.Angle(rayCastOut.normal, Vector3.up);
            Vector3 temp = Vector3.Cross(rayCastOut.normal, Vector3.down);
            Vector3 cross = Vector3.Cross(temp, rayCastOut.normal);

            float crossZ = last_hit_ps - rayCastOut.point.y >= 0 ? 1 : -1;
            if (side_multiplier < 0) crossZ = last_hit_ps - rayCastOut.point.y < 0 ? 1 : -1;
            float extra_powerslide = Mathf.Lerp(0, (vel_map * Main.settings.powerslide_maxangle * multiplier * side_multiplier), anim_length);
            boardCopy.transform.rotation = Quaternion.Euler(boardCopy.transform.rotation.eulerAngles.x, boardCopy.transform.rotation.eulerAngles.y, (multiplier * crossZ * -angle) + extra_powerslide);
            last_hit_ps = rayCastOut.point.y;
            /*Vector3 rot = GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.transform.rotation.eulerAngles;
            GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.transform.rotation = Quaternion.Euler(rot.x, rot.y, Mathf.Lerp(rothit.eulerAngles.z, rothit.eulerAngles.z + (vel_map * (Main.settings.powerslide_maxangle * multiplier * side_multiplier)), anim_length));*/

            GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.transform.rotation = boardCopy.transform.rotation;

            //GameStateMachine.Instance.MainPlayer.gameplay.boardController.UpdateBoardPosition();
            //GameStateMachine.Instance.MainPlayer.gameplay.SetRotationTarget();
        }

        void PreventBail()
        {
            GameStateMachine.Instance.MainPlayer.gameplay.CancelInvoke("DoBail");
        }

        private void setMotionType(Muscle muscle, ConfigurableJointMotion joint)
        {
            muscle.joint.xMotion = joint;
            muscle.joint.yMotion = joint;
            muscle.joint.zMotion = joint;
            SoftJointLimit limit = new UnityEngine.SoftJointLimit();
            limit.bounciness = 1;
            muscle.joint.linearLimit = limit;
        }

        int delay = 0;
        bool freeze_pos = false;
        float last_time_bailed = 0;
        int slow_count = 0, bailed_count = 0;
        GameObject deck, deck_replay, visible_deck, visible_deck_replay, backtruck, backtruck_replay, fronttruck, fronttruck_replay;
        public void FixedUpdate()
        {
            if (!Main.settings.enabled) return;
            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState != PlayerStateEnum.Pop && GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState != PlayerStateEnum.Release) ScaleAnimSpeed(1f);

            if (Main.settings.alternative_powerslide)
            {
                Physics.Raycast(GameStateMachine.Instance.MainPlayer.gameplay.transformReference.boardTransform.position, -GameStateMachine.Instance.MainPlayer.gameplay.transformReference.boardTransform.up, out rayCastOut, 0.8f, LayerUtility.GroundMask);
            }

            DoDebug();

            if (MultiplayerManager.ROOMSIZE != (byte)Main.settings.multiplayer_lobby_size) MultiplayerManager.ROOMSIZE = (byte)Main.settings.multiplayer_lobby_size;

            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Powerslide && Main.settings.powerslide_force) PowerSlideForce();

            if (Main.settings.wave_on != "Disabled") WaveAnim();
            if (Main.settings.celebrate_on != "Disabled" && GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState != PlayerStateEnum.Bailed) CheckLetsgoAnim();

            if (Main.settings.bails && GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Bailed && GameStateMachine.Instance.MainPlayer.gameplay.playerData.Bailed)
            {
                if (!bailed_puppet) SetBailedPuppet();
                else
                {
                    /*if(bailed_count <= 4)
                    {
                        GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[2].rigidbody.AddForce(0, 60f, 0f, ForceMode.Impulse);
                    }*/
                    bailed_count++;
                }
                letsgo_anim_time = Time.unscaledTime - 10f;
            }
            else
            {
                if (bailed_puppet) RestorePuppet();
                GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[6].rigidbody.mass = Main.settings.left_hand_weight;
                GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[9].rigidbody.mass = Main.settings.right_hand_weight;
                bailed_count = 0;
            }

            if (Main.settings.displacement_curve) GameStateMachine.Instance.MainPlayer.gameplay.playerData.ScaleCOMDisplacementCurve(Vector3.ProjectOnPlane(GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRoot.position - GameStateMachine.Instance.MainPlayer.gameplay.transformReference.boardTransform.position, GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRoot.forward).magnitude * 1.25f);
            if (Main.settings.alternative_arms) AlternativeArms();

            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Pushing) PushModes();

            if (!GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.puppetMaster.isBlending)
            {
                if (Main.settings.camera_feet) CameraFeet();
                //if (Main.settings.feet_rotation || Main.settings.feet_offset) DynamicFeet();
            }

            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Riding && last_state == PlayerStateEnum.Pushing.ToString()) ScaleAnimSpeed(0f);
            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Grinding || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.ExitCoping) GrindVerticalFlip();
            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Manual) ManualVerticalFlip();

            LetsGoAnim();

            if (Main.settings.filmer_object && object_found != null) object_found.transform.position = (GameStateMachine.Instance.CurrentState.GetType() == typeof(ReplayState) ? pelvis_replay.position : pelvis.position);

            if (Main.settings.forward_force_onpop && GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Pop)
            {
                int multiplier = GameStateMachine.Instance.MainPlayer.gameplay.playerData.GetBoardBackwards() ? -1 : 1;
                GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.AddRelativeForce(0, 0, multiplier * Main.settings.forward_force * (GameStateMachine.Instance.MainPlayer.gameplay.playerData.IsSwitch ? -1 : 1), ForceMode.Impulse);
                GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRigidbody.AddRelativeForce(0, 0, (Main.settings.forward_force / 50f) * (GameStateMachine.Instance.MainPlayer.gameplay.playerData.IsSwitch ? -1 : 1), ForceMode.Impulse);
            }

            if (Main.settings.BetterDecay)
            {
                GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.AddForce(GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.velocity * -(Main.settings.decay / 6f));
            }

            GameStateMachine.Instance.MainPlayer.gameplay.settings.skaterCOMPID_Riding.P = Main.settings.ridingCOM.x;
            GameStateMachine.Instance.MainPlayer.gameplay.settings.skaterCOMPID_Riding.I = Main.settings.ridingCOM.y;
            GameStateMachine.Instance.MainPlayer.gameplay.settings.skaterCOMPID_Riding.D = Main.settings.ridingCOM.z;

            GameStateMachine.Instance.MainPlayer.gameplay.settings.skaterCOMPID_Impact.P = Main.settings.impactCOM.x;
            GameStateMachine.Instance.MainPlayer.gameplay.settings.skaterCOMPID_Impact.I = Main.settings.impactCOM.y;
            GameStateMachine.Instance.MainPlayer.gameplay.settings.skaterCOMPID_Impact.D = Main.settings.impactCOM.z;

            GameStateMachine.Instance.MainPlayer.gameplay.settings.skaterCOMPID_ImpactUp.P = Main.settings.impactUpCOM.x;
            GameStateMachine.Instance.MainPlayer.gameplay.settings.skaterCOMPID_ImpactUp.I = Main.settings.impactUpCOM.y;
            GameStateMachine.Instance.MainPlayer.gameplay.settings.skaterCOMPID_ImpactUp.D = Main.settings.impactUpCOM.z;

            GameStateMachine.Instance.MainPlayer.gameplay.settings.skaterCOMPID_Setup.P = Main.settings.setupCOM.x;
            GameStateMachine.Instance.MainPlayer.gameplay.settings.skaterCOMPID_Setup.I = Main.settings.setupCOM.y;
            GameStateMachine.Instance.MainPlayer.gameplay.settings.skaterCOMPID_Setup.D = Main.settings.setupCOM.z;

            GameStateMachine.Instance.MainPlayer.gameplay.settings.skaterCOMPID_Grind.P = Main.settings.grindCOM.x;
            GameStateMachine.Instance.MainPlayer.gameplay.settings.skaterCOMPID_Grind.I = Main.settings.grindCOM.y;
            GameStateMachine.Instance.MainPlayer.gameplay.settings.skaterCOMPID_Grind.D = Main.settings.grindCOM.z;

            //deck.transform.localScale = Main.settings.board_size;
            /*visible_deck.transform.localScale = visible_deck_replay.transform.localScale = Main.settings.board_size;
            backtruck.transform.localScale = fronttruck.transform.localScale = backtruck_replay.transform.localScale = fronttruck_replay.transform.localScale = Main.settings.truck_size;*/
        }

        string last_gsm_state = "";
        void UpdateLastState()
        {
            last_gsm_state = GameStateMachine.Instance.CurrentState.GetType().ToString();
        }

        public bool CanFlickCatchWithLeftStick()
        {
            return GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.ForwardDir > Main.settings.FlickThreshold || GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.ForwardDir < -Main.settings.FlickThreshold || GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.ToeAxis > Main.settings.FlickThreshold || GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.ToeAxis < -Main.settings.FlickThreshold;
        }

        // Token: 0x060001F9 RID: 505 RVA: 0x0002AB78 File Offset: 0x00028D78
        public bool CanFlickCatchWithRightStick()
        {
            return GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.ForwardDir > Main.settings.FlickThreshold || GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.ForwardDir < -Main.settings.FlickThreshold || GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.ToeAxis > Main.settings.FlickThreshold || GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.ToeAxis < -Main.settings.FlickThreshold;
        }

        void DoDebug()
        {
            if (Main.settings.debug)
            {
                if (center_collider.GetComponent<MeshRenderer>()) center_collider.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.white);
                else { center_collider.gameObject.AddComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.white); }

                if (tail_collider.GetComponent<MeshRenderer>()) tail_collider.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.white);
                else { tail_collider.gameObject.AddComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.white); }

                if (nose_collider.GetComponent<MeshRenderer>()) nose_collider.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.white);
                else { nose_collider.gameObject.AddComponent<MeshRenderer>().material.SetColor("_BaseColor", Color.white); }
            }
            else
            {
                if (center_collider.GetComponent<MeshRenderer>()) Destroy(center_collider.GetComponent<MeshRenderer>());
                if (tail_collider.GetComponent<MeshRenderer>()) Destroy(tail_collider.GetComponent<MeshRenderer>());
                if (nose_collider.GetComponent<MeshRenderer>()) Destroy(nose_collider.GetComponent<MeshRenderer>());
            }
        }

        void PowerSlideForce()
        {
            int multiplier = GameStateMachine.Instance.MainPlayer.gameplay.playerData.GetBoardBackwards() ? -1 : 1;
            multiplier *= GameStateMachine.Instance.MainPlayer.gameplay.playerData.IsSwitch ? -1 : 1;

            if (Main.settings.powerslide_velocitybased)
            {
                GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.AddRelativeForce(new Vector3(0, 0, (GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRigidbody.velocity.magnitude / 40) * multiplier), ForceMode.Impulse);
            }
            else
            {
                GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.AddRelativeForce(new Vector3(0, 0, .16f * multiplier), ForceMode.Impulse);
            }
        }

        void AlternativeArms()
        {
            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup)
            {
                if (Main.settings.alternative_arms_damping) GameStateMachine.Instance.MainPlayer.gameplay.playerData.ragdoll.SetArmWeights(.25f, .46f, .25f);
            }

            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.BeginPop)
            {
                LerpDisableArmPhysics();
            }

            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Pop)
            {
                LerpEnableArmPhysics();
            }
        }

        void PushModes()
        {
            if (Main.settings.sonic_mode)
            {
                ScaleAnimSpeed(.2f + GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRigidbody.velocity.magnitude);
            }
            if (Main.settings.push_by_velocity)
            {
                ScaleAnimSpeed(1 - GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRigidbody.velocity.magnitude / 90);
            }
        }

        void ScaleAnimSpeed(float speed)
        {
            GameStateMachine.Instance.MainPlayer.gameplay.animationController.skaterAnim.speed = speed;
            GameStateMachine.Instance.MainPlayer.gameplay.animationController.ikAnim.speed = speed;
            GameStateMachine.Instance.MainPlayer.gameplay.animationController.steezeAnim.speed = speed;
            GameStateMachine.Instance.MainPlayer.gameplay.playerData.animation.Speed = speed;
        }

        void ScaleAnimSpeedSteez(float speed)
        {
            GameStateMachine.Instance.MainPlayer.gameplay.animationController.steezeAnim.speed = speed;
        }

        void LerpDisableArmPhysics()
        {
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.minMappingWeight = Mathf.Lerp(GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.minMappingWeight, 0, Time.smoothDeltaTime / 10);
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.maxMappingWeight = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[1].props.minMappingWeight = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[1].props.maxMappingWeight = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[2].props.minMappingWeight = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[2].props.maxMappingWeight = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[3].props.minMappingWeight = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[3].props.maxMappingWeight = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[4].props.minMappingWeight = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[4].props.maxMappingWeight = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[5].props.minMappingWeight = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[5].props.maxMappingWeight = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
        }
        void LerpEnableArmPhysics()
        {
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.minMappingWeight = 0f;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.maxMappingWeight = Mathf.Lerp(GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.maxMappingWeight, 1, Time.smoothDeltaTime / 10);
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[1].props.minMappingWeight = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.maxMappingWeight;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[1].props.maxMappingWeight = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.maxMappingWeight;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[2].props.minMappingWeight = 0f;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[2].props.maxMappingWeight = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.maxMappingWeight;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[3].props.minMappingWeight = 0f;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[3].props.maxMappingWeight = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.maxMappingWeight;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[4].props.minMappingWeight = 0f;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[4].props.maxMappingWeight = 0f;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[5].props.minMappingWeight = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.maxMappingWeight;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[5].props.maxMappingWeight = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.groupOverrides[0].props.maxMappingWeight;
        }

        public void PlayLetsGoAnim()
        {
            letsgo_anim_time = 0;
        }

        float letsgo_anim_time = 1;
        float letsgoanim_frame = 0;
        float letsgoanim_length = 30;
        void LetsGoAnim()
        {
            if (letsgo_anim_time == 0) letsgo_anim_time = Time.unscaledTime;

            if (Time.unscaledTime - letsgo_anim_time <= 3f)
            {
                GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[6].rigidbody.mass = 1f;
                GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[9].rigidbody.mass = 1f;

                float sin = (float)Math.Sin(Time.unscaledTime * 12f) / 7f;
                float multiplier = 1;
                multiplier = GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Regular ? multiplier : -multiplier;
                float sw_multiplier = GameStateMachine.Instance.MainPlayer.gameplay.playerData.IsSwitch ? -1 : 1;

                GameObject center_left = new GameObject();
                center_left.transform.position = GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRoot.position;
                center_left.transform.rotation = GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRoot.rotation;
                if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Braking && GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Regular)
                {
                    center_left.transform.Translate(new Vector3(sw_multiplier == -1 ? .2f * multiplier : -.2f * multiplier, .75f + sin, .25f * multiplier), Space.Self);
                }
                else
                {
                    if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Regular)
                    {
                        center_left.transform.Translate(new Vector3(sw_multiplier == -1 ? .2f * multiplier : .2f * multiplier, .75f + sin, .25f * multiplier), Space.Self);
                    }
                    else
                    {
                        center_left.transform.Translate(new Vector3(sw_multiplier == -1 ? -.2f * multiplier : .2f * multiplier, .75f + sin, .25f * multiplier), Space.Self);
                    }
                }

                GameObject center_right = new GameObject();
                center_right.transform.position = GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRoot.position;
                center_right.transform.rotation = GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRoot.rotation;
                if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Braking && GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Goofy)
                {
                    center_right.transform.Translate(new Vector3(sw_multiplier == -1 ? .2f * multiplier : -.2f * multiplier, .75f + sin, -.25f * multiplier), Space.Self);
                }
                else
                {
                    center_right.transform.Translate(new Vector3(sw_multiplier == -1 && GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Regular ? -.2f * multiplier : .2f * multiplier, .75f + sin, -.25f * multiplier), Space.Self);
                }

                GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[6].transform.position = Vector3.Lerp(GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[6].transform.position, center_left.transform.position, map01(letsgoanim_frame, 0, letsgoanim_length));
                GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[9].transform.position = Vector3.Lerp(GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[9].transform.position, center_right.transform.position, map01(letsgoanim_frame, 0, letsgoanim_length));
                letsgoanim_frame++;

                Destroy(center_left);
                Destroy(center_right);
            }
            else
            {
                letsgoanim_frame = 0;
            }
        }

        GameObject pclone;
        void LookForward()
        {
            string actual_state = "";
            if (!Main.settings.look_forward) return;

            bool inState = false;

            int count = 0;
            foreach (var state in EnumHelper.StatesReal)
            {
                if (state.ToString() == GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState.ToString())
                {
                    if (Main.settings.look_forward_states[count] == true)
                    {
                        actual_state = GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState.ToString();
                        inState = true;
                    }
                }
                count++;
            }

            //if (actual_state != last_state) head_frame = 0;

            if (inState || head_frame > 0)
            {
                if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.IsSwitch)
                {
                    int state_id = (int)GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState - 1;
                    Vector3 offset = GameStateMachine.Instance.MainPlayer.gameplay.animationController.skaterAnim.GetFloat("Nollie") == 0f ? Main.settings.head_rotation_fakie[state_id] : Main.settings.head_rotation_switch[state_id];

                    if (state_id >= 12 && state_id <= 14)
                    {
                        int grind_id = (int)GameStateMachine.Instance.MainPlayer.gameplay.playerData.grind.type - 1;
                        offset = GameStateMachine.Instance.MainPlayer.gameplay.animationController.skaterAnim.GetFloat("Nollie") == 0f ? Main.settings.head_rotation_grinds_fakie[grind_id] : Main.settings.head_rotation_grinds_switch[grind_id];
                    }

                    float windup_side = GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup ? GameStateMachine.Instance.MainPlayer.gameplay.animationController.skaterAnim.GetFloat("WindUp") : 0;
                    if (Main.settings.debug) UnityModManager.Logger.Log(windup_side.ToString());

                    if (pclone == null)
                    {
                        pclone = new GameObject("HeadRotationTarget");
                        /*pclone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        pclone.GetComponent<SphereCollider>().enabled = false;
                        pclone.transform.localScale = new Vector3(.1f, .1f, .1f);*/
                    }

                    pclone.transform.rotation = GameStateMachine.Instance.MainPlayer.gameplay.skaterController.transform.rotation;
                    pclone.transform.position = GameStateMachine.Instance.MainPlayer.gameplay.skaterController.transform.position;
                    pclone.transform.Translate(0, -.4f, -1f, Space.Self);

                    Vector3 target = pclone.transform.position;

                    GameObject head_copy = new GameObject();
                    head_copy.transform.rotation = neck.rotation;
                    head_copy.transform.position = neck.position;
                    head_copy.transform.LookAt(target);

                    if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Regular)
                    {
                        head_copy.transform.Rotate(90f, 0f, 0f, Space.Self);
                        head_copy.transform.Rotate(0f, -18.5f, 0f, Space.Self);
                        head_copy.transform.Rotate(0f, 0f, 18.5f, Space.Self);
                        head_copy.transform.Rotate(0f, -13f, -10f, Space.Self);
                        head_copy.transform.Rotate(14f, 9f, 0f, Space.Self);

                        if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup)
                        {
                            if (windup_side >= 0)
                            {
                                head_copy.transform.Rotate(new Vector3(33f, 17.4f, 8.7f) * windup_side, Space.Self);
                                head_copy.transform.Rotate(new Vector3(0f, 17.4f, 0f) * windup_side, Space.Self);
                            }
                            else
                            {
                                head_copy.transform.Rotate(new Vector3(20.9f, 45.2f, 10.4f) * windup_side, Space.Self);
                            }
                        }
                    }
                    else
                    {
                        head_copy.transform.Rotate(-90f, 0f, 180f, Space.Self);
                        head_copy.transform.Rotate(0f, 18.5f, 0f, Space.Self);
                        head_copy.transform.Rotate(0f, 0f, 18.5f, Space.Self);
                        head_copy.transform.Rotate(0f, 13f, -10f, Space.Self);
                        head_copy.transform.Rotate(-14f, -9f, 0f, Space.Self);

                        if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup)
                        {
                            if (windup_side < 0)
                            {
                                head_copy.transform.Rotate(new Vector3(-33f, -17.4f, 8.7f) * windup_side, Space.Self);
                                head_copy.transform.Rotate(new Vector3(0f, -17.4f, 0f) * windup_side, Space.Self);
                            }
                            else
                            {
                                head_copy.transform.Rotate(new Vector3(-20.9f, -45.2f, 10.4f) * windup_side, Space.Self);
                            }
                        }
                    }
                    head_copy.transform.Rotate(offset, Space.Self);

                    neck.rotation = Quaternion.Slerp(neck.rotation, head_copy.transform.rotation, Mathf.Lerp(0, 1, map01(head_frame, 0, Main.settings.look_forward_length)));

                    Destroy(head_copy);

                    if (inState)
                    {
                        if (delay_head >= Main.settings.look_forward_delay)
                        {
                            if (last_head == Quaternion.identity) last_head = neck.rotation;
                            head_frame++;
                        }
                        delay_head++;
                    }

                    head_frame = head_frame > Main.settings.look_forward_length ? Main.settings.look_forward_length : head_frame;
                }
            }
            if (!inState && head_frame > 0)
            {
                head_frame = head_frame > Main.settings.look_forward_length ? Main.settings.look_forward_length : head_frame;
                head_frame -= 1;
                delay_head = 0;
            }
        }
        void LetsGoAnimHead()
        {
            if (letsgo_anim_time == 0) letsgo_anim_time = Time.unscaledTime;

            if (Time.unscaledTime - letsgo_anim_time <= 3f)
            {
                float multiplier = 0;
                multiplier = GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Regular ? multiplier : 1;
                float sin = (float)Math.Sin(Time.unscaledTime * 12f) / 7f;
                neck.transform.rotation = Quaternion.Slerp(neck.transform.rotation, spine.transform.rotation, map01(letsgoanim_frame, 0, letsgoanim_length));
                Vector3 target = Vector3.Lerp(Vector3.zero, new Vector3(sin * 10, sin * 15, (sin * 33.3f)), map01(letsgoanim_frame, 0, letsgoanim_length) / 2);
                neck.transform.Rotate(target);
            }
        }

        public static float map01(float value, float min, float max)
        {
            return (value - min) * 1f / (max - min);
        }

        public static bool IsInside(Collider c, Vector3 point)
        {
            Vector3 closest = c.ClosestPoint(point);
            return closest == point;
        }

        GameObject LeftLight;
        Light leftLightComp;
        HDAdditionalLightData leftLightAdditionalData;

        GameObject RightLight;
        Light rightLightComp;
        HDAdditionalLightData rightLightAdditionalData;

        Vector3 last_multi_pos = Vector3.zero;

        void MultiplayerFilmer()
        {
            Muscle left_hand = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[6];
            Muscle right_hand = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[9];

            if (!Main.settings.follow_mode_left && !Main.settings.follow_mode_right && !Main.settings.follow_mode_head) return;

            if (IsPumping() || Main.settings.multi_filmer_activation == "Always on")
            {
                if (Main.settings.follow_mode_left || Main.settings.follow_mode_right || Main.settings.follow_mode_head)
                {
                    Transform player = getOnlinePlayer();
                    if (player != null)
                    {
                        if (Main.settings.follow_mode_left)
                        {
                            Vector3 rot = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[4].transform.rotation.eulerAngles;
                            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[4].transform.rotation = Quaternion.Euler(-Main.settings.filmer_arm_angle, rot.y, rot.z);
                            left_hand.transform.LookAt(player);
                            left_hand.transform.Rotate(0, -90, 90);
                        }
                        if (Main.settings.follow_mode_right)
                        {
                            Vector3 rot = GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[7].transform.rotation.eulerAngles;
                            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[7].transform.rotation = Quaternion.Euler(Main.settings.filmer_arm_angle, rot.y, rot.z);
                            right_hand.transform.LookAt(player);
                            right_hand.transform.Rotate(0, -90, 90);
                        }

                        if (Main.settings.follow_mode_head)
                        {
                            head.transform.LookAt(player);
                            head.transform.Rotate(0, -90, -90);
                        }
                    }
                }
            }

            if (GameStateMachine.Instance.CurrentState.GetType() == typeof(PlayState))
            {
                if (Main.settings.filmer_light)
                {
                    if (Main.settings.follow_mode_left)
                    {
                        if (LeftLight == null) CreateLeftLight();

                        leftLightAdditionalData.intensity = Main.settings.filmer_light_intensity;
                        leftLightComp.intensity = Main.settings.filmer_light_intensity;
                        leftLightComp.spotAngle = Main.settings.filmer_light_spotangle;
                        leftLightComp.range = Main.settings.filmer_light_range;

                        LeftLight.transform.position = left_hand.transform.position;
                        LeftLight.transform.Translate(new Vector3(-.075f, 0f, .15f), Space.Self);
                        LeftLight.transform.rotation = left_hand.transform.rotation;
                        LeftLight.transform.transform.Rotate(90, 0, 0);
                    }
                    else
                    {
                        if (leftLightComp != null) leftLightComp.intensity = 0;
                    }

                    if (Main.settings.follow_mode_right)
                    {
                        if (RightLight == null) CreateRightLight();

                        rightLightAdditionalData.intensity = Main.settings.filmer_light_intensity;
                        rightLightComp.intensity = Main.settings.filmer_light_intensity;
                        rightLightComp.spotAngle = Main.settings.filmer_light_spotangle;
                        rightLightComp.range = Main.settings.filmer_light_range;

                        RightLight.transform.position = right_hand.transform.position;
                        RightLight.transform.Translate(new Vector3(-.075f, 0f, .15f), Space.Self);
                        RightLight.transform.rotation = right_hand.transform.rotation;
                        RightLight.transform.transform.Rotate(90, 0, 0);
                    }
                    else
                    {
                        if (rightLightComp != null) rightLightComp.intensity = 0;
                    }
                }
            }
        }

        void LerpLookAt(Transform obj, Transform target, Vector3 offset)
        {
            Vector3 relativePos = target.transform.position - obj.transform.position;
            Quaternion toRotation = Quaternion.LookRotation(relativePos);
            obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, toRotation, 1);
            obj.transform.Rotate(offset.x, offset.y, offset.z);
        }

        void CreateLeftLight()
        {
            LeftLight = new GameObject("LeftLight");
            leftLightComp = LeftLight.AddComponent<Light>();
            leftLightAdditionalData = LeftLight.AddComponent<HDAdditionalLightData>();
            leftLightAdditionalData.lightUnit = LightUnit.Ev100;
            leftLightComp.type = LightType.Spot;
            leftLightAdditionalData.intensity = Main.settings.filmer_light_intensity;
            leftLightComp.intensity = Main.settings.filmer_light_intensity;
            leftLightComp.spotAngle = Main.settings.filmer_light_spotangle;
            leftLightComp.range = Main.settings.filmer_light_range;
            LeftLight.transform.localScale = new Vector3(.1f, 0.1f, 0.1f);
            LeftLight.AddComponent<BoxCollider>().enabled = false;
            LeftLight.AddComponent<Rigidbody>();
            LeftLight.AddComponent<ObjectTracker>();
        }

        void CreateRightLight()
        {
            RightLight = new GameObject("RightLight");
            rightLightComp = RightLight.AddComponent<Light>();
            rightLightAdditionalData = RightLight.AddComponent<HDAdditionalLightData>();
            rightLightAdditionalData.lightUnit = LightUnit.Ev100;
            rightLightComp.type = LightType.Spot;
            rightLightAdditionalData.intensity = Main.settings.filmer_light_intensity;
            rightLightComp.intensity = Main.settings.filmer_light_intensity;
            rightLightComp.spotAngle = Main.settings.filmer_light_spotangle;
            rightLightComp.range = Main.settings.filmer_light_range;
            RightLight.transform.localScale = new Vector3(.1f, 0.1f, 0.1f);
            RightLight.AddComponent<BoxCollider>().enabled = false;
            RightLight.AddComponent<Rigidbody>();
            RightLight.AddComponent<ObjectTracker>();
        }

        public void ManualVerticalFlip()
        {
            int multiplier = 1;
            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.y > 0f || GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.y > 0f) multiplier = -1;
            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.GetBoardBackwards()) multiplier *= -1;
            GameStateMachine.Instance.MainPlayer.gameplay.playerData.air.ForwardSpeed = Main.settings.ManualFlipVerticality * multiplier;
        }

        public void GrindVerticalFlip()
        {
            int multiplier = 1;
            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.y > 0f || GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.y > 0f) multiplier = -1;
            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.GetBoardBackwards()) multiplier *= -1;
            GameStateMachine.Instance.MainPlayer.gameplay.playerData.air.ForwardSpeed = Main.settings.GrindFlipVerticality * multiplier;
        }

        public bool LeaningInputRight(float sensibility)
        {
            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.x >= 0.5f && GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.x >= 0.5f)
            {
                bool left_centered = GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.y <= sensibility && GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.y >= -sensibility;
                bool right_centered = GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.y <= sensibility && GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.y >= -sensibility;
                if (left_centered && right_centered) return true;
            }
            return false;
        }

        public bool LeaningInputLeft(float sensibility)
        {
            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.x <= -0.5f && GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.x <= -0.5f)
            {
                bool left_centered = GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.y <= sensibility && GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.y >= -sensibility;
                bool right_centered = GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.y <= sensibility && GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.y >= -sensibility;
                if (left_centered && right_centered) return true;
            }
            return false;
        }

        public void Wobble()
        {
            Vector3 euler = GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.transform.localRotation.eulerAngles;
            GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.transform.localRotation = Quaternion.Euler(euler.x, euler.y, euler.z + WobbleValue());

            //GameStateMachine.Instance.MainPlayer.gameplay.comController.UpdateCOM();
        }

        public float WobbleValue(float multiplier = 1f)
        {
            float mag_multi = GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Pushing || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Powerslide ? .15f / 2 : .15f;
            float vel = GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.velocity.magnitude - Main.settings.wobble_offset;
            vel = vel < 0 ? 0 : vel * multiplier;

            return Mathf.Sin(Time.fixedUnscaledTime * 30 + (vel * mag_multi) + ((float)rd.NextDouble() * 2)) * (mag_multi * vel);
        }

        public void SetBailedPuppet()
        {
            setMotionType(GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[3], ConfigurableJointMotion.Free);
            setMotionType(GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[4], ConfigurableJointMotion.Limited);
            setMotionType(GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[5], ConfigurableJointMotion.Limited);
            setMotionType(GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[6], ConfigurableJointMotion.Limited);
            setMotionType(GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[7], ConfigurableJointMotion.Limited);
            setMotionType(GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[8], ConfigurableJointMotion.Limited);
            setMotionType(GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[9], ConfigurableJointMotion.Limited);
            setMotionType(GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[10], ConfigurableJointMotion.Limited);
            setMotionType(GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[11], ConfigurableJointMotion.Limited);
            setMotionType(GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[12], ConfigurableJointMotion.Locked);
            setMotionType(GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[13], ConfigurableJointMotion.Limited);
            setMotionType(GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[14], ConfigurableJointMotion.Limited);
            setMotionType(GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[15], ConfigurableJointMotion.Locked);

            for (int i = 0; i < GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles.Length; i++)
            {
                if (Main.settings.debug) UnityModManager.Logger.Log(i + " " + GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[i].name + " " + GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[i].joint.xMotion);
                GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.pinWeight = 1;
                GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[i].props.mappingWeight = 1f;
                GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[i].rigidbody.solverIterations = 5;
                GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[i].rigidbody.mass = 5f;

                if (i >= 10)
                {
                    setMotionType(GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[i], ConfigurableJointMotion.Limited);
                }
            }

            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[1].rigidbody.mass = 20f;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[3].rigidbody.mass = 5f;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[12].rigidbody.mass = 1f;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[15].rigidbody.mass = 1f;

            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[6].rigidbody.mass = 1f;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[9].rigidbody.mass = 1f;

            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.maxCollisions = 50;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.maxRigidbodyVelocity = 100f;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.canGetUp = true;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.maxGetUpVelocity = 0f;
            GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.getUpDelay = 0;

            bailed_puppet = true;
        }

        public void RestorePuppet()
        {
            for (int i = 0; i < GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles.Length; i++)
            {
                GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[i].rigidbody.solverIterations = 1;
                GameStateMachine.Instance.MainPlayer.gameplay.ragdollController.behaviourPuppet.puppetMaster.muscles[i].rigidbody.mass = 1f;
            }

            bailed_puppet = false;
        }

        public void DisableCameraCollider(bool enabled)
        {
            if (!cinemachine_collider)
            {
                cinemachine_collider = GameStateMachine.Instance.MainPlayer.gameplay.cameraController.gameObject.GetComponentInChildren<Cinemachine.CinemachineCollider>();
            }
            if (cinemachine_collider != null)
            {
                cinemachine_collider.enabled = enabled;
            }
        }

        public bool keyframe_state = false;
        int keyframe_count = 0;
        float saved_time = 0;
        public void FilmerKeyframes()
        {
            if (saved_time == 0) saved_time = ReplayEditorController.Instance.playbackController.CurrentTime;
            float time = ReplayEditorController.Instance.playbackController.ClipEndTime - saved_time;

            if (Main.settings.keyframe_start_of_clip)
            {
                ReplayEditorController.Instance.playbackController.CurrentTime = ReplayEditorController.Instance.playbackController.ClipStartTime;
                time = ReplayEditorController.Instance.playbackController.ClipEndTime - ReplayEditorController.Instance.playbackController.ClipStartTime;
            }

            float pace = time / Main.settings.keyframe_sample;

            if (keyframe_count < Main.settings.keyframe_sample)
            {
                if (Main.settings.keyframe_start_of_clip)
                {
                    ReplayEditorController.Instance.playbackController.UpdateTimeAndScale(ReplayEditorController.Instance.playbackController.ClipStartTime + (keyframe_count * pace), 1);
                }
                else
                {
                    ReplayEditorController.Instance.playbackController.UpdateTimeAndScale(saved_time + (keyframe_count * pace), 1);
                }

                Transform target = head_replay;
                if (Main.settings.keyframe_target == "Left Hand") target = left_hand_replay;
                if (Main.settings.keyframe_target == "Right Hand") target = right_hand_replay;
                if (Main.settings.keyframe_target == "Filmer Object" && object_found != null) target = object_found.transform;

                // UnityModManager.Logger.Log(target == null ? "null" : target.name);

                if (target == null) return;

                if (Main.settings.keyframe_start_of_clip)
                {
                    AddKeyFrame(ReplayEditorController.Instance.playbackController.ClipStartTime + (keyframe_count * pace), target);
                }
                else
                {
                    AddKeyFrame(saved_time + (keyframe_count * pace), target);
                }
                ReplayEditorController.Instance.cameraController.keyframeUI.UpdateKeyframes(ReplayEditorController.Instance.cameraController.keyFrames);
                keyframe_count++;
            }
            else
            {
                keyframe_count = 0;
                saved_time = 0;
                keyframe_state = false;
            }
        }

        Transform replay_transform;
        ReplayEditor.KeyframeUIController keyframes;
        void getReplayUI()
        {
            Transform main = GameStateMachine.Instance.MainPlayer.gameplay.skaterController.transform.parent.transform.parent.transform.parent;
            replay_transform = main.Find("ReplayEditor");
            keyframes = replay_transform.GetComponent<ReplayEditor.ReplayEditorController>().cameraController.keyframeUI;
        }

        void AddKeyFrame(float time, Transform target)
        {
            time = time + Main.settings.time_offset;
            int index = FindKeyFrameInsertIndex(time);
            KeyFrame keyFrame;
            GameObject copy = new GameObject();
            copy.transform.position = target.transform.position;
            copy.transform.rotation = target.transform.rotation;
            if (Main.settings.keyframe_target != "Head")
            {
                copy.transform.Rotate(90, 0, -90);
                copy.transform.Translate(new Vector3(0, -.175f, .25f), Space.Self);
            }
            else
            {
                copy.transform.Rotate(-90, 0, 90);
                copy.transform.Translate(new Vector3(0, 0.125f, 0.125f), Space.Self);
            }
            keyFrame = new FreeCameraKeyFrame(copy.transform, Main.settings.keyframe_fov, time);
            keyFrame.AddKeyframes(ReplayEditorController.Instance.cameraController.cameraCurve);
            ReplayEditorController.Instance.cameraController.keyFrames.Insert(index, keyFrame);
            Destroy(copy);
        }

        int FindKeyFrameInsertIndex(float time)
        {
            if (ReplayEditorController.Instance.cameraController.keyFrames.Count == 0)
            {
                return 0;
            }
            if (time < ReplayEditorController.Instance.cameraController.keyFrames[0].time)
            {
                return 0;
            }
            if (ReplayEditorController.Instance.cameraController.keyFrames.Count == 1)
            {
                return 1;
            }
            for (int i = 0; i < ReplayEditorController.Instance.cameraController.keyFrames.Count - 1; i++)
            {
                if (time > ReplayEditorController.Instance.cameraController.keyFrames[i].time && time < ReplayEditorController.Instance.cameraController.keyFrames[i + 1].time)
                {
                    return i + 1;
                }
            }
            return ReplayEditorController.Instance.cameraController.keyFrames.Count;
        }

        int bumpoutcount = -1;
        public bool IsBumping = false;
        public void LateUpdate()
        {
            if (!Main.settings.enabled) return;

            if (MultiplayerManager.Instance.clientState != ClientState.Disconnected && Main.settings.reset_inactive)
            {
                Traverse.Create(MultiplayerManager.Instance).Field("AFKTimeOutFactor").SetValue(Mathf.Infinity);
            }

            if (Main.settings.disable_popup)
            {
                MessageSystem.instance.enabled = false;
            }
            else if (!MessageSystem.instance.enabled)
            {
                MessageSystem.instance.enabled = true;
            }

            if (Main.settings.wobble && IsGrounded())
            {
                Wobble();
            }

            if (GameStateMachine.Instance.CurrentState.GetType() == typeof(PlayState))
            {
                UpdateColliders();
                DoScalingPlaystate();
            }
            if (GameStateMachine.Instance.CurrentState.GetType() == typeof(ReplayState))
            {
                if (!replay_skater) replay_skater = getReplayEditor();
                DoScalingReplaystate();
            }

            UpdateLastState();
            LogState();
        }

        void DoScalingPlaystate()
        {
            head.localScale = new Vector3(Main.settings.custom_scale_head, Main.settings.custom_scale_head, Main.settings.custom_scale_head);
            left_hand.localScale = new Vector3(Main.settings.custom_scale_hand_l, Main.settings.custom_scale_hand_l, Main.settings.custom_scale_hand_l);
            right_hand.localScale = new Vector3(Main.settings.custom_scale_hand_r, Main.settings.custom_scale_hand_r, Main.settings.custom_scale_hand_r);
            left_foot.localScale = new Vector3(Main.settings.custom_scale_foot_l, Main.settings.custom_scale_foot_l, Main.settings.custom_scale_foot_l);
            right_foot.localScale = new Vector3(Main.settings.custom_scale_foot_r, Main.settings.custom_scale_foot_r, Main.settings.custom_scale_foot_r);

            neck.localScale = new Vector3(Main.settings.custom_scale_neck, Main.settings.custom_scale_neck, Main.settings.custom_scale_neck);
            pelvis.localScale = new Vector3(Main.settings.custom_scale_pelvis, Main.settings.custom_scale_pelvis, Main.settings.custom_scale_pelvis);
            spine1.localScale = new Vector3(Main.settings.custom_scale_spine, Main.settings.custom_scale_spine, Main.settings.custom_scale_spine);
            spine2.localScale = new Vector3(Main.settings.custom_scale_spine2, Main.settings.custom_scale_spine2, Main.settings.custom_scale_spine2);
            left_arm.localScale = new Vector3(Main.settings.custom_scale_arm_l, Main.settings.custom_scale_arm_l, Main.settings.custom_scale_arm_l);
            left_forearm.localScale = new Vector3(Main.settings.custom_scale_forearm_l, Main.settings.custom_scale_forearm_l, Main.settings.custom_scale_forearm_l);
            right_arm.localScale = new Vector3(Main.settings.custom_scale_arm_r, Main.settings.custom_scale_arm_r, Main.settings.custom_scale_arm_r);
            right_forearm.localScale = new Vector3(Main.settings.custom_scale_forearm_r, Main.settings.custom_scale_forearm_r, Main.settings.custom_scale_forearm_r);
            left_upleg.localScale = new Vector3(Main.settings.custom_scale_upleg_l, Main.settings.custom_scale_upleg_l, Main.settings.custom_scale_upleg_l);
            left_leg.localScale = new Vector3(Main.settings.custom_scale_leg_l, Main.settings.custom_scale_leg_l, Main.settings.custom_scale_leg_l);
            right_upleg.localScale = new Vector3(Main.settings.custom_scale_upleg_r, Main.settings.custom_scale_upleg_r, Main.settings.custom_scale_upleg_r);
            right_leg.localScale = new Vector3(Main.settings.custom_scale_leg_r, Main.settings.custom_scale_leg_r, Main.settings.custom_scale_leg_r);

            GameStateMachine.Instance.MainPlayer.gameplay.skaterController.gameObject.transform.localScale = Main.settings.custom_scale;
            //GameStateMachine.Instance.MainPlayer.gameplay.playerData.skater.playerCollider.transform.localScale = Main.settings.custom_scale;
        }

        void DoScalingReplaystate()
        {
            replay_skater.localScale = Main.settings.custom_scale;
            head_replay.localScale = new Vector3(Main.settings.custom_scale_head, Main.settings.custom_scale_head, Main.settings.custom_scale_head);
            left_hand_replay.localScale = new Vector3(Main.settings.custom_scale_hand_l, Main.settings.custom_scale_hand_l, Main.settings.custom_scale_hand_l);
            right_hand_replay.localScale = new Vector3(Main.settings.custom_scale_hand_r, Main.settings.custom_scale_hand_r, Main.settings.custom_scale_hand_r);
            left_foot_replay.localScale = new Vector3(Main.settings.custom_scale_foot_l, Main.settings.custom_scale_foot_l, Main.settings.custom_scale_foot_l);
            right_foot_replay.localScale = new Vector3(Main.settings.custom_scale_foot_r, Main.settings.custom_scale_foot_r, Main.settings.custom_scale_foot_r);

            neck_replay.localScale = new Vector3(Main.settings.custom_scale_neck, Main.settings.custom_scale_neck, Main.settings.custom_scale_neck);
            pelvis_replay.localScale = new Vector3(Main.settings.custom_scale_pelvis, Main.settings.custom_scale_pelvis, Main.settings.custom_scale_pelvis);
            spine1_replay.localScale = new Vector3(Main.settings.custom_scale_spine, Main.settings.custom_scale_spine, Main.settings.custom_scale_spine);
            spine2_replay.localScale = new Vector3(Main.settings.custom_scale_spine2, Main.settings.custom_scale_spine2, Main.settings.custom_scale_spine2);
            left_arm_replay.localScale = new Vector3(Main.settings.custom_scale_arm_l, Main.settings.custom_scale_arm_l, Main.settings.custom_scale_arm_l);
            left_forearm_replay.localScale = new Vector3(Main.settings.custom_scale_forearm_l, Main.settings.custom_scale_forearm_l, Main.settings.custom_scale_forearm_l);
            right_arm_replay.localScale = new Vector3(Main.settings.custom_scale_arm_r, Main.settings.custom_scale_arm_r, Main.settings.custom_scale_arm_r);
            right_forearm_replay.localScale = new Vector3(Main.settings.custom_scale_forearm_r, Main.settings.custom_scale_forearm_r, Main.settings.custom_scale_forearm_r);
            left_upleg_replay.localScale = new Vector3(Main.settings.custom_scale_upleg_l, Main.settings.custom_scale_upleg_l, Main.settings.custom_scale_upleg_l);
            left_leg_replay.localScale = new Vector3(Main.settings.custom_scale_leg_l, Main.settings.custom_scale_leg_l, Main.settings.custom_scale_leg_l);
            right_upleg_replay.localScale = new Vector3(Main.settings.custom_scale_upleg_r, Main.settings.custom_scale_upleg_r, Main.settings.custom_scale_upleg_r);
            right_leg_replay.localScale = new Vector3(Main.settings.custom_scale_leg_r, Main.settings.custom_scale_leg_r, Main.settings.custom_scale_leg_r);
        }

        float tail_collider_offset = 0;
        float nose_collider_offset = 0;
        void UpdateColliders()
        {
            float nose_tail_size = Main.settings.nose_tail_collider / 100f;
            tail_collider.transform.localScale = new Vector3(.2014f + tail_collider_offset, .116f, nose_tail_size);
            tail_collider.transform.localPosition = new Vector3(0, .307f, -nose_tail_size / 2f);
            nose_collider.transform.localScale = new Vector3(.2014f + nose_collider_offset, .116f, nose_tail_size);
            nose_collider.transform.localPosition = new Vector3(0, -.307f, -nose_tail_size / 2f);
        }

        void WaveAnim()
        {
            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState.ToString() == Main.settings.wave_on)
            {
                SkaterXL.Gameplay.AnimatorStateInfo currentAnimatorStateInfo = GameStateMachine.Instance.MainPlayer.gameplay.animationController.skaterAnim.GetCurrentAnimatorStateInfo(1);
                if (!currentAnimatorStateInfo.IsName("Wink"))
                {
                    GestureAnimationController.Instances[GestureAnimationController.Instances.Count - 1].PlayAnimation("Wink");
                }
            }
        }

        void CheckLetsgoAnim()
        {
            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState.ToString() == Main.settings.celebrate_on && GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState != PlayerStateEnum.Bailed)
            {
                PlayLetsGoAnim();
            }
        }

        public void DisableMultiPopup(bool disabled)
        {
            if (disabled)
            {
                MultiplayerManager.PopupDuration = 0.00000001f;
                MessageSystem.instance.enabled = false;
            }
            else
            {
                MultiplayerManager.PopupDuration = 3f;
                MessageSystem.instance.enabled = true;
            }
        }

        public void CameraFeet()
        {
            Transform left_pos = (Transform)Traverse.Create(GameStateMachine.Instance.MainPlayer.gameplay.ikController).Field("ikLeftFootPositionOffset").GetValue();
            left_pos.position = new Vector3(left_pos.position.x, GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.position.y, left_pos.position.z);
            Transform right_pos = (Transform)Traverse.Create(GameStateMachine.Instance.MainPlayer.gameplay.ikController).Field("ikRightFootPositionOffset").GetValue();
            right_pos.position = new Vector3(right_pos.position.x, GameStateMachine.Instance.MainPlayer.gameplay.boardController.boardRigidbody.position.y, right_pos.position.z);
        }

        int skate_mask = 1 << LayerMask.NameToLayer("Skateboard");
        public void DynamicFeet()
        {
            int multi = 1;
            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.GetBoardBackwards()) multi = -1;

            if (was_leaning || GameStateMachine.Instance.CurrentState.GetType() != typeof(PlayState) || IsBumping) return;

            int count = 0;
            foreach (var state in EnumHelper.StatesReal)
            {
                if (state.ToString() == GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState.ToString())
                {
                    if (Main.settings.dynamic_feet_states[count] == true)
                    {
                        LeftFootRaycast(multi);
                        RightFootRaycast(multi);
                        //GameStateMachine.Instance.MainPlayer.gameplay.ScalePlayerCollider();
                    }
                }
                count++;
            }
        }

        int grinding_count = 0, offset_frame = 0, offset_delay = 0;
        float left_foot_setup_rand = 0;
        GameObject temp_go_left, temp_go_right;
        void LeftFootRaycast(int multiplier)
        {
            if(!temp_go_left) temp_go_left = new GameObject("Left foot raycast dynamic feet");
            Transform left_pos = (Transform)Traverse.Create(GameStateMachine.Instance.MainPlayer.gameplay.ikController).Field("ikAnimLeftFootTarget").GetValue();
            if (left_pos == null) return;

            temp_go_left.transform.position = left_pos.transform.position;
            temp_go_left.transform.rotation = GameStateMachine.Instance.MainPlayer.gameplay.transformReference.boardTransform.rotation;
            multiplier = GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Regular ? multiplier : -multiplier;

            temp_go_left.transform.Translate(new Vector3(0.085f * multiplier, 0f, 0.060f * multiplier), Space.Self);
            Vector3 target = temp_go_left.transform.position;

            if (Physics.Raycast(target, -temp_go_left.transform.up, out left_hit, 10f, skate_mask))
            {
                float offset = (Main.settings.left_foot_offset / 10);

                if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.IsSwitch)
                {
                    if (GameStateMachine.Instance.MainPlayer.gameplay.animationController.skaterAnim.GetFloat("Nollie") == 0f)
                    {
                        if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup)
                        {
                            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Regular) offset += 0;
                            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Goofy) offset -= .008f;
                        }
                    }
                    else
                    {
                        if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup)
                        {
                            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Regular) offset -= .008f;
                            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Goofy) offset += .002f;
                        }
                    }
                }
                else
                {
                    if (GameStateMachine.Instance.MainPlayer.gameplay.animationController.skaterAnim.GetFloat("Nollie") == 0f)
                    {
                        if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup)
                        {
                            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Regular) offset += .002f;
                            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Goofy) offset -= .01f;
                        }
                    }
                    else
                    {
                        if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup)
                        {
                            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Regular) offset -= .008f;
                            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Goofy) offset += .004f;
                        }
                    }
                }

                temp_go_left.transform.position = left_hit.point;
                temp_go_left.transform.rotation = Quaternion.LookRotation(temp_go_left.transform.forward, left_hit.normal);

                if (Main.settings.feet_rotation)
                {
                    float old_y = left_pos.rotation.eulerAngles.y;
                    float offset_setup = 0;
                    if (Main.settings.jiggle_on_setup)
                    {
                        if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup)
                        {
                            float limit = Main.settings.jiggle_limit;
                            if (offset_frame <= limit && offset_delay >= Main.settings.jiggle_delay)
                            {
                                if (left_foot_setup_rand >= .5f)
                                {
                                    offset_setup = -Mathf.Sin((limit - offset_frame) / (12f + left_foot_setup_rand)) * (limit - offset_frame);
                                }
                                offset_frame++;
                            }
                            offset_delay++;
                        }
                        else
                        {
                            left_foot_setup_rand = (float)rd.NextDouble();
                            offset_frame = 0;
                            offset_delay = 0;
                        }
                    }

                    left_pos.rotation = Quaternion.LookRotation(left_pos.transform.forward, left_hit.normal);
                    left_pos.transform.Rotate(new Vector3(0, 0, GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Riding ? 90f : 93f), Space.Self);
                    left_pos.rotation = Quaternion.Euler(left_pos.rotation.eulerAngles.x, old_y + offset_setup, left_pos.rotation.eulerAngles.z);
                    GameStateMachine.Instance.MainPlayer.gameplay.playerData.ik.ikAnimLeftFootTarget.rotation = left_pos.rotation;
                }

                if (Main.settings.feet_offset && !Main.settings.camera_feet)
                {
                    left_pos.position = new Vector3(left_pos.position.x, left_pos.position.y + (offset - left_hit.distance), left_pos.position.z);
                    GameStateMachine.Instance.MainPlayer.gameplay.playerData.ik.ikAnimLeftFootTarget.position = left_pos.position;
                }
            }
        }

        float right_foot_setup_rand = 0;
        void RightFootRaycast(int multiplier)
        {
            if (!temp_go_right) temp_go_right = new GameObject("Right foot raycast dynamic feet");
            Transform right_pos = (Transform)Traverse.Create(GameStateMachine.Instance.MainPlayer.gameplay.ikController).Field("ikAnimRightFootTarget").GetValue();
            if (right_pos == null) return;

            temp_go_right.transform.position = right_pos.transform.position;
            temp_go_right.transform.rotation = GameStateMachine.Instance.MainPlayer.gameplay.transformReference.boardTransform.rotation;
            multiplier = GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Regular ? multiplier : -multiplier;

            bool additional_offset = GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Goofy && (GameStateMachine.Instance.MainPlayer.gameplay.playerData.IsSwitch || GameStateMachine.Instance.MainPlayer.gameplay.animationController.skaterAnim.GetFloat("Nollie") > 0) && (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Manual);
            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Regular) additional_offset = (!GameStateMachine.Instance.MainPlayer.gameplay.playerData.IsSwitch || GameStateMachine.Instance.MainPlayer.gameplay.animationController.skaterAnim.GetFloat("Nollie") == 0) && (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Manual);
            temp_go_right.transform.Translate(new Vector3(0.085f * multiplier, 0f, additional_offset ? -0.04f * multiplier : 0.01f * multiplier), Space.Self);
            Vector3 target = temp_go_right.transform.position;

            if (Physics.Raycast(target, -temp_go_right.transform.up, out right_hit, 10f, skate_mask))
            {
                float offset = (Main.settings.right_foot_offset / 10);

                if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.IsSwitch)
                {
                    if (GameStateMachine.Instance.MainPlayer.gameplay.animationController.skaterAnim.GetFloat("Nollie") == 0f)
                    {
                        if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup)
                        {
                            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Regular) offset -= .01f;
                            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Goofy) offset += .006f;
                        }
                    }
                    else
                    {
                        if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup)
                        {
                            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Regular) offset += .006f;
                            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Goofy) offset -= .008f;
                        }
                    }
                }
                else
                {
                    if (GameStateMachine.Instance.MainPlayer.gameplay.animationController.skaterAnim.GetFloat("Nollie") == 0f)
                    {
                        if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup)
                        {
                            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Regular) offset -= .006f;
                            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Goofy) offset += .002f;
                        }
                    }
                    else
                    {
                        if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup)
                        {
                            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Regular) offset += .003f;
                            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Goofy) offset -= .003f;
                        }
                    }
                }

                temp_go_right.transform.position = right_hit.point;
                temp_go_right.transform.rotation = Quaternion.LookRotation(temp_go_right.transform.forward, right_hit.normal);

                if (Main.settings.feet_rotation)
                {
                    float old_y = right_pos.rotation.eulerAngles.y;

                    float offset_setup = 0;

                    if (Main.settings.jiggle_on_setup)
                    {
                        if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup)
                        {
                            float limit = Main.settings.jiggle_limit;
                            if (offset_frame <= limit && offset_delay >= Main.settings.jiggle_delay)
                            {
                                if (right_foot_setup_rand >= .5f)
                                {
                                    offset_setup = Mathf.Sin((limit - offset_frame) / (14f + right_foot_setup_rand)) * (limit - offset_frame);
                                }
                                offset_frame++;
                            }
                            offset_delay++;
                        }
                        else
                        {
                            right_foot_setup_rand = (float)rd.NextDouble();
                            offset_frame = 0;
                            offset_delay = 0;
                        }
                    }

                    right_pos.rotation = Quaternion.LookRotation(right_pos.transform.forward, right_hit.normal);
                    right_pos.transform.Rotate(new Vector3(0, 0, GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Riding ? 90f : 92f), Space.Self);
                    right_pos.rotation = Quaternion.Euler(right_pos.rotation.eulerAngles.x, old_y + offset_setup, right_pos.rotation.eulerAngles.z);
                }

                if (Main.settings.feet_offset && !Main.settings.camera_feet)
                {
                    right_pos.position = new Vector3(right_pos.position.x, right_pos.position.y + (offset - right_hit.distance), right_pos.position.z);
                }
            }
        }

        void setDebugCube(GameObject cube, GameObject temp_go, Color c)
        {
            if (!Main.settings.debug) return;
            try
            {
                if (cube)
                {
                    cube.transform.position = temp_go.transform.position;
                    cube.transform.rotation = temp_go.transform.rotation;
                    cube.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", c);
                }
            }
            catch
            {
                UnityModManager.Logger.Log("error");
            }
        }

        bool InAir()
        {
            return GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Pop || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.InAir || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Release;
        }

        bool Grinding()
        {
            return GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Grinding || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.EnterCoping || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.ExitCoping;
        }

        public bool IsGroundedNoGrind()
        {
            return GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Impact || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Riding || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Manual || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Powerslide || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Pushing || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Braking;
        }

        public bool IsGrounded()
        {
            return GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Impact || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Riding || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Manual || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Powerslide || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Pushing || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Braking || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Grinding || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.EnterCoping || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.ExitCoping;
        }

        public bool IsGroundedForFeet()
        {
            return GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Impact || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Setup || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Manual || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Grinding || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.EnterCoping || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.ExitCoping;
        }

        Transform replay;
        Transform left_foot_replay, right_foot_replay;
        Transform neck_replay, pelvis_replay, spine1_replay, spine2_replay, left_arm_replay, left_forearm_replay, right_arm_replay, right_forearm_replay, left_upleg_replay, left_leg_replay, right_upleg_replay, right_leg_replay;
        Transform getReplayEditor()
        {
            Transform main = GameStateMachine.Instance.MainPlayer.gameplay.skaterController.transform.parent.transform.parent.transform.parent;
            replay = main.Find("ReplayEditor");
            Transform playback = replay.Find("Playback Skater Root");
            Transform skater = playback.Find("NewSkater");
            Transform joints = skater.Find("Skater_Joints");

            head_replay = joints.FindChildRecursively("Skater_Head");
            left_hand_replay = joints.FindChildRecursively("Skater_hand_l");
            right_hand_replay = joints.FindChildRecursively("Skater_hand_r");
            left_foot_replay = joints.FindChildRecursively("Skater_foot_l");
            right_foot_replay = joints.FindChildRecursively("Skater_foot_r");
            pelvis_replay = joints.FindChildRecursively("Skater_pelvis");
            left_upleg_replay = joints.FindChildRecursively("Skater_UpLeg_l");
            left_leg_replay = joints.FindChildRecursively("Skater_Leg_l");
            right_upleg_replay = joints.FindChildRecursively("Skater_UpLeg_r");
            right_leg_replay = joints.FindChildRecursively("Skater_Leg_r");
            spine1_replay = joints.FindChildRecursively("Skater_Spine1");
            spine2_replay = joints.FindChildRecursively("Skater_Spine2");
            neck_replay = joints.FindChildRecursively("Skater_Neck");
            left_forearm_replay = joints.FindChildRecursively("Skater_ForeArm_l");
            right_forearm_replay = joints.FindChildRecursively("Skater_ForeArm_r");
            left_arm_replay = joints.FindChildRecursively("Skater_Arm_l");
            right_arm_replay = joints.FindChildRecursively("Skater_Arm_r");

            return skater;
        }

        public void getDeck()
        {
            Transform parent = GameStateMachine.Instance.MainPlayer.gameplay.boardController.gameObject.transform;
            Transform deck = parent.Find("Deck");
            Transform colliders = deck.Find("Colliders");

            center_collider = colliders.Find("Cube (5)");
            center_collider.transform.localScale = new Vector3(.2137f, .5149f, .01f);
            center_collider.transform.localPosition = new Vector3(0, 0, -0.01f);
            /*if (center_collider.GetComponent<MeshRenderer>() == null)
            {
                center_collider.gameObject.AddComponent<MeshRenderer>();
                center_collider.GetComponent<MeshRenderer>().material.shader = Shader.Find("HDRP/Lit");
            }*/

            tail_collider = colliders.Find("Cube (1)");
            /*if (tail_collider.GetComponent<MeshRenderer>() == null)
            {
                tail_collider.gameObject.AddComponent<MeshRenderer>();
                tail_collider.GetComponent<MeshRenderer>().material.shader = Shader.Find("HDRP/Lit");
            }*/

            nose_collider = colliders.Find("Cube (2)");
            /*if (nose_collider.GetComponent<MeshRenderer>() == null)
            {
                nose_collider.gameObject.AddComponent<MeshRenderer>();
                nose_collider.GetComponent<MeshRenderer>().material.shader = Shader.Find("HDRP/Lit");
            }*/

            UnityModManager.Logger.Log("Deck initialized, " + center_collider.name + " " + tail_collider.name + " " + nose_collider.name);
        }

        public void checkDebug()
        {
            try
            {
                if (Main.settings.debug)
                {
                    debug_cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    debug_cube.GetComponent<Collider>().enabled = false;
                    debug_cube.GetComponent<MeshRenderer>().material.shader = Shader.Find("HDRP/Lit");
                    debug_cube.transform.localScale = new Vector3(.1f, .1f, .1f);
                    debug_cube.AddComponent<Rigidbody>();
                    // debug_cube.AddComponent<ObjectTracker>();

                    debug_cube_2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    debug_cube_2.GetComponent<Collider>().enabled = false;
                    debug_cube_2.GetComponent<MeshRenderer>().material.shader = Shader.Find("HDRP/Lit");
                    debug_cube_2.transform.localScale = new Vector3(.1f, .1f, .1f);
                    debug_cube_2.AddComponent<Rigidbody>();
                    // debug_cube_2.AddComponent<ObjectTracker>();

                    debug_cube_3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    debug_cube_3.GetComponent<Collider>().enabled = false;
                    debug_cube_3.GetComponent<MeshRenderer>().material.shader = Shader.Find("HDRP/Lit");
                    debug_cube_3.transform.localScale = new Vector3(.05f, .05f, .05f);
                    debug_cube_3.AddComponent<Rigidbody>();
                    // debug_cube_3.AddComponent<ObjectTracker>();

                    debug_cube_4 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    debug_cube_4.GetComponent<Collider>().enabled = false;
                    debug_cube_4.GetComponent<MeshRenderer>().material.shader = Shader.Find("HDRP/Lit");
                    debug_cube_4.transform.localScale = new Vector3(.05f, .05f, .05f);
                    debug_cube_4.AddComponent<Rigidbody>();
                    // debug_cube_4.AddComponent<ObjectTracker>();

                    center_collider.gameObject.GetComponent<MeshRenderer>().enabled = Main.settings.debug;
                    tail_collider.gameObject.GetComponent<MeshRenderer>().enabled = Main.settings.debug;
                    nose_collider.gameObject.GetComponent<MeshRenderer>().enabled = Main.settings.debug;
                }
                else
                {
                    if (debug_cube != null) Destroy(debug_cube);
                    if (debug_cube_2 != null) Destroy(debug_cube_2);
                    if (debug_cube_3 != null) Destroy(debug_cube_3);
                    if (debug_cube_4 != null) Destroy(debug_cube_4);
                }
            }
            catch { }
        }

        public Transform left_hand, right_hand, pelvis, spine1, spine2, left_arm, left_forearm, right_arm, right_forearm, left_upleg, left_leg, right_upleg, right_leg;
        Transform left_toe_1, left_toe_2, right_toe_1, right_toe_2;
        void getFeet()
        {
            Transform parent = GameStateMachine.Instance.MainPlayer.gameplay.skaterController.gameObject.transform;
            Transform joints = parent.Find("Skater_Joints");
            pelvis = joints.FindChildRecursively("Skater_pelvis");

            left_upleg = joints.FindChildRecursively("Skater_UpLeg_l");
            left_leg = joints.FindChildRecursively("Skater_Leg_l");
            left_foot = joints.FindChildRecursively("Skater_foot_l");

            right_upleg = joints.FindChildRecursively("Skater_UpLeg_r");
            right_leg = joints.FindChildRecursively("Skater_Leg_r");
            right_foot = joints.FindChildRecursively("Skater_foot_r");

            spine = joints.FindChildRecursively("Skater_Spine");
            spine1 = joints.FindChildRecursively("Skater_Spine1");
            spine2 = joints.FindChildRecursively("Skater_Spine2");
            neck = joints.FindChildRecursively("Skater_Neck");
            head = joints.FindChildRecursively("Skater_Head");

            left_forearm = joints.FindChildRecursively("Skater_ForeArm_l");
            right_forearm = joints.FindChildRecursively("Skater_ForeArm_r");
            left_arm = joints.FindChildRecursively("Skater_Arm_l");
            right_arm = joints.FindChildRecursively("Skater_Arm_r");
            left_hand = joints.FindChildRecursively("Skater_hand_l");
            right_hand = joints.FindChildRecursively("Skater_hand_r");

            left_toe_1 = joints.FindChildRecursively("Skater_Toe1_l");
            left_toe_2 = joints.FindChildRecursively("Skater_Toe2_l");
            right_toe_1 = joints.FindChildRecursively("Skater_Toe1_r");
            right_toe_2 = joints.FindChildRecursively("Skater_Toe2_r");

            UnityModManager.Logger.Log("Body initialized, " + right_foot.name + " " + left_foot.name);
        }

        bool IsPumping()
        {
            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.stance == SkaterXL.Core.Stance.Regular)
            {
                if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.y >= 0.5f || GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.y <= -0.5f) return true;
            }
            else
            {
                if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.RightStick.value.y <= -0.5f || GameStateMachine.Instance.MainPlayer.gameplay.playerData.controllerInput.LeftStick.value.y >= 0.5f) return true;
            }

            return false;
        }

        public string getStance()
        {
            string stance = "";
            if (GameStateMachine.Instance.MainPlayer.gameplay.playerData.IsSwitch)
            {
                stance = "Switch";
                if (GameStateMachine.Instance.MainPlayer.gameplay.animationController.skaterAnim.GetFloat("Nollie") == 0f) stance = "Fakie";
            }
            return stance;
        }

        public GameObject object_found = null;
        public void scanObject()
        {
            object_found = GameObject.Find(Main.settings.filmer_object_target);
        }

        public void ForceLODs()
        {
            var lod_objects = FindObjectsOfType<UnityEngine.LODGroup>();
            for (int i = 0; i < lod_objects.Length; i++)
            {
                lod_objects[i].ForceLOD(0);
            }
        }

        public void ResetLODs()
        {
            var lod_objects = FindObjectsOfType<UnityEngine.LODGroup>();
            for (int i = 0; i < lod_objects.Length; i++)
            {
                lod_objects[i].ForceLOD(-1);
            }
        }

        string[] ignore_scaling = new string[] { "SpawnPoint", "DistanceTool", "SceneSaveManager", "CinemachineCollider Collider", "Transform for customcameracontroller"};
        public void ScaleMap()
        {
            GameObject rootCollection = GameObject.Find("rootCollection");
            if(rootCollection == null)
            {
                rootCollection = new GameObject("rootCollection");

                // Find all root GameObjects in the scene
                GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

                // Iterate through the root objects and add them to the collectionObject, along with their children
                foreach (GameObject rootObject in rootObjects)
                {
                    bool add = true;
                    for(int i = 0; i < ignore_scaling.Length; i++)
                    {
                        if (rootObject.name == ignore_scaling[i]) add = false;
                    }

                    if(add) rootObject.transform.parent = rootCollection.transform;
                }
            }

            rootCollection.transform.localScale = Main.settings.map_scale;
        }
    }
}
