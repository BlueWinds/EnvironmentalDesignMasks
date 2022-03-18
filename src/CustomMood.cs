using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using BattleTech;
using BattleTech.Data;
using BattleTech.Rendering;
using BattleTech.Rendering.Mood;
using HBS.Collections;
using Newtonsoft.Json;
using UnityEngine;
using Harmony;

namespace EnvironmentalDesignMasks {
    public struct SunlightJson {
      public float? illuminance;
      public float? angularDiameter;
      public bool? useTemperature;
      public float? colorTemperature;
      public Color? sunColor;
      public float? cloudCover;
      public float? cloudOpacity;
      public int? cloudSoftness;
      public Color? flareTint;
      public bool? sunGIOverride;
      public float? sunGI;
      public Color? sundiscColor;
      public bool? nightLights;
    }

    public struct WeatherJson {
      public float? windDirection;
      public float? windMain;
      public float? windPulseFrequency;
      public float? windPulseMagnitude;
      public float? windTurbulence;
      public WeatherController.WeatherEffect? weatherEffect;
      public float? weatherEffectIntensity;
    }

    public struct SkyJson {
      public float? rayleighMultiplier;
      public float? mieMultiplier;
      public float? ozonePercent;
      public float? g;
      public Color? skyTint;
      public Color? mieTint;
      public Color? skyBoost;
      public float? starIntensity;
      public bool? martian;
      public float? skyGIIntensity;
      public float? reflectionIntensity;
    }

    public struct FogJson {
      public Color? fogTintColor;
      public float? fogMieMultiplier;
      public float? fogRayleighMultiplier;
      public float? fogG;
      public float? heightFogStart;
      public float? heightFogDensity;
      public float? heightMieMultiplier;
      public float? heightRayleighMultiplier;
      public float? surveyedMieMultiplier;
      public float? surveyedIntensity;
      public float? revealedMieMultiplier;
      public float? revealedIntensity;
    }

    public struct TonemapJson {
      public float? EV;
      public BTPostProcess.ColorTemps? tempPreset;
      public float? whiteBalanceTemp;
      public float? whiteBalanceTint;
    }

    public struct BloomJson {
      public float? threshold;
      public float? softKnee;
      public float? bloomRadius;
      public float? bloomIntensity;
      public float? thresholdLinear;
      public BTPostProcess.BloomSmear? bloomSmear;
      public float? bloomFlareIntensity;
      public Color[] bloomFlareColors;
    }

    public struct MechJson {
      public float? brightness;
      public float? contrast;
      public float? saturation;
    }
    public struct FlareJson {
      public BTPostProcess.FlareStreakMode? streakMode;
    }

    public class CustomMood {
        // The finalized MoodSettings usable by HBS BT
        public MoodSettings MoodSettings;
        public Mood_MDD Mood_MDD;
        public string designMask;
        public DialogueContent[] startMission;

        public string Name;
        public string baseMood;
        public TagSet moodTags;
        public float? sunXRotation;
        public float? sunYRotation;
        public SunlightJson sunlight;
        public WeatherJson weatherSettings;
        public SkyJson skySettings;
        public FogJson fogSettings;
        public TonemapJson tonemapSettings;
        public BloomJson bloomSettings;
        public MechJson mechSettings;
        public FlareJson flareSettings;

        public static CustomMood fromFile(string path) {
            CustomMood m;
            using (StreamReader reader = new StreamReader(path)) {
                string jdata = reader.ReadToEnd();
                m = JsonConvert.DeserializeObject<CustomMood>(jdata);
            }

            MoodSettings parent = EDM.customMoods.ContainsKey(m.baseMood) ? EDM.customMoods[m.baseMood].MoodSettings : MoodSettings.LoadByName(m.baseMood);

            if (parent == null) {
                throw new Exception("Couldn't load baseMood '{m.baseMood}'");
            }

            MoodSettings s = m.MoodSettings = new MoodSettings();
            s.uniqueFriendlyName = m.Name;
            s.sunXRotation = m.sunXRotation.GetValueOrDefault(parent.sunXRotation);
            s.sunYRotation = m.sunYRotation.GetValueOrDefault(parent.sunYRotation);
            s.reflectionMap = parent.reflectionMap;
            s.moodTags = new TagSet(BattletechConstants.MoodTags);
            s.moodTags.AddRange(m.moodTags);

            s.sunlight.illuminance = m.sunlight.illuminance.GetValueOrDefault(parent.sunlight.illuminance);
            s.sunlight.angularDiameter = m.sunlight.angularDiameter.GetValueOrDefault(parent.sunlight.angularDiameter);
            s.sunlight.useTemperature = m.sunlight.useTemperature.GetValueOrDefault(parent.sunlight.useTemperature);
            s.sunlight.colorTemperature = m.sunlight.colorTemperature.GetValueOrDefault(parent.sunlight.colorTemperature);
            s.sunlight.cloudCover = m.sunlight.cloudCover.GetValueOrDefault(parent.sunlight.cloudCover);
            s.sunlight.cloudOpacity = m.sunlight.cloudOpacity.GetValueOrDefault(parent.sunlight.cloudOpacity);
            s.sunlight.cloudSoftness = m.sunlight.cloudSoftness.GetValueOrDefault(parent.sunlight.cloudSoftness);
            s.sunlight.flareTint = m.sunlight.flareTint.GetValueOrDefault(parent.sunlight.flareTint);
            s.sunlight.sunGIOverride = m.sunlight.sunGIOverride.GetValueOrDefault(parent.sunlight.sunGIOverride);
            s.sunlight.sunGI = m.sunlight.sunGI.GetValueOrDefault(parent.sunlight.sunGI);
            s.sunlight.sunColor = m.sunlight.sunColor.GetValueOrDefault(parent.sunlight.sunColor);
            s.sunlight.sundiscColor = m.sunlight.sundiscColor.GetValueOrDefault(parent.sunlight.sundiscColor);
            s.sunlight.nightLights = m.sunlight.nightLights.GetValueOrDefault(parent.sunlight.nightLights);
            s.sunlight.castShadows = parent.sunlight.castShadows;
            s.sunlight.starPreset = parent.sunlight.starPreset;

            s.weatherSettings.windDirection = m.weatherSettings.windDirection.GetValueOrDefault(parent.weatherSettings.windDirection);
            s.weatherSettings.windMain = m.weatherSettings.windMain.GetValueOrDefault(parent.weatherSettings.windMain);
            s.weatherSettings.windPulseFrequency = m.weatherSettings.windPulseFrequency.GetValueOrDefault(parent.weatherSettings.windPulseFrequency);
            s.weatherSettings.windPulseMagnitude = m.weatherSettings.windPulseMagnitude.GetValueOrDefault(parent.weatherSettings.windPulseMagnitude);
            s.weatherSettings.windTurbulence = m.weatherSettings.windTurbulence.GetValueOrDefault(parent.weatherSettings.windTurbulence);
            s.weatherSettings.weatherEffect = m.weatherSettings.weatherEffect.GetValueOrDefault(parent.weatherSettings.weatherEffect);
            s.weatherSettings.weatherEffectIntensity = m.weatherSettings.weatherEffectIntensity.GetValueOrDefault(parent.weatherSettings.weatherEffectIntensity);
            s.weatherSettings.weatherCameraVFX = parent.weatherSettings.weatherCameraVFX;
            s.weatherSettings.weatherWorldVFX = parent.weatherSettings.weatherWorldVFX;
            s.weatherSettings.RefreshName();

            s.skySettings.rayleighMultiplier = m.skySettings.rayleighMultiplier.GetValueOrDefault(parent.skySettings.rayleighMultiplier);
            s.skySettings.mieMultiplier = m.skySettings.mieMultiplier.GetValueOrDefault(parent.skySettings.mieMultiplier);
            s.skySettings.ozonePercent = m.skySettings.ozonePercent.GetValueOrDefault(parent.skySettings.ozonePercent);
            s.skySettings.g = m.skySettings.g.GetValueOrDefault(parent.skySettings.g);
            s.skySettings.skyTint = m.skySettings.skyTint.GetValueOrDefault(parent.skySettings.skyTint);
            s.skySettings.mieTint = m.skySettings.mieTint.GetValueOrDefault(parent.skySettings.mieTint);
            s.skySettings.skyBoost = m.skySettings.skyBoost.GetValueOrDefault(parent.skySettings.skyBoost);
            s.skySettings.starIntensity = m.skySettings.starIntensity.GetValueOrDefault(parent.skySettings.starIntensity);
            s.skySettings.martian = m.skySettings.martian.GetValueOrDefault(parent.skySettings.martian);
            s.skySettings.skyGIIntensity = m.skySettings.skyGIIntensity.GetValueOrDefault(parent.skySettings.skyGIIntensity);
            s.skySettings.reflectionIntensity = m.skySettings.reflectionIntensity.GetValueOrDefault(parent.skySettings.reflectionIntensity);

            s.fogSettings.fogTintColor = m.fogSettings.fogTintColor.GetValueOrDefault(parent.fogSettings.fogTintColor);
            s.fogSettings.fogMieMultiplier = m.fogSettings.fogMieMultiplier.GetValueOrDefault(parent.fogSettings.fogMieMultiplier);
            s.fogSettings.fogRayleighMultiplier = m.fogSettings.fogRayleighMultiplier.GetValueOrDefault(parent.fogSettings.fogRayleighMultiplier);
            s.fogSettings.fogG = m.fogSettings.fogG.GetValueOrDefault(parent.fogSettings.fogG);
            s.fogSettings.heightFogStart = m.fogSettings.heightFogStart.GetValueOrDefault(parent.fogSettings.heightFogStart);
            s.fogSettings.heightFogDensity = m.fogSettings.heightFogDensity.GetValueOrDefault(parent.fogSettings.heightFogDensity);
            s.fogSettings.heightMieMultiplier = m.fogSettings.heightMieMultiplier.GetValueOrDefault(parent.fogSettings.heightMieMultiplier);
            s.fogSettings.heightRayleighMultiplier = m.fogSettings.heightRayleighMultiplier.GetValueOrDefault(parent.fogSettings.heightRayleighMultiplier);
            s.fogSettings.surveyedMieMultiplier = m.fogSettings.surveyedMieMultiplier.GetValueOrDefault(parent.fogSettings.surveyedMieMultiplier);
            s.fogSettings.surveyedIntensity = m.fogSettings.surveyedIntensity.GetValueOrDefault(parent.fogSettings.surveyedIntensity);
            s.fogSettings.revealedMieMultiplier = m.fogSettings.revealedMieMultiplier.GetValueOrDefault(parent.fogSettings.revealedMieMultiplier);
            s.fogSettings.revealedIntensity = m.fogSettings.revealedIntensity.GetValueOrDefault(parent.fogSettings.revealedIntensity);

            s.tonemapSettings.EV = m.tonemapSettings.EV.GetValueOrDefault(parent.tonemapSettings.EV);
            s.tonemapSettings.tempPreset = m.tonemapSettings.tempPreset.GetValueOrDefault(parent.tonemapSettings.tempPreset);
            s.tonemapSettings.whiteBalanceTemp = m.tonemapSettings.whiteBalanceTemp.GetValueOrDefault(parent.tonemapSettings.whiteBalanceTemp);
            s.tonemapSettings.whiteBalanceTint = m.tonemapSettings.whiteBalanceTint.GetValueOrDefault(parent.tonemapSettings.whiteBalanceTint);

            s.bloomSettings.threshold = m.bloomSettings.threshold.GetValueOrDefault(parent.bloomSettings.threshold);
            s.bloomSettings.softKnee = m.bloomSettings.softKnee.GetValueOrDefault(parent.bloomSettings.softKnee);
            s.bloomSettings.bloomRadius = m.bloomSettings.bloomRadius.GetValueOrDefault(parent.bloomSettings.bloomRadius);
            s.bloomSettings.bloomIntensity = m.bloomSettings.bloomIntensity.GetValueOrDefault(parent.bloomSettings.bloomIntensity);
            s.bloomSettings.thresholdLinear = m.bloomSettings.thresholdLinear.GetValueOrDefault(parent.bloomSettings.thresholdLinear);
            s.bloomSettings.bloomSmear = m.bloomSettings.bloomSmear.GetValueOrDefault(parent.bloomSettings.bloomSmear);
            s.bloomSettings.bloomFlareIntensity = m.bloomSettings.bloomFlareIntensity.GetValueOrDefault(parent.bloomSettings.bloomFlareIntensity);
            s.bloomSettings.bloomFlareColors = m.bloomSettings.bloomFlareColors == null ? parent.bloomSettings.bloomFlareColors : m.bloomSettings.bloomFlareColors;
            s.bloomSettings.bloomTints = parent.bloomSettings.bloomTints;
            s.bloomSettings.preset = parent.bloomSettings.preset;
            s.bloomSettings.bloomFlares = parent.bloomSettings.bloomFlares;
            s.bloomSettings.bloomFlaresElements = parent.bloomSettings.bloomFlaresElements;
            s.bloomSettings.bloomFlaresDispersions = parent.bloomSettings.bloomFlaresDispersions;
            s.bloomSettings.thresholdGamma = parent.bloomSettings.thresholdGamma;

            s.mechSettings.brightness = m.mechSettings.brightness.GetValueOrDefault(parent.mechSettings.brightness);
            s.mechSettings.contrast = m.mechSettings.contrast.GetValueOrDefault(parent.mechSettings.contrast);
            s.mechSettings.saturation = m.mechSettings.saturation.GetValueOrDefault(parent.mechSettings.saturation);

            s.flareSettings.streakMode = m.flareSettings.streakMode.GetValueOrDefault(parent.flareSettings.streakMode);
            s.flareSettings.showCores = parent.flareSettings.showCores;
            s.flareSettings.showStarburst = parent.flareSettings.showStarburst;

            s.OnValidate();
            MoodSettings.GetAllMoods(false).Add(s);

            m.Mood_MDD = new Mood_MDD(m.Name, m.Name, m.Name, s.moodTags.GetTagSetSourceFile());
            Traverse.Create(m.Mood_MDD).Field("moodSettings").SetValue(s);

            EDM.modLog.Info?.Write($"Loaded custom mood {m.Name}, based on {m.baseMood} from {path}."
              + "\n    It has {(startMission == null ? 0 : startMission.Count)} startMission dialogue chunks and applies the \"{designMask}\" designMask"
            );
            Utils.logMoodSettings(m.MoodSettings);
            return m;
        }
    }
}
