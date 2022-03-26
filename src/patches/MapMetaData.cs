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
                string mood = UnityGameInstance.BattleTechGame.Combat.ActiveContract.mapMood;
                if (EDM.customMoods.ContainsKey(mood) && EDM.customMoods[mood].designMask != null) {
                    string designMask = EDM.customMoods[mood].designMask;
                    Traverse biomeDesignMaskName = Traverse.Create(__instance).Field("biomeDesignMaskName");

                    EDM.modLog.Info?.Write($"Overriding biomeDesignMaskName from {biomeDesignMaskName.GetValue()} to {designMask} based on mood {mood}");
                    EDM.modLog.Info?.Write($"Adding loadRequest for DesignMaskDef: {designMask}");

                    biomeDesignMaskName.SetValue(designMask);

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
