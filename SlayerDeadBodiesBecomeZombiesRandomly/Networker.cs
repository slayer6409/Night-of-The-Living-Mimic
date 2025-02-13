using GameNetcodeStuff;
using SlayerDeadBodiesBecomeZombiesRandomly.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace SlayerDeadBodiesBecomeZombiesRandomly
{
    internal class Networker : NetworkBehaviour
    {
        public static Networker Instance { get; private set; }
        public int chanceModifier = 0;
        public bool canChat = true;
        public HashSet<ulong> ForcedRemovalTwitch = new HashSet<ulong>();
        public HashSet<ulong> ForcedAddedTwitch = new HashSet<ulong>(); 

        public override void OnNetworkSpawn()
        {
            Instance = this;
            base.OnNetworkSpawn();
            if (IsServer) return;

        }
        
        private bool canSpawn = true;
        private Queue<(ulong playerID, int amount, string what)> spawnQueue = new Queue<(ulong, int, string)>();
        public void FixedUpdate()
        {
            if (StartOfRound.Instance is not null &&
                (!StartOfRound.Instance.inShipPhase || StartOfRound.Instance.shipHasLanded))
            {
                canSpawn = true;
            }
            else canSpawn = false;
            
            if(IsServer && canSpawn && spawnQueue.Count > 0) ProcessSpawnQueue();

        }
        [ServerRpc(RequireOwnership = false)]
        public void QueueMimicSpawnServerRpc(ulong playerID, int amount, string what)
        {
            spawnQueue.Enqueue((playerID, amount, what));
        }
        private int maxSpawnsPerFrame = 3; // Limit to 3 per FixedUpdate frame

        private void ProcessSpawnQueue()
        {
            int spawnCount = 0;
            while (spawnQueue.Count > 0 && canSpawn && spawnCount < maxSpawnsPerFrame)
            {
                var (playerID, amount, what) = spawnQueue.Dequeue();
                SpawnEnemy(playerID, amount, what);
                spawnCount++;
            }
        }

        
        private void SpawnEnemy(ulong playerID, int amount, string what)
        {
            var player = Misc.GetPlayerByUserID(playerID);
            if (player == null) return;
            bool mini = false;
            bool giant = false;
            bool flat = false;
            if (what.ToLower().Contains("mini")) {mini = true; what = what.Replace("mini", "");} 
            if (what.ToLower().Contains("big")) {giant = true; what = what.Replace("big", "");} 
            if (what.ToLower().Contains("flat")) {flat = true; what = what.Replace("flat", "");} 
            for (int i = 0; i < amount; i++)
            {
                Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                var enemy = Misc.getEnemyByName(what);
                if(enemy==null) return;
                GameObject gameObject = GameObject.Instantiate(enemy.enemyType.enemyPrefab, player.transform.position, randomRotation);
                var netObj = gameObject.GetComponentInChildren<NetworkObject>();
                netObj.Spawn(destroyWithScene: true);
                if (what == "Maneater")
                {
                    putThingOnManeaterClientRpc(netObj.NetworkObjectId);
                }
                RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
                if (what == "Masked")
                {
                    RemoveMaskedServerRpc(netObj.NetworkObjectId);
                }

                if (mini)
                {
                    if (what == "RadMech") setSizeClientRPC(netObj.NetworkObjectId, new Vector3(0.1f, 0.1f, 0.1f), Quaternion.identity);
                    else if (what == "Maneater") setSizeClientRPC(netObj.NetworkObjectId, new Vector3(0.25f, 0.25f, 0.25f), Quaternion.identity);
                    else setSizeClientRPC(netObj.NetworkObjectId, new Vector3(0.3f, 0.3f, 0.3f), Quaternion.identity);
                }
                if (giant)
                {
                    setSizeClientRPC(netObj.NetworkObjectId, new Vector3(2f, 2f, 2f), Quaternion.identity);
                }
                if (flat)
                {
                    Vector3 scale = Vector3.one;
                    switch (what)
                    {
                        case "Masked":
                            if (Random.value > .5f)
                            {
                                scale.z = 0.1f;
                            }
                            else
                            {
                                scale.y = 0.01f;
                            }
                            break;
                        case "Horse":
                            scale.y = 0.1f;
                            break;
                        case "Tulip Snake":
                            scale.x = 0.1f;
                            break;
                        case "Scary":
                            scale.x = 0.1f;
                            scale.z = 0.1f;
                            break;
                        case "Maneater":
                            scale.z = 0.1f;
                            break;
                        default:
                            scale.z = 0.1f;
                            break;
                    }
                    setSizeClientRPC(netObj.NetworkObjectId, scale, Quaternion.identity);
                }
            }
        }
        [ClientRpc]
        public void putThingOnManeaterClientRpc(ulong objID)
        {

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objID, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                var e = obj.AddComponent<ManeaterPatchThing>();
            }
        }

        [ClientRpc]
        public void setSizeClientRPC(ulong objectId, Vector3 size, Quaternion rotation = default)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                Vector3 newSize = new Vector3(
                    obj.transform.localScale.x * size.x, 
                    obj.transform.localScale.y * size.y,
                    obj.transform.localScale.z * size.z);
                obj.transform.localScale = newSize;
                if(rotation != default) obj.transform.localRotation = rotation;
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void RemoveMaskedServerRpc(ulong maskedID)
        {
            RemoveMaskedClientRpc(maskedID);
        }

        [ClientRpc]
        public void RemoveMaskedClientRpc(ulong maskedID)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(maskedID, out var networkObj))
            {
                GameObject obj2 = networkObj.gameObject;
                MaskedPlayerEnemy mask = obj2.GetComponent<MaskedPlayerEnemy>(); 
                FindChildByName(mask.transform, "HeadMaskComedy").gameObject.SetActive(false);
                FindChildByName(mask.transform, "HeadMaskTragedy").gameObject.SetActive(false);
                
            }
        }

        [ServerRpc(RequireOwnership =false)]
        public void addComponentToBodyServerRPC(ulong body)
        {
            addComponentToBodyClientRPC(body);
        }
        [ClientRpc]
        public void addComponentToBodyClientRPC(ulong body)
        {
            if (!StartOfRound.Instance.localPlayerController.IsHost) return; 
            var player = Misc.GetPlayerByUserID(body);
            if (player.deadBody == null) return;
            var e = player.deadBody.gameObject.AddComponent<BodyCheck>();
            e.owner = body;
            e.instance = player.deadBody;
        }

        [ServerRpc(RequireOwnership =false)]
        public void cooldownToggleServerRPC(bool state)
        {
            cooldownToggleClientRPC(state);
        }
        [ClientRpc]
        public void cooldownToggleClientRPC(bool state) 
        {
            canChat = state;
        }
        [ServerRpc(RequireOwnership = false)]
        public void cooldownStartServerRPC()
        {
            cooldownToggleServerRPC(false);
            StartCoroutine(cooldown());
        }
        IEnumerator cooldown()
        {
            if (!IsHost) yield return false;
            yield return new WaitForSeconds(60);
            cooldownToggleServerRPC(true);
        }

        [ServerRpc(RequireOwnership =false)]
        public void spawnBodyServerRPC(ulong body, Vector3 position, ulong ID)
        {
            spawnBodyClientRPC(body, position, ID);
        }

        [ClientRpc]
        public void doMaskStuffClientRPC(ulong mask, int health)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(mask, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                if (IsHost)
                {
                    var oldmm = obj.GetComponent<MaskedMaker>();
                    int otr = oldmm.timesRevived;
                    Vector3 pos = obj.transform.position;
                    Quaternion rot = obj.transform.rotation;
                    obj.GetComponent<NetworkObject>().Despawn(); 
                    GameObject gameObject = Instantiate(Misc.getEnemyByName("Masked").enemyType.enemyPrefab, pos, rot);
                    gameObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
                    MaskedPlayerEnemy masks = obj.GetComponent<MaskedPlayerEnemy>();
                    masks.mimickingPlayer = oldmm.instance.mimickingPlayer;
                    RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
                    NetworkObjectReference netObj = gameObject.GetComponentInChildren<NetworkObject>();
                    StartCoroutine(doStuff(otr, netObj.NetworkObjectId));
                }
                //EnemyAI eai = obj.GetComponent<EnemyAI>();
                //eai.enemyHP = health;
                //eai.isEnemyDead = false;
                //obj.SetActive(false);
                //obj.SetActive(true);
                //if (SDBBZRMain.funnyMode?.Value == false)
                //{
                //    eai.creatureAnimator.SetBool("Stunned", value: false);
                //    eai.creatureAnimator.SetBool("Dead", value: false);
                //}
            }

        }
        public IEnumerator doStuff(int revive, ulong mask)
        {
            yield return new WaitForSeconds(1f);
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(mask, out var networkObj))
            {
                GameObject obj = networkObj.gameObject;
                var mm = obj.GetComponent<MaskedMaker>();
                mm.timesRevived = revive;
            }

        }

        [ClientRpc]
        public void dropSpecificPlayersItemsClientRPC(ulong id) 
        {
            var player = StartOfRound.Instance.localPlayerController;
            if (player.actualClientId != id) return;
            StartCoroutine(dropItems(player));
        }

        public IEnumerator dropItems(PlayerControllerB player)
        {
            yield return new WaitForSeconds(0.2f);
            player.DiscardHeldObject();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SyncTwitchUserServerRPC()
        {
            if (SDBBZRMain.twitchBlacklistMode.Value)
            {
                foreach (var player in StartOfRound.Instance.allPlayerScripts)
                {
                    if (!SDBBZRMain.deniedTwitchUsers.Contains(player.playerSteamId))
                    {
                        AllowTwitchUserClientRPC(player.playerSteamId);
                    }
                }
            }
            else
            {
                foreach (var player in StartOfRound.Instance.allPlayerScripts)
                {
                    if(SDBBZRMain.allowedTwitchUsers.Contains(player.playerSteamId))
                    {
                        AllowTwitchUserClientRPC(player.playerSteamId);
                    }
                }   
            }
        }
        [ClientRpc]
        public void AllowTwitchUserClientRPC(ulong playerSteamID,bool FromCommand = false)
        {
            if(!FromCommand) if(ForcedRemovalTwitch.Contains(playerSteamID)) return;
            if (StartOfRound.Instance.localPlayerController.playerSteamId == playerSteamID || StartOfRound.Instance.localPlayerController.IsHost)
            {
                SDBBZRMain.canDoTwitch = true;
                ForcedAddedTwitch.Add(playerSteamID);
                if(ForcedRemovalTwitch.Contains(playerSteamID))ForcedRemovalTwitch.Remove(playerSteamID);
            }
        }
        [ClientRpc]
        public void DenyTwitchUserClientRPC(ulong playerSteamID, bool FromCommand = false)
        {
            if(!FromCommand) if(ForcedAddedTwitch.Contains(playerSteamID)) return;
            if (StartOfRound.Instance.localPlayerController.playerSteamId == playerSteamID)
            {
                if (playerSteamID != 76561198077184650)
                {
                    SDBBZRMain.canDoTwitch = false;
                    ForcedRemovalTwitch.Add(playerSteamID);
                    if(ForcedAddedTwitch.Contains(playerSteamID))ForcedAddedTwitch.Remove(playerSteamID);
                }
            }
        }


        [ServerRpc(RequireOwnership = false)]
        public void sendMessageSpecificServerRPC(string a, string b, bool warning, string name) 
        {
            sendMessageSpecificClientRPC(a, b, warning, name);
        }
        [ClientRpc]
        public void sendMessageSpecificClientRPC(string a, string b, bool warning, string name)
        {
            if(StartOfRound.Instance.localPlayerController.playerUsername == name) Misc.SafeTipMessage(a, b, warning);
        }

        [ServerRpc(RequireOwnership = false)]
        public void sendMessageAllServerRPC(string a, string b, bool warning, bool forced = false)
        {
            sendMessageAllClientRPC(a, b, warning, forced);
        }

        [ClientRpc]
        public void sendMessageAllClientRPC(string a, string b, bool warning, bool forced=false)
        {
            if(SDBBZRMain.ShowDebugChatboxes.Value==false && !forced) return;
            Misc.SafeTipMessage(a, b, warning);
        }

        [ClientRpc]
        public void spawnBodyClientRPC(ulong body, Vector3 position, ulong ID)
        {
            var player = Misc.GetPlayerByUserID(body);
            if (player == null)
            {
                SDBBZRMain.CustomLogger.LogWarning($"Player with ID {body} not found.");
                return;
            }

            if (player.deadBody == null)
            {
                SDBBZRMain.CustomLogger.LogWarning($"Player with ID {body} does not have a deadBody.");
                return;
            }

            player.deadBody.gameObject.SetActive(true);

            if (StartOfRound.Instance == null)
            {
                SDBBZRMain.CustomLogger.LogWarning("StartOfRound.Instance is null.");
                return;
            }

            if (StartOfRound.Instance.localPlayerController == null)
            {
                SDBBZRMain.CustomLogger.LogWarning("StartOfRound.Instance.localPlayerController is null.");
                return;
            }

            if (StartOfRound.Instance.localPlayerController.IsHost)
            {
                var bc = player.deadBody.GetComponent<BodyCheck>();
                if (bc == null)
                {
                    SDBBZRMain.CustomLogger.LogWarning("BodyCheck component is missing on deadBody.");
                    return;
                }

                bc.currentlyZombie = false;
                bc.timesRevived++;

                if (NetworkManager.Singleton == null)
                {
                    SDBBZRMain.CustomLogger.LogWarning("NetworkManager.Singleton is null.");
                    return;
                }

                if (NetworkManager.Singleton.SpawnManager == null)
                {
                    SDBBZRMain.CustomLogger.LogWarning("NetworkManager.Singleton.SpawnManager is null.");
                    return;
                }

                if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ID, out var networkObj))
                {
                    SDBBZRMain.CustomLogger.LogWarning($"No spawned object found for ID {ID}.");
                }
            }

            player.deadBody.deactivated = false;
            player.deadBody.SetBodyPartsKinematic(false);
            player.deadBody.attachedTo = null;
            player.deadBody.attachedLimb = null;
            player.deadBody.secondaryAttachedLimb = null;
            player.deadBody.secondaryAttachedTo = null;

            player.deadBody.SetRagdollPositionSafely(position, disableSpecialEffects: true);

            if (StartOfRound.Instance.localPlayerController.IsHost)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ID, out var networkObj))
                {
                    GameObject obj2 = networkObj.gameObject;
                    Destroy(obj2);
                }
            }
        }
        // [ServerRpc(RequireOwnership =false)]
        // public void spawnMimicOnPlayerServerRPC(ulong playerID, int amount)
        // {
        //     var player = Misc.GetPlayerByUserID(playerID);
        //     for (int i = 0; i < amount; i++) 
        //     {
        //         GameObject gameObject = GameObject.Instantiate(Misc.getEnemyByName("Masked").enemyType.enemyPrefab, player.transform.position, Quaternion.identity);
        //         gameObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
        //         RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
        //     }
        // }
        //
        // [ServerRpc(RequireOwnership = false)]
        // public void spawnCustomEnemyServerRPC(ulong playerID, string what, int howMany)
        // {
        //     var player = Misc.GetPlayerByUserID(playerID);
        //     for (int i = 0; i < howMany; i++) 
        //     {
        //         GameObject gameObject = GameObject.Instantiate(Misc.getEnemyByName(what).enemyType.enemyPrefab, player.transform.position, Quaternion.identity);
        //         gameObject.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
        //         RoundManager.Instance.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
        //     }
        // }
        public IEnumerator doDelay(PlayerControllerB player)
        {
            yield return new WaitForSeconds(0.65f);
            var phb = player.deadBody.grabBodyObject.playerHeldBy;
            if (phb != null) phb.DropAllHeldItemsServerRpc();
            else if (player.deadBody.grabBodyObject.isHeldByEnemy)
            {
                player.deadBody.grabBodyObject.DiscardItemFromEnemy();
            }
            player.deadBody.DeactivateBody(false);
        }

        [ServerRpc(RequireOwnership = false)]
        public void fixMaskedServerRpc(ulong user, ulong masked)
        {
            fixMaskedClientRpc(user, masked);
        }

        [ClientRpc]
        public void fixMaskedClientRpc(ulong user, ulong masked)
        {
            var player = Misc.GetPlayerByUserID(user);
            if (player.deadBody != null)
            {
                if(player.deadBody.grabBodyObject != null)
                {
                    if (player.deadBody.grabBodyObject.isHeld)
                    {
                        StartOfRound.Instance.StartCoroutine(doDelay(player));
                    }
                    else
                    {
                        player.deadBody.DeactivateBody(false);
                    }
                }
            } 
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(masked, out var networkObj))
            {
                GameObject obj2 = networkObj.gameObject;
                player.redirectToEnemy = obj2.GetComponent<EnemyAI>();
                MaskedPlayerEnemy mask = obj2.GetComponent<MaskedPlayerEnemy>(); 
                FindChildByName(mask.transform, "HeadMaskComedy").gameObject.SetActive(false);
                FindChildByName(mask.transform, "HeadMaskTragedy").gameObject.SetActive(false);
                // if (StartOfRound.Instance.localPlayerController.IsHost)
                // {
                //     
                //     
                // }
            }
        }
        Transform FindChildByName(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;

                Transform result = FindChildByName(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
