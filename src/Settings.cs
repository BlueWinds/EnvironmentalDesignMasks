using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EnvironmentalDesignMasks {
    public class Settings {
        public bool debug = false;
        public bool trace = false;

        public Dictionary<Biome.BIOMESKIN, Dictionary<string, int>> biomeMoods = new Dictionary<Biome.BIOMESKIN, Dictionary<string, int>>();
        public Dictionary<string, Dictionary<Biome.BIOMESKIN, Dictionary<string, int>>> biomeMoodBySystemTag = new Dictionary<string, Dictionary<Biome.BIOMESKIN, Dictionary<string, int>>>();
        public bool debugServer = false;
    }
}
