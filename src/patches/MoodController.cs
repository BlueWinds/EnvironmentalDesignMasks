using System;
using Harmony;
using BattleTech;
using BattleTech.Rendering.Mood;
using CustomVoices;

namespace EnvironmentalDesignMasks {
    [HarmonyPatch(typeof(MoodController), "SetMoodByName")]
    public static class MoodController_SetMoodByName {
        private static bool Prefix(MoodController __instance, string moodName, bool applyMood = true) {
            try {
                if (EDM.customMoods.TryGetValue(moodName, out var customMood)) {
                    EDM.modLog.Info?.Write($"[SetMoodByName] Setting custom mood {moodName}.");
                    __instance.currentMood = customMood.MoodSettings;

                    CustomMoodController customMoodController = __instance.gameObject.GetComponent<CustomMoodController>();
                    if(customMoodController == null) { customMoodController = __instance.gameObject.AddComponent<CustomMoodController>(); }
                    CustomWeatherController customWeatherController = __instance.weatherController.gameObject.GetComponent<CustomWeatherController>();
                    if(customWeatherController == null) { customWeatherController = __instance.weatherController.gameObject.AddComponent<CustomWeatherController>(); }
                    customMoodController.currentCustomMood = customMood;
                    customWeatherController.currentCustomMood = customMood;
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
  [HarmonyPatch(typeof(MoodController), "RefreshAmbienceAudio")]
  public static class MoodController_RefreshAmbienceAudio {
    private static bool Prefix(MoodController __instance, ContractTypeValue contractTypeValue) {
      try {
        
        CustomMoodController customMoodController = __instance.gameObject.GetComponent<CustomMoodController>();
        if (customMoodController == null) {
          EDM.modLog.Info?.Write($"[RefreshAmbienceAudio] no CustomMoodController");
          return true;
        }
        if (customMoodController.currentCustomMood == null) {
          EDM.modLog.Info?.Write($"[RefreshAmbienceAudio] no currentCustomMood");
          return true;
        }
        if (string.IsNullOrEmpty(customMoodController.currentCustomMood.AmbienceAudio)) {
          EDM.modLog.Info?.Write($"[RefreshAmbienceAudio] no AmbienceAudio");
          return true;
        }
        if (customMoodController.customAmbienceSFX != null) {
          customMoodController.customAmbienceSFX.Stop();
          customMoodController.customAmbienceSFX = null;
        }
        EDM.modLog.Info?.Write($"[RefreshAmbienceAudio] playing "+ customMoodController.currentCustomMood.AmbienceAudio);
        customMoodController.customAmbienceSFX = AudioEngine.Instance?.AmbientBus?.Play(customMoodController.currentCustomMood.AmbienceAudio, true);
        return false;
      } catch (Exception e) {
        EDM.modLog.Error?.Write(e);
      }
      return true;
    }
  }
  [HarmonyPatch(typeof(MoodController), "RefreshWeatherAudio")]
  public static class MoodController_RefreshWeatherAudio {
    private static bool Prefix(MoodController __instance, ContractTypeValue contractTypeValue) {
      try {
        CustomMoodController customMoodController = __instance.gameObject.GetComponent<CustomMoodController>();
        if (customMoodController == null) {
          EDM.modLog.Info?.Write($"[RefreshWeatherAudio] no CustomMoodController");
          return true;
        }
        if (customMoodController.currentCustomMood == null) {
          EDM.modLog.Info?.Write($"[RefreshWeatherAudio] no currentCustomMood");
          return true;
        }
        if (string.IsNullOrEmpty(customMoodController.currentCustomMood.WeatherAudio)) {
          EDM.modLog.Info?.Write($"[RefreshWeatherAudio] no WeatherAudio");
          return true;
        }
        if (customMoodController.customWeatherSFX != null) {
          customMoodController.customWeatherSFX.Stop();
          customMoodController.customWeatherSFX = null;
        }
        EDM.modLog.Info?.Write($"[RefreshWeatherAudio] playing " + customMoodController.currentCustomMood.WeatherAudio);
        customMoodController.customWeatherSFX = AudioEngine.Instance?.AmbientBus?.Play(customMoodController.currentCustomMood.WeatherAudio, true);
      } catch (Exception e) {
        EDM.modLog.Error?.Write(e);
      }
      return true;
    }
  }
}
