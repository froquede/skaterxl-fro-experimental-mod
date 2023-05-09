using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace fro_mod
{
    public class BodyRotation
    {
        public static void RotateAll()
        {
            if (!Main.settings.body_rotation) return;

            for(int i = 1; i < Main.controller.skater_parts.Length - 12; i++)
            {
                Quaternion target = Quaternion.Euler(GetStateRotation(PlayerController.Instance.currentStateEnum.ToString(), Enums.BodyParts[i]));
                Main.controller.skater_parts[i].localRotation = target;
            }
        }

        static Vector3 GetStateRotation(string state, string body_part)
        {
            int state_index = GetStateIndex(state);
            int part_index = GetBodyPartIndex(body_part);

            return Main.settings.body_rotations[state_index][part_index];
        }

        static int GetStateIndex(string state)
        {
            for (int i = 0; i < Enums.StatesReal.Length; i++)
            {
                if (Enums.StatesReal[i] == state) return i;
            }

            return 0;
        }

        static int GetBodyPartIndex(string bp) {
            for (int i = 0; i < Enums.BodyParts.Length; i++)
            {
                if (Enums.BodyParts[i] == bp) return i;
            }

            return 0;
        }
    }
}
