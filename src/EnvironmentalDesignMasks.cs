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
        internal static DeferringLogger modLog;
        internal static string modDir;
        internal static Settings settings;
        public static Dictionary<string, CustomMood> customMoods = new Dictionary<string, CustomMood>();

        public static void Init(string modDirectory, string settingsJSON) {
            modDir = modDirectory;

            try {
                using (StreamReader reader = new StreamReader($"{modDir}/settings.json")) {
                    string jdata = reader.ReadToEnd();
                    settings = JsonConvert.DeserializeObject<Settings>(jdata);
                }
                modLog = new DeferringLogger(modDirectory, "EDM", "EDM", settings.debug, settings.trace);
                modLog.Debug?.Write($"Loaded settings from {modDir}/settings.json. Version {typeof(Settings).Assembly.GetName().Version}");
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

            if (!Utils.ValidateSettings(settings)) {
                throw new Exception("Settings are not valid, aborting to make this obvious.");
            }
            modLog.Info?.Write($"Settings are valid");

            var harmony = HarmonyInstance.Create("blue.winds.EnvironmentalDesignMasks");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
