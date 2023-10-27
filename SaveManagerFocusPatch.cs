using GameManagement;
using HarmonyLib;
using UnityEngine;

namespace fro_mod
{
    [HarmonyPatch(typeof(SaveManager), "OnApplicationFocus")]
    class SaveManagerFocusPatch
    {
        static bool Prefix(SaveManager __instance, bool focus)
        {
            if (!PlayerController.Instance.respawn.respawning && GameStateMachine.Instance.CurrentState.GetType() == typeof(PlayState))
            {
                Time.timeScale = 1;
            }
            return false;
        }

        public static void HandleCustomMapChanges()
        {
            MonoBehaviourSingleton<LevelManager>.Instance.FetchCustomMaps();
            if (MonoBehaviourSingleton<GameStateMachine>.Instance.CurrentState is LevelSelectionState)
            {
                GameStateMachine instance = MonoBehaviourSingleton<GameStateMachine>.Instance;
                LevelSelectionController levelSelectionController;
                if (instance == null)
                {
                    levelSelectionController = null;
                }
                else
                {
                    GameObject levelSelectionObject = instance.LevelSelectionObject;
                    levelSelectionController = ((levelSelectionObject != null) ? levelSelectionObject.GetComponent<LevelSelectionController>() : null);
                }

                if (levelSelectionController != null && levelSelectionController.enabled && MonoBehaviourSingleton<GameStateMachine>.Instance.CurrentState is LevelSelectionState && levelSelectionController != null && levelSelectionController.enabled)
                {
                    levelSelectionController.UpdateList();
                }
            }
        }

        public static void HandleCustomGearChanges()
        {
            GearDatabase.Instance.FetchCustomGear();
            if (MonoBehaviourSingleton<GameStateMachine>.Instance.CurrentState is GearSelectionState && GearSelectionController.Instance != null && GearSelectionController.Instance.initialized && GearSelectionController.Instance.enabled && MonoBehaviourSingleton<GameStateMachine>.Instance.CurrentState is GearSelectionState && GearSelectionController.Instance != null && GearSelectionController.Instance.initialized && GearSelectionController.Instance.enabled)
            {
                GearSelectionController.Instance.listView.UpdateList();
            }
        }
    }
}