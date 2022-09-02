using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using Harmony;
using IRBTModUtils.Logging;
using BattleTech;
using UnityEngine;

namespace EnvironmentalDesignMasks
{
    public class EDM {
        internal static Dictionary<string, string> weatherAssembles = new Dictionary<string, string>();
        internal static DeferringLogger modLog;
        internal static string modDir;
        internal static Settings settings;
        public static Dictionary<string, CustomMood> customMoods = new Dictionary<string, CustomMood>();
        public static Dictionary<string, EffectData[]> additionalStickyEffects = new Dictionary<string, EffectData[]>();
    public static void FinishedLoading(List<string> loadOrder) {
      modLog.Info?.Write("FinishedLoading");
      try {
        BasicHandler.WeatherLightFabric.CreateLight = BTWeatherLight.create;
      } catch (Exception e) {
        modLog.Error?.Write(e.ToString());
      }
    }

    private static Assembly ResolveAssembly(System.Object sender, ResolveEventArgs evt) {
      Assembly res = null;
      try {
        EDM.modLog.Info?.Write("fail to resolve assembly:" + evt.Name);
        AssemblyName assemblyName = new AssemblyName(evt.Name);
        if (weatherAssembles.TryGetValue(assemblyName.Name, out string path)) {
          EDM.modLog.Info?.Write(" loading weather assembly:"+path);
          res = Assembly.LoadFile(path);
        } else {
          EDM.modLog.Info?.Write(" not a weather assembly");
        }
      }catch(Exception err) {
        EDM.modLog.Error?.Write(err.ToString());
      }
      return res;
    }

    public static void Init(string modDirectory, string settingsJSON) {
            modDir = modDirectory;
            
            try {
                Assembly basicHandler = Assembly.LoadFile(Path.Combine(modDirectory,"BasicHandler.dll"));
                using (StreamReader reader = new StreamReader($"{modDir}/settings.json")) {
                    string jdata = reader.ReadToEnd();
                    settings = JsonConvert.DeserializeObject<Settings>(jdata);
                }
                modLog = new DeferringLogger(modDirectory, "EDM", "EDM", settings.debug, settings.trace);
                modLog.Debug?.Write($"Loaded settings from {modDir}/settings.json. Version {typeof(Settings).Assembly.GetName().Version}");
                foreach(string assemblyPath in Directory.GetFiles(Path.Combine(modDirectory,settings.assembliesFolder),"*.dll", SearchOption.AllDirectories)) {
                  try {
                    string name = AssemblyName.GetAssemblyName(assemblyPath).Name;
                    weatherAssembles.Add(name, assemblyPath);
                    modLog.Info?.Write($" '{name}' '{assemblyPath}'");
                  }catch(Exception e) {
                    modLog.Error?.Write(e.ToString());
                  }                  
                }
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ResolveAssembly;
                AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
            } catch (Exception e) {
                settings = new Settings();
                modLog = new DeferringLogger(modDir, "EDM", "EDM", true, true);
                modLog.Error?.Write(e);
            }


            Utils.logAllMoods();

            modLog.Info?.Write($"Initializing custom moods:");
            foreach (string path in Directory.GetFiles($"{modDir}/customMoods")) {
                try {
                    string jdata;
                    using (StreamReader reader = new StreamReader(path)) {
                        jdata = reader.ReadToEnd();
                    }
                    CustomMood mood = CustomMood.fromString(jdata);
                    customMoods[mood.ID] = mood;

                    EDM.modLog.Info?.Write($"Loaded custom mood {mood.ID}, based on {mood.baseMood} from {path}."
                      + $"\n    It has {(mood.startMission == null ? 0 : mood.startMission.Count())} startMission dialogue chunks and applies the \"{mood.designMask}\" designMask"
                    );
                    Utils.logMoodSettings(mood.MoodSettings);
                } catch (Exception e) {
                    modLog.Error?.Write($"Error processing {path}:");
                    modLog.Error?.Write(e);
                }
            }

            Utils.ValidateSettings(settings);

            var harmony = HarmonyInstance.Create("blue.winds.EnvironmentalDesignMasks");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
