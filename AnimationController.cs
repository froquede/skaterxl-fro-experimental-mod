using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace fro_mod
{
    class AnimationController : MonoBehaviour
    {
        public LetsGo LetsGoAnim = new LetsGo();
    }

    class LetsGo
    {
        System.Random rd = new System.Random();
        float left_rand = 1, right_rand = 1;
        float letsgo_anim_time = 1;

        public void Play()
        {
            letsgo_anim_time = 0;
            left_rand = 0;
            right_rand = 0;
        }

        void Step()
        {
            if (letsgo_anim_time == 0) letsgo_anim_time = Time.unscaledTime;

            if (Time.unscaledTime - letsgo_anim_time <= 3f)
            {
                if (left_rand == 0) left_rand = ((float)rd.NextDouble() / 8f);
                if (right_rand == 0) right_rand = ((float)rd.NextDouble() / 8f);

                float sin = (float)Math.Sin(Time.unscaledTime * 12f) / 7f;
                Vector3 left = PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].transform.position;
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[6].transform.position = new Vector3(left.x, PlayerController.Instance.skaterController.skaterTransform.position.y + .7f + sin + left_rand, left.z);
                Vector3 right = PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].transform.position;
                PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[9].transform.position = new Vector3(right.x, PlayerController.Instance.skaterController.skaterTransform.position.y + .7f + sin + right_rand, right.z);
            }
        }
    }
}
