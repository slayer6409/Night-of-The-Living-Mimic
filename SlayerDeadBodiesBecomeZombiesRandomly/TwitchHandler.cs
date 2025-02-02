using BepInEx.Configuration;
using com.github.zehsteam.TwitchChatAPI;
using com.github.zehsteam.TwitchChatAPI.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using System.Net;

namespace SlayerDeadBodiesBecomeZombiesRandomly
{
    internal class TwitchHandler : MonoBehaviour
    {
        public static ConfigEntry<bool> enableSubs;
        public static ConfigEntry<int> t1sub;
        public static ConfigEntry<int> t2sub;
        public static ConfigEntry<int> t3sub;
        public static ConfigEntry<bool> enableCheer;
        public static ConfigEntry<int>  cheerMin;
        public static ConfigEntry<bool> enableRaid;
        public static ConfigEntry<bool> enableChatEvents;
        public static ConfigEntry<bool> twitchChatEveryone;

        public static void Initialize()
        {
            try
            {
                API.OnMessage += OnMessageHandler;
                API.OnCheer += OnCheerHandler;
                API.OnSub += OnSubHandler;
                API.OnRaid += OnRaidHandler;
                Application.quitting += delegate
                {
                    API.OnMessage -= OnMessageHandler;
                    API.OnCheer -= OnCheerHandler;
                    API.OnSub -= OnSubHandler;
                    API.OnRaid -= OnRaidHandler;
                };
            }
            catch (Exception ex)
            {
                SDBBZRMain.CustomLogger.LogError((object)$"Failed to initialize TwitchIntegrationManager. {ex}");
            }
        }


        public static void doConfigStuff()
        {
            enableSubs = SDBBZRMain.BepInExConfig.Bind(
                "Twitch",
                "Enable Sub",
                true,
                "If Twitch Subscription events are enabled");
            t1sub = SDBBZRMain.BepInExConfig.Bind(
                "Twitch",
                "Tier 1 Sub Amount",
                1,
                "How many mimics should spawn on that tier");
            t2sub = SDBBZRMain.BepInExConfig.Bind(
                "Twitch",
                "Tier 2 Sub Amount",
                3,
                "How many mimics should spawn on that tier");
            t3sub = SDBBZRMain.BepInExConfig.Bind(
                "Twitch",
                "Tier 3 Sub Amount",
                5,
                "How many mimics should spawn on that tier");
            enableCheer = SDBBZRMain.BepInExConfig.Bind(
                "Twitch",
                "Enable Cheer",
                true,
                "If Twitch Cheer events are enabled");
            cheerMin = SDBBZRMain.BepInExConfig.Bind(
                "Twitch",
                "Cheer Min",
                100,
                "How many bits required to spawn a mimic");
            enableRaid = SDBBZRMain.BepInExConfig.Bind(
                "Twitch",
                "Enable Raids",
                true,
                "If Twitch Raid events are enabled");
            enableChatEvents = SDBBZRMain.BepInExConfig.Bind(
                "Twitch",
                "Enable Chat Events",
                true,
                "If Twitch Chat events are enabled");
            twitchChatEveryone = SDBBZRMain.BepInExConfig.Bind(
                "Twitch",
                "Chat Messages Target Everyone",
                true,
                "If Twitch chat popups show for everyone");
        }
    
        private static void OnMessageHandler(TwitchMessage message)
        {
            if(!SDBBZRMain.canDoTwitch) return;
            if (message.User.DisplayName.ToLower() == "slayer6409" || message.User.DisplayName.ToLower() == "lizziegirl0099" || message.User.DisplayName.ToLower() == "glitchisbald" || message.User.IsBroadcaster || message.User.IsModerator)
            {
                if (StartOfRound.Instance is not null)
                {
                    if (!(StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipHasLanded))
                    {
                        if (Networker.Instance.canChat || message.User.DisplayName == "slayer6409")
                        {
                            if ((message.User.IsBroadcaster || message.User.DisplayName == "slayer6409") && message.Message.ToUpper().Contains("WSM10"))
                            {
                                if (message.Message.Contains("on"))
                                {
                                    string[] e = message.Message.Split(new string[] { "on" }, StringSplitOptions.None);
                                    var player = Misc.GetClosestPlayerByName(e[1]);
                                    Networker.Instance.spawnMimicOnPlayerServerRPC(player.actualClientId, 10);
                                }
                                else
                                {
                                    Networker.Instance.spawnMimicOnPlayerServerRPC(Misc.GetRandomAlivePlayer().actualClientId, 10);
                                }
                            }
                            else if (message.Message.ToUpper().Contains("MIMIC") && message.Message.ToUpper().Contains("SPAWN"))
                            {
                                if (message.Message.Contains("on"))
                                {
                                    string[] e = message.Message.Split(new string[] { "on" }, StringSplitOptions.None);
                                    var player = Misc.GetClosestPlayerByName(e[1]);
                                    Networker.Instance.spawnMimicOnPlayerServerRPC(player.actualClientId, 1);
                                    Networker.Instance.cooldownStartServerRPC();
                                }
                                else
                                {
                                    Networker.Instance.spawnMimicOnPlayerServerRPC(Misc.GetRandomAlivePlayer().actualClientId, 1);
                                    Networker.Instance.cooldownStartServerRPC();
                                }
                            }
                        }
                    }
                }
            }
            if (enableChatEvents.Value == false) return;
            if (message.Message.StartsWith("BALD"))
            {
                string topValue = StartOfRound.Instance?.localPlayerController.playerUsername+"'s Twitch Chat:";
                if (twitchChatEveryone.Value)
                {
                    Networker.Instance.sendMessageAllClientRPC(topValue, message.User.DisplayName + ": " +  message.Message[5..], false);
                }
                else
                {
                    Networker.Instance.sendMessageSpecificServerRPC(topValue, message.Message[5..], false, StartOfRound.Instance?.localPlayerController.playerUsername);
                }
            }
        }

        private static void OnSubHandler(TwitchSubEvent subEvent)
        {
            if(!SDBBZRMain.canDoTwitch) return;
            if (!enableSubs.Value) return;
            int mimicsToSpawn = 1;
            if (subEvent.Tier == com.github.zehsteam.TwitchChatAPI.Enums.SubTier.One)
            {
                //tier 1 sub
                mimicsToSpawn = t1sub.Value;
            }
            else if (subEvent.Tier == com.github.zehsteam.TwitchChatAPI.Enums.SubTier.Two)
            {
                //tier 2 sub
                mimicsToSpawn = t2sub.Value;
            }
            else if(subEvent.Tier == com.github.zehsteam.TwitchChatAPI.Enums.SubTier.Three)
            {
                //tier 3 sub
                mimicsToSpawn = t3sub.Value;
            }
            Networker.Instance.spawnMimicOnPlayerServerRPC(StartOfRound.Instance.localPlayerController.actualClientId, mimicsToSpawn);
        }
        private static void OnCheerHandler(TwitchCheerEvent cheerEvent)
        {
            if(!SDBBZRMain.canDoTwitch) return;
            if (!enableCheer.Value) return;
            if (cheerEvent.CheerAmount == 329)
            {
                //ReviveAllMimics (coming soon lol) 
            }
            else if (cheerEvent.CheerAmount >= cheerMin.Value)
            {
                int toSpawn = cheerEvent.CheerAmount/cheerMin.Value;
                if (toSpawn > 25) toSpawn = 25;
                if(toSpawn <= 0) toSpawn = 1;
                ulong playerToSpawn = StartOfRound.Instance.localPlayerController.actualClientId;
                if (StartOfRound.Instance.localPlayerController.isPlayerDead)
                {
                    playerToSpawn = Misc.GetRandomAlivePlayer().actualClientId;
                }
                Networker.Instance.spawnMimicOnPlayerServerRPC(playerToSpawn, toSpawn);
            }
        }

        private static void OnRaidHandler(TwitchRaidEvent raidEvent)
        {
            if (!SDBBZRMain.canDoTwitch) return;
            if (!enableRaid.Value) return;
            int toSpawn = raidEvent.ViewerCount;
            if(toSpawn > 2)
            {
                if (toSpawn > 20) toSpawn = 20;
                Networker.Instance.spawnMimicOnPlayerServerRPC(StartOfRound.Instance.localPlayerController.actualClientId, toSpawn);
            }
        }
    }
}
