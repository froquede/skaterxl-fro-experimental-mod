using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;

namespace fro_mod
{
    [EnableReloading]
    public class Main
    {
        public static Settings settings;
        public static Harmony harmonyInstance;
        public static UnityModManager.ModEntry modEntry;
        public static Controller controller;
        public static UIController ui;
        public static ChatBubbleTest cbt;
        public static Multiplayer multi;

        static bool Unload(UnityModManager.ModEntry modEntry)
        {
            UnityEngine.Object.Destroy(controller);
            UnityEngine.Object.Destroy(ui);
            UnityEngine.Object.Destroy(cbt);
            UnityEngine.Object.Destroy(multi);

            try
            {
                harmonyInstance.UnpatchAll(harmonyInstance.Id);
            }
            catch { }

            return true;
        }

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            harmonyInstance = new Harmony(modEntry.Info.Id);
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            controller = new GameObject().AddComponent<Controller>();
            ui = new GameObject().AddComponent<UIController>();
            cbt = new GameObject().AddComponent<ChatBubbleTest>();
            multi = new GameObject().AddComponent<Multiplayer>();
            UnityEngine.Object.DontDestroyOnLoad(controller);
            UnityEngine.Object.DontDestroyOnLoad(ui);
            UnityEngine.Object.DontDestroyOnLoad(cbt);
            UnityEngine.Object.DontDestroyOnLoad(multi);

            /*modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = new Action<UnityModManager.ModEntry>(OnSaveGUI);
            modEntry.OnToggle = new Func<UnityModManager.ModEntry, bool, bool>(OnToggle);*/
            modEntry.OnUnload = Unload;
            Main.modEntry = modEntry;
            checkLists(modEntry);

            try
            {
                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch { }

            UnityModManager.Logger.Log("Loaded " + modEntry.Info.Id);
            return true;
        }

        public static void checkLists(UnityModManager.ModEntry modEntry, bool force = false)
        {
            if (settings.dynamic_feet_states.Count == 0 || force)
            {
                settings.dynamic_feet_states = new List<bool>(new bool[16]);
                settings.dynamic_feet_states[0] = true;
                settings.dynamic_feet_states[1] = true;
                settings.dynamic_feet_states[2] = true;
                settings.dynamic_feet_states[7] = true;
                settings.dynamic_feet_states[8] = true;
                settings.dynamic_feet_states[9] = true;
                settings.dynamic_feet_states[10] = true;
                settings.dynamic_feet_states[11] = true;
                settings.dynamic_feet_states[12] = true;
                settings.Save(modEntry);
            }


            if (settings.look_forward_states.Count == 0 || force)
            {
                settings.look_forward_states = new List<bool>(new bool[16]);
                settings.look_forward_states[1] = true;
                settings.Save(modEntry);
            }
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {

        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            UnityModManager.Logger.Log("Toggled " + modEntry.Info.Id);

            if (value)
            {
                harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }
            else
            {
                UnityEngine.Object.Destroy(controller);
                UnityEngine.Object.Destroy(ui);
                UnityEngine.Object.Destroy(cbt);
                UnityEngine.Object.Destroy(multi);
                harmonyInstance.UnpatchAll(harmonyInstance.Id);
            }

            return true;
        }
    }
}
