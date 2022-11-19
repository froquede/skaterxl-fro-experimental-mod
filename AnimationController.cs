using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace fro_mod
{
    public class AnimationController : MonoBehaviour
    {
        int frame = 0;
        int clap_length = 24;
        void FixedUpdate()
        {
            /*ResetArms();
            ClapAnimation();*/
        }

        void ResetArms()
        {
            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].rigidbody.mass = 1f;
            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].rigidbody.mass = 1f;
        }

        void ClapAnimation()
        {
            float sin = (float)Math.Sin(Time.unscaledTime * 12f) / 7f;
            float multiplier = 1;
            multiplier = SettingsManager.Instance.stance == SkaterXL.Core.Stance.Regular ? multiplier : -multiplier;
            float sw_multiplier = PlayerController.Instance.IsSwitch ? -1 : 1;

            GameObject center_left = new GameObject();
            center_left.transform.position = PlayerController.Instance.skaterController.skaterTransform.position;
            center_left.transform.rotation = PlayerController.Instance.skaterController.skaterTransform.rotation;
            center_left.transform.Translate(.25f, .25f, .25f - sin, Space.Self);
            center_left.transform.Rotate(0, 0, -180, Space.Self);

            GameObject center_right = new GameObject();
            center_right.transform.position = PlayerController.Instance.skaterController.skaterTransform.position;
            center_right.transform.rotation = PlayerController.Instance.skaterController.skaterTransform.rotation;
            center_right.transform.Translate(.20f, .25f, 0 + sin, Space.Self);
            center_right.transform.Rotate(0, 0, 180, Space.Self);

            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].transform.rotation = Quaternion.Lerp(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].transform.rotation, center_left.transform.rotation, Controller.map01(frame, 0, clap_length));
            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].transform.position = Vector3.Lerp(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].transform.position, center_left.transform.position, Controller.map01(frame, 0, clap_length));
            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].transform.rotation = Quaternion.Lerp(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].transform.rotation, center_right.transform.rotation, Controller.map01(frame, 0, clap_length));
            PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].transform.position = Vector3.Lerp(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].transform.position, center_right.transform.position, Controller.map01(frame, 0, clap_length));
            frame++;

            Destroy(center_left);
            Destroy(center_right);
        }
    }
}
