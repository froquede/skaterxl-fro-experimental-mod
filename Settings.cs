using System;
using System.Collections.Generic;
using UnityEngine;
using UnityModManagerNet;

namespace fro_mod
{
    [Serializable]
    public class Settings : UnityModManager.ModSettings
    {
        [Draw(DrawType.KeyBinding)] public KeyBinding Hotkey = new KeyBinding { keyCode = KeyCode.F };
        public float speed = 300f;
        public float grind_speed = 40f;
        public float wallride_downforce = 80f;
        public int wait_threshold = 10;
        public bool enabled = true;
        public bool feet_rotation = false;
        public bool lean = false;
        public bool hippie = false;
        public float HippieForce = 1f;
        public float HippieTime = 0.3f;

        public float left_foot_offset = 1f;
        public float right_foot_offset = 1f;

        public bool swap_lean = false;

        public string selected_player = "";

        public bool follow_mode_left = false;
        public bool follow_mode_right = false;
        public bool follow_mode_head = false;
        public bool push_by_velocity = true;

        public float follow_target_offset = -.3f;
        public bool camera_feet = false;

        public bool reset_inactive = true;
        public bool disable_popup = false;
        public int multiplayer_lobby_size = 20;

        public bool chat_messages = false;
        public int left_page = 0;
        public int right_page = 1;
        public int up_page = 2;
        public int down_page = 3;

        public bool sonic_mode = false;
        public bool displacement_curve = true;
        public bool feet_offset = false;

        public string wave_on = "Disabled";
        public string celebrate_on = "Disabled";

        public bool camera_avoidance = true;

        public bool wobble = true;
        public float wobble_offset = 4;

        public bool bails = true;

        public float GrindFlipVerticality = 0f;
        public float ManualFlipVerticality = 0f;

        public Vector3 custom_scale = new Vector3(1f, 1f, 1f);

        public float left_hand_weight = 1f;
        public float right_hand_weight = 1f;
        public float arms_weight = 1f;
        public float arms_dampening = 1f;
        public float filmer_arm_angle = 28f;
        public float filmer_hand_height = 0f;

        public bool filmer_light = false;
        public float filmer_light_intensity = 6000f;
        public float filmer_light_spotangle = 120f;
        public float filmer_light_range = 5f;
        public float body_height = 0f;

        public float nose_tail_collider = 1f;

        public float input_threshold = 20f;

        public bool BetterDecay = true;
        public float decay = 1.5f;
        public List<bool> dynamic_feet_states = new List<Boolean>();

        public List<bool> look_forward_states = new List<Boolean>();

        public float custom_scale_head = 1f;
        public float custom_scale_hand_l = 1f;
        public float custom_scale_hand_r = 1f;
        public float custom_scale_foot_l = 1f;
        public float custom_scale_foot_r = 1f;
        public float custom_scale_pelvis = 1f;
        public float custom_scale_spine = 1f;
        public float custom_scale_spine2 = 1f;
        public float custom_scale_arm_l = 1f;
        public float custom_scale_forearm_l = 1f;
        public float custom_scale_arm_r = 1f;
        public float custom_scale_forearm_r = 1f;
        public float custom_scale_upleg_l = 1f;
        public float custom_scale_leg_l = 1f;
        public float custom_scale_upleg_r = 1f;
        public float custom_scale_leg_r = 1f;
        public float custom_scale_neck = 1f;

        public int keyframe_sample = 50;

        public int keyframe_fov = 120;
        public float lookat_speed = 1;
        public float time_offset = 0f;
        public bool keyframe_start_of_clip = false;
        public bool look_forward = false;
        public int look_forward_delay = 0;
        public int look_forward_length = 18;

        public int RoomIDlength = 5;

        public bool powerslide_force = true;
        public bool powerslide_velocitybased = true;
        public bool alternative_powerslide = false;
        public float powerslide_animation_length = 24f;
        public float powerslide_maxangle = 20f;
        public float powerslide_minimum_velocity = 0f;
        public float powerslide_max_velocity = 15f;

        public string keyframe_target = "Head";

        public bool alternative_arms = false;
        public bool alternative_arms_damping = false;

        public bool multiplayer_collision = false;

        public List<Vector3> head_rotation_fakie = new List<Vector3>();
        public List<Vector3> head_rotation_switch = new List<Vector3>();

        public List<Vector3> head_rotation_grinds_fakie = new List<Vector3>();
        public List<Vector3> head_rotation_grinds_switch = new List<Vector3>();

        public bool show_colliders = false;

        // UnityModManager doesnt seems to like a list of list for saving the settings so unfortunately this is going to be declared manually
        public List<Vector3> ollie_customization_rotation = new List<Vector3>();
        public List<float> ollie_customization_length = new List<float>();
        public List<Vector3> ollie_customization_rotation_backwards = new List<Vector3>();
        public List<float> ollie_customization_length_backwards = new List<float>();
        public List<Vector3> ollie_customization_rotation_left_stick = new List<Vector3>();
        public List<float> ollie_customization_length_left_stick = new List<float>();
        public List<Vector3> ollie_customization_rotation_right_stick = new List<Vector3>();
        public List<float> ollie_customization_length_right_stick = new List<float>();
        public List<Vector3> ollie_customization_rotation_left_stick_backwards = new List<Vector3>();
        public List<float> ollie_customization_length_left_stick_backwards = new List<float>();
        public List<Vector3> ollie_customization_rotation_right_stick_backwards = new List<Vector3>();
        public List<float> ollie_customization_length_right_stick_backwards = new List<float>();

        public List<Vector3> ollie_customization_rotation_both_outside = new List<Vector3>();
        public List<float> ollie_customization_length_both_outside = new List<float>();
        public List<Vector3> ollie_customization_rotation_both_inside = new List<Vector3>();
        public List<float> ollie_customization_length_both_inside = new List<float>();

        public List<Vector3> ollie_customization_rotation_left2left = new List<Vector3>();
        public List<float> ollie_customization_length_left2left = new List<float>();
        public List<Vector3> ollie_customization_rotation_left2right = new List<Vector3>();
        public List<float> ollie_customization_length_left2right = new List<float>();

        public List<Vector3> ollie_customization_rotation_right2left = new List<Vector3>();
        public List<float> ollie_customization_length_right2left = new List<float>();
        public List<Vector3> ollie_customization_rotation_right2right = new List<Vector3>();
        public List<float> ollie_customization_length_right2right = new List<float>();

        public List<Vector3> ollie_customization_rotation_both2left = new List<Vector3>();
        public List<float> ollie_customization_length_both2left = new List<float>();
        public List<Vector3> ollie_customization_rotation_both2right = new List<Vector3>();
        public List<float> ollie_customization_length_both2right = new List<float>();
        // Ollie customization end

        public bool force_stick_backwards = false;
        public float force_stick_backwards_multiplier = .125f;

        public bool filmer_object = false;
        public string filmer_object_target = "Target";

        public bool camera_shake = true;
        public float camera_shake_offset = 7;
        public float camera_shake_multiplier = 3f;
        public float camera_shake_fov_multiplier = 1.5f;
        public float camera_fov_offset = 4;

        public bool trick_customization = true;

        public float Kp = 5000f;
        public float Ki = 0f;
        public float Kd = 900f;
        public float KpImpact = 5000f;
        public float KdImpact = 1000f;
        public float KpSetup = 20000f;
        public float KdSetup = 1500f;
        public float KpGrind = 2000f;
        public float KdGrind = 900f;
        public float comOffset_y = 0.07f;
        public float comHeightRiding = 1.06f;
        public float maxLegForce = 5000f;

        public bool forward_force_onpop = false;
        public float forward_force = .35f;

        public string multi_filmer_activation = "On pump input";

        public float camera_shake_range = .2f;
        public int camera_shake_length = 4;

        public bool walk_after_bail = false;
        public bool haunting_arms = false;
        public bool bump_anim = false;
        public bool bump_anim_pop = true;

        public float bump_pop_delay = .15f;

        public float catch_acc = 10f;
        public bool catch_acc_enabled = false;
        public bool catch_acc_onflick = false;
        public float catch_lerp_speed = 30f;
        public int bounce_delay = 1;
        public bool snappy_catch = true;
        public float FlickThreshold = 0.6f;

        public bool partial_gear = false;

        public bool custom_board_correction = false;
        public float board_p = 5000f;
        public float board_i = 0f;
        public float board_d = 1f;

        public bool jiggle_on_setup = false;
        public float jiggle_limit = 40f;
        public float jiggle_delay = 24f;
        public float jiggle_randommax = 10f;

        public Vector3 map_scale = new Vector3(1f, 1f, 1f);

        public bool experimental_dynamic_catch = false;

        public bool body_rotation = false;
        public List<List<Vector3>> body_rotations = new List<List<Vector3>>();

        public float catch_left_time = 0.04f;
        public float catch_right_time = 0.04f;

        public Vector3 reset_head = new Vector3(-19f, 0f, -15.5f);

        public bool trick_customizer_grinds = false;

        public bool alternative_coping = false;
        public float coping_detection_distance = .5f;
        public float coping_part_speed = .1f;
        public float coping_part_distance = 5f;
        public float coping_max_velocity = 5f;

        public bool shuv_fix = false;

        public bool nudge_manual = false;

#if DEBUG
        public bool debug = true;
#else
        public bool debug = false;
#endif


        public void OnChange()
        {
            throw new NotImplementedException();
        }

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save<Settings>(this, modEntry);
        }
    }
}
