using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace fro_mod
{
    public class Multiplayer : MonoBehaviour
    {
        void Start()
        {
        }

        void FixedUpdate()
        {
                       
        }

        public void CreateRoom()
        {
            MultiplayerManager.Instance.CreateRoom(true, "ANYSIZE?");
        }
    }
}
