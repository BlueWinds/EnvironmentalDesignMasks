using System;
using System.Collections.Generic;
using Harmony;
using BattleTech;
using BattleTech.Data;
using HBS.Util;

namespace EnvironmentalDesignMasks {
    [HarmonyPatch(typeof(MapMetaData), "Load", new Type[] { typeof(SerializationStream) })]
    public static class MapMetaData_Load {
        private static void Postfix(MapMetaData __instance) {
            try {
                // CustomUnit's map preview functionality can cause map metadata to be loaded before Combat exists
                if (UnityGameInstance.BattleTechGame.Combat == null) { return; }

                string mood = UnityGameInstance.BattleTechGame.Combat.ActiveContract.mapMood;
                if (EDM.customMoods.ContainsKey(mood) && EDM.customMoods[mood].designMask != null) {
                    string designMask = EDM.customMoods[mood].designMask;
                    EDM.modLog.Info?.Write($"Overriding biomeDesignMaskName from {__instance.biomeDesignMaskName} to {designMask} based on mood {mood}");
                    EDM.modLog.Info?.Write($"Adding loadRequest for DesignMaskDef: {designMask}");

                    __instance.biomeDesignMaskName = designMask;

                    LoadRequest loadRequest = UnityGameInstance.BattleTechGame.DataManager.CreateLoadRequest();
                    loadRequest.AddBlindLoadRequest(BattleTechResourceType.DesignMaskDef, designMask);
                    loadRequest.ProcessRequests(1000U);
                }
            } catch (Exception e) {
                EDM.modLog.Error?.Write(e);
            }
        }
    }
}
