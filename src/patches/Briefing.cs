using System;
using BattleTech;
using BattleTech.Data;
using BattleTech.Rendering;
using BattleTech.UI;
using Harmony;

namespace EnvironmentalDesignMasks {
    [HarmonyPatch(typeof(Briefing), "Init")]
    public static class Briefing_Init {
        private static void Postfix(Briefing __instance, LoadingCamera ___loadCam) {
            try {
                string moodName = UnityGameInstance.BattleTechGame?.Combat?.ActiveContract?.mapMood;
                if (moodName == null) {
                    return;
                }
                TagSet moodTags = EDM.customMoods.ContainsKey(moodName) ? EDM.customMoods[moodName].moodTags : MetadataDatabase.Instance.GetMood(moodName).MoodSettings.moodTags;
                if (EDM.settings.briefingCameraTimeTags.Count <= 0 || moodTags.Count <= 0) {
                    return;
                }
                foreach (int tagSetting in EDM.settings.briefingCameraTimeTags) {
                    if (!moodTags.Contains(tagSetting.Key)) {
                        continue;
                    }

                    int idx = tagSetting.Value.GetRandomElement();
                    for (int i = 0; i < ___loadCam.lights.Length; i++) {
                        ___loadCam.lights[idx].SetActive(i == idx);
                        EDM.modLog.Debug?.Write($"[Briefing.Init] found tag {tagSetting.Key}, setting lights at index {idx} active");
                    }
                    return;
                }
            }
            catch (Exception e) {
                EDM.modLog.Error?.Write(e);
            }
        }
    }
}
