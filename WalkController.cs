using SkaterXL.Data;
using UnityEngine;
using Cinemachine;
using HarmonyLib;

namespace fro_mod
{
    public class WalkController : MonoBehaviour
    {
        int bail_count = 0;
        float last_muscle_pos = 0;
        bool running = false, attached = false, running_drop = false, can_press = true;
        RaycastHit walk_hit;
        Vector3 last_velocity = Vector3.zero;
        RespawnInfo last_nr;

        public void FixedUpdate()
        {
            if (Main.settings.bails && PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Bailed && PlayerController.Instance.respawn.bail.bailed)
            {
                if (Main.settings.walk_after_bail)
                {
                    PlayerController.Instance.SetKneeBendWeightManually(1f);
                    if (bail_count == 0) { running = false; attached = false; }
                    if (PlayerController.Instance.inputController.player.GetButtonDoublePressDown("Left Stick Button")) running = !running;
                    Main.controller.DisableCameraCollider(false);
                    PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[2].rigidbody.AddForce(0, (PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[2].transform.position.y - last_muscle_pos) * -60f, 0f, ForceMode.Impulse);
                    PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].rigidbody.transform.Rotate(-PlayerController.Instance.inputController.RightStick.rawInput.pos.x, 0, 0, Space.Self);
                    PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].rigidbody.AddRelativeForce(0, PlayerController.Instance.inputController.LeftStick.rawInput.pos.y * 200f, -PlayerController.Instance.inputController.LeftStick.rawInput.pos.x * 200f, ForceMode.Force);

                    var opposite = -PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].rigidbody.velocity;
                    PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].rigidbody.AddForce(opposite * (running ? 20f : 75f));

                    if (attached)
                    {
                        GameObject copy = new GameObject();

                        copy.transform.position = PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].transform.position;
                        copy.transform.rotation = PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].transform.rotation;
                        copy.transform.Rotate(90f, 0f, 0, Space.Self);
                        copy.transform.Rotate(0f, 45f, 0, Space.Self);
                        copy.transform.Translate(0, -0.2f, 0, Space.Self);
                        PlayerController.Instance.boardController.gameObject.transform.position = Vector3.Lerp(PlayerController.Instance.boardController.gameObject.transform.position, copy.transform.position, Time.deltaTime * 50f);
                        PlayerController.Instance.boardController.gameObject.transform.rotation = Quaternion.Lerp(PlayerController.Instance.boardController.gameObject.transform.rotation, copy.transform.rotation, Time.deltaTime * 25f);
                        Destroy(copy);

                        PlayerController.Instance.boardController.boardRigidbody.isKinematic = true;
                        foreach (var collider in PlayerController.Instance.boardController.boardColliders)
                        {
                            collider.enabled = false;
                        }

                        if (PlayerController.Instance.inputController.player.GetButton("B"))
                        {
                            running_drop = true;
                            last_nr = (RespawnInfo)Traverse.Create(PlayerController.Instance.respawn).Field("markerRespawnInfos").GetValue();
                            Vector3 forward = PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].transform.forward;
                            RespawnInfo nr = new RespawnInfo
                            {
                                position = walk_hit.point,
                                IsBoardBackwards = false,
                                rotation = Quaternion.LookRotation(new Vector3(forward.z, forward.y, -forward.x)),
                                isSwitch = PlayerController.Instance.GetBoardBackwards()
                            };
                            last_velocity = PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].rigidbody.velocity;
                            PlayerController.Instance.respawn.SetSpawnPoint(nr);
                            PlayerController.Instance.respawn.DoRespawn();
                            PlayerController.Instance.respawn.SetSpawnPoint(last_nr);
                            //Main.controller.Respawn(nr, false);
                            //PlayerController.Instance.respawn.DoRespawn();
                            //PreventBail_();
                        }
                    }
                    else
                    {
                        if (PlayerController.Instance.boardController.boardRigidbody.isKinematic)
                        {
                            PlayerController.Instance.boardController.boardRigidbody.isKinematic = false;
                            foreach (var collider in PlayerController.Instance.boardController.boardColliders)
                            {
                                collider.enabled = true;
                            }
                            PlayerController.Instance.boardController.boardRigidbody.AddExplosionForce(1, transform.forward, 1, 0, ForceMode.Impulse);
                        }
                    }

                    if (Physics.Raycast(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].transform.position, -transform.up, out walk_hit, 1000f, LayerMask.GetMask(new string[] { "Default", "Skateboard" })))
                    {
                        if (walk_hit.collider.gameObject.layer == LayerMask.NameToLayer("Skateboard"))
                        {
                            if (bail_count >= 128 && !attached)
                            {
                                last_nr = (RespawnInfo)Traverse.Create(PlayerController.Instance.respawn).Field("markerRespawnInfos").GetValue();
                                RespawnInfo nr = new RespawnInfo
                                {
                                    position = walk_hit.point,
                                    IsBoardBackwards = false,
                                    rotation = Quaternion.LookRotation(PlayerController.Instance.boardController.boardTransform.forward),
                                    isSwitch = PlayerController.Instance.GetBoardBackwards()
                                };
                                PlayerController.Instance.respawn.SetSpawnPoint(nr);
                                PlayerController.Instance.respawn.DoRespawn();
                                PlayerController.Instance.respawn.SetSpawnPoint(last_nr);
                            }
                            else
                            {
                                last_muscle_pos = walk_hit.point.y + 1.2f;
                            }
                        }
                        else
                        {
                            last_muscle_pos = walk_hit.point.y + (bail_count >= 128 ? 1.43f : 1.2f);
                        }
                    }
                    bail_count++;

                    AnimatorStateInfo currentAnimatorStateInfo = PlayerController.Instance.animationController.skaterAnim.GetCurrentAnimatorStateInfo(0);
                    if (!currentAnimatorStateInfo.IsName("Falling"))
                    {
                        PlayerController.Instance.animationController.ForceAnimation("Falling");
                    }
                    PlayerController.Instance.animationController.ScaleAnimSpeed(PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].rigidbody.velocity.magnitude / 3f);
                }
            }
            else
            {
                bail_count = 0;

                last_muscle_pos = PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[2].transform.position.y;

                Main.controller.DisableCameraCollider(Main.settings.camera_avoidance);
            }
        }

        public void LateUpdate()
        {
            if (Main.settings.walk_after_bail)
            {
                if (PlayerController.Instance.currentStateEnum == PlayerController.CurrentState.Bailed && bail_count >= 128)
                {
                    if (Main.controller.mainCam == null) Main.controller.mainCam = PlayerController.Instance.cameraController._actualCam.GetComponent<CinemachineVirtualCamera>();
                    Main.controller.mainCam.transform.position = PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].transform.position;
                    Main.controller.mainCam.transform.rotation = PlayerController.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].transform.rotation;
                    Main.controller.mainCam.transform.Rotate(0, 0, 90f, Space.Self);
                    Main.controller.mainCam.transform.Rotate(0, 90f, 0f, Space.Self);
                    Main.controller.mainCam.transform.Translate(0, 0, -1.25f, Space.Self);

                    if (PlayerController.Instance.inputController.player.GetButton("X"))
                    {
                        if (can_press)
                        {
                            attached = !attached;
                            can_press = false;
                        }
                    }
                    else
                    {
                        can_press = true;
                    }
                }
                else
                {
                    can_press = true;
                }

                if (running_drop && !PlayerController.Instance.respawn.respawning)
                {
                    PlayerController.Instance.OnEnterSetupState();
                    PlayerController.Instance.currentStateEnum = PlayerController.CurrentState.Riding;
                    PlayerController.Instance.currentState = PlayerController.CurrentState.Riding.ToString();
                    PlayerController.Instance.boardController.boardRigidbody.AddForce(last_velocity * 10f, ForceMode.Impulse);
                    running_drop = false;
                }
            }
        }
    }
}
