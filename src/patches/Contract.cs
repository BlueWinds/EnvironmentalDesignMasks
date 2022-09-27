using System;
using System.Collections.Generic;
using Harmony;
using BattleTech;
using BattleTech.Data;
using BattleTech.Rendering.Mood;
using HBS;

namespace EnvironmentalDesignMasks {
    [HarmonyPatch(typeof(Contract), "GetMood")]
    public static class Contract_GetMood {
        private static void addToList(Dictionary<string, int> add, ref List<string> potentialMoods) {
            foreach (string mood in add.Keys) {
                for (int i = 0; i < add[mood]; i++) {
                    potentialMoods.Add(mood);
                }
            }
        }

        private static bool Prefix(string mapPath, string overrideMapMood, ref string __result) {
            try {
                // Certain travel-only contracts don't actually have a map, buth the game wants to know what mood to use
                // for them? IDK, return early so we don't have errors in logs.
                if (mapPath == null) {
                    __result = MoodSettings.DefaultMood;
                    return false;
                }

                Biome.BIOMESKIN biome = MetadataDatabase.Instance.GetBiomeForMapPath(mapPath).BiomeSkin;
                EDM.modLog.Info?.Write($"[GetMood] mapPath: {mapPath}, overrideMapMood: {overrideMapMood}, biome: {biome}");

                Settings s = EDM.settings;

                // If this biome isn't configured in EDM, or we have a mood explicitly set in the contract,
                // fall back to the HBS logic using tagsets (or just returning the ovrerride).
                if (!s.biomeMoods.ContainsKey(biome) || !string.IsNullOrEmpty(overrideMapMood)) {
                    EDM.modLog.Info?.Write($"[GetMood] No moods configured for {biome} (or override present), falling back to HBS logic");
                    return true;
                }

                List<string> potentialMoods = new List<string>();
                addToList(s.biomeMoods[biome], ref potentialMoods);
                EDM.modLog.Debug?.Write($"[GetMood] Biome {biome} has {potentialMoods.Count} base entries");

                SimGameState sim = SceneSingletonBehavior<UnityGameInstance>.Instance.Game.Simulation;;
                if (sim != null) {
                    foreach (string tag in sim.CurSystem.Tags) {
                        if (s.biomeMoodBySystemTag.ContainsKey(tag) && s.biomeMoodBySystemTag[tag].ContainsKey(biome)) {
                            EDM.modLog.Debug?.Write($"System tag {tag} has moods for {biome}");

                            addToList(s.biomeMoodBySystemTag[tag][biome], ref potentialMoods);
                        }
                    }
                }

                __result = potentialMoods[UnityEngine.Random.Range(0, potentialMoods.Count)];
                EDM.modLog.Info?.Write($"[GetMood] Selected {__result} from {potentialMoods.Count} entries");

                return false;
            } catch (Exception e) {
                EDM.modLog.Error?.Write(e);
                return true;
            }
        }
    }
}
