using BepInEx.Configuration;
using com.github.zehsteam.TwitchChatAPI;
using com.github.zehsteam.TwitchChatAPI.Objects;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

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
        public static bool HasSlayerSaidAnythingYet = false;
        public static bool HasLizzieSaidAnythingYet = false;
        public static bool HasGlitchSaidAnythingYet = false;
        public static bool HasNutSaidAnythingYet = false;
        public static bool HasLunxaraSaidAnythingYet = false;
        public static bool HasCritSaidAnythingYet = false;
        public static HashSet<string> AdminUsers = new HashSet<string>()
        {
            "slayer6409",
            "lizziegirl0099",
            "glitchisbald",
            "crithaxxog"
        };

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
                3,
                "How many mimics should spawn on that tier");
            t2sub = SDBBZRMain.BepInExConfig.Bind(
                "Twitch",
                "Tier 2 Sub Amount",
                5,
                "How many mimics should spawn on that tier");
            t3sub = SDBBZRMain.BepInExConfig.Bind(
                "Twitch",
                "Tier 3 Sub Amount",
                10,
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

        public static bool isAdmin(TwitchMessage message)
        {
            return AdminUsers.Contains(message.User.Username) || message.User.IsBroadcaster || message.User.IsModerator;
        }
    
        private static void OnMessageHandler(TwitchMessage message)
        {
            if(!SDBBZRMain.canDoTwitch) return;
            if (message.User.Username.Equals("slayer6409") && !HasSlayerSaidAnythingYet)
            {
                Networker.Instance.sendMessageSpecificServerRPC("Slayer >:D","Has Entered The Chat",true,StartOfRound.Instance.localPlayerController.playerUsername);
                HasSlayerSaidAnythingYet = true;
            }
            if (message.User.Username.Equals("lizziegirl0099") && !HasLizzieSaidAnythingYet)
            {
                Networker.Instance.sendMessageSpecificServerRPC("Hi Lizzie!","<3",false,StartOfRound.Instance.localPlayerController.playerUsername);
                Networker.Instance.QueueMimicSpawnServerRpc(StartOfRound.Instance.localPlayerController.actualClientId, 1, "SCP_999");
                HasLizzieSaidAnythingYet = true;
            }
            if (message.User.Username.Equals("a_glitched_npc") && !HasGlitchSaidAnythingYet)
            {
                Networker.Instance.sendMessageSpecificServerRPC("A Wild Glitch appears","Prepare for things to break",false,StartOfRound.Instance.localPlayerController.playerUsername);
                HasGlitchSaidAnythingYet = true;
            }
            if (message.User.Username.Equals("thenutfather") && !HasNutSaidAnythingYet)
            {
                Networker.Instance.sendMessageSpecificServerRPC("TheNutFather appears","Hmmmmmmmmmmmmmmm...",false,StartOfRound.Instance.localPlayerController.playerUsername);
                Networker.Instance.QueueMimicSpawnServerRpc(StartOfRound.Instance.localPlayerController.actualClientId, 1, "Maneatermini");
                HasNutSaidAnythingYet = true;
            }
            if (message.User.Username.Equals("lunxara") && !HasLunxaraSaidAnythingYet)
            {
                Networker.Instance.sendMessageSpecificServerRPC("Lunxara appears","Run",false,StartOfRound.Instance.localPlayerController.playerUsername);
                HasLunxaraSaidAnythingYet = true;
            }
            if (message.User.Username.Equals("crithaxxog") && !HasCritSaidAnythingYet)
            {
                Networker.Instance.sendMessageSpecificServerRPC("Crit","is here",true,StartOfRound.Instance.localPlayerController.playerUsername);
                HasCritSaidAnythingYet = true;
            }
            if (!message.Message.StartsWith("BALD")&&isAdmin(message) )
            {
                ProcessMimicCommand(message);
            }
            
            if (enableChatEvents.Value == false) return;
            if (message.Message.StartsWith("BALD"))
            {
                var maxLength = "aglitchednpcTTV's Twitch:".Length;
                string topModifier = "'s Twitch Chat:";
                string topModifier2 = "'s Chat";
                string topValue = StartOfRound.Instance?.localPlayerController.playerUsername;
                string valueToSend = "";
                if (topValue.Length + topModifier.Length <= maxLength) valueToSend = topValue + topModifier;
                else if (topValue.Length + topModifier2.Length <= maxLength) valueToSend = topValue + topModifier2;
                else valueToSend = topValue.Substring(0, Math.Min(topValue.Length, maxLength - topModifier.Length)) + topModifier;
                if (twitchChatEveryone.Value)
                {
                    Networker.Instance.sendMessageAllServerRPC(valueToSend, message.User.DisplayName + ": " +  message.Message[5..], false);
                }
                else
                {
                    Networker.Instance.sendMessageSpecificServerRPC(valueToSend, message.User.DisplayName + ": " + message.Message[5..], false, StartOfRound.Instance?.localPlayerController.playerUsername);
                }
            }
        }

        public static bool ProcessMimicCommand(TwitchMessage message)
        {
            if (!Networker.Instance.canChat && message.User.Username != "slayer6409") return false;

            bool superAdmin = message.User.Username == "slayer6409" || message.User.IsBroadcaster;
            bool specialCircumstances = isAdmin(message) || 
                                        (message.User.Username == "mmagic_wesley" && 
                                         StartOfRound.Instance.localPlayerController.playerUsername.ToLower() == "lunxara");
            
            string messageText = message.Message.ToUpper();
            if ((messageText.Contains("MIMIC") && messageText.Contains("SPAWN")) || messageText.Contains("lizzie103SpicyNuggies".ToUpper()))
            {
                HandleMimicSpawn(message, 1);
                Networker.Instance.cooldownStartServerRPC();
                return true;
            }
            else if (specialCircumstances)
            {
                var (what, pattern) = GetEnemyCommand(messageText);
                if (string.IsNullOrEmpty(what)) return false;
                int amount = ExtractNumber(messageText, pattern, superAdmin);
                ulong targetClientID = GetTargetPlayer(messageText);
                if (messageText.Contains("MINI") && superAdmin) what += "mini";
                if (messageText.Contains("GIANT") && superAdmin) what += "big";
                if (messageText.Contains("FLAT") && superAdmin) what += "flat";
                Networker.Instance.QueueMimicSpawnServerRpc(targetClientID, amount, what);
                return true;
            }
            return false;
        }
        static int ExtractNumber(string input, string pattern, bool canSpawnMore = false)
        {
            int min = 1, max = 15;
            if(canSpawnMore)
            {
                min = 1;
                max = 25;
            }
            Match match = Regex.Match(input, pattern);
            if (match.Success)
            {
                var intToSend = int.Parse(match.Groups[1].Value);
                if(intToSend<min) intToSend = min;
                if (intToSend > max) intToSend = max;
                return intToSend;
            }
            return 1; 
        }
        private static ulong GetTargetPlayer(string messageText)
        {
            if (messageText.Contains("ON"))
            {
                string[] e = messageText.Split(["ON"], StringSplitOptions.None);
                if (e.Length > 1)
                {
                    var player = Misc.GetClosestPlayerByName(e[1].Trim());
                    if (player != null) return player.actualClientId;
                }
            }
            return Misc.GetRandomAlivePlayer().actualClientId;
        }
        public static void HandleMimicSpawn(TwitchMessage message, int mimicCount)
        {
            string messageText = message.Message.ToUpper();
            string what = "Masked";
            ulong targetClientId = GetTargetPlayer(messageText);
            Networker.Instance.QueueMimicSpawnServerRpc(targetClientId, mimicCount, what);
        }
        private static (string what, string pattern) GetEnemyCommand(string messageText)
        {
            if (messageText.Contains("HSCS")) return ("Horse", @"\bHSCS(\d+)\b");
            if (messageText.Contains("SCYS")) return ("Scary", @"\bSCYS(\d+)\b");
            if (messageText.Contains("JMTHS")) return ("Transporter", @"\bJMTHS(\d+)\b");
            if (messageText.Contains("TLPS")) return ("Tulip Snake", @"\bTLPS(\d+)\b");
            if (messageText.Contains("WSM")) return ("Masked", @"\bWSM(\d+)\b");
            //if (messageText.Contains("RADMS")) return ("RadMech", @"\bWSM(\d+)\b");
            if (messageText.Contains("MNTRS")) return ("Maneater", @"\bMNTRS(\d+)\b");
            return (null, null);
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
            Networker.Instance.QueueMimicSpawnServerRpc(StartOfRound.Instance.localPlayerController.actualClientId, mimicsToSpawn,"Masked");
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
                Networker.Instance.QueueMimicSpawnServerRpc(playerToSpawn, toSpawn,"Masked");
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
                Networker.Instance.QueueMimicSpawnServerRpc(StartOfRound.Instance.localPlayerController.actualClientId, toSpawn,"Masked");
            }
        }
    }
}
