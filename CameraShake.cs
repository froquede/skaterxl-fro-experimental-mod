using Cinemachine;
using GameManagement;
using SkaterXL.Core;
using SkaterXL.Data;
using SkaterXL.Gameplay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;

namespace xlperimental_mod
{
    public class CameraShake : MonoBehaviour
    {
        System.Random rd = new System.Random();
        CinemachineVirtualCamera camera;

        void Start()
        {
            GetCamera();
        }

        float last_x = 0, last_y = 0;
        float initial_point_x = 0, initial_point_y = 0;
        float last_fov = -1;
        int count = 0;
        int zero_count = 0;

        void FixedUpdate()
        {            
            if (GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRigidbody.velocity.magnitude <= 0.15f && !GameStateMachine.Instance.MainPlayer.gameplay.playerData.IsRespawning)
            {
                if (zero_count >= 24f)
                {
                    last_fov = camera.m_Lens.FieldOfView;
                    last_x = 0;
                    last_y = 0;
                    initial_point_x = 0;
                    initial_point_y = 0;
                    zero_count = 0;
                }
                zero_count++;
            }
        }

        void LateUpdate()
        {
            
            if (!Main.settings.camera_shake || GameStateMachine.Instance.MainPlayer.gameplay.playerData.currentState == PlayerStateEnum.Bailed) return;

            float velocity = GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRigidbody.velocity.magnitude - Main.settings.camera_shake_offset;
            if (velocity < 0) velocity = 0;
            if (velocity > 0)
            {
                camera.transform.Translate(Mathf.SmoothStep(last_x, initial_point_x, Controller.map01(count, 0, Main.settings.camera_shake_length)), Mathf.SmoothStep(last_y, initial_point_y, Controller.map01(count, 0, Main.settings.camera_shake_length)), 0, Space.Self);

                if (count >= Main.settings.camera_shake_length)
                {
                    last_x = initial_point_x;
                    last_y = initial_point_y;
                    count = 0;
                }
                if (count == 0)
                {
                    initial_point_x = WobbleValue(velocity) * Main.settings.camera_shake_multiplier;
                    initial_point_y = WobbleValue(velocity) * Main.settings.camera_shake_multiplier;
                }

                count++;
            }

            float velocity_fov = (GameStateMachine.Instance.MainPlayer.gameplay.transformReference.skaterRigidbody.velocity.magnitude / 2) - Main.settings.camera_fov_offset;
            if (velocity_fov < 0) velocity_fov = 0;

            if (last_fov >= 0 && velocity_fov > 0)
            {
                float target_fov = last_fov + (velocity_fov * Main.settings.camera_shake_fov_multiplier);
                camera.m_Lens.FieldOfView = target_fov;
            }
        }

        public float WobbleValue(float vel)
        {
            return vel * (float)rd.NextDouble() / 1200f;
        }

        public void GetCamera()
        {
            camera = GameStateMachine.Instance.MainPlayer.gameplay.cameraController.actualCamera.GetComponent<CinemachineVirtualCamera>();
        }
    }
}
