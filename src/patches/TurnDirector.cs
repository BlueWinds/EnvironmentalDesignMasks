using System;
using System.Collections.Generic;
using Harmony;
using Localize;
using BattleTech;
using BattleTech.Designed;
using BattleTech.Rendering.Mood;
using UnityEngine;

namespace EnvironmentalDesignMasks {
    [HarmonyPatch(typeof(TurnDirector), "QueuePilotChatter")]
    public static class TurnDirector_QueuePilotChatter {
        private static void Prefix(TurnDirector __instance) {
            try {
                if (MoodController.Instance == null
                    || MoodController.Instance.CurrentMood == null
                    || !EDM.customMoods.ContainsKey(MoodController.Instance.CurrentMood.uniqueFriendlyName)
                ) {
                    return;
                }

                CustomMood mood = EDM.customMoods[MoodController.Instance.CurrentMood.uniqueFriendlyName];
                if (mood.startMission == null) {
                    EDM.modLog.Info?.Write($"[QueuePilotChatter] {MoodController.Instance.CurrentMood.uniqueFriendlyName} has no startMission dialogue");
                    return;
                }

                string name = $"Dialog_{mood.Name}_startMission";
                EDM.modLog.Info?.Write($"[QueuePilotChatter] Displaying {name}");

                GameObject encounterLayerGameObject = __instance.Combat.EncounterLayerData.gameObject;
                GameObject dialogChunk = new GameObject($"Chunk_{name}");
                dialogChunk.transform.parent = encounterLayerGameObject.transform;
                dialogChunk.transform.localPosition = Vector3.zero;

                DialogueChunkGameLogic dialogueChunkGameLogic = dialogChunk.AddComponent<DialogueChunkGameLogic>();
                dialogueChunkGameLogic.encounterObjectName = $"Chunk_{name}";
                dialogueChunkGameLogic.encounterObjectGuid = System.Guid.NewGuid().ToString();

                GameObject go = new GameObject(name);
                go.transform.parent = dialogChunk.transform;
                go.transform.localPosition = Vector3.zero;

                DialogueGameLogic dgl = go.AddComponent<DialogueGameLogic>();
                dgl.conversationContent = new ConversationContent(name, mood.startMission);
                dgl.conversationContent.ContractInitialize(__instance.Combat);
                dgl.SetCombat(__instance.Combat);
                dgl.GenerateNewGuid();
                __instance.Combat.ItemRegistry.AddItem(dgl);

                dgl.TriggerDialogue(true);
            } catch (Exception e) {
                EDM.modLog.Error?.Write(e);
            }
        }
    }
}

