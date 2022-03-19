using System;
using Harmony;
using BattleTech;
using BattleTech.Rendering.Mood;
using UnityEngine;

namespace EnvironmentalDesignMasks {
    [HarmonyPatch(typeof(WeatherController), "SetWeather")]
    public static class WeatherController_SetWeather {
        private static bool Prefix(WeatherController __instance) {
            try {
                    if (__instance.currentWeatherVFX != null && (__instance.weatherSettings.weatherVFXName == "" || __instance.currentWeatherVFX.name != __instance.weatherSettings.weatherVFXName)) {
                        __instance.currentWeatherVFX = null;
                        for (int i = 0; i < __instance.transform.childCount; i++) {
                            UnityEngine.Object.Destroy(__instance.transform.GetChild(i).gameObject);
                        }
                    }
                    if (__instance.weatherSettings.weatherVFXName != "" && __instance.currentWeatherVFX == null) {
                        if (Application.isPlaying && UnityGameInstance.BattleTechGame != null) {
                            __instance.currentWeatherVFX = UnityGameInstance.BattleTechGame.DataManager.PooledInstantiate(__instance.weatherSettings.weatherVFXName, BattleTechResourceType.Prefab);
                        }
                        if (__instance.currentWeatherVFX != null) {
                            __instance.currentWeatherVFX.name = __instance.weatherSettings.weatherVFXName;
                            __instance.currentWeatherVFX.hideFlags = HideFlags.DontSave;
                            __instance.currentWeatherVFX.transform.parent = __instance.transform;
                        }
                    }
                    switch (__instance.weatherSettings.weatherEffect) {
                    case WeatherController.WeatherEffect.Rain:
                        Shader.EnableKeyword("_OVERLAY_RAIN");
                        Shader.DisableKeyword("_OVERLAY_SNOW");
                        Shader.SetGlobalFloat("_WeatherAmount", __instance.weatherSettings.weatherEffectIntensity);
                        break;
                    case WeatherController.WeatherEffect.Snow:
                        Shader.DisableKeyword("_OVERLAY_RAIN");
                        Shader.EnableKeyword("_OVERLAY_SNOW");
                        Shader.SetGlobalFloat("_WeatherAmount", __instance.weatherSettings.weatherEffectIntensity);
                        break;
                    default:
                        Shader.DisableKeyword("_OVERLAY_RAIN");
                        Shader.DisableKeyword("_OVERLAY_SNOW");
                        break;
                    }
                return false;
            } catch (Exception e) {
                EDM.modLog.Error?.Write(e);
                return true;
            }
        }
    }
}
