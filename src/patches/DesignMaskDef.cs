using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using Harmony;
using Newtonsoft.Json.Linq;
using HBS.Util;

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
                int length = definition["additionalStickyEffects"].Count();

                EDM.additionalStickyEffects[Id] = new EffectData[length];
                EDM.modLog.Debug?.Write($"Read {length} additionalStickyEffects for {Id}.");

                int i = 0;
                foreach (JObject additional in definition["additionalStickyEffects"]) {
                    EffectData effect = new EffectData();
                    JSONSerializationUtility.FromJSON<EffectData>(effect, additional.ToString());

                    JToken d = additional["Description"];

                    effect.Description = new BaseDescriptionDef(d["Id"].ToObject<string>(), d["Name"].ToObject<string>(), d["Details"].ToObject<string>(), d["Icon"].ToObject<string>());

                    EDM.additionalStickyEffects[Id][i] = effect;
                    i++;
                }

                definition.Remove("additionalStickyEffects");
                json = definition.ToString();
            } catch (Exception e) {
                EDM.modLog.Error?.Write(e);
            }
        }
    }
}
