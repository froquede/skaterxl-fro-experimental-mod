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

        public float left_foot_offset = .95f;
        public float right_foot_offset = .95f;

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

        public Vector3 custom_scale = new Vector3(1f, 1f, 1f);

        public float left_hand_weight = 1f;
        public float right_hand_weight = 1f;
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
        public float decay = 3.25f;
        public List<bool> dynamic_feet_states = new List<Boolean>();

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
        public bool look_forward = true;
        public int look_forward_delay = 16;

        public bool powerslide_force = true;

        public string keyframe_target = "Head";

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
