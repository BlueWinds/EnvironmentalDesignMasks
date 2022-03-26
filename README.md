EnvironmentalDesignMasks (EDM) is a mod for HBS's Battletech computer game. It allows mod makers to create custom weather and Moods, and apply these to contracts in game. This is an extremely powerful visual tool - Moods have extensive control on how the game renders, and can drastically alter the feel of a map.

## Logging Levels
In `settings.json`, it is recommended to set `trace: false` when distributing to users. The trace logging level is extremely useful for modpack developers, but redundant and not very useful in bug reports. Leaving `debug: true` is recommended, as it contains information useful for diagnosing any issues users may encounter.

## DesignMask extensions

EDM allows DesignMasks to define a new field, `additionalStickyEffects`, which is an array of status effects to apply in addition to the normal `stickyEffect`. If `stickyEffect` is not defined, they will be ignored. Always use `stickyEffect` first.

See the included `designmasks/DesignMaskVerdantDayWindy.json` for a usage example.

## Selecting Moods
EDM overrides the basegame logic for how Moods are selected. When generating a contract, EDM first assembles a list of all potential Moods, then draws one from the hat. This is done in two steps.

0. If the contract specifies a `mapMood`, this will be used, bypassing all other steps.
1. If the biome exists in `biomeMoods`, then each Mood is added to the list a number of times equal to its values.
2. For each system tag that the current star system matches in `biomeMoodBySystemTag`, additional tags will be added, as above.
3. An entry is randomly selected.

For example, consider the following settings:
```json
{
  "biomeMoods": {
    "highlandsFall": {
      "VerdantAfternoonFoggy": 1,
      "BoulderFieldMood": 2,
    }
  },
  "biomeMoodBySystemTag": {
    "planet_other_taintedair": {
      "highlandsFall": {
        "RedFog": 3
      }
    }
  }
}
```

We'll select a Mood for a contract with the `highlandsFall` biome.
1. We start with a list with three items: `[VerdantAfternoonFoggy, BoulderFieldMood, BoulderFieldMood]`.
2. If the player is currently at a star system with the `planet_other_taintedair` tag, then we'll add three more items: `[RedFog, RedFog, RedFog]`
3. From our list of 6 items (one FoggyAfternoon, two BoulderFields, three RedFogs), one is now selected at random.

### Skirmish
For skirmish battles, the player selects a mood from the dropdown. This dropdown is populated with the appropriate basegame Moods, and then also any Custom Moods (see below) whose `moodTags` contain the appropriate biome.

## Custom Moods
EDM also supports the creation of custom moods (such as RedFog in the example above) by adding JSON documents to the `customMoods` folder.

It is highly recommended to use the builtin debug server, which lets you modify moods on the fly and test out your settings without restarting the game. As long as it's enabled (`debugServer: true` in `settings.json`), you can access it any time Battletech is running by visiting [http://localhost:3062](http://localhost:3062).

Use the dropdown on the left to view data about existing Moods - both builtin and Custom Moods loaded from json. Then drop into a contract (skirmish or career) and use the text entry on the right + the apply mood button to see your modifications live in game.

### Basic Fields
Only two fields are required:
- `ID`: A human readable name for the mood. Must be globally unique. This may be visible when selecting the mood for a skirmish, but is otherwise not used in the UI.
- `baseMood`: The ID of the mood this one inherets from. Any optional properties (other than `moodTags`) not defined will be copied from the baseMood into this one. This can be either any mood from the base game (see the log file after loading into the game for a list), or another custom Mood already loaded.
  - Custom Moods are loaded alphabetically, which means `moodZ` can have `moodA` as a baseMood, but not vis versa.

All other fields are optional:
- `designMask`: The name of a design mask to apply when this custom mood applies. It replaces the basic biome design mask (which provides reduced heatsinking in deserts and lunar environments, for example).
  - If this DesignMask has a `stickyEffect` (and maybe also `additionalStickyEffects`), these effects are reapplied every turn, during `OnActivationBegin`. It usually makes sense to use `"durationData": {"duration": -1, "stackLimit": 1}` for sticky effects applied by biomes
- `startMission`: An array of dialogues to show on mission start (after other mission start dialogue, at the same time as pilot chatter). Interpolation works. Format is the same as other places dialogue chunks are defined (such as in contracts):
  ```
  "startMission": [
    {
      "words": "Everything is on fire. At least {TEAM_TAR.FactionDef.Demonym} are as screwed as you, {COMMANDER.Callsign}.",
      "wordsColor": {"r": 1, "g": 1, "b": 1},
      "selectedCastDefId": "castDef_KameaDefault",
      "emote": "Default",
      "audioName": "NONE"
    }
  ]
  ```
- `moodTags`: A list of biomes for which this Mood can be selected in the Skirmish bay. These tags have no effect outside the skirmish bay.
- `sunXRotation`: A float (0 - 360) specifying where the sun appears in the sky.
- `sunYRotation`: A float (0 - 360) specifying where the sun appears in the sky.

Colors are specified in the format `{"r": 1, "g": 0.818, "b": 0.666}`, with an optional alpha value (`"a"`).

### sunlight (optional)
- `illuminance`: float
- `angularDiameter`: float
- `useTemperature`: bool
- `colorTemperature`: float
- `sunColor`: color
- `cloudCover`: float
- `cloudOpacity`: float
- `cloudSoftness`: int
- `flareTint`: color
- `sunGIOverride`: bool
- `sunGI`: float
- `sundiscColor`: color
- `nightLights`: bool

### weatherSettings
- `windDirection`: float
- `windMain`: float
- `windPulseFrequency`: float
- `windPulseMagnitude`: float
- `windTurbulence`: float
- `weatherVFXName`: One of:
  - `vfxPrfPrtl_weatherCamRain`
  - `vfxPrfPrtl_weatherCamRain_drizzle`
  - `vfxPrfPrtl_weatherCamRain_storm`
  - `vfxPrfPrtl_weatherCamRain_heavy`
  - `vfxPrfPrtl_weatherCamRain_jungleMist`
  - `vfxPrfPrtl_weatherCamRain_jungleStorm`
  - `vfxPrfPrtl_weatherCamSnow_blizzard`
  - `vfxPrfPrtl_weatherCamSnow_calm`
  - `vfxPrfPrtl_weatherCamSnow_martian`
  - `vfxPrfPrtl_weatherCam_windstorm`
  - `vfxPrfPrtl_weatherCam_windstorm_martian`
  - `vfxPrfPrtl_weatherCam_allergySeason`
- `weatherEffectIntensity`: float

### skySettings (optional)
- `rayleighMultiplier`: float
- `mieMultiplier`: float
- `ozonePercent`: float
- `g`: float
- `skyTint`: Color
- `mieTint`: Color
- `skyBoost`: Color
- `starIntensity`: float
- `martian`: bool
- `skyGIIntensity`: float
- `reflectionIntensity`: float

### fogSettings (optional)
- `fogTintColor`: Color
- `fogMieMultiplier`: float
- `fogRayleighMultiplier`: float
- `fogG`: float
- `heightFogStart`: float
- `heightFogDensity`: float
- `heightMieMultiplier`: float
- `heightRayleighMultiplier`: float
- `surveyedMieMultiplier`: float
- `surveyedIntensity`: float
- `revealedMieMultiplier`: float
- `revealedIntensity`: float

### tonemapSettings (optional)
- `EV`: float
- `tempPreset`: One of:
  - `Tungsten`
  - `Flourescent`
  - `Daylight`
  - `Cloudy`
  - `Shade`
  - `Custom`
- `whiteBalanceTemp`: float
- `whiteBalanceTint`: float

### bloomSettings (optional)
- `threshold`: float
- `softKnee`: float
- `bloomRadius`: float
- `bloomIntensity`: float
- `thresholdLinear`: float
- `bloomSmear`: One of:
  - `Off`
  - `Horizontal`
  - `Vertical`
- `bloomFlareIntensity`: float
- `bloomFlareColors`: A list of exactly 10 colors. No more, no less.

### mechSettings (optional)
- `brightness`: float
- `contrast`: float
- `saturation`: float

### flareSettings (optional)
- `streakMode`: One of:
  - `None`
  - `Horizontal`
  - `Vertical`
  - `HorizAndVert`
