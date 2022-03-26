using System;
using Harmony;
using BattleTech;
using BattleTech.Rendering.Mood;

namespace EnvironmentalDesignMasks {
    [HarmonyPatch(typeof(MoodController), "SetMoodByName")]
    public static class MoodController_SetMoodByName {
        private static bool Prefix(MoodController __instance, string moodName, bool applyMood = true) {
            try {
                if (EDM.customMoods.ContainsKey(moodName)) {
                    EDM.modLog.Info?.Write($"[SetMoodByName] Setting custom mood {moodName}.");
                    __instance.currentMood = EDM.customMoods[moodName].MoodSettings;

                    if (applyMood) {
                        __instance.ApplyMood(refreshSky: true, refreshMusic: false);
                    }

                    return false;
                }
            } catch (Exception e) {
                EDM.modLog.Error?.Write(e);
            }
            return true;
        }
    }
}
