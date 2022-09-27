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
                applySticky(__instance, biomeDesignMask, false);
                applySticky(__instance, __instance.occupiedDesignMask, true);
            } catch (Exception e) {
                EDM.modLog.Error?.Write(e);
            }
        }

        public static void applySticky(AbstractActor actor, DesignMaskDef mask, bool allowIgnore) {
            if (mask == null || mask.stickyEffect == null || (allowIgnore && !Utils.affectedByDesignMask(actor))) {
                return;
            }

            EffectManager em = UnityGameInstance.BattleTechGame.Combat.EffectManager;
            EDM.modLog.Debug?.Write($"OnNewRound: Applying stickyEffect(s) from {mask.Id} to {actor.GUID}");

            em.CreateEffect(mask.stickyEffect, mask.stickyEffect.Description.Id, -1, actor, actor, default(WeaponHitInfo), 1);
            if (EDM.additionalStickyEffects.ContainsKey(mask.Id)) {
                foreach (EffectData effect in EDM.additionalStickyEffects[mask.Id]) {
                    em.CreateEffect(effect, effect.Description.Id, -1, actor, actor, default(WeaponHitInfo), 1);
                }
            }
        }
    }
}
