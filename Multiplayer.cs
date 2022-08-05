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
        public Canvas canvas;
        void Start()
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.enabled = true;
            // ShowClothNames();
        }

        void FixedUpdate()
        {
                       
        }

        void OnGUI()
        {
            /*Transform parent = PlayerController.Instance.skaterController.gameObject.transform;
            Transform skater = parent.Find("Skater");
            ShowName(skater);*/
            //ShowClothNames();
        }
        void ShowClothNames()
        {
            foreach (KeyValuePair<int, NetworkPlayerController> entry in MultiplayerManager.Instance.networkPlayers)
            {
                if (entry.Value)
                {
                    Transform player = entry.Value.gameObject.transform;
                    Transform LVP = player.Find("Live Playback Controller");
                    Transform newskater = LVP.Find("NewSkater");
                    Transform Skater = newskater.Find("Skater");

                    Dictionary<int, GearObject> cache = (Dictionary<int, GearObject>)Traverse.Create(entry.Value.customizationSyncer.customizer).Field("gearCache").GetValue();

                    foreach (GearObject gearObject in cache.Values)
                    {
                        //UnityModManager.Logger.Log(gearObject.gearInfo.name);
                    }

                    //ShowName(Skater);
                }
            }
        }

        void ShowName(Transform Skater)
        {
            for (int i = 0; i < Skater.childCount; i++)
            {
                Transform cloth = Skater.GetChild(i);
                 
                Vector3 screenpos = Camera.main.WorldToScreenPoint(cloth.position);
                var textSize = GUI.skin.label.CalcSize(new GUIContent(cloth.gameObject.name));
                GUI.Label(new Rect(screenpos.x, Screen.height - screenpos.y + (i * 12), textSize.x, textSize.y), cloth.gameObject.name);
            }
        }
    }
}
