using System;
using BattleTech.UI;
using UnityEngine;
using Harmony;

namespace EnvironmentalDesignMasks {
    [HarmonyPatch(typeof(UIManager), "Awake")]
    public static class UIManager_Awake {
        public static void Postfix(UIManager __instance) {
            try {
                if (EDM.settings.debugServer && __instance.gameObject.GetComponent<DebugServer>() == null) {
                    DebugServer server = __instance.gameObject.AddComponent<DebugServer>();
                }
            } catch (Exception e) {
                EDM.modLog.Error?.Write(e);
            }
        }
    }
}
