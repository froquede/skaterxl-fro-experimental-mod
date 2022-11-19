using HarmonyLib;

namespace fro_mod
{
    [HarmonyPatch(typeof(PlayerController), "DoBailDelay")]

    class RespawnCancel
    {
        private static bool Prefix()
        {
            if (Main.settings.enabled && Main.settings.walk_after_bail)
            {
                PlayerController.Instance.Invoke("DoBail", 9999f);
                return false;
            }
            return true;
        }
    }
}