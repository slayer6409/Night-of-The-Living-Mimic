using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace SlayerDeadBodiesBecomeZombiesRandomly.Patches
{
    [HarmonyPatch(typeof(MaskedPlayerEnemy))]
    internal class MaskedPlayerEnemyPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MaskedPlayerEnemy), "KillEnemy")]
        public static void doKill(MaskedPlayerEnemy __instance)
        {
            zombieBody zb = __instance.GetComponent<zombieBody>();
            if (zb != null)
            {
                Networker.Instance.spawnBodyServerRPC(zb.bodyID, __instance.transform.position, __instance.NetworkObject.NetworkObjectId);
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HauntedMaskItem), "FinishAttaching")]
        public static void FinishAttaching(HauntedMaskItem __instance)
        {

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MaskedPlayerEnemy), "Start")]
        public static void startStuff(MaskedPlayerEnemy __instance)
        {
            if (!SDBBZRMain.maskTurn.Value) return;
            if (!StartOfRound.Instance.localPlayerController.IsHost) return;
            if (__instance.gameObject.GetComponent<zombieBody>()) return;
            var mm = __instance.gameObject.AddComponent<MaskedMaker>();
            var eai = __instance.gameObject.GetComponent<EnemyAI>();
            mm.enemyAI = eai;
            mm.instance = __instance;
            mm.starterHP = eai.enemyHP;
        }
    }
    internal class MaskedMaker : MonoBehaviour
    {
        public MaskedPlayerEnemy instance;
        public EnemyAI enemyAI;
        private float timeSinceLastCheck = 0f;
        public int starterHP = 0;
        public int timesRevived = 0;
        public bool currentlyZombie = false;
        public static int timerModifier = 0;
        public static int chanceModifier = 0;
        public void Update()
        {
            if (!enemyAI.isEnemyDead) return;
            if (StartOfRound.Instance.shipIsLeaving) return;
            if (timesRevived >= 1 && SDBBZRMain.continuous.Value == false) return;
            timeSinceLastCheck += Time.deltaTime;
            if (timeSinceLastCheck >= (SDBBZRMain.timer.Value + timerModifier))
            {
                becomeZombieCheck();
                timeSinceLastCheck = 0f;
            }
        }

        public void becomeZombieCheck()
        {
            if (SDBBZRMain.percentChance.Value + chanceModifier <= 0) return;
            int randomNumber = UnityEngine.Random.Range(0, 101);
            Debug.Log(randomNumber);
            randomNumber += timesRevived * SDBBZRMain.chanceDecrease.Value;
            if (randomNumber < (SDBBZRMain.percentChance.Value + chanceModifier)) setZombie();
        }

        public void setZombie()
        {
            timeSinceLastCheck = 0f;
            timesRevived++;
            Networker.Instance.doMaskStuffClientRPC(instance.NetworkObjectId, starterHP);
        }
    }
}
