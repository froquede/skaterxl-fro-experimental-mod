using Cinemachine;
using GameManagement;
using HarmonyLib;
using ModIO.UI;
using Photon.Realtime;
using ReplayEditor;
using RootMotion.Dynamics;
using SkaterXL.Core;
using FSMHelper;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace fro_mod
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
        public bool was_leaning = false;
        Cinemachine.CinemachineCollider cinemachine_collider;
        public CinemachineVirtualCamera mainCam;
        bool bailed_puppet = false;
        System.Random rd = new System.Random();
        Transform replay_skater;
        FSMStateMachineLogic m_Logic;
        List<FSMStateMachineLogic> m_ChildSMs;

        public void Start()
        {
            m_Logic = (FSMStateMachineLogic)Traverse.Create(PlayerController.Instance.playerSM).Field("m_Logic").GetValue();
            m_ChildSMs = (List<FSMStateMachineLogic>)Traverse.Create(m_Logic).Field("m_ChildSMs").GetValue();

            replay_skater = getReplayEditor();

            DisableMultiPopup(Main.settings.disable_popup);
            DisableCameraCollider(Main.settings.camera_avoidance);
            MultiplayerManager.ROOMSIZE = 10;
            PlayerController.Instance.skaterController.skaterRigidbody.maxDepenetrationVelocity = .1f;
            PlayerController.Instance.skaterController.leftFootCollider.attachedRigidbody.maxDepenetrationVelocity = .1f;
            PlayerController.Instance.skaterController.rightFootCollider.attachedRigidbody.maxDepenetrationVelocity = .1f;

            PlayerController.Instance.boardController.boardRigidbody.solverIterations = 1;
            PlayerController.Instance.boardController.backTruckRigidbody.solverIterations = 1;
            PlayerController.Instance.boardController.frontTruckRigidbody.solverIterations = 1;
            PlayerController.Instance.boardController.boardRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            PlayerController.Instance.boardController.backTruckRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            PlayerController.Instance.boardController.frontTruckRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            PlayerController.Instance.boardController.backTruckRigidbody.interpolation = PlayerController.Instance.boardController.frontTruckRigidbody.interpolation = RigidbodyInterpolation.None;

            setSkateDepenetration(1.25f, 1.25f);

# if DEBUG
            checkDebug();
# endif
            getReplayUI();

            PlayerController.Instance.respawn.DoRespawn();
        }

        public void setSkateDepenetration(float valboard, float valtrucks)
        {
            PlayerController.Instance.boardController.boardRigidbody.maxDepenetrationVelocity = valboard;
            PlayerController.Instance.boardController.backTruckRigidbody.maxDepenetrationVelocity = valtrucks;
            PlayerController.Instance.boardController.frontTruckRigidbody.maxDepenetrationVelocity = valtrucks;
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
            if (PlayerController.Instance.currentStateEnum.ToString() != last_state)
            {
                last_state = PlayerController.Instance.currentStateEnum.ToString();
                if (Main.settings.debug) Utils.Log(last_state);
            }
        }

        public bool IsGrabbing() { return (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Grabs) || EventManager.Instance.IsGrabbing; }
        int powerslide_anim = 0;
        bool dividedVertical = false;
        bool shouldResetTarget = false;

        LayerMask layerMask = ~(1 << LayerMask.NameToLayer("Skateboard"));
        public float head_frame = 0, delay_head = 0;
        bool bumpOverride = false;
        public void Update()
        {
            if (!Main.settings.enabled) return;

            if (center_collider == null) getDeck();
            if (left_foot == null) getFeet();

            MultiplayerFilmer();

            //LookForward();
            LetsGoAnimHead();

            if (Main.settings.shuv_fix)
            {
                if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.BeginPop || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Pop)
                {
                    if (PlayerController.Instance.boardController.secondVel >= 1f || PlayerController.Instance.boardController.secondVel <= -1f)
                    {
                        if (!Utils.isOllie() && PlayerController.Instance.boardController.thirdVel >= -1f && PlayerController.Instance.boardController.thirdVel <= 1f)
                        {
                            PlayerController.Instance.ikController.SetIKLerpSpeed(24f);

                            PlayerController.Instance.ikController._finalIk.solver.leftFootEffector.positionWeight = 0;
                            PlayerController.Instance.ikController._finalIk.solver.rightFootEffector.positionWeight = 0;
                        }
                    }
                }
            }

            if (keyframe_state == true)
            {
                FilmerKeyframes();
            }

            if (Main.settings.alternative_powerslide && PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Powerslide)
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

            /*if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.InAir)
            {
                PlayerController.Instance.ikController.ForceLeftLerpValue(1);
                PlayerController.Instance.ikController.ForceRightLerpValue(1);
            }

            if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Impact)
            {
                PlayerController.Instance.ikController.ForceLeftLerpValue(0);
                PlayerController.Instance.ikController.ForceRightLerpValue(0);
            }*/

            bool left = PlayerController.Instance.inputController.player.GetButtonDown("Left Stick Button"), right = PlayerController.Instance.inputController.player.GetButtonDown("Right Stick Button");

            if ((last_state == PlayerController.CurrentState.Grinding.ToString() || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.ExitCoping) && PlayerController.Instance.currentStateEnum != PlayerController.CurrentState.Impact)
            {
                if ((right || left) && !IsBumping)
                {
                    if (Main.settings.bump_anim)
                    {
                        PlayerController.Instance.ScaleDisplacementCurve(Vector3.ProjectOnPlane(PlayerController.Instance.skaterController.skaterTransform.position - PlayerController.Instance.boardController.boardTransform.position, PlayerController.Instance.skaterController.skaterTransform.forward).magnitude * .75f);
                        PlayerController.Instance.ikController.ResetIKOffsets();
                        //PlayerController.Instance.boardController.ResetAll();
                        PlayerController.Instance.AnimSetGrinding(false);
                        PlayerController.Instance.animationController.ikAnim.SetFloat("Nollie", PlayerController.Instance.inputController.RightStick.rawInput.pos.y > .1f || PlayerController.Instance.inputController.LeftStick.rawInput.pos.y >= .1f ? 1f : 0f);
                        PlayerController.Instance.animationController.skaterAnim.SetFloat("Nollie", PlayerController.Instance.inputController.RightStick.rawInput.pos.y > .1f || PlayerController.Instance.inputController.LeftStick.rawInput.pos.y >= .1f ? 1f : 0f);
                        PlayerController.Instance.animationController.ikAnim.SetBool("Released", false);
                        PlayerController.Instance.animationController.skaterAnim.SetBool("Released", false);
                        PlayerController.Instance.animationController.SetTweakValues(0, 0);
                        PlayerController.Instance.animationController.SetTweakMagnitude(0, 0);
                        PlayerController.Instance.AnimSetFlip(0);
                        PlayerController.Instance.AnimForceFlipValue(0);
                        PlayerController.Instance.AnimSetScoop(0);
                        PlayerController.Instance.AnimForceScoopValue(0);

                        PlayerController.Instance.SetTurnMultiplier(3f);
                        PlayerController.Instance.SetKneeBendWeightManually(1f);
                        PlayerController.Instance.respawn.behaviourPuppet.BoostImmunity(1000f);
                        PlayerController.Instance.cameraController.NeedToSlowLerpCamera = true;
                        MonoBehaviourSingleton<SoundManager>.Instance.PlayMovementFoleySound(0.3f, true);
                        PlayerController.Instance.SetIKLerpSpeed(4.5f);
                        PlayerController.Instance.boardController.SetBoardControllerUpVector(PlayerController.Instance.skaterController.skaterTransform.up);
                        PlayerController.Instance.ScalePlayerCollider();
                        PlayerController.Instance.SetRotationTarget();
                        PlayerController.Instance.ikController.SetLeftLerpTarget(0f, 0f);
                        PlayerController.Instance.ikController.SetRightLerpTarget(0f, 0f);
                        PlayerController.Instance.ikController._finalIk.solver.leftFootEffector.positionWeight = PlayerController.Instance.ikController._finalIk.solver.rightFootEffector.positionWeight = 1;

                        if (Main.settings.bump_anim_pop)
                        {
                            PlayerController.Instance.animationController.ikAnim.CrossFadeInFixedTime("Pop", Main.settings.bump_pop_delay);
                            PlayerController.Instance.animationController.skaterAnim.CrossFadeInFixedTime("Pop", Main.settings.bump_pop_delay);
                        }

                        PlayerController.Instance.boardController.ResetTweakValues();
                        PlayerController.Instance.boardController.CacheBoardUp();
                        PlayerController.Instance.boardController.UpdateReferenceBoardTargetRotation();

                        //PlayerController.Instance.boardController.boardRigidbody.angularVelocity = Vector3.zero;
                        PlayerController.Instance.boardController.backTruckRigidbody.angularVelocity = Vector3.zero;
                        PlayerController.Instance.boardController.frontTruckRigidbody.angularVelocity = Vector3.zero;
                        //PlayerController.Instance.skaterController.skaterRigidbody.angularVelocity = Vector3.zero;
                    }

                    //MonoBehaviourSingleton<EventManager>.Instance.EnterAir(PlayerController.Instance.inputController.RightStick.rawInput.pos.y > .1f || PlayerController.Instance.inputController.LeftStick.rawInput.pos.y >= .1f ? PlayerController.Instance.IsSwitch ? PlayerController.Instance.animationController.skaterAnim.GetFloat("Nollie") == 0f ? PopType.Fakie : PopType.Switch : PopType.Nollie : PopType.Ollie, 0f);
                    IsBumping = true;
                    bumpOverride = true;
                }

                if (IsGroundedNoGrind()) IsBumping = false;
            }
            else IsBumping = false;

            if (Main.settings.catch_acc_enabled)
            {
                if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Pop)
                {
                    if (!dividedVertical)
                    {
                        PlayerController.Instance.boardController.firstVel *= 1.01f;
                        dividedVertical = true;
                    }
                }
                else
                {
                    dividedVertical = false;
                }

                CatchAtAnyMoment();
            }

            if (IsGrounded() && PlayerController.Instance.currentStateEnum != PlayerController.CurrentState.Pushing && shouldResetTarget)
            {
                shouldResetTarget = false;
            }

            if (IsGrounded())
            {
                bumpOverride = false;

                left_foot_velocity = right_foot_velocity = 0f;
                leftpos = PlayerController.Instance.ikController._finalIk.solver.leftFootEffector.positionWeight;
                rightpos = PlayerController.Instance.ikController._finalIk.solver.rightFootEffector.positionWeight;
                left_caught = right_caught = forced_caught = false;
                forced_caught_count = 0;
                PlayerController.Instance.ikController._finalIk.solver.leftFootEffector.rotationWeight = 1;
                PlayerController.Instance.ikController._finalIk.solver.rightFootEffector.rotationWeight = 1;
            }

            if ((Main.settings.alternative_powerslide && PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Powerslide) || powerslide_anim > 0)
            {
                CustomPowerSlideBoard();
            }
        }

        public bool shouldRunCatch()
        {
            if (Main.settings.catch_acc_enabled && !IsGrounded() && !Utils.IsGrabbing())
            {
                if (Utils.isOllie()) return true;
                else
                {
                    if (Main.controller.forced_caught) return true;
                    else return false;
                }
            }
            else return true;
        }

        Vector3 contact_point_l, contact_point_r;
        int point_count = 0;
        bool point_found = false;
        RaycastHit rayCastOut;
        void CustomPowerSlide()
        {
            if (!Main.settings.alternative_powerslide) return;

            if (Physics.Raycast(PlayerController.Instance.boardController.boardTransform.position, -PlayerController.Instance.boardController.boardTransform.up, out rayCastOut, 0.4f, LayerUtility.GroundMask))
            {
                int multiplier = PlayerController.Instance.GetBoardBackwards() ? -1 : 1;
                multiplier *= PlayerController.Instance.IsSwitch ? -1 : 1;
                int side_multiplier = Main.tc.getTypeOfInput().Contains("both-inside") ? -1 : 1;

                if (SettingsManager.Instance.stance == Stance.Goofy) side_multiplier *= -1;

                float vel_map = map01(PlayerController.Instance.skaterController.skaterRigidbody.velocity.magnitude, Main.settings.powerslide_minimum_velocity, Main.settings.powerslide_max_velocity);
                vel_map = vel_map > 1 ? 1 : vel_map;
                float anim_length = map01(powerslide_anim, 0, Main.settings.powerslide_animation_length);
                anim_length = anim_length > 1 ? 1 : anim_length;

                PlayerController.Instance.SetKneeBendWeightManually(anim_length);
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

                PlayerController.Instance.animationController.ScaleAnimSpeed(anim_length);

                Destroy(pelvis_copy);
            }
        }

        float last_hit_ps = 0;
        GameObject boardCopy;
        void CustomPowerSlideBoard()
        {
            int multiplier = PlayerController.Instance.GetBoardBackwards() ? -1 : 1;
            multiplier *= PlayerController.Instance.IsSwitch ? -1 : 1;
            int side_multiplier = Main.tc.getTypeOfInput().Contains("both-inside") ? -1 : 1;

            if (SettingsManager.Instance.stance == Stance.Goofy) side_multiplier *= -1;

            float vel_map = map01(PlayerController.Instance.skaterController.skaterRigidbody.velocity.magnitude, Main.settings.powerslide_minimum_velocity, Main.settings.powerslide_max_velocity);
            vel_map = vel_map > 1 ? 1 : vel_map;
            float anim_length = map01(powerslide_anim, 0, Main.settings.powerslide_animation_length);
            anim_length = anim_length > 1 ? 1 : anim_length;

            Quaternion rothit = Quaternion.LookRotation(rayCastOut.normal);

            if (!boardCopy) boardCopy = new GameObject();
            boardCopy.transform.position = PlayerController.Instance.boardController.boardRigidbody.transform.position;
            boardCopy.transform.rotation = PlayerController.Instance.boardController.boardRigidbody.transform.rotation;

            float angle = Vector3.Angle(rayCastOut.normal, Vector3.up);
            Vector3 temp = Vector3.Cross(rayCastOut.normal, Vector3.down);
            Vector3 cross = Vector3.Cross(temp, rayCastOut.normal);

            float crossZ = last_hit_ps - rayCastOut.point.y >= 0 ? 1 : -1;
            if (side_multiplier < 0) crossZ = last_hit_ps - rayCastOut.point.y < 0 ? 1 : -1;
            float extra_powerslide = Mathf.Lerp(0, (vel_map * Main.settings.powerslide_maxangle * multiplier * side_multiplier), anim_length);
            boardCopy.transform.rotation = Quaternion.Euler(boardCopy.transform.rotation.eulerAngles.x, boardCopy.transform.rotation.eulerAngles.y, (multiplier * crossZ * -angle) + extra_powerslide);
            last_hit_ps = rayCastOut.point.y;
            /*Vector3 rot = PlayerController.Instance.boardController.boardRigidbody.transform.rotation.eulerAngles;
            PlayerController.Instance.boardController.boardRigidbody.transform.rotation = Quaternion.Euler(rot.x, rot.y, Mathf.Lerp(rothit.eulerAngles.z, rothit.eulerAngles.z + (vel_map * (Main.settings.powerslide_maxangle * multiplier * side_multiplier)), anim_length));*/

            PlayerController.Instance.boardController.boardRigidbody.transform.rotation = boardCopy.transform.rotation;

            //PlayerController.Instance.boardController.UpdateBoardPosition();
            PlayerController.Instance.SetRotationTarget();
        }

        void PreventBail()
        {
            PlayerController.Instance.CancelInvoke("DoBail");
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
        StickInput left_stick_freezed = new StickInput();
        StickInput right_stick_freezed = new StickInput();
        float last_time_bailed = 0;
        int slow_count = 0, bailed_count = 0;
        public Vector3 velPopCache = Vector3.zero;
        public float target_left = 0, target_right = 0;

        public void FixedUpdate()
        {
            if (!Main.settings.enabled) return;
            if (PlayerController.Instance.currentStateEnum != PlayerController.CurrentState.Pop && PlayerController.Instance.currentStateEnum != PlayerController.CurrentState.Release) PlayerController.Instance.animationController.ScaleAnimSpeed(1f);

            if (Main.settings.alternative_powerslide)
            {
                Physics.Raycast(PlayerController.Instance.boardController.boardTransform.position, -PlayerController.Instance.boardController.boardTransform.up, out rayCastOut, 0.8f, LayerUtility.GroundMask);
            }

            //if (IsGrounded()) PlayerController.Instance.skaterController.skaterTargetTransform.position = Vector3.MoveTowards(PlayerController.Instance.skaterController.skaterTargetTransform.position, PlayerController.Instance.skaterController.animBoardTargetTransform.position, Time.fixedDeltaTime * 72f);

            DoDebug();

            if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Pop)
            {
                target_left = (float)Traverse.Create(PlayerController.Instance.ikController).Field("_ikLeftLerpPosTarget").GetValue();
                leftpos = PlayerController.Instance.ikController._finalIk.solver.leftFootEffector.positionWeight;
                target_right = (float)Traverse.Create(PlayerController.Instance.ikController).Field("_ikRightLerpPosTarget").GetValue();
                rightpos = PlayerController.Instance.ikController._finalIk.solver.rightFootEffector.positionWeight;
            }

            if (MultiplayerManager.ROOMSIZE != (byte)Main.settings.multiplayer_lobby_size) MultiplayerManager.ROOMSIZE = (byte)Main.settings.multiplayer_lobby_size;

            if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Powerslide && Main.settings.powerslide_force) PowerSlideForce();

            if (Main.settings.wave_on != "Disabled") WaveAnim();
            if (Main.settings.celebrate_on != "Disabled" && PlayerController.Instance.currentStateEnum != PlayerController.CurrentState.Bailed) CheckLetsgoAnim();

            if (Main.settings.bails && PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Bailed && PlayerController.Instance.respawn.bail.bailed)
            {
                if (!bailed_puppet) SetBailedPuppet();
                else
                {
                    /*if(bailed_count <= 4)
                    {
                        PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[2].rigidbody.AddForce(0, 60f, 0f, ForceMode.Impulse);
                    }*/
                    bailed_count++;
                }
                letsgo_anim_time = Time.unscaledTime - 10f;
            }
            else
            {
                if (bailed_puppet) RestorePuppet();
                if (PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].rigidbody.mass != Main.settings.left_hand_weight || PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].rigidbody.mass != Main.settings.right_hand_weight)
                {
                    UpdateTotalMass();
                }
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].rigidbody.mass = Main.settings.left_hand_weight;
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].rigidbody.mass = Main.settings.right_hand_weight;
                bailed_count = 0;
            }

            if (Main.settings.displacement_curve) PlayerController.Instance.ScaleDisplacementCurve(Vector3.ProjectOnPlane(PlayerController.Instance.skaterController.skaterTransform.position - PlayerController.Instance.boardController.boardTransform.position, PlayerController.Instance.skaterController.skaterTransform.forward).magnitude * 1.25f);
            if (Main.settings.alternative_arms) AlternativeArms();

            if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Pushing) PushModes();
            else pushAnim = 0f;

            if (!PlayerController.Instance.respawn.puppetMaster.isBlending)
            {
                if (Main.settings.camera_feet) CameraFeet();
                if (Main.settings.feet_rotation || Main.settings.feet_offset) DynamicFeet();
            }

            if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Riding && last_state == PlayerController.CurrentState.Pushing.ToString()) PlayerController.Instance.animationController.ScaleAnimSpeed(0f);
            if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Grinding || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.ExitCoping) GrindVerticalFlip();
            else {
                if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Manual) ManualVerticalFlip();
                else
                {
                    if (IsGrounded()) PlayerController.Instance.boardController.firstVel = 0f;
                }
            }
            if (Main.settings.lean) Lean();
            if (Main.settings.BetterDecay) PlayerController.Instance.boardController.ApplyFrictionTowardsVelocity(1 - (Main.settings.decay / 1000));

            LetsGoAnim();

            if (Main.settings.filmer_object && object_found != null) object_found.transform.position = (GameStateMachine.Instance.CurrentState.GetType() == typeof(ReplayState) ? pelvis_replay.position : pelvis.position);

            //EA.LookAtAreaAround(PlayerController.Instance.boardController.transform.position);

            if (Main.settings.forward_force_onpop && PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Pop)
            {
                int multiplier = PlayerController.Instance.GetBoardBackwards() ? -1 : 1;
                PlayerController.Instance.boardController.boardRigidbody.AddRelativeForce(0, 0, multiplier * Main.settings.forward_force * (PlayerController.Instance.IsSwitch ? -1 : 1), ForceMode.Impulse);
                PlayerController.Instance.skaterController.skaterRigidbody.AddRelativeForce(0, 0, (Main.settings.forward_force / 50f) * (PlayerController.Instance.IsSwitch ? -1 : 1), ForceMode.Impulse);
            }
            // if (IsPumping()) PlayerController.Instance.animationController.SetValue("Pumping", true);
            /*if (Grinding()) PlayerController.Instance.SetBoardPhysicsMaterial(PlayerController.FrictionType.Default);*/
        }

        string last_gsm_state = "";
        void UpdateLastState()
        {
            last_gsm_state = GameStateMachine.Instance.CurrentState.GetType().ToString();
        }

        public bool forced_caught = false;
        public int forced_caught_count = 0;
        public float leftpos = 0, rightpos = 0;
        bool left_caught = false, right_caught = false;
        float left_foot_velocity = 0f, right_foot_velocity = 0f;
        bool need_to_reset_weights = false;

        void CatchAtAnyMoment()
        {
            /*if (bumpOverride)
            {
                forced_caught = true;
                leftpos = rightpos = 0;
                return;
            }*/

            if ((PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Release || (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.InAir && !forced_caught)) && !Utils.isOllie())
            {
                List<bool> feet_detached = getFootOff();

                if (Main.settings.catch_acc_onflick)
                {
                    if (!left_caught) left_caught = SettingsManager.Instance.stance == Stance.Goofy ? CanFlickCatchWithRightStick() : CanFlickCatchWithLeftStick();
                    if (!right_caught) right_caught = SettingsManager.Instance.stance == Stance.Goofy ? CanFlickCatchWithLeftStick() : CanFlickCatchWithRightStick();
                }
                else
                {
                    if (!left_caught) left_caught = PlayerController.Instance.inputController.player.GetButtonDown("Left Stick Button");
                    if (!right_caught) right_caught = PlayerController.Instance.inputController.player.GetButtonDown("Right Stick Button");
                }


                if (feet_detached.Count >= 2)
                {
                    if (feet_detached[0] == false && feet_detached[1] == false) { }
                    else
                    {
                        if (feet_detached[0] && !feet_detached[1])
                        {
                            left_caught = false;
                            right_caught = true;
                        }
                        if (!feet_detached[0] && feet_detached[1])
                        {
                            left_caught = true;
                            right_caught = false;
                        }
                    }
                }

                if (bumpOverride)
                {
                    left_caught = right_caught = true;
                }

                shouldResetTarget = true;
                PlayerController.Instance.ScalePlayerCollider();
                PlayerController.Instance.boardController.firstVel /= 1.025f;
                if (!forced_caught) PlayerController.Instance.skaterController.leftFootCollider.isTrigger = PlayerController.Instance.skaterController.rightFootCollider.isTrigger = true;
                PlayerController.Instance.SetIKLerpSpeed(12f);
                Vector3 from = Vector3.ProjectOnPlane(PlayerController.Instance.skaterController.skaterTransform.up, PlayerController.Instance.boardController.boardTransform.forward);
                float angle = Vector3.Angle(from, PlayerController.Instance.boardController.boardTransform.up);
                float relative_rot = 1.05f - Mathf.Clamp01(Utils.map01(angle, 0, 70f));
                float step_rot = Time.fixedDeltaTime * 24f;

                if (!bumpOverride)
                {
                    PlayerController.Instance.ikController._finalIk.solver.leftFootEffector.rotationWeight = Mathf.SmoothStep(PlayerController.Instance.ikController._finalIk.solver.leftFootEffector.rotationWeight, forced_caught && left_caught ? relative_rot : 0, step_rot);
                    PlayerController.Instance.ikController._finalIk.solver.rightFootEffector.rotationWeight = Mathf.SmoothStep(PlayerController.Instance.ikController._finalIk.solver.rightFootEffector.rotationWeight, forced_caught && right_caught ? relative_rot : 0, step_rot);
                }

                if (forced_caught) forced_caught_count++;

                leftpos = Mathf.SmoothDamp(leftpos, left_caught ? 1f : 0.2f, ref left_foot_velocity, Main.settings.catch_left_time);
                rightpos = Mathf.SmoothDamp(rightpos, right_caught ? 1f : 0.2f, ref right_foot_velocity, Main.settings.catch_right_time);

                if (left_caught) PlayerController.Instance.SetLeftIKLerpTarget(0f);
                else PlayerController.Instance.SetLeftIKLerpTarget(1f);
                PlayerController.Instance.ikController._finalIk.solver.leftFootEffector.positionWeight = leftpos;

                if (right_caught) PlayerController.Instance.SetRightIKLerpTarget(0f);
                else PlayerController.Instance.SetRightIKLerpTarget(1f);
                PlayerController.Instance.ikController._finalIk.solver.rightFootEffector.positionWeight = rightpos;

                if (!forced_caught && (left_caught || right_caught))
                {
                    PlayerController.Instance.boardController.SetBoardBackwards();
                }

                bool left_c = left_caught && Utils.AlmostEquals(leftpos, .95f, .05f);
                bool right_c = right_caught && Utils.AlmostEquals(rightpos, .95f, .05f);

                if ((left_c) || (right_c))
                {
                    if (!forced_caught)
                    {
                        PlayerController.Instance.boardController.boardRigidbody.isKinematic = false;
                        PlayerController.Instance.AnimCaught(true);
                        PlayerController.Instance.AnimRelease(false);
                        ExitRelease();
                    }

                    if (left_c)
                    {
                        PlayerController.Instance.skaterController.leftFootCollider.isTrigger = false;
                        if (!forced_caught) PlayerController.Instance.playerSM.OnStickPressedSM(false);
                    }
                    if (right_c)
                    {
                        PlayerController.Instance.skaterController.rightFootCollider.isTrigger = false;
                        if (!forced_caught) PlayerController.Instance.playerSM.OnStickPressedSM(true);
                    }

                    MonoBehaviourSingleton<EventManager>.Instance.OnCatched(right_c, left_c);
                    forced_caught = true;
                }

                if (!forced_caught)
                {
                    PlayerController.Instance.AnimCaught(false);
                    PlayerController.Instance.ToggleFlipColliders(false);
                }

                need_to_reset_weights = true;
            }

            if ((IsGrounded() && need_to_reset_weights) || ((last_state == PlayerController.CurrentState.Grinding.ToString() || last_state == PlayerController.CurrentState.ExitCoping.ToString()) && PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.InAir))
            {
                PlayerController.Instance.SetLeftIKLerpTarget(0f);
                PlayerController.Instance.SetRightIKLerpTarget(0f);
                PlayerController.Instance.ikController._finalIk.solver.leftFootEffector.positionWeight = PlayerController.Instance.ikController._finalIk.solver.leftFootEffector.rotationWeight = 1f;
                PlayerController.Instance.ikController._finalIk.solver.rightFootEffector.positionWeight = PlayerController.Instance.ikController._finalIk.solver.rightFootEffector.rotationWeight = 1f;
                need_to_reset_weights = false;
                left_foot_velocity = right_foot_velocity = 0f;
                leftpos = rightpos = 1f;
            }
        }

        List<bool> getFootOff()
        {
            string res = "";
            List<bool> feet = new List<bool>();
            for (int i = 0; i < m_ChildSMs.Count; i++)
            {
                res += m_ChildSMs[i].LeftFootOff() + ", ";
                feet.Add(m_ChildSMs[i].LeftFootOff());
                res += m_ChildSMs[i].RightFootOff() + ", ";
                feet.Add(m_ChildSMs[i].RightFootOff());
            }

            return feet;
        }

        void ExitRelease()
        {
            PlayerController.Instance.cameraController.NeedToSlowLerpCamera = false;
            PlayerController.Instance.ToggleFlipTrigger(false);
            PlayerController.Instance.AnimOllieTransition(false);
            PlayerController.Instance.AnimSetRollOff(false);
            PlayerController.Instance.SetIKLerpSpeed(8f);
            PlayerController.Instance.AnimSetNoComply(false);
            PlayerController.Instance.SetMaxSteeze(0f);
            PlayerController.Instance.ScalePlayerCollider();
            PlayerController.Instance.ToggleFlipColliders(false);
            PlayerController.Instance.animationController.ScaleAnimSpeed(1f);
            PlayerController.Instance.skaterController.InitializeSkateRotation();
        }

        void SetIK()
        {
            Vector3 pos_l = (Vector3)Traverse.Create(PlayerController.Instance.ikController).Field("_finalLeftPos").GetValue();
            Vector3 pos_r = (Vector3)Traverse.Create(PlayerController.Instance.ikController).Field("_finalRightPos").GetValue();
            Quaternion rot_l = (Quaternion)Traverse.Create(PlayerController.Instance.ikController).Field("_skaterLeftFootRot").GetValue();
            Quaternion rot_r = (Quaternion)Traverse.Create(PlayerController.Instance.ikController).Field("_skaterRightFootRot").GetValue();

            PlayerController.Instance.ikController._finalIk.solver.leftFootEffector.position = pos_l;
            PlayerController.Instance.ikController._finalIk.solver.leftFootEffector.rotation = rot_l;
            PlayerController.Instance.ikController._finalIk.solver.rightFootEffector.position = pos_r;
            PlayerController.Instance.ikController._finalIk.solver.rightFootEffector.rotation = rot_r;
        }

        public bool CanFlickCatchWithLeftStick()
        {
            return PlayerController.Instance.inputController.LeftStick.ForwardDir > Main.settings.FlickThreshold || PlayerController.Instance.inputController.LeftStick.ForwardDir < -Main.settings.FlickThreshold || PlayerController.Instance.inputController.LeftStick.ToeAxis > Main.settings.FlickThreshold || PlayerController.Instance.inputController.LeftStick.ToeAxis < -Main.settings.FlickThreshold;
        }

        // Token: 0x060001F9 RID: 505 RVA: 0x0002AB78 File Offset: 0x00028D78
        public bool CanFlickCatchWithRightStick()
        {
            return PlayerController.Instance.inputController.RightStick.ForwardDir > Main.settings.FlickThreshold || PlayerController.Instance.inputController.RightStick.ForwardDir < -Main.settings.FlickThreshold || PlayerController.Instance.inputController.RightStick.ToeAxis > Main.settings.FlickThreshold || PlayerController.Instance.inputController.RightStick.ToeAxis < -Main.settings.FlickThreshold;
        }

        void UpdateTotalMass()
        {
            Rigidbody[] componentsInChildren = PlayerController.Instance.gameObject.GetComponentsInChildren<Rigidbody>();
            float num = 0f;
            foreach (Rigidbody rigidbody in componentsInChildren)
            {
                num += rigidbody.mass;
            }
            PlayerController.Instance.skaterController.totalSystemMass = num;
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
            int multiplier = PlayerController.Instance.GetBoardBackwards() ? -1 : 1;
            multiplier *= PlayerController.Instance.IsSwitch ? -1 : 1;

            if (Main.settings.powerslide_velocitybased)
            {
                PlayerController.Instance.boardController.boardRigidbody.AddRelativeForce(new Vector3(0, 0, (PlayerController.Instance.skaterController.skaterRigidbody.velocity.magnitude / 40) * multiplier), ForceMode.Impulse);
            }
            else
            {
                PlayerController.Instance.boardController.boardRigidbody.AddRelativeForce(new Vector3(0, 0, .16f * multiplier), ForceMode.Impulse);
            }
        }

        void AlternativeArms()
        {
            if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup)
            {
                if (Main.settings.alternative_arms_damping) PlayerController.Instance.SetArmWeights(.25f, .46f, .25f);
            }

            if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.BeginPop)
            {
                LerpDisableArmPhysics();
            }

            if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Pop)
            {
                LerpEnableArmPhysics();
            }
        }

        float pushAnim = 0f;
        void PushModes()
        {
            if (Main.settings.sonic_mode)
            {
                PlayerController.Instance.animationController.ScaleAnimSpeed(.2f + PlayerController.Instance.skaterController.skaterRigidbody.velocity.magnitude);
            }
            if (Main.settings.push_by_velocity)
            {
                pushAnim = Mathf.SmoothStep(pushAnim, Mathf.Clamp01(PlayerController.Instance.skaterController.skaterRigidbody.velocity.magnitude / 90), Time.fixedDeltaTime * 48f);
                PlayerController.Instance.animationController.ScaleAnimSpeed(1.1f - pushAnim);
            }
        }

        void LerpDisableArmPhysics()
        {
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.minMappingWeight = Mathf.SmoothStep(PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.minMappingWeight, 0f, Time.fixedDeltaTime * 12f);
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.maxMappingWeight = PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[1].props.minMappingWeight = PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[1].props.maxMappingWeight = PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[2].props.minMappingWeight = PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[2].props.maxMappingWeight = PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[3].props.minMappingWeight = PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[3].props.maxMappingWeight = PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[4].props.minMappingWeight = PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[4].props.maxMappingWeight = PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[5].props.minMappingWeight = PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[5].props.maxMappingWeight = PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.minMappingWeight;
        }
        void LerpEnableArmPhysics()
        {
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.minMappingWeight = 0f;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.maxMappingWeight = Mathf.SmoothStep(PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.minMappingWeight, 1f, Time.fixedDeltaTime * 12f); ;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[1].props.minMappingWeight = PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.maxMappingWeight;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[1].props.maxMappingWeight = PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.maxMappingWeight;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[2].props.minMappingWeight = 0f;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[2].props.maxMappingWeight = PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.maxMappingWeight;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[3].props.minMappingWeight = 0f;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[3].props.maxMappingWeight = PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.maxMappingWeight;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[4].props.minMappingWeight = 0f;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[4].props.maxMappingWeight = 0f;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[5].props.minMappingWeight = PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.maxMappingWeight;
            PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[5].props.maxMappingWeight = PlayerController.Instance.respawn.behaviourPuppet.groupOverrides[0].props.maxMappingWeight;
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
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].rigidbody.mass = 1f;
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].rigidbody.mass = 1f;

                float sin = (float)Math.Sin(Time.unscaledTime * 12f) / 7f;
                float multiplier = 1;
                multiplier = SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular ? multiplier : -multiplier;
                float sw_multiplier = PlayerController.Instance.IsSwitch ? -1 : 1;

                GameObject center_left = new GameObject();
                center_left.transform.position = PlayerController.Instance.skaterController.skaterTransform.position;
                center_left.transform.rotation = PlayerController.Instance.skaterController.skaterTransform.rotation;
                if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Braking && SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular)
                {
                    center_left.transform.Translate(new Vector3(sw_multiplier == -1 ? .2f * multiplier : -.2f * multiplier, .75f + sin, .25f * multiplier), Space.Self);
                }
                else
                {
                    if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular)
                    {
                        center_left.transform.Translate(new Vector3(sw_multiplier == -1 ? .2f * multiplier : .2f * multiplier, .75f + sin, .25f * multiplier), Space.Self);
                    }
                    else
                    {
                        center_left.transform.Translate(new Vector3(sw_multiplier == -1 ? -.2f * multiplier : .2f * multiplier, .75f + sin, .25f * multiplier), Space.Self);
                    }
                }

                GameObject center_right = new GameObject();
                center_right.transform.position = PlayerController.Instance.skaterController.skaterTransform.position;
                center_right.transform.rotation = PlayerController.Instance.skaterController.skaterTransform.rotation;
                if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Braking && SettingsManager.Instance.stance == SkaterXL.Core.Stance.Goofy)
                {
                    center_right.transform.Translate(new Vector3(sw_multiplier == -1 ? .2f * multiplier : -.2f * multiplier, .75f + sin, -.25f * multiplier), Space.Self);
                }
                else
                {
                    center_right.transform.Translate(new Vector3(sw_multiplier == -1 && SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular ? -.2f * multiplier : .2f * multiplier, .75f + sin, -.25f * multiplier), Space.Self);
                }

                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].transform.position = Vector3.Lerp(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].transform.position, center_left.transform.position, map01(letsgoanim_frame, 0, letsgoanim_length));
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].transform.position = Vector3.Lerp(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].transform.position, center_right.transform.position, map01(letsgoanim_frame, 0, letsgoanim_length));
                letsgoanim_frame++;

                Destroy(center_left);
                Destroy(center_right);
            }
            else
            {
                letsgoanim_frame = 0;
            }
        }

        Quaternion targetReset;
        GameObject neck_copy;
        float actual_weight = 0f;
        public bool inState = false;
        public float step_head = 0f;
        float head_velocity = 0f;
        float startHeadAnimation = 0f;

        void LookForward()
        {
            string actual_state = "";
            if (!Main.settings.look_forward) return;

            inState = false;

            int count = 0;
            foreach (var state in Enum.GetValues(typeof(PlayerController.CurrentState)))
            {
                if (state.ToString() == PlayerController.Instance.currentStateEnum.ToString())
                {
                    if (Main.settings.look_forward_states[count] == true)
                    {
                        actual_state = PlayerController.Instance.currentStateEnum.ToString();
                        if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Impact && Time.unscaledDeltaTime - startHeadAnimation >= .5f) inState = false;
                        else inState = true;
                    }
                }
                count++;
            }


            if (inState || head_frame > 0)
            {
                if (PlayerController.Instance.IsSwitch)
                {
                    int state_id = (int)PlayerController.Instance.currentStateEnum;
                    Vector3 offset = PlayerController.Instance.animationController.skaterAnim.GetFloat("Nollie") == 0f ? Main.settings.head_rotation_fakie[state_id] : Main.settings.head_rotation_switch[state_id];

                    if (state_id >= 9 && state_id <= 11)
                    {
                        int grind_id = (int)PlayerController.Instance.boardController.triggerManager.grindDetection.grindType - 1;
                        offset = PlayerController.Instance.animationController.skaterAnim.GetFloat("Nollie") == 0f ? Main.settings.head_rotation_grinds_fakie[grind_id] : Main.settings.head_rotation_grinds_switch[grind_id];
                    }

                    float windup_side = PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup ? PlayerController.Instance.animationController.skaterAnim.GetFloat("WindUp") : 0;

                    Vector3 velocity2d = PlayerController.Instance.boardController.boardRigidbody.velocity;
                    velocity2d.y = 0f;
                    Vector3 projected = Utils.TranslateWithRotation(PlayerController.Instance.skaterController.transform.position, velocity2d, PlayerController.Instance.boardController.boardRigidbody.transform.rotation);
                    Vector3 target = Utils.TranslateWithRotation(Vector3.Lerp(projected, PlayerController.Instance.skaterController.transform.position, .9f), new Vector3(-1f + (Mathf.Sin(PlayTime.time) / 9f), -.4f + (Mathf.Sin(PlayTime.time) / 4f), -3f + (Mathf.Cos(PlayTime.time / 2f) / 14f)), PlayerController.Instance.skaterController.transform.rotation);

                    if (neck_copy == null)
                    {
                        neck_copy = new GameObject();
                    }
                    neck_copy.transform.rotation = neck.rotation;
                    neck_copy.transform.position = neck.position;
                    neck_copy.transform.LookAt(target);

                    if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular)
                    {
                        neck_copy.transform.Rotate(90f, 0f, 0f, Space.Self);
                        neck_copy.transform.Rotate(0f, -18.5f, 0f, Space.Self);
                        neck_copy.transform.Rotate(0f, 0f, 18.5f, Space.Self);
                        neck_copy.transform.Rotate(0f, -13f, -10f, Space.Self);
                        neck_copy.transform.Rotate(14f, 9f, 0f, Space.Self);

                        if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup)
                        {
                            if (windup_side >= 0)
                            {
                                neck_copy.transform.Rotate(new Vector3(33f, 17.4f, 8.7f) * windup_side, Space.Self);
                                neck_copy.transform.Rotate(new Vector3(0f, 17.4f, 0f) * windup_side, Space.Self);
                            }
                            else
                            {
                                neck_copy.transform.Rotate(new Vector3(20.9f, 45.2f, 10.4f) * windup_side, Space.Self);
                            }
                        }
                    }
                    else
                    {
                        neck_copy.transform.Rotate(-90f, 0f, 180f, Space.Self);
                        neck_copy.transform.Rotate(0f, 18.5f, 0f, Space.Self);
                        neck_copy.transform.Rotate(0f, 0f, 18.5f, Space.Self);
                        neck_copy.transform.Rotate(0f, 13f, -10f, Space.Self);
                        neck_copy.transform.Rotate(-14f, -9f, 0f, Space.Self);

                        if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup)
                        {
                            if (windup_side < 0)
                            {
                                neck_copy.transform.Rotate(new Vector3(-33f, -17.4f, 8.7f) * windup_side, Space.Self);
                                neck_copy.transform.Rotate(new Vector3(0f, -17.4f, 0f) * windup_side, Space.Self);
                            }
                            else
                            {
                                neck_copy.transform.Rotate(new Vector3(-20.9f, -45.2f, 10.4f) * windup_side, Space.Self);
                            }
                        }
                    }
                    neck_copy.transform.Rotate(offset, Space.Self);

                    if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Impact)
                    {
                        neck_copy.transform.Rotate(new Vector3(0, -20f, 0), Space.Self);
                    }

                    float step = map01(head_frame, 0, Main.settings.look_forward_length);
                    step_head = Mathf.SmoothStep(step_head, step, Time.fixedDeltaTime * 32f);
                    PlayerController.Instance.headIk.SetCurrentLookDirectionWeight(1f - step);

                    Quaternion origin_rot = inState ? neck.rotation : spine2.transform.rotation * Quaternion.Euler(Main.settings.reset_head);
                    Quaternion target_rot = inState ? Quaternion.Lerp(spine2.transform.rotation, neck_copy.transform.rotation, .75f) : neck.rotation;

                    neck.rotation = Quaternion.Lerp(origin_rot, target_rot, step_head);

                    Destroy(neck_copy);

                    if (inState)
                    {
                        if (head_frame == 0) startHeadAnimation = Time.unscaledTime;
                        if (delay_head >= Main.settings.look_forward_delay)
                        {
                            head_frame++;
                        }
                        delay_head++;
                    }

                    head_frame = head_frame > Main.settings.look_forward_length ? Main.settings.look_forward_length : head_frame;
                }
            }

            if (!inState && head_frame > 0)
            {
                actual_weight = 0f;
                //PlayerController.Instance.headIk.SetCurrentLookDirectionWeight(actual_weight);
                head_frame = head_frame > Main.settings.look_forward_length ? Main.settings.look_forward_length : head_frame;
                head_frame--;
                delay_head = 0;
            }

            if (!inState && head_frame == 0)
            {
                actual_weight = Mathf.SmoothStep(actual_weight, 1, Time.fixedDeltaTime);
                //PlayerController.Instance.headIk.SetCurrentLookDirectionWeight(actual_weight);
            }
        }
        void LetsGoAnimHead()
        {
            if (letsgo_anim_time == 0) letsgo_anim_time = Time.unscaledTime;

            if (Time.unscaledTime - letsgo_anim_time <= 3f)
            {
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

        string last_side = "";
        float lean_velocity = 0f;
        Vector3 last_velocity_lean = Vector3.zero;
        void Lean()
        {
            int multiplier = 0;
            if (InAir())
            {
                //if (IsGrabbing()) return;

                List<string> input = Main.tc.getTypeOfInput();
                if (input.Contains("both-right"))
                {
                    last_side = "right";
                    multiplier = 1;
                    count++;
                }

                if (input.Contains("both-left"))
                {
                    last_side = "left";
                    multiplier = -1;
                    count++;
                }

                if (multiplier == 0)
                {
                    was_leaning = false;
                    count = 0;
                    return;
                }

                if (count <= Main.settings.wait_threshold)
                {
                    return;
                }

                if (Main.settings.swap_lean) multiplier = -multiplier;

                PlayerController.Instance.DisableArmPhysics();

                was_leaning = true;
                if (PlayerController.Instance.movementMaster != PlayerController.MovementMaster.Skater)
                {
                    PlayerController.Instance.SetSkaterToMaster();
                }

                PlayerController.Instance.skaterController.skaterTransform.RotateAround(head.position, PlayerController.Instance.skaterController.skaterTransform.forward, multiplier * (Main.settings.speed / 1.25f) * Time.fixedDeltaTime);
                PlayerController.Instance.boardController.SetBoardControllerUpVector(PlayerController.Instance.skaterController.skaterTransform.up);

                if (PlayerController.Instance.GetBoardBackwards()) multiplier *= -1;

                lean_velocity = Mathf.SmoothDamp(lean_velocity, Main.settings.speed, ref lean_velocity, .1f);
                PlayerController.Instance.boardController.gameObject.transform.Rotate(0, -multiplier * (lean_velocity * 5f) * Time.fixedDeltaTime, multiplier * (lean_velocity * 9f) * Time.fixedDeltaTime, Space.Self);
                PlayerController.Instance.boardController.gameObject.transform.Translate(multiplier * .005f, multiplier * .015f, .0f);
                PlayerController.Instance.boardController.boardRigidbody.AddRelativeForce(0, 0f, lean_velocity * Time.fixedDeltaTime, ForceMode.Impulse);
                PlayerController.Instance.skaterController.maxComboAccelLerp = Mathf.Infinity;

                /*PlayerController.Instance.boardController.UpdateReferenceBoardTargetRotation();*/
                //PlayerController.Instance.boardController.UpdateBoardPosition();
                //PlayerController.Instance.comController.UpdateCOM();
                PlayerController.Instance.LerpKneeIkWeight();
                //PlayerController.Instance.SnapRotation();
                //PlayerController.Instance.SetRotationTarget(true);
                PlayerController.Instance.boardController.currentRotationTarget = PlayerController.Instance.skaterController.skaterTransform.rotation;
                lean_reset = false;
                last_velocity_lean = PlayerController.Instance.boardController.boardRigidbody.velocity;
            }
            else
            {
                lean_velocity = 0;
                if (was_leaning)
                {
                    if (IsGrounded())
                    {
                        PlayerController.Instance.boardController.boardRigidbody.AddRelativeForce(0, 0, -Physics.gravity.y * Time.fixedDeltaTime, ForceMode.Impulse);
                        PlayerController.Instance.boardController.boardRigidbody.AddForce(0, Physics.gravity.y * Time.fixedDeltaTime, 0, ForceMode.Impulse);
                        PlayerController.Instance.skaterController.skaterRigidbody.AddRelativeTorque(0, (last_side == "right" ? -1 : 1) * Main.settings.wallride_downforce * Time.fixedDeltaTime * 10f, 0);
                        raycast();
                    }

                    checkAngles();
                }
            }
            /*if (Grinding())
            {
                PlayerController.Instance.boardController.gameObject.transform.Rotate(0, 0, -multiplier * Main.settings.grind_speed * Time.deltaTime, Space.Self);
                PlayerController.Instance.boardController.SetBoardControllerUpVector(PlayerController.Instance.skaterController.skaterTransform.up);
                PlayerController.Instance.skaterController.skaterTransform.Rotate(0, 0, -multiplier * Main.settings.grind_speed * Time.deltaTime, Space.Self);
                PlayerController.Instance.boardController.UpdateReferenceBoardTargetRotation();
                PlayerController.Instance.boardController.UpdateBoardPosition();
                //PlayerController.Instance.comController.UpdateCOM();
                PlayerController.Instance.boardController.currentRotationTarget = PlayerController.Instance.skaterController.skaterTransform.rotation;
                Traverse.Create(PlayerController.Instance.boardController).Field("_playerGrindRotation").SetValue(PlayerController.Instance.skaterController.skaterTransform.rotation);
                Traverse.Create(PlayerController.Instance.boardController).Field("_boardGrindRotation").SetValue(PlayerController.Instance.boardController.gameObject.transform.rotation);
            }*/
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
            Muscle left_hand = PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6];
            Muscle right_hand = PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9];

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
                            Vector3 rot = PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[4].transform.rotation.eulerAngles;
                            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[4].transform.rotation = Quaternion.Euler(-Main.settings.filmer_arm_angle, rot.y, rot.z);
                            left_hand.transform.LookAt(player);
                            left_hand.transform.Rotate(0, -90, 90);
                        }
                        if (Main.settings.follow_mode_right)
                        {
                            Vector3 rot = PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[7].transform.rotation.eulerAngles;
                            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[7].transform.rotation = Quaternion.Euler(Main.settings.filmer_arm_angle, rot.y, rot.z);
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
            if (PlayerController.Instance.inputController.RightStick.rawInput.pos.y >= .15f || PlayerController.Instance.inputController.LeftStick.rawInput.pos.y >= .15f) multiplier = -1;
            if (PlayerController.Instance.GetBoardBackwards()) multiplier *= -1;
            PlayerController.Instance.boardController.firstVel = Main.settings.ManualFlipVerticality * multiplier;
        }

        public void GrindVerticalFlip()
        {
            int multiplier = 1;
            if (PlayerController.Instance.inputController.RightStick.rawInput.pos.y >= .15f || PlayerController.Instance.inputController.LeftStick.rawInput.pos.y >= .15f) multiplier = -1;
            if (PlayerController.Instance.GetBoardBackwards()) multiplier *= -1;
            PlayerController.Instance.boardController.firstVel = Main.settings.GrindFlipVerticality * multiplier;
        }

        public bool LeaningInputRight(float sensibility)
        {
            if (PlayerController.Instance.inputController.LeftStick.rawInput.pos.x >= 0.5f && PlayerController.Instance.inputController.RightStick.rawInput.pos.x >= 0.5f)
            {
                bool left_centered = PlayerController.Instance.inputController.LeftStick.rawInput.pos.y <= sensibility && PlayerController.Instance.inputController.LeftStick.rawInput.pos.y >= -sensibility;
                bool right_centered = PlayerController.Instance.inputController.RightStick.rawInput.pos.y <= sensibility && PlayerController.Instance.inputController.RightStick.rawInput.pos.y >= -sensibility;
                if (left_centered && right_centered) return true;
            }
            return false;
        }

        public bool LeaningInputLeft(float sensibility)
        {
            if (PlayerController.Instance.inputController.LeftStick.rawInput.pos.x <= -0.5f && PlayerController.Instance.inputController.RightStick.rawInput.pos.x <= -0.5f)
            {
                bool left_centered = PlayerController.Instance.inputController.LeftStick.rawInput.pos.y <= sensibility && PlayerController.Instance.inputController.LeftStick.rawInput.pos.y >= -sensibility;
                bool right_centered = PlayerController.Instance.inputController.RightStick.rawInput.pos.y <= sensibility && PlayerController.Instance.inputController.RightStick.rawInput.pos.y >= -sensibility;
                if (left_centered && right_centered) return true;
            }
            return false;
        }

        public void Wobble()
        {
            if (GameStateMachine.Instance.CurrentState.GetType() != typeof(PlayState)) return;
            Vector3 euler = PlayerController.Instance.boardController.boardRigidbody.transform.localRotation.eulerAngles;
            PlayerController.Instance.boardController.boardRigidbody.transform.localRotation = Quaternion.Euler(euler.x, euler.y, euler.z + WobbleValue());

            //PlayerController.Instance.comController.UpdateCOM();
        }

        public float WobbleValue(float multiplier = 1f)
        {
            float mag_multi = PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Pushing || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Powerslide ? .15f / 2 : .15f;
            float vel = PlayerController.Instance.boardController.boardRigidbody.velocity.magnitude - Main.settings.wobble_offset;
            vel = vel < 0 ? 0 : vel * multiplier;

            return Mathf.Sin(Time.fixedUnscaledTime * 30 + (vel * mag_multi) + ((float)rd.NextDouble() * 2)) * (mag_multi * vel);
        }

        public void SetBailedPuppet()
        {
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[3], ConfigurableJointMotion.Free);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[4], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[5], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[7], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[8], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[10], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[11], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[12], ConfigurableJointMotion.Locked);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[13], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[14], ConfigurableJointMotion.Limited);
            setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[15], ConfigurableJointMotion.Locked);

            for (int i = 0; i < PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles.Length; i++)
            {
                if (Main.settings.debug) Utils.Log(i + " " + PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[i].name + " " + PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[i].joint.xMotion);
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.pinWeight = 1;
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[i].props.mappingWeight = 1f;
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[i].rigidbody.solverIterations = 5;
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[i].rigidbody.mass = 5f;
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[i].colliders[0].material = PlayerController.Instance.boardBrakePhysicsMaterial;

                if (i >= 10)
                {
                    setMotionType(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[i], ConfigurableJointMotion.Limited);
                }
            }

            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[1].rigidbody.mass = 20f;
            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[3].rigidbody.mass = 5f;
            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[12].rigidbody.mass = 1f;
            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[15].rigidbody.mass = 1f;

            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].rigidbody.mass = 1f;
            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].rigidbody.mass = 1f;

            //PlayerController.Instance.respawn.behaviourPuppet.BoostImmunity(500f);
            PlayerController.Instance.respawn.behaviourPuppet.maxCollisions = 50;
            PlayerController.Instance.respawn.behaviourPuppet.maxRigidbodyVelocity = 100f;
            PlayerController.Instance.respawn.behaviourPuppet.canGetUp = true;
            PlayerController.Instance.respawn.behaviourPuppet.maxGetUpVelocity = 0f;
            PlayerController.Instance.respawn.behaviourPuppet.getUpDelay = 0;
            //PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.mode = PuppetMaster.Mode.Disabled;
            //PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.state = PuppetMaster.State.Alive;
            PlayerController.Instance.DisableBodyPhysics();
            PlayerController.Instance.DisableArmPhysics();

            PlayerController.Instance.ResetAllAnimations();

            bailed_puppet = true;
        }

        public void RestorePuppet()
        {
            for (int i = 0; i < PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles.Length; i++)
            {
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[i].rigidbody.solverIterations = 1;
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[i].rigidbody.mass = 1f;
            }

            bailed_puppet = false;
        }

        public void DisableCameraCollider(bool enabled)
        {
            if (!cinemachine_collider)
            {
                cinemachine_collider = PlayerController.Instance.cameraController.gameObject.GetComponentInChildren<Cinemachine.CinemachineCollider>();
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

                // Utils.Log(target == null ? "null" : target.name);

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
            Transform main = PlayerController.Instance.skaterController.transform.parent.transform.parent;
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

            LookForward();

            if (Main.settings.hippie && (InAir() || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Pop))
            {
                HandleHippieOllie();
            }

            if (MultiplayerManager.Instance.clientState != ClientState.Disconnected && Main.settings.reset_inactive)
            {
                Traverse.Create(MultiplayerManager.Instance).Field("inactiveTime").SetValue(0f);
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
                BodyRotation.RotateAll();
            }
            if (GameStateMachine.Instance.CurrentState.GetType() == typeof(ReplayState))
            {
                if (!replay_skater) replay_skater = getReplayEditor();
                DoScalingReplaystate();
            }

            if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup)
            {
                Traverse.Create(PlayerController.Instance.headIk).Field("currentDirTarget").SetValue(head.rotation);
                Traverse.Create(PlayerController.Instance.headIk).Field("currentTargetRot").SetValue(head.rotation);
            }

            COMControllerValues();

            if (Main.settings.custom_board_correction)
            {
                PlayerController.Instance.boardController.KRp = Main.settings.board_p;
                PlayerController.Instance.boardController.KRi = Main.settings.board_i;
                PlayerController.Instance.boardController.KRd = Main.settings.board_d;
            }
            else
            {
                PlayerController.Instance.boardController.KRp = 5000;
                PlayerController.Instance.boardController.KRi = 0;
                PlayerController.Instance.boardController.KRd = 1;
            }

            if (last_gsm_state != "GameManagement.PauseState" && GameStateMachine.Instance.CurrentState.GetType() == typeof(PauseState))
            {
                SaveManagerFocusPatch.HandleCustomMapChanges();
                // SaveManagerFocusPatch.HandleCustomGearChanges();
            }

            /*if (Main.settings.alternative_powerslide && PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Powerslide)
            {
                CustomPowerSlide();
            }*/

            UpdateLastState();
            LogState();
        }

        private void KickAdd()
        {
            float num = 5f;
            float num2 = Mathf.Clamp(Mathf.Abs(PlayerController.Instance.VelocityOnPop.magnitude) / num, -0.7f, 0.7f);
            float num3 = 1.1f;
            num3 *= 0.5f;
            float num4 = num3 - num3 * num2;
            PlayerController.Instance.DoKick(true, num4);
        }

        void COMControllerValues()
        {
            COMController cc = PlayerController.Instance.comController;
            cc.Kp = Main.settings.Kp;
            cc.Ki = Main.settings.Ki;
            cc.Kd = Main.settings.Kd;
            cc.KpImpact = Main.settings.KpImpact;
            cc.KdImpact = Main.settings.KdImpact;
            cc.KpSetup = Main.settings.KpSetup;
            cc.KdSetup = Main.settings.KdSetup;
            cc.KpGrind = Main.settings.KpGrind;
            cc.KdGrind = Main.settings.KdGrind;
            cc.comOffset.y = Main.settings.comOffset_y;
            cc.comHeightRiding = Main.settings.comHeightRiding;
            cc.maxLegForce = Main.settings.maxLegForce;
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

            PlayerController.Instance.skaterController.gameObject.transform.localScale = Main.settings.custom_scale;
            PlayerController.Instance.playerCollider.transform.localScale = Main.settings.custom_scale;
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
            if (PlayerController.Instance.currentStateEnum.ToString() == Main.settings.wave_on)
            {
                AnimatorStateInfo currentAnimatorStateInfo = PlayerController.Instance.animationController.skaterAnim.GetCurrentAnimatorStateInfo(1);
                if (!currentAnimatorStateInfo.IsName("Wink"))
                {
                    PlayerController.Instance.animationController.gestureController.PlayAnimation("Wink");
                }
            }
        }

        void CheckLetsgoAnim()
        {
            if (PlayerController.Instance.currentStateEnum.ToString() == Main.settings.celebrate_on && PlayerController.Instance.currentStateEnum != PlayerController.CurrentState.Bailed)
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

        private void HandleHippieOllie()
        {
            if (PlayerController.Instance.currentStateEnum != PlayerController.CurrentState.BeginPop && PlayerController.Instance.currentStateEnum != PlayerController.CurrentState.Pop && !PlayerController.Instance.inputController.player.GetButton("B")) return;
            if ((PlayerController.Instance.inputController.player.GetButton("Right Stick Button") && PlayerController.Instance.inputController.player.GetButton("Left Stick Button")) || PlayerController.Instance.inputController.player.GetButton("B"))
            {
                PlayerController.Instance.SetRightIKLerpTarget(1, 1);
                PlayerController.Instance.SetRightSteezeWeight(0f);
                PlayerController.Instance.SetMaxSteezeRight(0f);
                PlayerController.Instance.SetRightKneeIKTargetWeight(1);
                PlayerController.Instance.SetRightIKWeight(1);
                PlayerController.Instance.SetRightKneeBendWeight(1);
                PlayerController.Instance.SetRightKneeBendWeightManually(1);
                PlayerController.Instance.SetLeftIKLerpTarget(1, 1);
                PlayerController.Instance.SetLeftSteezeWeight(0f);
                PlayerController.Instance.SetMaxSteezeLeft(0f);
                PlayerController.Instance.SetLeftKneeIKTargetWeight(1);
                PlayerController.Instance.SetLeftIKWeight(1);
                PlayerController.Instance.SetLeftKneeBendWeight(1);
                PlayerController.Instance.SetLeftKneeBendWeightManually(1);
                PlayerController.Instance.animationController.skaterAnim.SetBool("Released", true);
                PlayerController.Instance.CrossFadeAnimation("Extend", Main.settings.HippieTime);
                PlayerController.Instance.boardController.boardRigidbody.AddForce(new Vector3(0, 1, 0) * Physics.gravity.y * (Main.settings.HippieForce / 4), ForceMode.Impulse);
            }
        }

        public void CameraFeet()
        {
            Transform left_pos = (Transform)Traverse.Create(PlayerController.Instance.ikController).Field("ikLeftFootPositionOffset").GetValue();
            left_pos.position = new Vector3(left_pos.position.x, PlayerController.Instance.boardController.boardRigidbody.position.y, left_pos.position.z);
            Transform right_pos = (Transform)Traverse.Create(PlayerController.Instance.ikController).Field("ikRightFootPositionOffset").GetValue();
            right_pos.position = new Vector3(right_pos.position.x, PlayerController.Instance.boardController.boardRigidbody.position.y, right_pos.position.z);
        }

        int skate_mask = 1 << LayerMask.NameToLayer("Skateboard");
        public void DynamicFeet()
        {
            int multi = 1;
            if (PlayerController.Instance.GetBoardBackwards()) multi = -1;

            if (was_leaning || GameStateMachine.Instance.CurrentState.GetType() != typeof(PlayState) || IsBumping) return;

            int count = 0;
            foreach (var state in Enum.GetValues(typeof(PlayerController.CurrentState)))
            {
                if (state.ToString() == PlayerController.Instance.currentStateEnum.ToString())
                {
                    if (Main.settings.dynamic_feet_states[count] == true)
                    {
                        LeftFootRaycast(multi);
                        RightFootRaycast(multi);
                        //PlayerController.Instance.ScalePlayerCollider();
                    }
                }
                count++;
            }
        }

        int grinding_count = 0, offset_frame = 0, offset_delay = 0;
        float left_foot_setup_rand = 0;
        GameObject temp_go_left, temp_go_right;
        float jiggle_vel_left = 0f;
        float jiggle_vel_right = 0f;

        float offset_setup_left = 0;
        float offset_setup_right = 0;

        void LeftFootRaycast(int multiplier)
        {
            if (!temp_go_left) temp_go_left = new GameObject("Left foot raycast dynamic feet");
            Transform left_pos = (Transform)Traverse.Create(PlayerController.Instance.ikController).Field("ikAnimLeftFootTarget").GetValue();
            if (left_pos == null) return;

            temp_go_left.transform.position = left_pos.transform.position;
            temp_go_left.transform.rotation = PlayerController.Instance.boardController.boardMesh.rotation;
            multiplier = SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular ? multiplier : -multiplier;

            temp_go_left.transform.Translate(new Vector3(0.085f * multiplier, 0f, 0.060f * multiplier), Space.Self);
            Vector3 target = temp_go_left.transform.position;

            if (Physics.Raycast(target, -temp_go_left.transform.up, out left_hit, 10f, skate_mask))
            {
                float offset = (Main.settings.left_foot_offset / 10);

                if (PlayerController.Instance.IsSwitch)
                {
                    if (PlayerController.Instance.animationController.skaterAnim.GetFloat("Nollie") == 0f)
                    {
                        if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup)
                        {
                            if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular) offset += 0;
                            if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Goofy) offset -= .008f;
                        }
                    }
                    else
                    {
                        if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup)
                        {
                            if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular) offset -= .008f;
                            if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Goofy) offset += .002f;
                        }
                    }
                }
                else
                {
                    if (PlayerController.Instance.animationController.skaterAnim.GetFloat("Nollie") == 0f)
                    {
                        if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup)
                        {
                            if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular) offset += .002f;
                            if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Goofy) offset -= .01f;
                        }
                    }
                    else
                    {
                        if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup)
                        {
                            if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular) offset -= .008f;
                            if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Goofy) offset += .004f;
                        }
                    }
                }

                temp_go_left.transform.position = left_hit.point;
                temp_go_left.transform.rotation = Quaternion.LookRotation(temp_go_left.transform.forward, left_hit.normal);

                if (Main.settings.feet_rotation)
                {
                    float old_y = left_pos.rotation.eulerAngles.y;
                    if (Main.settings.jiggle_on_setup)
                    {
                        if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup)
                        {
                            float limit = Main.settings.jiggle_limit;
                            if (offset_frame <= limit && offset_delay >= Main.settings.jiggle_delay)
                            {
                                if (left_foot_setup_rand >= .5f)
                                {
                                    offset_setup_left = Mathf.SmoothDamp(offset_setup_left, -Mathf.Sin((limit - offset_frame) / (12f + left_foot_setup_rand)) * (limit - offset_frame), ref jiggle_vel_left, .025f);
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
                    left_pos.transform.Rotate(new Vector3(0, 0, PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Riding ? 90f : 93f), Space.Self);
                    left_pos.rotation = Quaternion.Euler(left_pos.rotation.eulerAngles.x, old_y + offset_setup_left, left_pos.rotation.eulerAngles.z);
                }

                if (Main.settings.feet_offset && !Main.settings.camera_feet)
                {
                    left_pos.position = new Vector3(left_pos.position.x, left_pos.position.y + (offset - left_hit.distance), left_pos.position.z);
                }
            }
        }

        float right_foot_setup_rand = 0;
        void RightFootRaycast(int multiplier)
        {
            if (!temp_go_right) temp_go_right = new GameObject("Right foot raycast dynamic feet");
            Transform right_pos = (Transform)Traverse.Create(PlayerController.Instance.ikController).Field("ikAnimRightFootTarget").GetValue();
            if (right_pos == null) return;

            temp_go_right.transform.position = right_pos.transform.position;
            temp_go_right.transform.rotation = PlayerController.Instance.boardController.boardMesh.transform.rotation;
            multiplier = SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular ? multiplier : -multiplier;

            bool additional_offset = SettingsManager.Instance.stance == SkaterXL.Core.Stance.Goofy && (PlayerController.Instance.IsSwitch || PlayerController.Instance.animationController.skaterAnim.GetFloat("Nollie") > 0) && (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Manual);
            if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular) additional_offset = (!PlayerController.Instance.IsSwitch || PlayerController.Instance.animationController.skaterAnim.GetFloat("Nollie") == 0) && (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Manual);
            temp_go_right.transform.Translate(new Vector3(0.085f * multiplier, 0f, additional_offset ? -0.04f * multiplier : 0.01f * multiplier), Space.Self);
            Vector3 target = temp_go_right.transform.position;

            if (Physics.Raycast(target, -temp_go_right.transform.up, out right_hit, 10f, skate_mask))
            {
                float offset = (Main.settings.right_foot_offset / 10);

                if (PlayerController.Instance.IsSwitch)
                {
                    if (PlayerController.Instance.animationController.skaterAnim.GetFloat("Nollie") == 0f)
                    {
                        if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup)
                        {
                            if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular) offset -= .01f;
                            if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Goofy) offset += .006f;
                        }
                    }
                    else
                    {
                        if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup)
                        {
                            if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular) offset += .006f;
                            if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Goofy) offset -= .008f;
                        }
                    }
                }
                else
                {
                    if (PlayerController.Instance.animationController.skaterAnim.GetFloat("Nollie") == 0f)
                    {
                        if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup)
                        {
                            if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular) offset -= .006f;
                            if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Goofy) offset += .002f;
                        }
                    }
                    else
                    {
                        if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup)
                        {
                            if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular) offset += .003f;
                            if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Goofy) offset -= .003f;
                        }
                    }
                }

                temp_go_right.transform.position = right_hit.point;
                temp_go_right.transform.rotation = Quaternion.LookRotation(temp_go_right.transform.forward, right_hit.normal);

                if (Main.settings.feet_rotation)
                {
                    float old_y = right_pos.rotation.eulerAngles.y;

                    if (Main.settings.jiggle_on_setup)
                    {
                        if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup)
                        {
                            float limit = Main.settings.jiggle_limit;
                            if (offset_frame <= limit && offset_delay >= Main.settings.jiggle_delay)
                            {
                                if (right_foot_setup_rand >= .5f)
                                {
                                    offset_setup_right = Mathf.SmoothDamp(offset_setup_right, Mathf.Sin((limit - offset_frame) / (14f + right_foot_setup_rand)) * (limit - offset_frame), ref jiggle_vel_right, .025f);
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
                    right_pos.transform.Rotate(new Vector3(0, 0, PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Riding ? 90f : 92f), Space.Self);
                    right_pos.rotation = Quaternion.Euler(right_pos.rotation.eulerAngles.x, old_y + offset_setup_right, right_pos.rotation.eulerAngles.z);
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
                Utils.Log("error");
            }
        }

        bool InAir()
        {
            return PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Pop || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.InAir || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Release;
        }

        bool Grinding()
        {
            return PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Grinding || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.EnterCoping || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.ExitCoping;
        }

        bool lean_reset = false;
        void raycast()
        {
            if (Physics.Raycast(PlayerController.Instance.boardController.gameObject.transform.position, -PlayerController.Instance.boardController.gameObject.transform.up, out _hit, 2f, layerMask))
            {
                if (_hit.collider.gameObject.name != "Skater_foot_l" && _hit.collider.gameObject.name != "Skater_foot_r" && _hit.collider.gameObject.layer != LayerMask.NameToLayer("Skateboard") && _hit.collider.gameObject.layer != LayerMask.NameToLayer("Character"))
                {
                    if (!lean_reset)
                    {
                        lean_reset = true;
                        PlayerController.Instance.boardController.boardRigidbody.angularVelocity = Vector3.zero;
                        PlayerController.Instance.boardController.boardRigidbody.velocity = last_velocity_lean;
                        EventManager.Instance.OnCatched(true, true);
                    }

                    PlayerController.Instance.currentStateEnum = PlayerController.CurrentState.Pushing;
                    EventManager.Instance.EndTrickCombo(false, false);
                    PlayerController.Instance.skaterController.InitializeSkateRotation();
                    PlayerController.Instance.boardController.DoBoardLean();
                    PlayerController.Instance.movementMaster = PlayerController.MovementMaster.Board;
                    PlayerController.Instance.SetBoardToMaster();
                    PlayerController.Instance.cameraController.NeedToSlowLerpCamera = true;
                    PlayerController.Instance.ScalePlayerCollider();
                    PlayerController.Instance.ToggleFlipColliders(false);
                    PlayerController.Instance.boardController.SetBoardControllerUpVector(_hit.normal);

                    PlayerController.Instance.boardController.SnapRotation();
                    /*ApplyWeightOnBoard();*/
                    //PlayerController.Instance.comController.UpdateCOM();
                    PlayerController.Instance.skaterController.AddCollisionOffset();
                    if (!IsGrounded())
                    {
                        Vector3 force = PlayerController.Instance.skaterController.PredictLanding(PlayerController.Instance.skaterController.skaterRigidbody.velocity);
                        PlayerController.Instance.skaterController.skaterRigidbody.AddForce(force, ForceMode.Impulse);
                    }
                }
            }
            else
            {
                was_leaning = false;
                lean_reset = false;
            }
        }

        void checkAngles()
        {
            Vector3 rot = PlayerController.Instance.boardController.gameObject.transform.rotation.eulerAngles;
            if (rot.z >= 350 || rot.z <= 10)
            {
                was_leaning = false;
                PlayerController.Instance.EnableArmPhysics();
            }
        }

        public void ApplyWeightOnBoard()
        {
            Vector3 velocity = PlayerController.Instance.boardController.boardRigidbody.velocity;
            PlayerController.Instance.boardController.boardRigidbody.AddForce(-PlayerController.Instance.skaterController.skaterTransform.up * Main.settings.wallride_downforce * PlayerController.Instance.impactBoardDownForce, ForceMode.Force);
        }

        public bool IsGroundedNoGrind()
        {
            return PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Impact || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Riding || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Manual || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Powerslide || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Pushing || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Braking;
        }

        public bool IsGrounded()
        {
            return PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Impact || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Riding || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Manual || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Powerslide || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Pushing || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Braking || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Grinding || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.EnterCoping || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.ExitCoping;
        }

        public bool IsGrinding()
        {
            return PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Grinding || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.EnterCoping || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.ExitCoping;
        }

        public bool IsPopping()
        {
            return PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Pop && PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.BeginPop;
        }

        public bool IsGroundedForFeet()
        {
            return PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Impact || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Setup || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Manual || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Grinding || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.EnterCoping || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.ExitCoping;
        }

        Transform replay;
        Transform left_foot_replay, right_foot_replay;
        Transform neck_replay, pelvis_replay, spine1_replay, spine2_replay, left_arm_replay, left_forearm_replay, right_arm_replay, right_forearm_replay, left_upleg_replay, left_leg_replay, right_upleg_replay, right_leg_replay;
        Transform getReplayEditor()
        {
            Transform main = PlayerController.Instance.skaterController.transform.parent.transform.parent;
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
            Transform parent = PlayerController.Instance.boardController.gameObject.transform;
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

            //Utils.Log("Deck initialized, " + center_collider.name + " " + tail_collider.name + " " + nose_collider.name);
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

        public Transform[] skater_parts = new Transform[18];
        public Transform left_hand, right_hand, pelvis, spine1, spine2, left_arm, left_forearm, right_arm, right_forearm, left_upleg, left_leg, right_upleg, right_leg;
        Transform left_toe_1, left_toe_2, right_toe_1, right_toe_2;
        void getFeet()
        {
            Transform parent = PlayerController.Instance.skaterController.gameObject.transform;
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

            //Utils.Log("Body initialized, " + right_foot.name + " " + left_foot.name);

            skater_parts = new Transform[] { pelvis, spine, spine1, spine2, head, neck, left_arm, left_forearm, left_hand, right_arm, right_forearm, right_hand, left_upleg, left_leg, left_foot, right_upleg, right_leg, right_foot };
        }

        bool IsPumping()
        {
            if (SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular)
            {
                if (PlayerController.Instance.inputController.RightStick.rawInput.pos.y >= 0.5f || PlayerController.Instance.inputController.LeftStick.rawInput.pos.y <= -0.5f) return true;
            }
            else
            {
                if (PlayerController.Instance.inputController.RightStick.rawInput.pos.y <= -0.5f || PlayerController.Instance.inputController.LeftStick.rawInput.pos.y >= 0.5f) return true;
            }

            return false;
        }

        public string getStance()
        {
            string stance = "";
            if (PlayerController.Instance.IsSwitch)
            {
                stance = "Switch";
                if (PlayerController.Instance.animationController.skaterAnim.GetFloat("Nollie") == 0f) stance = "Fakie";
            }
            return stance;
        }

        public GameObject object_found = null;
        public Vector3 copingTargetVelocity = Vector3.zero;

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

        public void ScaleMap()
        {
            GameObject rootCollection = GameObject.Find("rootCollection");
            if (rootCollection == null)
            {
                rootCollection = new GameObject("rootCollection");

                // Find all root GameObjects in the scene
                GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

                // Iterate through the root objects and add them to the collectionObject, along with their children
                foreach (GameObject rootObject in rootObjects)
                {
                    rootObject.transform.parent = rootCollection.transform;
                }
            }

            rootCollection.transform.localScale = new Vector3(Main.settings.map_scale.x, Main.settings.map_scale.y, Main.settings.map_scale.z);
        }
    }
}
