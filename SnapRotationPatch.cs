using System;
using HarmonyLib;
using UnityEngine;

namespace fro_mod { 
	[HarmonyPatch(typeof(PlayerController), "SnapRotation", new Type[] { typeof(Quaternion) })]
	internal class SnapRotationPatch
	{
		private static bool Prefix(Quaternion p_rot)
		{
			return Main.settings.enabled && (Main.settings.trick_customization || Main.settings.snappy_catch);
		}
	}
}
