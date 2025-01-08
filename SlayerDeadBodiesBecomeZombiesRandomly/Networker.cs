using GameNetcodeStuff;
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
                EnemyAI eai = obj.GetComponent<EnemyAI>();
                eai.enemyHP = health;
                eai.isEnemyDead = false;
                obj.SetActive(false);
                obj.SetActive(true);
                if (SDBBZRMain.funnyMode?.Value == false)
                {
                    eai.creatureAnimator.SetBool("Stunned", value: false);
                    eai.creatureAnimator.SetBool("Dead", value: false);
                }
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
        public void sendMessageAllServerRPC(string a, string b, bool warning)
        {
            sendMessageAllClientRPC(a, b, warning);
        }

        [ClientRpc]
        public void sendMessageAllClientRPC(string a, string b, bool warning)
        {
            Misc.SafeTipMessage(a, b, warning);
        }

        [ClientRpc]
        public void spawnBodyClientRPC(ulong body, Vector3 position, ulong ID)
        {

            var player = Misc.GetPlayerByUserID(body);
            if(player.deadBody == null) return;
            player.deadBody.gameObject.SetActive(true);
            if (StartOfRound.Instance.localPlayerController.IsHost)
            {
                var bc = player.deadBody.GetComponent<BodyCheck>();
                bc.currentlyZombie = false;
                bc.timesRevived++;
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ID, out var networkObj))
                {
                    GameObject obj2 = networkObj.gameObject;
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
        [ServerRpc(RequireOwnership = false)]
        public void fixMaskedServerRpc(ulong user, ulong masked)
        {
            fixMaskedClientRpc(user, masked);
        }
        [ClientRpc]
        public void fixMaskedClientRpc(ulong user, ulong masked)
        {
            var player = Misc.GetPlayerByUserID(user); 
            if (player.deadBody.grabBodyObject.isHeld)
            {
                var phb = player.deadBody.grabBodyObject.playerHeldBy;
                if (phb != null)
                {
                    dropSpecificPlayersItemsClientRPC(phb.actualClientId);
                }
                else if (player.deadBody.grabBodyObject.isHeldByEnemy)
                {
                    player.deadBody.grabBodyObject.DiscardItemFromEnemy();
                }
            }

            player.deadBody.DeactivateBody(false); 
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(masked, out var networkObj))
            {
                GameObject obj2 = networkObj.gameObject;
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
