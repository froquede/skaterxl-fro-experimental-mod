using HarmonyLib;
using System;
using UnityModManagerNet;
namespace fro_mod
{
	[HarmonyPatch(typeof(SaveManager), "OnApplicationFocus", new Type[] { typeof(void) })]
	class MultiplayerManagerFocusPatch
    {
		static bool Prefix(bool focus)
		{
			return false;
		}
	}
}