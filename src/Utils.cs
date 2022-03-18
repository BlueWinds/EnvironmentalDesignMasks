using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTech.Rendering;
using BattleTech.Rendering.Mood;
using HBS.Collections;

namespace EnvironmentalDesignMasks {
    public class Utils {
        public static bool ValidateSettings(Settings settings) {
            bool valid = true;

            foreach (Biome.BIOMESKIN biome in settings.biomeMoods.Keys) {
                foreach (string mood in settings.biomeMoods[biome].Keys) {
                    Mood_MDD mood_MDD = MetadataDatabase.Instance.GetMood(mood);

                    if (mood_MDD == null && !EDM.customMoods.ContainsKey(mood)) {
                        EDM.modLog.Error?.Write($"biomeMoods[{biome}] contains {mood}, which was not found in MDD or as a custom mood. EDM is misconfigured.");
                        valid = false;
                    }
                }
            }

            foreach (string systemTag in settings.biomeMoodBySystemTag.Keys) {
                foreach (Biome.BIOMESKIN biome in settings.biomeMoodBySystemTag[systemTag].Keys) {
                    foreach (string mood in settings.biomeMoodBySystemTag[systemTag][biome].Keys) {
                        Mood_MDD mood_MDD = MetadataDatabase.Instance.GetMood(mood);

                        if (mood_MDD == null && !EDM.customMoods.ContainsKey(mood)) {
                            EDM.modLog.Error?.Write($"biomeMoodBySystemTag[{systemTag}][{biome}] contains {mood}, which was not found in MDD or as a custom mood. EDM is misconfigured.");
                            valid = false;
                        }
                    }
                }
            }

            return valid;
        }

        public static void logAllMoods() {
            List<Mood_MDD> moods = MetadataDatabase.Instance.GetMatchingDataByTagSet<Mood_MDD>(TagSetType.Mood, new TagSet(), new TagSet(), "Mood", "ORDER BY FriendlyName");

            EDM.modLog.Debug?.Write($"Listing all base game moods:");
            foreach (Mood_MDD mood in moods) {
                if (mood.MoodSettings != null) {
                    EDM.modLog.Debug?.Write($"{mood.Name}: {mood.MoodSettings.moodTags.ToString()}");
                    logMoodSettings(mood.MoodSettings);
                }
            }
        }

        public static void logMoodSettings(MoodSettings s) {
            BTSunlight.SunlightSettings ss = s.sunlight;
            WeatherController.WeatherSettings ws = s.weatherSettings;
            SkyScattering.SkySettings sk = s.skySettings;
            FogScattering.FogScatteringSettings fs = s.fogSettings;
            BTPostProcess.TonemapperSettings ts = s.tonemapSettings;
            BTPostProcess.BloomSettings  bs = s.bloomSettings;
            GlobalAdjustments.MechSettings ms = s.mechSettings;
            BTPostProcess.FlareSettings  fl = s.flareSettings;

            EDM.modLog.Trace?.Write($"    uniqueFriendlyName: {s.uniqueFriendlyName}, sunXRotation: {s.sunXRotation}, sunYRotation: {s.sunYRotation}"
            + $"\n    Sunlight illuminance: {ss.illuminance}, angularDiameter: {ss.angularDiameter}, useTemperature: {ss.useTemperature}, colorTemperature: {ss.colorTemperature}"
            + $"\n    Sunlight cloudCover: {ss.cloudCover}, cloudOpacity: {ss.cloudOpacity}, cloudSoftness: {ss.cloudSoftness}, flareTint: {ss.flareTint.ToString()}, "
            + $"\n    Sunlight sunGIOverride: {ss.sunGIOverride}, sunGI: {ss.sunGI}, sunColor: {ss.sunColor.ToString()}, sundiscColor: {ss.sundiscColor.ToString()}, nightLights: {ss.nightLights}"
            + $"\n    Weather windDirection: {ws.windDirection}, windMain: {ws.windMain}, windPulseFrequency: {ws.windPulseFrequency}, windPulseMagnitude: {ws.windPulseMagnitude}, windTurbulence: {ws.windTurbulence}"
            + $"\n    Weather weatherVFXName: {ws.weatherVFXName}, weatherEffect: {ws.weatherEffect}, weatherEffectIntensity: {ws.weatherEffectIntensity}"
            + $"\n    Sky planetRadius: {sk.planetRadius}, atmosphereHeight: {sk.atmosphereHeight}, rayleighMultiplier: {sk.rayleighMultiplier}, mieMultiplier: {sk.mieMultiplier}, ozonePercent: {sk.ozonePercent} g: {sk.g}"
            + $"\n    Sky skyTint: {sk.skyTint.ToString()}, mieTint: {sk.mieTint.ToString()}, skyBoost: {sk.skyBoost.ToString()}, starIntensity: {sk.starIntensity}"
            + $"\n    Sky martian: {sk.martian}, skyGIIntensity: {sk.skyGIIntensity}, reflectionIntensity: {sk.reflectionIntensity}"
            + $"\n    Fog fogTintColor {fs.fogTintColor.ToString()}, fogMieMultiplier: {fs.fogMieMultiplier}, fogRayleighMultiplier: {fs.fogRayleighMultiplier}, fogG: {fs.fogG}, heightFogStart: {fs.heightFogStart}"
            + $"\n    Fog heightFogDensity: {fs.heightFogDensity}, heightMieMultiplier: {fs.heightMieMultiplier}, heightRayleighMultiplier: {fs.heightRayleighMultiplier}, surveyedMieMultiplier: {fs.surveyedMieMultiplier}"
            + $"\n    Fog surveyedIntensity: {fs.surveyedIntensity}, revealedIntensity: {fs.revealedIntensity}"
            + $"\n    Tonemap EV: {ts.EV}, whiteBalanceTemp: {ts.whiteBalanceTemp}, whiteBalanceTint: {ts.whiteBalanceTint.ToString()}"
            + $"\n    Bloom threshold: {bs.threshold}, softKnee: {bs.softKnee}, bloomRadius: {bs.bloomRadius}, bloomIntensity: {bs.bloomIntensity}, thresholdLinear: {bs.thresholdLinear}"
            + $"\n    Bloom bloomSmear: {bs.bloomSmear}, bloomFlareIntensity: {bs.bloomFlareIntensity}, bloomFlaresDispersions: {string.Join(", ", bs.bloomFlaresDispersions)}"
            + $"\n    Bloom bloomFlareColors: {string.Join(", ", bs.bloomFlareColors.Select(c => c.ToString()))}"
            + $"\n    Mech brightness: {ms.brightness}, contrast: {ms.contrast}, saturation: {ms.saturation}"
            + $"\n    Flare streakMode: {fl.streakMode}"
            + "\n");
        }
    }
}
