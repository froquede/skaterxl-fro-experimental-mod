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
        float last_fov = 0;
        void LateUpdate()
        {
            if (!Main.settings.camera_shake) return;

            float velocity = PlayerController.Instance.boardController.boardRigidbody.velocity.magnitude - Main.settings.camera_shake_offset;
            if (velocity < 0) velocity = 0;

            if (velocity == 0) last_fov = camera.m_Lens.FieldOfView;
            else
            {
                camera.m_Lens.FieldOfView = last_fov + ((velocity / 2) * Main.settings.camera_shake_fov_multiplier);
            }

            float wobble_x = WobbleValueX(velocity) * Main.settings.camera_shake_multiplier;
            float wobble_y = WobbleValueY(velocity) * Main.settings.camera_shake_multiplier;

            camera.transform.Translate(Mathf.Lerp(last_x, wobble_x, Time.deltaTime), Mathf.Lerp(last_y, wobble_y, Time.deltaTime), 0, Space.Self);

            last_x = wobble_x;
            last_y = wobble_y;
        }

        public float WobbleValueX(float vel)
        {
            return vel * (float)rd.NextDouble() / 1200f;
        }

        public float WobbleValueY(float vel)
        {
            return vel * (float)rd.NextDouble() / 1200f;
        }

        public void GetCamera()
        {
            camera = MonoBehaviourSingleton<PlayerController>.Instance.cameraController._actualCam.GetComponent<CinemachineVirtualCamera>();
        }
    }
}
