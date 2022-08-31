using System;
using System.Reflection;
using Harmony;
using BattleTech;
using BattleTech.Rendering.Mood;
using UnityEngine;

namespace EnvironmentalDesignMasks {
  [HarmonyPatch(typeof(WeatherController), "Update")]
  public static class WeatherController_Update {
    public static float t = 0f;
    public static void Postfix(WeatherController __instance) {
      t += Time.deltaTime;
      if (t > 1f) {
        t = 0f;
        if (__instance.currentWeatherVFX.activeSelf == false) {
          CustomWeatherController customWeatherController = __instance.gameObject.GetComponent<CustomWeatherController>();
          if (customWeatherController != null) {
            if(customWeatherController.currentCustomMood != null) {
              if (customWeatherController.currentCustomMood.weatherSettings.forceWeatherVFXEnable.Value) {
                __instance.currentWeatherVFX.gameObject.SetActive(true);
                Transform[] transforms = __instance.currentWeatherVFX.GetComponentsInChildren<Transform>(true);
                foreach (Transform tr in transforms) {
                  EDM.modLog.Info?.Write($" {tr.gameObject.name} state: {tr.gameObject.activeSelf}");
                  tr.gameObject.SetActive(true);
                  EDM.modLog.Info?.Write($" {tr.gameObject.name} state: {tr.gameObject.activeSelf}");
                }
              }
            }
          }
        }
      }
    }
  }
  [HarmonyPatch(typeof(WeatherController), "SetWeather")]
  public static class WeatherController_SetWeather {
    public static bool Prefix(WeatherController __instance) {
      try {
        EDM.modLog.Info?.Write($"SetWeather: {__instance.weatherSettings.weatherVFXName}");
        CustomWeatherController customWeatherController = __instance.gameObject.GetComponent<CustomWeatherController>();
        if (customWeatherController != null) {
          if (customWeatherController.additionalWeatherVFX != null) {
            if (customWeatherController.currentCustomMood == null) {
              UnityGameInstance.BattleTechGame.DataManager.PoolGameObject(customWeatherController.additionalWeatherVFX.gameObject.name, customWeatherController.additionalWeatherVFX.transform.parent.gameObject);
              customWeatherController.additionalWeatherVFX = null;
            } else if (customWeatherController.currentCustomMood.weatherSettings.weatherVFXAdditionalName != customWeatherController.additionalWeatherVFX.gameObject.name) {
              UnityGameInstance.BattleTechGame.DataManager.PoolGameObject(customWeatherController.additionalWeatherVFX.gameObject.name, customWeatherController.additionalWeatherVFX.transform.parent.gameObject);
              customWeatherController.additionalWeatherVFX = null;
            }
          }
        }
        if (__instance.currentWeatherVFX != null && (__instance.weatherSettings.weatherVFXName == "" || __instance.currentWeatherVFX.name != __instance.weatherSettings.weatherVFXName)) {
          __instance.currentWeatherVFX = null;
          if (customWeatherController != null) {
            if (customWeatherController.additionalWeatherVFX != null) {
              UnityGameInstance.BattleTechGame.DataManager.PoolGameObject(customWeatherController.additionalWeatherVFX.transform.parent.gameObject.name, customWeatherController.additionalWeatherVFX.gameObject);
            }
            customWeatherController.additionalWeatherVFX = null;
          }
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
            //Transform[] transforms = __instance.currentWeatherVFX.GetComponentsInChildren<Transform>(true);
            //foreach (Transform tr in transforms) { tr.gameObject.SetActive(true); }
          }
        }
        if ((customWeatherController != null) && Application.isPlaying && (UnityGameInstance.BattleTechGame != null)) {
          if ((customWeatherController.currentCustomMood != null) && (customWeatherController.additionalWeatherVFX == null)) {
            if (string.IsNullOrEmpty(customWeatherController.currentCustomMood.weatherSettings.weatherVFXAdditionalName) == false) {
              EDM.modLog.Info?.Write($" Instantiate {customWeatherController.currentCustomMood.weatherSettings.weatherVFXAdditionalName}");

              GameObject go = UnityGameInstance.BattleTechGame.DataManager.PooledInstantiate(customWeatherController.currentCustomMood.weatherSettings.weatherVFXAdditionalName, BattleTechResourceType.Prefab);
              if (go != null) {
                EDM.modLog.Info?.Write(" searching for ControllableWeather");
                BasicHandler.ControllableWeather[] controllableWeathers = go.GetComponentsInChildren<BasicHandler.ControllableWeather>();
                foreach (var controllableWeather in controllableWeathers) {
                  EDM.modLog.Info?.Write("  "+ controllableWeather.transform.name);
                  if (controllableWeather.transform.parent == go.transform) { customWeatherController.additionalWeatherVFX = controllableWeather; break; }
                }
                if ((customWeatherController.additionalWeatherVFX == null) && (controllableWeathers.Length > 0)) {
                  customWeatherController.additionalWeatherVFX = controllableWeathers[0];
                }
                if (customWeatherController.additionalWeatherVFX == null) {
                  UnityGameInstance.BattleTechGame.DataManager.PoolGameObject(customWeatherController.currentCustomMood.weatherSettings.weatherVFXAdditionalName, go);
                  //go.name = customWeatherController.currentCustomMood.weatherSettings.weatherVFXAdditionalName;
                  //go.hideFlags = HideFlags.DontSave;
                  //go.transform.parent = __instance.transform;
                } else {
                  EDM.modLog.Info?.Write($" result found {customWeatherController.additionalWeatherVFX.transform.name}");
                  go.name = customWeatherController.currentCustomMood.weatherSettings.weatherVFXAdditionalName;
                  go.hideFlags = HideFlags.DontSave;
                  go.transform.parent = __instance.transform;
                  customWeatherController.additionalWeatherVFX.basicController = CustomWeatherController.createController(customWeatherController.currentCustomMood.weatherSettings.weatherVFXAdditionalType, customWeatherController.additionalWeatherVFX, customWeatherController.currentCustomMood);
                }
              }
            }
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

          // blizzard weather comes with the snow particles disabled. #HBSWhy
          ParticleSystemRenderer snow = __instance.currentWeatherVFX.GetComponent<ParticleSystemRenderer>();
          snow.enabled = true;

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
