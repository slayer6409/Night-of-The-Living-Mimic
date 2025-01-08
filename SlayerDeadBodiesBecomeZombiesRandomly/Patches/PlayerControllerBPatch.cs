using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SlayerDeadBodiesBecomeZombiesRandomly.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "KillPlayer")]
        public static void AttachObject(PlayerControllerB __instance, int causeOfDeath, int deathAnimation, bool spawnBody, Vector3 bodyVelocity)
        {
            if (Networker.Instance == null)
            {
                Debug.LogError("Networker.Instance is null. Cannot add component to body.");
                return;
            }
            Networker.Instance.addComponentToBodyServerRPC(__instance.actualClientId);
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "KillPlayerServerRpc")]
        public static void AttachObjectTwo(PlayerControllerB __instance, int causeOfDeath, int deathAnimation, bool spawnBody, Vector3 bodyVelocity)
        {
            if (Networker.Instance == null)
            {
                Debug.LogError("Networker.Instance is null. Cannot add component to body.");
                return;
            }
            Networker.Instance.addComponentToBodyServerRPC(__instance.actualClientId);
        }
    }
}
