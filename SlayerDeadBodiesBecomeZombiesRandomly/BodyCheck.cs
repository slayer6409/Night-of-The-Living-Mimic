
using Unity.Netcode;
using UnityEngine;

namespace SlayerDeadBodiesBecomeZombiesRandomly
{
    internal class BodyCheck : MonoBehaviour
    {
        private float timeSinceLastCheck = 0f;
        public ulong owner = 0;
        public int timesRevived = 0;
        public bool currentlyZombie = false;
        public DeadBodyInfo instance;
        public static int timerModifier = 0; 
        public static int chanceModifier = 0; 
        public void Update()
        {
            if (!StartOfRound.Instance.localPlayerController.IsHost) return;
            if (currentlyZombie) return;
            if (StartOfRound.Instance.shipIsLeaving) return;
            if (timesRevived>=1 && SDBBZRMain.continuous.Value==false) return;
            if (instance?.grabBodyObject?.playerHeldBy != null)
            {
                var playerSteamId = instance.grabBodyObject.playerHeldBy.playerSteamId;

                if (ShouldBecomeZombie(playerSteamId))
                {
                    becomeZombieCheck(true);
                }
            }
            timeSinceLastCheck += Time.deltaTime;
            if(timeSinceLastCheck >= (SDBBZRMain.timer.Value+timerModifier))
            {
                becomeZombieCheck();
                timeSinceLastCheck = 0f;
            }
        }

        public bool ShouldBecomeZombie(ulong playerSteamId)
        {
            if (SDBBZRMain.chaosMode?.Value == true) return true;

            if (SDBBZRMain.CursedPlayersList.Contains(playerSteamId))
            {
                if (playerSteamId == 76561198984467725 && SDBBZRMain.DoubleCurseGlitch)
                {
                    return Random.value < 0.69f;
                }
                else
                {
                    return Random.value < 0.31f;
                }
            }

            // Default: not cursed, no transformation
            return false;
        }

        public void becomeZombieCheck(bool chaos = false)
        {
            if (SDBBZRMain.percentChance.Value + chanceModifier <= 0) return;
            int randomNumber = UnityEngine.Random.Range(0, 101);
            Debug.Log(randomNumber);
            randomNumber += timesRevived * SDBBZRMain.chanceDecrease.Value;
            if (randomNumber < (SDBBZRMain.percentChance.Value+chanceModifier)) setZombie();
            if (chaos) setZombie();
        }

        public void setZombie()
        {
            timeSinceLastCheck = 0f;
            currentlyZombie=true;
            GameObject gameObject = Instantiate(Misc.getEnemyByName("Masked").enemyType.enemyPrefab, instance.grabBodyObject.transform.position, Quaternion.Euler(new Vector3(0f, 0f, 0f)));
            gameObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
            RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
            NetworkObjectReference netObj = gameObject.GetComponentInChildren<NetworkObject>();
            var zb = gameObject.AddComponent<zombieBody>();
            zb.bodyID = owner;
            Networker.Instance.fixMaskedClientRpc(instance.playerScript.actualClientId, netObj.NetworkObjectId); 
           
        }
    }internal class zombieBody : MonoBehaviour
    {
        public ulong bodyID = 0;
    }


}
