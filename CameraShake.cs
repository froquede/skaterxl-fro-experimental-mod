using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;

namespace fro_mod
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
        float last_fov = 0;
        int count = 0;

        void LateUpdate()
        {
            if (!Main.settings.camera_shake || PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Bailed) return;

            float velocity = PlayerController.Instance.skaterController.skaterRigidbody.velocity.magnitude - Main.settings.camera_shake_offset;
            if (velocity < 0) velocity = 0;

            if (velocity == 0)
            {
                last_fov = camera.m_Lens.FieldOfView;
                last_x = 0;
                last_y = 0;
                initial_point_x = 0;
                initial_point_y = 0;
            }
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

                camera.m_Lens.FieldOfView = last_fov + ((velocity / 2) * Main.settings.camera_shake_fov_multiplier);
            }
        }

        public float WobbleValue(float vel)
        {
            return vel * (float)rd.NextDouble() / 1200f;
        }

        public void GetCamera()
        {
            camera = MonoBehaviourSingleton<PlayerController>.Instance.cameraController._actualCam.GetComponent<CinemachineVirtualCamera>();
        }
    }
}
