using System;
using System.Reflection;
using System.Collections.Generic;
using BattleTech;
using Harmony;

namespace EnvironmentalDesignMasks {
    [HarmonyPatch(typeof(AbstractActor), "ApplyDesignMaskStickyEffect")]
    public static class AbstractActor_ApplyDesignMaskStickyEffect {
        public static void Postfix(AbstractActor __instance, DesignMaskDef mask, int stackItemUID) {
            try {
                if (mask == null || !EDM.additionalStickyEffects.ContainsKey(mask.Id))  {
                    return;
                }

                EDM.modLog.Debug?.Write($"Applying additionalStickyEffects from {mask.Id}");
                foreach (EffectData effect in EDM.additionalStickyEffects[mask.Id]) {
                    __instance.CreateEffect(effect, null, __instance.GUID, stackItemUID, __instance);
                }
            } catch (Exception e) {
                EDM.modLog.Error?.Write(e);
            }
        }
    }

    // By default, Battletech only applies the stickyEffect from the occupiedDesignMask / PriorityDesignMask of each cell, which is never
    // the biomeDesignMask.

    // We don't run this through ApplyDesignMaskStickyEffect because - since we're the biome mask - it will decide to skip the stickyEffect, and
    // we want to apply that in addition to our extension (additionalStickyEffects).
    [HarmonyPatch(typeof(AbstractActor), "OnNewRound")]
    public static class AbstractActor_OnActivationBegin {
        public static void Prefix(AbstractActor __instance) {
            try {
                DesignMaskDef biomeDesignMask = UnityGameInstance.BattleTechGame.Combat.MapMetaData.biomeDesignMask;
                if (biomeDesignMask.stickyEffect == null)  {
                    return;
                }

                EDM.modLog.Debug?.Write($"OnNewRound: Applying stickyEffect(s) from {biomeDesignMask.Id}");

                __instance.CreateEffect(biomeDesignMask.stickyEffect, null, __instance.GUID, -1, __instance);
                if (EDM.additionalStickyEffects.ContainsKey(biomeDesignMask.Id)) {
                    foreach (EffectData effect in EDM.additionalStickyEffects[biomeDesignMask.Id]) {
                        __instance.CreateEffect(effect, null, __instance.GUID, -1, __instance);
                    }
                }
            } catch (Exception e) {
                EDM.modLog.Error?.Write(e);
            }
        }
    }
}
