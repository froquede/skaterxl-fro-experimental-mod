using FSMHelper;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace fro_mod
{
    [HarmonyPatch(typeof(PlayerStateMachine), "SetupDefinition")]
    class SetupDefinitionPatch
    {
        public static bool Prefix(ref FSMStateType stateType, ref List<Type> children)
        {
            if (Main.settings.enabled)
            {
                children.Add(typeof(WalkingState));
                return false;
            }
            return true;
        }
    }
}