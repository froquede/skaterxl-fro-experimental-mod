using System;
using Cinemachine;
using HarmonyLib;
using SkaterXL.Data;
using TMPro;
using UnityEngine;

namespace fro_mod
{
	// Token: 0x0200002B RID: 43
	public class WalkController : MonoBehaviour
	{
		// Token: 0x06000120 RID: 288 RVA: 0x00012C7C File Offset: 0x00010E7C

		public void FixedUpdate()
		{
			if (Main.settings.bails && MonoBehaviourSingleton<PlayerController>.Instance.currentStateEnum == PlayerController.CurrentState.Bailed && MonoBehaviourSingleton<PlayerController>.Instance.respawn.bail.bailed)
			{
				if (Main.settings.walk_after_bail || bail_count < 24)
				{
					MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.pinWeightThreshold = 1f;
					MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.pinWeight = 1f;
					MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.mappingWeight = 1f;
					MonoBehaviourSingleton<PlayerController>.Instance.skaterController.rightFootCollider.isTrigger = Main.settings.walk_after_bail;
					MonoBehaviourSingleton<PlayerController>.Instance.skaterController.leftFootCollider.isTrigger = Main.settings.walk_after_bail;
					MonoBehaviourSingleton<PlayerController>.Instance.SetKneeBendWeightManually(1f);
					if (this.bail_count == 0)
					{
						this.running = false;
						this.attached = false;
					}

					if (MonoBehaviourSingleton<PlayerController>.Instance.inputController.player.GetButtonDown("Left Stick Button") && Main.settings.walk_after_bail)
					{
						this.running = !this.running;
						NotificationManager.Instance.ShowNotification((this.running ? "Running" : "Walking"), 1f, false, NotificationManager.NotificationType.Normal, TextAlignmentOptions.TopRight, 0f);
					}

					Main.controller.DisableCameraCollider(false);
					float num = MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[2].transform.position.y - this.last_muscle_pos;
					MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[2].rigidbody.AddForce(0f, ((num <= -0.01f) ? num : 0f) * -40f, 0f, ForceMode.Impulse);
					MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].rigidbody.transform.Rotate(-MonoBehaviourSingleton<PlayerController>.Instance.inputController.RightStick.rawInput.pos.x, 0f, 0f, Space.Self);
					MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].rigidbody.AddRelativeForce(0f, MonoBehaviourSingleton<PlayerController>.Instance.inputController.LeftStick.rawInput.pos.y * (this.running ? 300f : 200f), -MonoBehaviourSingleton<PlayerController>.Instance.inputController.LeftStick.rawInput.pos.x * 200f, ForceMode.Force);
					Vector3 a = -MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].rigidbody.velocity;
					MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].rigidbody.AddForce(a * (this.running ? 20f : 75f));
					if (num <= 0f && this.bail_count >= 128)
					{
						Vector3 position = MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[1].transform.position;
						MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[12].transform.position = new Vector3(MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[12].transform.position.x, this.walk_hit.point.y, MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[12].transform.position.z);
						MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[15].transform.position = new Vector3(MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[15].transform.position.x, this.walk_hit.point.y, MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[15].transform.position.z);
					}
					if (this.attached)
					{
						GameObject gameObject = new GameObject();
						gameObject.transform.position = MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].transform.position;
						gameObject.transform.rotation = MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].transform.rotation;
						gameObject.transform.Rotate(90f, 0f, 0f, Space.Self);
						gameObject.transform.Rotate(0f, 45f, 0f, Space.Self);
						gameObject.transform.Translate(0f, -0.2f, 0f, Space.Self);
						MonoBehaviourSingleton<PlayerController>.Instance.boardController.gameObject.transform.position = Vector3.Lerp(MonoBehaviourSingleton<PlayerController>.Instance.boardController.gameObject.transform.position, gameObject.transform.position, Time.smoothDeltaTime * 50f);
						MonoBehaviourSingleton<PlayerController>.Instance.boardController.gameObject.transform.rotation = Quaternion.Slerp(MonoBehaviourSingleton<PlayerController>.Instance.boardController.gameObject.transform.rotation, gameObject.transform.rotation, Time.smoothDeltaTime * 25f);
						UnityEngine.Object.Destroy(gameObject);
						MonoBehaviourSingleton<PlayerController>.Instance.boardController.boardRigidbody.isKinematic = true;
						Collider[] boardColliders = MonoBehaviourSingleton<PlayerController>.Instance.boardController.boardColliders;
						for (int i = 0; i < boardColliders.Length; i++)
						{
							boardColliders[i].enabled = false;
						}
						if (MonoBehaviourSingleton<PlayerController>.Instance.inputController.player.GetButton("X"))
						{
							this.running_drop = true;
							this.last_nr = (RespawnInfo)Traverse.Create(MonoBehaviourSingleton<PlayerController>.Instance.respawn).Field("markerRespawnInfos").GetValue();
							Vector3 forward = MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].transform.forward;
							RespawnInfo respawnInfo = new RespawnInfo
							{
								position = this.walk_hit.point,
								IsBoardBackwards = false,
								rotation = Quaternion.LookRotation(new Vector3(forward.z, forward.y, -forward.x)),
								isSwitch = MonoBehaviourSingleton<PlayerController>.Instance.GetBoardBackwards()
							};
							this.last_velocity = MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].rigidbody.velocity;
							MonoBehaviourSingleton<PlayerController>.Instance.respawn.SetSpawnPoint(respawnInfo, Respawn.SpawnPointChangeMethod.Auto);
							MonoBehaviourSingleton<PlayerController>.Instance.respawn.DoRespawn();
							MonoBehaviourSingleton<PlayerController>.Instance.respawn.SetSpawnPoint(this.last_nr, Respawn.SpawnPointChangeMethod.Auto);
						}
					}
					else if (MonoBehaviourSingleton<PlayerController>.Instance.boardController.boardRigidbody.isKinematic)
					{
						MonoBehaviourSingleton<PlayerController>.Instance.boardController.boardRigidbody.isKinematic = false;
						Collider[] boardColliders = MonoBehaviourSingleton<PlayerController>.Instance.boardController.boardColliders;
						for (int i = 0; i < boardColliders.Length; i++)
						{
							boardColliders[i].enabled = true;
						}
						MonoBehaviourSingleton<PlayerController>.Instance.boardController.boardRigidbody.AddExplosionForce(1f, base.transform.forward, 1f, 0f, ForceMode.Impulse);
					}
					if (Physics.Raycast(MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[0].transform.position, -base.transform.up, out this.walk_hit, 1000f, LayerMask.GetMask(new string[]
					{
						"Default",
						"Skateboard"
					})))
					{
						if (this.walk_hit.collider.gameObject.layer == LayerMask.NameToLayer("Skateboard"))
						{
							if (this.bail_count >= 128 && !this.attached)
							{
								this.last_nr = (RespawnInfo)Traverse.Create(MonoBehaviourSingleton<PlayerController>.Instance.respawn).Field("markerRespawnInfos").GetValue();
								RespawnInfo respawnInfo2 = new RespawnInfo
								{
									position = this.walk_hit.point,
									IsBoardBackwards = false,
									rotation = Quaternion.LookRotation(MonoBehaviourSingleton<PlayerController>.Instance.boardController.boardTransform.forward),
									isSwitch = MonoBehaviourSingleton<PlayerController>.Instance.GetBoardBackwards()
								};
								MonoBehaviourSingleton<PlayerController>.Instance.respawn.SetSpawnPoint(respawnInfo2, Respawn.SpawnPointChangeMethod.Auto);
								MonoBehaviourSingleton<PlayerController>.Instance.respawn.DoRespawn();
								MonoBehaviourSingleton<PlayerController>.Instance.respawn.SetSpawnPoint(this.last_nr, Respawn.SpawnPointChangeMethod.Auto);
							}
							else
							{
								this.last_muscle_pos = this.walk_hit.point.y + 1.2f;
							}
						}
						else
						{
							this.last_muscle_pos = this.walk_hit.point.y + ((this.bail_count >= 128) ? 1.36f : 1.2f);
						}
					}
					this.bail_count++;
					MonoBehaviourSingleton<PlayerController>.Instance.animationController.skaterAnim.enabled = true;
					MonoBehaviourSingleton<PlayerController>.Instance.animationController.ikAnim.enabled = true;
				}
			}
			else
			{
				this.bail_count = 0;
				this.last_muscle_pos = MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[2].transform.position.y;
				Main.controller.DisableCameraCollider(Main.settings.camera_avoidance);
			}
			if (Main.settings.walk_after_bail && this.running_drop)
			{
				MonoBehaviourSingleton<PlayerController>.Instance.BoardFreezedAfterRespawn = false;
				if (!MonoBehaviourSingleton<PlayerController>.Instance.IsRespawning)
				{
					MonoBehaviourSingleton<PlayerController>.Instance.respawn.StopCoroutine("RespawnRoutine");
					Time.timeScale = 1f;
					MonoBehaviourSingleton<EventManager>.Instance.OnCatched(true, true);
					MonoBehaviourSingleton<PlayerController>.Instance.AnimSetBraking(false);
					MonoBehaviourSingleton<PlayerController>.Instance.SetBoardPhysicsMaterial(PlayerController.FrictionType.Default);
					MonoBehaviourSingleton<PlayerController>.Instance.currentStateEnum = PlayerController.CurrentState.Riding;
					MonoBehaviourSingleton<PlayerController>.Instance.currentState = PlayerController.CurrentState.Riding.ToString();
					MonoBehaviourSingleton<PlayerController>.Instance.boardController.boardRigidbody.AddForce(this.last_velocity * 5f, ForceMode.VelocityChange);
					MonoBehaviourSingleton<PlayerController>.Instance.skaterController.skaterRigidbody.AddForce(this.last_velocity * 5f, ForceMode.VelocityChange);
					this.running_drop = false;
				}
			}
		}

		// Token: 0x06000121 RID: 289 RVA: 0x00013744 File Offset: 0x00011944
		public void LateUpdate()
		{
			if (Main.settings.walk_after_bail)
			{
				if (Main.controller.mainCam == null)
				{
					Main.controller.mainCam = MonoBehaviourSingleton<PlayerController>.Instance.cameraController._actualCam.GetComponent<CinemachineVirtualCamera>();
				}
				if (MonoBehaviourSingleton<PlayerController>.Instance.currentStateEnum == PlayerController.CurrentState.Bailed)
				{
					GameObject gameObject = new GameObject();
					gameObject.transform.position = MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[1].transform.position;
					gameObject.transform.rotation = MonoBehaviourSingleton<PlayerController>.Instance.respawn.behaviourPuppet.puppetMaster.muscles[1].transform.rotation;
					gameObject.transform.Rotate(0f, 0f, 90f);
					gameObject.transform.Rotate(0f, 90f, 0f);
					gameObject.transform.Translate(0f, 0f, -1.35f);
					Main.controller.mainCam.transform.rotation = Quaternion.Slerp(this.last_rot_camera, gameObject.transform.rotation, Time.smoothDeltaTime * (this.running ? 14f : 10f));
					Main.controller.mainCam.transform.position = Vector3.Lerp(this.last_pos_camera, gameObject.transform.position, Time.smoothDeltaTime * (this.running ? 10f : 6f));
					this.last_rot_camera = Main.controller.mainCam.transform.rotation;
					this.last_pos_camera = Main.controller.mainCam.transform.position;
					UnityEngine.Object.Destroy(gameObject);
					if (!MonoBehaviourSingleton<PlayerController>.Instance.inputController.player.GetButton("B"))
					{
						this.can_press = true;
						return;
					}
					if (this.can_press)
					{
						this.attached = !this.attached;
						this.can_press = false;
						return;
					}
				}
				else
				{
					this.can_press = true;
					this.last_rot_camera = Main.controller.mainCam.transform.rotation;
					this.last_pos_camera = Main.controller.mainCam.transform.position;
				}
			}
		}

		// Token: 0x040001AB RID: 427
		private int bail_count;

		// Token: 0x040001AC RID: 428
		private float last_muscle_pos;

		// Token: 0x040001AD RID: 429
		private bool running;

		// Token: 0x040001AE RID: 430
		private bool attached;

		// Token: 0x040001AF RID: 431
		private bool running_drop;

		// Token: 0x040001B0 RID: 432
		private bool can_press = true;

		// Token: 0x040001B1 RID: 433
		private RaycastHit walk_hit;

		// Token: 0x040001B2 RID: 434
		private Vector3 last_velocity = Vector3.zero;

		// Token: 0x040001B3 RID: 435
		private RespawnInfo last_nr;

		// Token: 0x040001B4 RID: 436
		private Quaternion last_rot_camera;

		// Token: 0x040001B5 RID: 437
		private Vector3 last_pos_camera;
	}
}
