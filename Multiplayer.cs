using SkaterXL.Multiplayer;
using SkaterXL.Core;
using UnityEngine;
using HarmonyLib;
using UnityModManagerNet;
using System.Collections.Generic;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Collections;
using SkaterXL.Data;

namespace fro_mod
{
    public class Multiplayer : MonoBehaviour
    {
        public class ClothWatcherObj
        {
            public ClothingGearObjet cgo;
            public CharacterGearInfo characterGearInfo;
            public CharacterCustomizer customizer;
            public BoardGearObject bgo;
            public BoardGearInfo gearInfo;

            public ClothWatcherObj(ClothingGearObjet cgo, CharacterGearInfo characterGearInfo, CharacterCustomizer customizer)
            {
                this.cgo = cgo;
                this.characterGearInfo = characterGearInfo;
                this.customizer = customizer;
            }

            public ClothWatcherObj(BoardGearObject bgo, BoardGearInfo gearInfo, CharacterCustomizer customizer)
            {
                this.bgo = bgo;
                this.gearInfo = gearInfo;
                this.customizer = customizer;
            }
        }

        public List<ClothWatcherObj> toCheck = new List<ClothWatcherObj>();
        public List<ClothWatcherObj> toCheckBoardGear = new List<ClothWatcherObj>();        

        void Start()
        {
            last_visibility = Main.settings.show_colliders;
        }

        List<GameObject> players = new List<GameObject>();
        List<GameObject> skates = new List<GameObject>();
        string last_scene = "";
        bool last_visibility = false;
        bool destroyed = false;
        Vector3 last_respawn_point = Vector3.zero;
        double time = 0;

        void FixedUpdate()
        {
            if (MultiplayerManager.Instance.InRoom)
            {
                if (Time.unscaledTime - time >= 20f)
                {
                    // UnityModManager.Logger.Log("Updating custom scale");

                    CheckCustomData();

                    time = Time.unscaledTime;
                }

                foreach (ClothWatcherObj cwo in toCheck)
                {
                    if (cwo.cgo.State != GearObject.GearObjectState.Loading)
                    {
                        if (cwo.cgo.State == GearObject.GearObjectState.Finished || cwo.cgo.State == GearObject.GearObjectState.Initialized)
                        {
                            cwo.customizer.EquipCharacterGear(cwo.characterGearInfo);
                            toCheck.Remove(cwo);
                        }
                        if (cwo.cgo.State == GearObject.GearObjectState.Failed || cwo.cgo.State == GearObject.GearObjectState.Canceled || cwo.cgo.State == GearObject.GearObjectState.Disposed)
                        {
                            toCheck.Remove(cwo);
                        }
                    }

                    foreach (ClothWatcherObj cwobg in toCheckBoardGear)
                    {
                        if (cwobg.bgo.State != GearObject.GearObjectState.Loading)
                        {
                            if (cwobg.bgo.State == GearObject.GearObjectState.Finished || cwobg.bgo.State == GearObject.GearObjectState.Initialized)
                            {
                                cwobg.customizer.EquipBoardGear(cwobg.gearInfo);
                                toCheckBoardGear.Remove(cwobg);
                            }
                            if (cwobg.bgo.State == GearObject.GearObjectState.Failed || cwobg.bgo.State == GearObject.GearObjectState.Canceled || cwobg.bgo.State == GearObject.GearObjectState.Disposed)
                            {
                                toCheckBoardGear.Remove(cwo);
                            }
                        }
                    }
                }
            }

            if (MultiplayerManager.Instance.InRoom && Main.settings.multiplayer_collision)
            {
                destroyed = false;
                if (PlayerController.Instance.IsRespawning)
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
                                else if (!collider.enabled)
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

        void CheckCustomData()
        {
            foreach (KeyValuePair<int, NetworkPlayerController> entry in MultiplayerManager.Instance.networkPlayers)
            {
                if (entry.Value)
                {
                    if (entry.Value.GetPlayer().CustomProperties["scale"] != null)
                    {
                        entry.Value.GetBody().transform.localScale = (Vector3)entry.Value.GetPlayer().CustomProperties["scale"];
                    }
                    if (entry.Value.GetPlayer().CustomProperties["bpscale"] != null)
                    {
                        float[] scale = (float[])entry.Value.GetPlayer().CustomProperties["bpscale"];
                        Transform joints = entry.Value.GetBody().transform.Find("Skater_Joints");
                        Transform head = joints.FindChildRecursively("Skater_Head");
                        if (head) head.localScale = new Vector3(scale[0], scale[0], scale[0]);

                        Transform lhand = joints.FindChildRecursively("Skater_hand_l");
                        if (lhand) lhand.localScale = new Vector3(scale[1], scale[1], scale[1]);

                        Transform rhand = joints.FindChildRecursively("Skater_hand_r");
                        if (rhand) rhand.localScale = new Vector3(scale[2], scale[2], scale[2]);

                        Transform lfoot = joints.FindChildRecursively("Skater_foot_l");
                        if (lfoot) lfoot.localScale = new Vector3(scale[3], scale[3], scale[3]);

                        Transform rfoot = joints.FindChildRecursively("Skater_foot_r");
                        if (rfoot) rfoot.localScale = new Vector3(scale[4], scale[4], scale[4]);

                        Transform pelvis = joints.FindChildRecursively("Skater_pelvis");
                        if (pelvis) pelvis.localScale = new Vector3(scale[5], scale[5], scale[5]);

                        Transform spine = joints.FindChildRecursively("Skater_Spine1");
                        if (spine) spine.localScale = new Vector3(scale[6], scale[6], scale[6]);

                        Transform spine2 = joints.FindChildRecursively("Skater_Spine2");
                        if (spine2) spine2.localScale = new Vector3(scale[7], scale[7], scale[7]);

                        Transform larm = joints.FindChildRecursively("Skater_Arm_l");
                        if (larm) larm.localScale = new Vector3(scale[8], scale[8], scale[8]);

                        Transform lfarm = joints.FindChildRecursively("Skater_ForeArm_l");
                        if (lfarm) lfarm.localScale = new Vector3(scale[9], scale[9], scale[9]);

                        Transform rarm = joints.FindChildRecursively("Skater_Arm_r");
                        if (rarm) rarm.localScale = new Vector3(scale[10], scale[10], scale[10]);

                        Transform rfarm = joints.FindChildRecursively("Skater_ForeArm_r");
                        if (rfarm) rfarm.localScale = new Vector3(scale[11], scale[11], scale[11]);

                        Transform lupleg = joints.FindChildRecursively("Skater_UpLeg_l");
                        if (lupleg) lupleg.localScale = new Vector3(scale[12], scale[12], scale[12]);

                        Transform lleg = joints.FindChildRecursively("Skater_Leg_l");
                        if (lleg) lleg.localScale = new Vector3(scale[13], scale[13], scale[13]);

                        Transform rupleg = joints.FindChildRecursively("Skater_UpLeg_r");
                        if (rupleg) rupleg.localScale = new Vector3(scale[14], scale[14], scale[14]);

                        Transform rleg = joints.FindChildRecursively("Skater_Leg_r");
                        if (rleg) rleg.localScale = new Vector3(scale[15], scale[15], scale[15]);

                        Transform neck = joints.FindChildRecursively("Skater_Neck");
                        if (neck) neck.localScale = new Vector3(scale[16], scale[16], scale[16]);
                    }
                }
            }

            ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
            hashtable["allowSpectators"] = true;
            hashtable["scale"] = Main.settings.custom_scale;
            float[] scales = { Main.settings.custom_scale_head,
                                    Main.settings.custom_scale_hand_l,
                                    Main.settings.custom_scale_hand_r,
                                    Main.settings.custom_scale_foot_l,
                                    Main.settings.custom_scale_foot_r,
                                    Main.settings.custom_scale_pelvis,
                                    Main.settings.custom_scale_spine,
                                    Main.settings.custom_scale_spine2,
                                    Main.settings.custom_scale_arm_l,
                                    Main.settings.custom_scale_forearm_l,
                                    Main.settings.custom_scale_arm_r,
                                    Main.settings.custom_scale_forearm_r,
                                    Main.settings.custom_scale_upleg_l,
                                    Main.settings.custom_scale_leg_l,
                                    Main.settings.custom_scale_upleg_r,
                                    Main.settings.custom_scale_leg_r,
                                    Main.settings.custom_scale_neck };
            hashtable["bpscale"] = scales;
            PhotonNetwork.SetPlayerCustomProperties(hashtable);
        }

            public void LogGear()
        {
            foreach (KeyValuePair<int, NetworkPlayerController> entry in MultiplayerManager.Instance.networkPlayers)
            {
                if (entry.Value)
                {
                    if (entry.Value.UserId != MultiplayerManager.Instance.localPlayer.UserId)
                    {
                        Dictionary<int, GearObject> gearCache = (Dictionary<int, GearObject>)Traverse.Create(entry.Value.customizationSyncer.customizer).Field("gearCache").GetValue();
                        List<GearInfo> gear = entry.Value.customizationSyncer.customizer.CurrentCustomizations.GetAllGearInfos();

                        UnityModManager.Logger.Log(entry.Value.NickName);
                        foreach (var obj in gearCache)
                        {
                            UnityModManager.Logger.Log(obj.Key + " " + obj.Value.gearInfo.ToString());
                        }
                    }
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

        public void KickPlayer(string kickid)
        {
            string index = kickid.Split(':')[1];

            foreach (KeyValuePair<int, NetworkPlayerController> entry in MultiplayerManager.Instance.networkPlayers)
            {
                if (entry.Value)
                {
                    string id = entry.Value.NickName;
                    UnityModManager.Logger.Log(id);
                    if (id == index)
                    {
                        PhotonNetwork.CloseConnection(entry.Value.GetPlayer());
                    }
                }
            }
        }


        Player lastMaster;
        public void setMeAsMaster()
        {
            foreach (KeyValuePair<int, NetworkPlayerController> entry in MultiplayerManager.Instance.networkPlayers)
            {
                if (entry.Value)
                {
                    if (entry.Value.GetPlayer().IsMasterClient)
                    {
                        lastMaster = entry.Value.GetPlayer();
                        UnityModManager.Logger.Log(lastMaster.NickName);
                    }
                }
            }

            PhotonNetwork.SetMasterClient(MultiplayerManager.Instance.localPlayer.GetPlayer());
        }

        public void restoreMaster()
        {
            PhotonNetwork.SetMasterClient(lastMaster);
        }

        public void MasterPlayer(string kickid)
        {
            string index = kickid.Split(':')[1];

            foreach (KeyValuePair<int, NetworkPlayerController> entry in MultiplayerManager.Instance.networkPlayers)
            {
                if (entry.Value)
                {
                    string id = entry.Value.NickName;
                    UnityModManager.Logger.Log(id);
                    if (id == index)
                    {
                        PhotonNetwork.SetMasterClient(entry.Value.GetPlayer());
                    }
                }
            }
        }

        public bool isMaster()
        {
            return PhotonNetwork.IsMasterClient;
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
