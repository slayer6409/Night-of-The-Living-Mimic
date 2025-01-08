using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace SlayerDeadBodiesBecomeZombiesRandomly.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void AddPrefab(ref GameNetworkManager __instance)
        {
            __instance.GetComponent<NetworkManager>().AddNetworkPrefab(SDBBZRMain.NetworkerPrefab);
        }
    }
}
