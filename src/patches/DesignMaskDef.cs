using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using Harmony;
using Newtonsoft.Json.Linq;

namespace EnvironmentalDesignMasks {
    [HarmonyPatch(typeof(DesignMaskDef), "FromJSON")]
    public static class DesignMaskDef_fromJSON {

        public static void Prefix(ref string json) {
            try {
                JObject definition = JObject.Parse(json);
                if (definition["additionalStickyEffects"] == null) {
                    return;
                }

                string Id = (string)definition["Description"]["Id"];
                EffectData[] additional = definition["additionalStickyEffects"].ToObject<EffectData[]>();
                EDM.modLog.Debug?.Write($"Read {additional.Length} additionalStickyEffects for {Id}.");

                int i = 0;
                foreach (EffectData effect in additional) {
                    JToken d = definition["additionalStickyEffects"][i]["Description"];

                    effect.Description = new BaseDescriptionDef(d["Id"].ToObject<string>(), d["Name"].ToObject<string>(), d["Details"].ToObject<string>(), d["Icon"].ToObject<string>());
                    i++;
                }

                EDM.additionalStickyEffects[Id] = additional;
                definition.Remove("additionalStickyEffects");
                json = definition.ToString();
            } catch (Exception e) {
                EDM.modLog.Error?.Write(e);
            }
        }
    }
}
