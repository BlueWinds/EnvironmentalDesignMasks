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
                MoodSettings currentMood = Utils.getCurrentMood();
                if (currentMood == null|| !EDM.customMoods.ContainsKey(currentMood.uniqueFriendlyName)
                ) {
                    return;
                }

                CustomMood mood = EDM.customMoods[currentMood.uniqueFriendlyName];
                if (mood.startMission == null) {
                    EDM.modLog.Info?.Write($"[QueuePilotChatter] {currentMood.uniqueFriendlyName} has no startMission dialogue");
                    return;
                }

                string name = $"Dialog_{mood.ID}_startMission";
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

