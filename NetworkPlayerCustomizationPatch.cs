using HarmonyLib;
using Photon.Pun;
using SkaterXL.Data;
using SkaterXL.Gear;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;

namespace fro_mod
{
    [HarmonyPatch(typeof(NetworkPlayerCustomisation), "rpcSetCustomization", new Type[] { typeof(CustomizedPlayerDataV2), typeof(PhotonMessageInfo) })]
    class NetworkPlayerCustomizationPatch : MonoBehaviour
    {
        static bool Prefix(NetworkPlayerCustomisation __instance, CustomizedPlayerDataV2 customizations, PhotonMessageInfo info)
        {
            if (Main.settings.partial_gear)
            {
                CustomizedPlayerDataV2 defaultDanny = CustomizedPlayerDataV2.Default;
                CharacterBodyObject bod = new CharacterBodyObject(customizations.body, __instance.customizer);
                bod.LoadOn();

                NetworkPlayerController npc = __instance.GetComponentInParent<NetworkPlayerController>();
                
                __instance.customizer.LoadCustomizations(defaultDanny);
                __instance.customizer.EquipBody(customizations.body);

                for (int i = 0; i < customizations.clothingGear.Length; i++)
                {
                    ClothingGearObjet cgo = new ClothingGearObjet(customizations.clothingGear[i], __instance.customizer);

                    if (cgo.State == GearObject.GearObjectState.Finished)
                    {
                        __instance.customizer.EquipCharacterGear(customizations.clothingGear[i]);
                    }
                    else
                    {
                        pushToCheck(cgo, customizations.clothingGear[i], __instance.customizer);
                    }
                }
                for (int i = 0; i < customizations.boardGear.Length; i++)
                {
                    BoardGearObject bgo = BoardGearObject.CreateFor(customizations.boardGear[i], __instance.customizer);

                    if (bgo.State == GearObject.GearObjectState.Finished)
                    {
                        __instance.customizer.EquipGear(customizations.boardGear[i]);
                    }
                    else
                    {
                        pushToCheckBoardGear(bgo, customizations.boardGear[i], __instance.customizer);
                    }
                    // else UnityModManager.Logger.Log("Not init: " + customizations.boardGear[i].name + " " + bgo.State);
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

        private static void pushToCheck(ClothingGearObjet cgo, CharacterGearInfo characterGearInfo, CharacterCustomizer customizer)
        {
            Main.multi.toCheck.Add(new Multiplayer.ClothWatcherObj(cgo, characterGearInfo, customizer));
        }

        private static void pushToCheckBoardGear(BoardGearObject bgo, BoardGearInfo gearInfo, CharacterCustomizer customizer)
        {
            Main.multi.toCheckBoardGear.Add(new Multiplayer.ClothWatcherObj(bgo, gearInfo, customizer));
        }
    }
}
