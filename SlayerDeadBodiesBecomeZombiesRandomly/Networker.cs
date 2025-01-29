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

namespace SlayerDeadBodiesBecomeZombiesRandomly
{
    internal class Networker : NetworkBehaviour
    {
        public static Networker Instance { get; private set; }
        public override void OnNetworkSpawn()
        {
            Instance = this;
            base.OnNetworkSpawn();
            if (IsServer) return;

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
        public IEnumerator doDelay(PlayerControllerB player)
        {
            yield return new WaitForSeconds(0.5f);
            var phb = player.deadBody.grabBodyObject.playerHeldBy;
            if (phb != null) phb.DropAllHeldItemsServerRpc();
            else if (player.deadBody.grabBodyObject.isHeldByEnemy)
            {
                player.deadBody.grabBodyObject.DiscardItemFromEnemy();
            }
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
                }
            }
            player.deadBody.DeactivateBody(false); 
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(masked, out var networkObj))
            {
                GameObject obj2 = networkObj.gameObject;
                player.redirectToEnemy = obj2.GetComponent<EnemyAI>();
                if (StartOfRound.Instance.localPlayerController.IsHost)
                {
                    MaskedPlayerEnemy mask = obj2.GetComponent<MaskedPlayerEnemy>(); 
                    FindChildByName(mask.transform, "HeadMaskComedy").gameObject.SetActive(false);
                    FindChildByName(mask.transform, "HeadMaskTragedy").gameObject.SetActive(false);
                    mask.mimickingPlayer = player;
                }
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
