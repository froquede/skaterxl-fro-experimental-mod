using SkaterXL.Multiplayer;
using SkaterXL.Core;
using UnityEngine;
using HarmonyLib;
using UnityModManagerNet;
using System.Collections.Generic;
using Photon.Realtime;

namespace fro_mod
{
    public class Multiplayer : MonoBehaviour
    {
        void Start()
        {
            last_visibility = Main.settings.show_colliders;
        }

        List<GameObject> players = new List<GameObject>();
        List<GameObject> skates = new List<GameObject>();
        string last_scene;
        bool last_visibility = false;
        bool destroyed = false;
        Vector3 last_respawn_point = Vector3.zero;

        void FixedUpdate()
        {
            if (MultiplayerManager.Instance.InRoom && Main.settings.multiplayer_collision)
            {
                destroyed = false;
                if(PlayerController.Instance.IsRespawning)
                {
                    last_respawn_point = PlayerController.Instance.boardController.boardMesh.position;
                }

                Vector3 board = PlayerController.Instance.boardController.boardMesh.position;
                bool isRespawning = FastApproximately(last_respawn_point.x, board.x, 1.05f) && FastApproximately(last_respawn_point.y, board.y, 1.05f) && FastApproximately(last_respawn_point.z, board.z, 1.05f);

                if (MultiplayerManager.Instance.GetMapOfCurrentRoom().name != last_scene)
                {
                    last_scene = MultiplayerManager.Instance.GetMapOfCurrentRoom().name;
                    players = new List<GameObject>();
                    skates = new List<GameObject>();
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

                                GameObject collider_skate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                collider_skate.GetComponent<MeshRenderer>().material.shader = Shader.Find("HDRP/Lit");
                                collider_skate.GetComponent<MeshRenderer>().enabled = Main.settings.show_colliders;
                                collider_skate.transform.localScale = new Vector3(.2f, .1f, .65f);
                                collider_skate.name = "collider_skate_" + id;
                                collider_skate.isStatic = false;
                                skates.Add(collider_skate);
                            }
                            else
                            {

                                GameObject skate = FindSkate("collider_skate_" + id);
                                if (skate != null)
                                {
                                    Transform target_skate = entry.Value.GetSkateboard();
                                    skate.transform.position = target_skate.transform.position;
                                    skate.transform.rotation = target_skate.transform.rotation;
                                }

                                Transform body = entry.Value.GetBody();
                                player.transform.rotation = body.transform.rotation;
                                player.transform.position = body.transform.position;

                                if (last_visibility != Main.settings.show_colliders)
                                {
                                    player.GetComponent<MeshRenderer>().enabled = Main.settings.show_colliders;
                                    skate.GetComponent<MeshRenderer>().enabled = Main.settings.show_colliders;
                                }

                                Collider collider = player.GetComponent<Collider>();
                                if (isRespawning || entry.Value.transformSyncer.currentStateEnum != NetworkPlayerStateEnum.GamePlay)
                                {
                                    collider.enabled = false;
                                    skate.GetComponent<Collider>().enabled = false;
                                }
                                else if(!collider.enabled)
                                {
                                    collider.enabled = true;
                                    skate.GetComponent<Collider>().enabled = true;
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

                    foreach (GameObject skate in skates)
                    {
                        Destroy(skate);
                    }

                    players = new List<GameObject>();
                    skates = new List<GameObject>();
                    destroyed = true;
                }
            }
        }

        public bool FastApproximately(float a, float b, float threshold)
        {
            return ((a - b) < 0 ? ((a - b) * -1) : (a - b)) <= threshold;
        }

        GameObject FindPlayer(string name)
        {
            foreach (GameObject player in players)
            {
                if (player.name == name) return player;
            }
            return null;
        }

        GameObject FindSkate(string name)
        {
            foreach (GameObject skate in skates)
            {
                if (skate.name == name) return skate;
            }
            return null;
        }

        void FindAndDestroy(string id)
        {
            foreach (GameObject player in players)
            {
                if (player.name == "collider_" + id)
                {
                    players.Remove(player);
                    Destroy(player);
                }
            }

            foreach (GameObject skate in skates)
            {
                if (skate.name == "collider_skate_" + id)
                {
                    skates.Remove(skate);
                    Destroy(skate);
                }
            }
        }

        public void CreateRoom()
        {
            MultiplayerManager.Instance.CreateRoom(false, "YKTFV");
        }

        public void OnPlayerLeft(Player player)
        {
            FindAndDestroy(player.UserId);
        }
    }
}
