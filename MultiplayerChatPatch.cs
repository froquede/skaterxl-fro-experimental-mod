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
			if(Main.settings.debug) UnityModManager.Logger.Log("Sending Message: " + id);

			bool can_send = Main.cbt.checkLastMessage();
			if(can_send)
            {
				if ((int)id == 4) Main.controller.PlayLetsGoAnim();
            }

			return can_send;
		}
	}
}