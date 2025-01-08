using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace SlayerDeadBodiesBecomeZombiesRandomly.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void InstantiateNetworker(StartOfRound __instance)
        {

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                GameObject go = GameObject.Instantiate(SDBBZRMain.NetworkerPrefab, Vector3.zero, Quaternion.identity);
                go.GetComponent<NetworkObject>().Spawn(true);
            }
        }
    }
}
