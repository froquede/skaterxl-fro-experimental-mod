using SkaterXL.Multiplayer;
using SkaterXL.Core;
using UnityEngine;
using HarmonyLib;
using UnityModManagerNet;

namespace fro_mod
{
    public class Multiplayer : MonoBehaviour
    {
        void Start()
        {
            //MultiplayerManager.Instance.gameModePopup.enabled = true;
            MultiplayerManager.Instance.menuController.mainMenu.gameObject.SetActive(false);
            MultiplayerManager.Instance.menuController.roomList.gameObject.SetActive(false);
            MultiplayerManager.Instance.menuController.roomInfo.gameObject.SetActive(false);
            MultiplayerManager.Instance.menuController.playerListMenu.gameObject.SetActive(false);
            if (MultiplayerManager.Instance.menuController.playerInfoMenu.isActiveAndEnabled)
            {
                MultiplayerManager.Instance.menuController.playerInfoMenu.gameObject.SetActive(false);
            }
            MultiplayerManager.Instance.menuController.playerInfoMenu.gameObject.SetActive(false);
            MultiplayerManager.Instance.menuController.gameModeMenu.gameObject.SetActive(true);

            Traverse.Create(MultiplayerManager.Instance.menuController.gameModeMenu).Field("selectedGameMode").SetValue(null);
            GameModeInfo gmi = new GameModeInfo("S.K.A.T.E", typeof(GameMode_SKATE));
            MultiplayerManager.Instance.menuController.gameModeMenu.SelectGameMode(gmi);
            GameMode mode = (GameMode)Traverse.Create(MultiplayerManager.Instance.menuController.gameModeMenu).Field("selectedGameMode").GetValue();
            MultiplayerManager.Instance.localPlayer.ShowCountdown(15f);
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
