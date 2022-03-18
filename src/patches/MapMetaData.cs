using System;
using System.Collections.Generic;
using Harmony;
using BattleTech;
using HBS.Util;

namespace EnvironmentalDesignMasks {
    [HarmonyPatch(typeof(MapMetaData), "Load", new Type[] { typeof(SerializationStream) })]
    public static class MapMetaData_Load {
        private static void Postfix(MapMetaData __instance) {
            try {
                string mood = UnityGameInstance.BattleTechGame.Combat.ActiveContract.mapMood;
                if (EDM.customMoods.ContainsKey(mood) && EDM.customMoods[mood].designMask != null) {
                    string designMask = EDM.customMoods[mood].designMask;
                    Traverse biomeDesignMaskName = Traverse.Create(__instance).Field("biomeDesignMaskName");

                    EDM.modLog.Info?.Write($"Overriding biomeDesignMaskName from {biomeDesignMaskName.GetValue()} to {designMask} based on mood {mood}");

                    biomeDesignMaskName.SetValue(designMask);
                }
            } catch (Exception e) {
                EDM.modLog.Error?.Write(e);
            }
        }
    }
}
