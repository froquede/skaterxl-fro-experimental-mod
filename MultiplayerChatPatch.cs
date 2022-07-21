using HarmonyLib;
using System;
using UnityEngine;
using UnityModManagerNet;

namespace fro_mod
{
	[HarmonyPatch(typeof(MultiplayerChatManager), nameof(MultiplayerChatManager.SendMessage))]
	class MultiplayerChatPatch
	{
        static bool Prefix(ushort id)
		{
			// UnityModManager.Logger.Log("Sending Message: " + id);
			return Main.cbt.checkLastMessage();
		}
	}
}