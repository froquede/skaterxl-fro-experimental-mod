using HarmonyLib;

namespace fro_mod
{
    [HarmonyPatch(typeof(StopGameWhileEnabled), "OnEnable")]
    class StopWhileEnabledPatch
    {
        static bool Prefix()
        {
            return false;
        }
    }
}