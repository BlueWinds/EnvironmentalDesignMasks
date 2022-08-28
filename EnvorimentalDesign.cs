using UnityEngine;
using CustomVoices;
using BattleTech;
using Harmony;
using System;
using BattleTech.Rendering.Mood;

namespace EnvironmentalDesignMasks {
  [HarmonyPatch(typeof(CombatGameState))]
  [HarmonyPatch("OnCombatGameDestroyed")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { })]
  public static class CombatGameState_OnCombatGameDestroyed {
    public static bool Prefix(CombatGameState __instance) {
      try {
        if (MoodController.HasInstance) {
          CustomMoodController customMoodController = MoodController.Instance.gameObject.GetComponent<CustomMoodController>();
          if(customMoodController != null) {
            if(customMoodController.customAmbienceSFX != null) {
              customMoodController.customAmbienceSFX.Stop();
              customMoodController.customAmbienceSFX = null;
            }
            if (customMoodController.customWeatherSFX != null) {
              customMoodController.customWeatherSFX.Stop();
              customMoodController.customWeatherSFX = null;
            }
          }
        }
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString());
      }
      return true;
    }
    public static void Postfix(CombatGameState __instance) {
    }
  }

  public class CustomMoodController: MonoBehaviour {
    public CustomMood currentCustomMood { get; set; } = null;
    public AudioEngine.AudioChannel customAmbienceSFX { get; set; } = null;
    public AudioEngine.AudioChannel customWeatherSFX { get; set; } = null;
    public void OnDestroy() {
      if (this.customAmbienceSFX != null) {
        this.customAmbienceSFX.Stop();
        this.customAmbienceSFX = null;
      }
      if (this.customWeatherSFX != null) {
        this.customWeatherSFX.Stop();
        this.customWeatherSFX = null;
      }
    }
  }
}