using HarmonyLib;
using Photon.Pun;
using SkaterXL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityModManagerNet;

namespace fro_mod
{
    [HarmonyPatch(typeof(NetworkPlayerCustomisation), "rpcSetCustomization", new Type[] { typeof(CustomizedPlayerDataV2), typeof(PhotonMessageInfo) })]
    class NetworkPlayerCustomizationPatch
    {
        static bool Prefix(NetworkPlayerCustomisation __instance, CustomizedPlayerDataV2 customizations, PhotonMessageInfo info)
        {
            if (Main.settings.partial_gear)
            {
                Dictionary<int, GearObject> gearCache = (Dictionary<int, GearObject>)Traverse.Create(__instance.customizer).Field("gearCache").GetValue();

                CustomizedPlayerDataV2 defaultDanny = CustomizedPlayerDataV2.Default;
                NetworkPlayerController npc = __instance.GetComponentInParent<NetworkPlayerController>();

                CharacterBodyObject bod = new CharacterBodyObject(customizations.body, __instance.customizer);
                if (bod.State == GearObject.GearObjectState.Finished || bod.State == GearObject.GearObjectState.Initialized)
                {
                    __instance.customizer.EquipBody(customizations.body);
                    bod.LoadOn();
                }
                else
                {
                    UnityModManager.Logger.Log("No template found for " + customizations.body.type);
                    bod.Dispose();
                    __instance.customizer.EquipBody(defaultDanny.body);
                }


                UnityModManager.Logger.Log(customizations.body.ToString());

                SkaterInfo skater = GearDatabase.Instance.skaters.FirstOrDefault((SkaterInfo s) => customizations.IsSkater(s));

                for (int i = 0; i < customizations.clothingGear.Length; i++)
                {
                    ClothingGearObjet cgo = new ClothingGearObjet(customizations.clothingGear[i], __instance.customizer);
                    if (cgo.State == GearObject.GearObjectState.Finished || cgo.State == GearObject.GearObjectState.Initialized)
                    {
                        __instance.customizer.EquipCharacterGear(customizations.clothingGear[i]);
                    }
                    else UnityModManager.Logger.Log("Not init: " + customizations.clothingGear[i].name + " " + cgo.State);
                }
                for (int i = 0; i < customizations.boardGear.Length; i++)
                {
                    BoardGearObject bgo = BoardGearObject.CreateFor(customizations.boardGear[i], __instance.customizer);
                    if (bgo.State == GearObject.GearObjectState.Finished || bgo.State == GearObject.GearObjectState.Initialized)
                    {
                        __instance.customizer.EquipGear(customizations.boardGear[i]);
                    }
                    else UnityModManager.Logger.Log("Not init: " + customizations.boardGear[i].name + " " + bgo.State);
                }

                //__instance.customizer.LoadCustomizations(customizations);

                if (npc != null)
                {
                    npc.UpdateLocalVisibility();
                }

                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
