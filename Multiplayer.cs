using SkaterXL.Multiplayer;
using SkaterXL.Core;
using UnityEngine;
using HarmonyLib;
using UnityModManagerNet;
using System.Collections.Generic;

namespace fro_mod
{
    public class Multiplayer : MonoBehaviour
    {
        void Start()
        {
            last_visibility = Main.settings.show_colliders;
        }

        List<GameObject> players = new List<GameObject>();
        string last_scene;
        bool last_visibility = false;
        bool destroyed = false;
        void FixedUpdate()
        {
            if (MultiplayerManager.Instance.InRoom && Main.settings.multiplayer_collision)
            {
                destroyed = false;

                if(MultiplayerManager.Instance.GetMapOfCurrentRoom().name != last_scene)
                {
                    last_scene = MultiplayerManager.Instance.GetMapOfCurrentRoom().name;
                    players = new List<GameObject>();
                }

                foreach (KeyValuePair<int, NetworkPlayerController> entry in MultiplayerManager.Instance.networkPlayers)
                {
                    if (entry.Value)
                    {
                        string id = entry.Value.UserId;
                        if (MultiplayerManager.Instance.localPlayer.UserId != id)
                        {
                            GameObject player = FindPlayer("collider_" + id);
                            if (player == null)
                            {
                                GameObject collider = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                                collider.GetComponent<MeshRenderer>().material.shader = Shader.Find("HDRP/Lit");
                                collider.GetComponent<MeshRenderer>().enabled = Main.settings.show_colliders;
                                collider.transform.localScale = new Vector3(.5f, .5f, .5f);
                                collider.name = "collider_" + id;
                                players.Add(collider);
                            }
                            else
                            {
                                Transform body = entry.Value.GetBody();
                                player.transform.position = body.transform.position;
                                player.transform.rotation = body.transform.rotation;

                                if(last_visibility != Main.settings.show_colliders)
                                {
                                    player.GetComponent<MeshRenderer>().material.shader = Shader.Find("HDRP/Lit");
                                    player.GetComponent<MeshRenderer>().enabled = Main.settings.show_colliders;
                                } 
                            }
                        }
                    }
                }

                last_visibility = Main.settings.show_colliders;
            }
            else
            {
                if (!Main.settings.multiplayer_collision && !destroyed)
                {
                    foreach (GameObject player in players)
                    {
                        Destroy(player);
                    }

                    players = new List<GameObject>();
                    destroyed = true;
                }
            }
        }

        GameObject FindPlayer(string name)
        {
            foreach (GameObject player in players)
            {
                if (player.name == name) return player;
            }
            return null;
        }

        public void CreateRoom()
        {
            MultiplayerManager.Instance.CreateRoom(false, "YKTFV");
        }
    }
}
