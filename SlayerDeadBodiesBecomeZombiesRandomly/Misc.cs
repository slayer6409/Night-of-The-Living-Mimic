﻿using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using static UnityEngine.EventSystems.EventTrigger;

namespace SlayerDeadBodiesBecomeZombiesRandomly
{
    internal class Misc
    {
        public static SpawnableEnemyWithRarity getEnemyByName(string name)
        {
            List<SpawnableEnemyWithRarity> allenemies = new List<SpawnableEnemyWithRarity>();

            foreach (var level in StartOfRound.Instance.levels)
            {
                allenemies = allenemies
                    .Union(level.Enemies)
                    .Union(level.OutsideEnemies)
                    .Union(level.DaytimeEnemies)
                    .ToList();
            }
            allenemies = allenemies
            .GroupBy(x => x.enemyType.enemyName)
            .Select(g => g.First())
            .OrderBy(x => x.enemyType.enemyName)
            .ToList();
            SpawnableEnemyWithRarity enemy = allenemies.FirstOrDefault(x => x.enemyType.enemyName == name);
            if (enemy == null)
            { //do original method as backup
                foreach (SelectableLevel level in StartOfRound.Instance.levels)
                {

                    enemy = level.Enemies.FirstOrDefault(x => x.enemyType.enemyName.ToLower() == name.ToLower());
                    if (enemy == null)
                        enemy = level.DaytimeEnemies.FirstOrDefault(x => x.enemyType.enemyName.ToLower() == name.ToLower());
                    if (enemy == null)
                        enemy = level.OutsideEnemies.FirstOrDefault(x => x.enemyType.enemyName.ToLower() == name.ToLower());


                }
            }
            if (enemy == null)
            {
                SDBBZRMain.CustomLogger.LogWarning($"Enemy '{name}' not found. Available enemies: {string.Join(", ", allenemies.Select(e => e.enemyType.enemyName))}"); return null;
            }
            return enemy;
        }
        public static void SafeTipMessage(string title, string body, bool isWarning = false)
        {
            try
            {
                HUDManager.Instance.DisplayTip(title, body, isWarning);
            }
            catch
            {
                SDBBZRMain.CustomLogger.LogWarning("There's a problem with the DisplayTip method. This might have happened due to a new game verison, or some other mod.");
                try
                {
                    ChatWrite($"{title}: {body}");
                }
                catch
                {
                    SDBBZRMain.CustomLogger.LogWarning("There's a problem with writing to the chat. This might have happened due to a new game verison, or some other mod.");
                }
            }

        }
        public static void ChatWrite(string chatMessage)
        {
            HUDManager.Instance.lastChatMessage = chatMessage;
            HUDManager.Instance.PingHUDElement(HUDManager.Instance.Chat, 4f);
            if (HUDManager.Instance.ChatMessageHistory.Count >= 4)
            {
                HUDManager.Instance.chatText.text.Remove(0, HUDManager.Instance.ChatMessageHistory[0].Length);
                HUDManager.Instance.ChatMessageHistory.Remove(HUDManager.Instance.ChatMessageHistory[0]);
            }
            string text = $"<color=#00ffff>{chatMessage}</color>";
            HUDManager.Instance.ChatMessageHistory.Add(text);
            HUDManager.Instance.chatText.text = "";
            for (int i = 0; i < HUDManager.Instance.ChatMessageHistory.Count; i++)
            {
                TextMeshProUGUI textMeshProUGUI = HUDManager.Instance.chatText;
                textMeshProUGUI.text = textMeshProUGUI.text + "\n" + HUDManager.Instance.ChatMessageHistory[i];
            }
        }
        public static PlayerControllerB GetPlayerByUserID(ulong userID)
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player.actualClientId == userID)
                    return player;
            }
            return null;
        }
        public static PlayerControllerB GetClosestPlayerByName(string name)
        {
            PlayerControllerB closestPlayer = null;
            int smallestDistance = int.MaxValue;

            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                int distance = CalculateLevenshteinDistance(name, player.playerUsername);
                if (distance < smallestDistance)
                {
                    smallestDistance = distance;
                    closestPlayer = player;
                }
            }

            return closestPlayer;
        }

        // Levenshtein Distance Algorithm
        private static int CalculateLevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source))
                return target?.Length ?? 0;

            if (string.IsNullOrEmpty(target))
                return source.Length;

            int[,] distance = new int[source.Length + 1, target.Length + 1];

            for (int i = 0; i <= source.Length; distance[i, 0] = i++) { }
            for (int j = 0; j <= target.Length; distance[0, j] = j++) { }

            for (int i = 1; i <= source.Length; i++)
            {
                for (int j = 1; j <= target.Length; j++)
                {
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[source.Length, target.Length];
        }

        public static int getPlayerIntFromUserID(ulong userID) 
        {
            int index = -1;


            for (int i = 0; i < StartOfRound.Instance.allPlayerObjects.Count(); i++)
            {
                PlayerControllerB player = StartOfRound.Instance.allPlayerObjects[i].GetComponent<PlayerControllerB>();
                if (IsPlayerReal(player))
                    if (player.actualClientId == userID)
                    {
                        index = i;
                        break;
                    }
            }
            return index;
        }
        public static bool IsPlayerReal(PlayerControllerB player)
        {
            return player.isActiveAndEnabled &&
                   player.isPlayerControlled;
        }
    }
}
