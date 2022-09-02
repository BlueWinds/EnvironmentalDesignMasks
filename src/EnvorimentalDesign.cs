using UnityEngine;
using CustomVoices;
using BattleTech;
using Harmony;
using System;
using BattleTech.Rendering.Mood;
using BasicHandler;
using DigitalRuby.ThunderAndLightning;
using Random = UnityEngine.Random;
using BattleTech.Rendering;

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
  public class ThunderstormController: DefaultController {
    public SimpleTunderstormScript tunderstorm { get; set; } = null;
    public float SpawnIntervalMin = 5f;
    public float SpawnIntervalMax = 7f;
    public float Height = 1000f;
    public float Depth = 0f;
    protected float t;
    public bool Is01(float a) {
      return a > 0 && a < 1;
    }
    public override void Update() {
      base.Update();
      if (tunderstorm == null) { return; }
      t -= Time.deltaTime;
      if(t < Core.Epsilon) {
        t = next();
        Vector3 end = this.mainCamera.transform.position + this.mainCamera.transform.forward * 200f;
        end.y = this.Combat.MapMetaData.GetLerpedHeightAt(end);
        EDM.modLog.Info?.Write($"tunderstorm " + (tunderstorm == null ? "null" : "not null") + " generate lightning "+end);
        while (true) {
          Vector3 rndEnd = Vector3.zero;
          float dx = UnityEngine.Random.Range(-1000f, 1000f);
          float dz = UnityEngine.Random.Range(-1000f, 1000f);
          rndEnd.x = end.x + dx;
          rndEnd.z = end.z + dz;
          rndEnd.y = this.Combat.MapMetaData.GetLerpedHeightAt(rndEnd);
          Vector3 viewport = this.mainCamera.WorldToViewportPoint(rndEnd);
          bool inCameraFrustum = Is01(viewport.x) && Is01(viewport.y);
          bool inFrontOfCamera = viewport.z > 0;
          EDM.modLog.Info?.Write($" " + rndEnd+" viewport "+viewport+ " inCameraFrustum:"+ inCameraFrustum+ " inFrontOfCamera:"+ inFrontOfCamera);
          if (inCameraFrustum && inFrontOfCamera) { end = rndEnd; break; }
        };
        Vector3 start = end + Vector3.up * Height;
        end.y += Depth;
        tunderstorm?.spawnLightning(start,end);
        EDM.modLog.Info?.Write($"tunderstorm "+(tunderstorm == null?"null":"not null")+" start:"+start+" end:"+end);
      }
    }
    protected virtual float next() {
      return Random.Range(SpawnIntervalMin, SpawnIntervalMax);
    }
    public override void Init(ControllableWeather w, CustomMood m) {
      base.Init(w, m);
      this.tunderstorm = w as SimpleTunderstormScript;
      if (customMood == null) { goto end_init; }
      if (customMood.weatherSettings.thunderstormLightningSettings.HasValue == false) { goto end_init; }
      if (customMood.weatherSettings.thunderstormLightningSettings.Value.LightningSpawn.HasValue) {
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.LightningSpawn.Value.Minimum.HasValue) {
          this.SpawnIntervalMin = customMood.weatherSettings.thunderstormLightningSettings.Value.LightningSpawn.Value.Minimum.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.LightningSpawn.Value.Maximum.HasValue) {
          this.SpawnIntervalMax = customMood.weatherSettings.thunderstormLightningSettings.Value.LightningSpawn.Value.Maximum.Value;
        }
      }
      foreach (var lightning in this.tunderstorm.lightnings) {
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.JitterMultiplier.HasValue) {
          lightning.JitterMultiplier = customMood.weatherSettings.thunderstormLightningSettings.Value.JitterMultiplier.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.Turbulence.HasValue) {
          lightning.Turbulence = customMood.weatherSettings.thunderstormLightningSettings.Value.Turbulence.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.TurbulenceVelocity.HasValue) {
          lightning.TurbulenceVelocity = customMood.weatherSettings.thunderstormLightningSettings.Value.TurbulenceVelocity.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.IntensityFlicker.HasValue) {
          lightning.IntensityFlicker = customMood.weatherSettings.thunderstormLightningSettings.Value.IntensityFlicker.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.IntervalRange.HasValue) {
          if (customMood.weatherSettings.thunderstormLightningSettings.Value.IntervalRange.Value.Minimum.HasValue) {
            lightning.IntervalRange.Minimum = customMood.weatherSettings.thunderstormLightningSettings.Value.IntervalRange.Value.Minimum.Value;
          }
          if (customMood.weatherSettings.thunderstormLightningSettings.Value.IntervalRange.Value.Maximum.HasValue) {
            lightning.IntervalRange.Maximum = customMood.weatherSettings.thunderstormLightningSettings.Value.IntervalRange.Value.Maximum.Value;
          }
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.CountRange.HasValue) {
          if (customMood.weatherSettings.thunderstormLightningSettings.Value.CountRange.Value.Minimum.HasValue) {
            lightning.CountRange.Minimum = customMood.weatherSettings.thunderstormLightningSettings.Value.CountRange.Value.Minimum.Value;
          }
          if (customMood.weatherSettings.thunderstormLightningSettings.Value.CountRange.Value.Maximum.HasValue) {
            lightning.CountRange.Maximum = customMood.weatherSettings.thunderstormLightningSettings.Value.CountRange.Value.Maximum.Value;
          }
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.CountProbabilityModifier.HasValue) {
          lightning.CountProbabilityModifier = customMood.weatherSettings.thunderstormLightningSettings.Value.CountProbabilityModifier.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.DurationRange.HasValue) {
          if (customMood.weatherSettings.thunderstormLightningSettings.Value.DurationRange.Value.Minimum.HasValue) {
            lightning.DurationRange.Minimum = customMood.weatherSettings.thunderstormLightningSettings.Value.DurationRange.Value.Minimum.Value;
          }
          if (customMood.weatherSettings.thunderstormLightningSettings.Value.DurationRange.Value.Maximum.HasValue) {
            lightning.DurationRange.Maximum = customMood.weatherSettings.thunderstormLightningSettings.Value.DurationRange.Value.Maximum.Value;
          }
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.TrunkWidthRange.HasValue) {
          if (customMood.weatherSettings.thunderstormLightningSettings.Value.TrunkWidthRange.Value.Minimum.HasValue) {
            lightning.TrunkWidthRange.Minimum = customMood.weatherSettings.thunderstormLightningSettings.Value.TrunkWidthRange.Value.Minimum.Value;
          }
          if (customMood.weatherSettings.thunderstormLightningSettings.Value.TrunkWidthRange.Value.Maximum.HasValue) {
            lightning.TrunkWidthRange.Maximum = customMood.weatherSettings.thunderstormLightningSettings.Value.TrunkWidthRange.Value.Maximum.Value;
          }
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.LifeTime.HasValue) {
          lightning.LifeTime = customMood.weatherSettings.thunderstormLightningSettings.Value.LifeTime.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.Generations.HasValue) {
          lightning.Generations = customMood.weatherSettings.thunderstormLightningSettings.Value.Generations.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.ChaosFactor.HasValue) {
          lightning.ChaosFactor = customMood.weatherSettings.thunderstormLightningSettings.Value.ChaosFactor.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.ChaosFactorForks.HasValue) {
          lightning.ChaosFactorForks = customMood.weatherSettings.thunderstormLightningSettings.Value.ChaosFactorForks.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.Intensity.HasValue) {
          lightning.Intensity = customMood.weatherSettings.thunderstormLightningSettings.Value.Intensity.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.GlowIntensity.HasValue) {
          lightning.GlowIntensity = customMood.weatherSettings.thunderstormLightningSettings.Value.GlowIntensity.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.GlowWidthMultiplier.HasValue) {
          lightning.GlowWidthMultiplier = customMood.weatherSettings.thunderstormLightningSettings.Value.GlowWidthMultiplier.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.FadePercent.HasValue) {
          lightning.FadePercent = customMood.weatherSettings.thunderstormLightningSettings.Value.FadePercent.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.FadeInMultiplier.HasValue) {
          lightning.FadeInMultiplier = customMood.weatherSettings.thunderstormLightningSettings.Value.FadeInMultiplier.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.FadeFullyLitMultiplier.HasValue) {
          lightning.FadeFullyLitMultiplier = customMood.weatherSettings.thunderstormLightningSettings.Value.FadeFullyLitMultiplier.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.FadeOutMultiplier.HasValue) {
          lightning.FadeOutMultiplier = customMood.weatherSettings.thunderstormLightningSettings.Value.FadeOutMultiplier.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.GrowthMultiplier.HasValue) {
          lightning.GrowthMultiplier = customMood.weatherSettings.thunderstormLightningSettings.Value.GrowthMultiplier.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.EndWidthMultiplier.HasValue) {
          lightning.EndWidthMultiplier = customMood.weatherSettings.thunderstormLightningSettings.Value.EndWidthMultiplier.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.Forkedness.HasValue) {
          lightning.Forkedness = customMood.weatherSettings.thunderstormLightningSettings.Value.Forkedness.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.ForkLengthMultiplier.HasValue) {
          lightning.ForkLengthMultiplier = customMood.weatherSettings.thunderstormLightningSettings.Value.ForkLengthMultiplier.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.ForkLengthVariance.HasValue) {
          lightning.ForkLengthVariance = customMood.weatherSettings.thunderstormLightningSettings.Value.ForkLengthVariance.Value;
        }
        if (customMood.weatherSettings.thunderstormLightningSettings.Value.ForkEndWidthMultiplier.HasValue) {
          lightning.ForkEndWidthMultiplier = customMood.weatherSettings.thunderstormLightningSettings.Value.ForkEndWidthMultiplier.Value;
        }
      }
      if (customMood.weatherSettings.thunderstormLightningSettings.Value.ThunderSFX != null) {
        this.tunderstorm.thunderSFX = customMood.weatherSettings.thunderstormLightningSettings.Value.ThunderSFX;
      }
      if (customMood.weatherSettings.thunderstormLightningSettings.Value.LightningsHeight.HasValue) {
        this.Height = customMood.weatherSettings.thunderstormLightningSettings.Value.LightningsHeight.Value;
      }
      if (customMood.weatherSettings.thunderstormLightningSettings.Value.LightningsDepth.HasValue) {
        this.Depth = customMood.weatherSettings.thunderstormLightningSettings.Value.LightningsDepth.Value;
      }
    end_init:
      t = next();
    }
  }
  public class CameraFollowController : DefaultController {
    public override void Update() {
      base.Update();
      this.weather.transform.position = this.mainCamera.transform.position;
      this.weather.transform.rotation = this.mainCamera.transform.rotation;
    }
    public CameraFollowController() : base() { }
  }
  public class DefaultController: BasicControllerWeather {
    private Camera _mainCamera;
    public ControllableWeather weather { get; set; } = null;
    public CustomMood customMood { get; set; } = null;
    public virtual Camera mainCamera {
      get {
        if ((UnityEngine.Object)this._mainCamera == (UnityEngine.Object)null || !this._mainCamera.gameObject.activeInHierarchy)
          this._mainCamera = Camera.main;
        return this._mainCamera;
      }
    }
    public virtual CombatGameState Combat {
      get {
        return UnityGameInstance.BattleTechGame.Combat;
      }
    }
    public override void Update() {
    }
    public override void PlaySFX(string sfx) {
      EDM.modLog.Info?.Write("Play weather sfx "+sfx);
      AudioEngine.Instance?.AmbientBus?.Play(sfx, false);
    }
    public override void StopSFX(string sfx) {
    }
    public override void Debug(string message) {
      EDM.modLog.Debug?.Write(message);
    }
    public override void Info(string message) {
      EDM.modLog.Info?.Write(message);
    }
    public override void Error(string message) {
      EDM.modLog.Error?.Write(message);
    }
    public DefaultController() {
    }
    public virtual void Init(ControllableWeather w, CustomMood m) {
      this.weather = w;
      this.customMood = m;
    }
  }
  public class BTWeatherLight: WeatherLight {
    public void SetLight(BTLight l) { this.light = l; }
    protected BTLight light { get; set; }
    public override LightShadows shadows { get { return light.castShadows?LightShadows.Soft:LightShadows.None; } set { light.castShadows = value != LightShadows.None;  } }
    public override LightType type {
      get {
        switch(light.lightType){
          case BTLight.LightTypes.Area: return LightType.Area;
          case BTLight.LightTypes.Linear: return LightType.Directional;
          case BTLight.LightTypes.Point: return LightType.Point;
          case BTLight.LightTypes.Spot: return LightType.Spot;
          default: return LightType.Spot;
        }
      } set {
        switch (value) {
          case LightType.Area: light.lightType = BTLight.LightTypes.Area; break;
          case LightType.Directional: light.lightType = BTLight.LightTypes.Linear; break;
          case LightType.Point: light.lightType = BTLight.LightTypes.Point; break;
          case LightType.Spot: light.lightType = BTLight.LightTypes.Spot; break;
          default: light.lightType = BTLight.LightTypes.Spot; break;
        }
      }
    }
    //public override float bounceIntensity { get { return light.bounceIntensity; } set { light.bounceIntensity = value; } }
    //public override float shadowNormalBias { get { return light.shadowBias; } set { light.shadowBias = value; } }
    public override Color color { get { return light.lightColor; } set { light.lightColor = value; } }
    public override LightRenderMode renderMode { get { return LightRenderMode.Auto; } set {  } }
    public override float range { get { return light.radius; } set { light.radius = value; } }
    //public override float shadowStrength { get { return light.shadowStrength; } set { light.shadowStrength = value; } }
    public override float shadowBias { get { return light.shadowBias; } set { light.shadowBias = value; } }
    public override float intensity { get { return light.intensity; } set { light.intensity = value; } }
    public static new WeatherLight create(GameObject obj) {
      BTWeatherLight result = obj.AddComponent<BTWeatherLight>();
      result.SetLight(obj.AddComponent<BTLight>());
      return result;
    }
  }
  public class CustomWeatherController : MonoBehaviour {
    public CustomMood currentCustomMood { get; set; } = null;
    public BasicHandler.ControllableWeather additionalWeatherVFX { get; set; } = null;
    public static BasicControllerWeather createController(string name, ControllableWeather component, CustomMood mood) {
      DefaultController result = null;
      if (name == "Thunderstorm") {
        result = component.gameObject.GetComponent<ThunderstormController>();
        if (result == null) {
          ThunderstormController thunderstorm = component.gameObject.AddComponent<ThunderstormController>();
          result = thunderstorm;
        }
      }else if (name == "Default") {
        result = component.gameObject.GetComponent<DefaultController>();
        if (result == null) {
          result = component.gameObject.AddComponent<DefaultController>();
        }
      } else if (name == "CameraFollow") {
        result = component.gameObject.GetComponent<CameraFollowController>();
        if (result == null) {
          result = component.gameObject.AddComponent<CameraFollowController>();
        }
      } else {
        EDM.modLog.Error?.Write($"unknown additional weather controller type {name}");
        result = component.gameObject.GetComponent<DefaultController>();
        if (result == null) {
          result = component.gameObject.AddComponent<DefaultController>();
        }
      }
      result?.Init(component, mood);
      result?.gameObject.SetActive(false);
      result?.gameObject.SetActive(true);
      return result;
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