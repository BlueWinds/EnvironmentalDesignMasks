using System;
using System.Collections.Generic;
using Harmony;
using BattleTech.Data;
using HBS.Collections;

namespace EnvironmentalDesignMasks {
    [HarmonyPatch(typeof(TagSetQueryExtensions), "GetMatchingMoods")]
    public static class TagSetQueryExtensions_GetMatchingMoods {
        private static void Postfix(TagSet requiredTags, TagSet excludedTags, List<Mood_MDD> __result) {
            try {
                EDM.modLog.Debug?.Write($"[GetMatchingMoods] Mood requested with requiredTags {(requiredTags == null ? "(empty)" : requiredTags.ToString())} and excludedTags {(excludedTags == null ? "(empty)" : excludedTags.ToString())}");
                foreach (CustomMood mood in EDM.customMoods.Values) {
                    if (mood.moodTags.ContainsAll(requiredTags) && ! mood.moodTags.ContainsAny(excludedTags, false)) {
                        EDM.modLog.Debug?.Write($"[GetMatchingMoods] Adding {mood.ID}");
                        __result.Add(mood.Mood_MDD);
                    }
                }
            } catch (Exception e) {
                EDM.modLog.Error?.Write(e);
            }
        }
    }
}

