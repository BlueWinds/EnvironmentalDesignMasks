using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleTech;
using BattleTech.Data;
using BattleTech.Rendering;
using BattleTech.Rendering.Mood;
using BattleTech.UI;
using Harmony;
using HBS;
using HBS.Collections;

namespace EnvironmentalDesignMasks
{
    [HarmonyPatch(typeof(Briefing), "Init")]
    public static class Briefing_Init
    {
        private static void Postfix(Briefing __instance, LoadingCamera ___loadCam)
        {
            try
            {
                var moodName = UnityGameInstance.BattleTechGame?.Combat?.ActiveContract?.mapMood;
                if (moodName == null) return;
                var moodTags = EDM.customMoods.ContainsKey(moodName) ? EDM.customMoods[moodName].moodTags : MetadataDatabase.Instance.GetMood(moodName).MoodSettings.moodTags;
                if (EDM.settings.briefingCameraTimeTags.Count <= 0 || moodTags.Count <= 0) return;
                foreach (var tagSetting in EDM.settings.briefingCameraTimeTags)
                {
                    if (!moodTags.Contains(tagSetting.Key)) continue;
                    var idx = tagSetting.Value.GetRandomElement();
                    for (var i = 0; i < ___loadCam.lights.Length; i++)
                    {
                        ___loadCam.lights[i].SetActive(false);
                        if (i == idx)___loadCam.lights[idx].SetActive(true);
                        EDM.modLog.Trace?.Write($"[Briefing.Init] found tag {tagSetting.Key}, setting lights at index {idx} active");
                    }
                    return;
                }
            }
            catch (Exception e)
            {
                EDM.modLog.Error?.Write(e);
            }
        }
    }
}
