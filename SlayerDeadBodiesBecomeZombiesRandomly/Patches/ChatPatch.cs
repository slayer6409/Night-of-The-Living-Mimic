using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SlayerDeadBodiesBecomeZombiesRandomly.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class ChatPatch
    {
        internal static bool canDoThing = true;
        private const string BaseCommand = "=="; 
        private static readonly HashSet<ulong> AllowedSteamIds = new HashSet<ulong>
        {
            76561198077184650, //Me
            76561199094139351, //Lizzie
            76561198164429786, //Rodrigo
            76561198984467725, //Glitch
            76561198044500483, //s1ckboy
            76561198383757792, //Wesley
            76561198086325047, //lunxara
            76561198993437314, //funo
            76561198399127090  //Xu
        };
        [HarmonyPatch("SubmitChat_performed")]
        [HarmonyPrefix]
        private static bool ChatStuff(HUDManager __instance)
        {
            // Get the current chat text from the HUDManager's chat input field
            string chatMessage = __instance.chatTextField.text;
            bool isWarning = false;
            bool isForced = false;

            // Check if the message starts with the BaseCommand
            if (chatMessage.StartsWith(BaseCommand))
            {
                if (chatMessage.StartsWith(BaseCommand + "!")) isWarning = true;
                if (chatMessage.StartsWith(BaseCommand + "=!")) { isWarning = true; isForced = true; }
                if (chatMessage.StartsWith(BaseCommand + "=")) isForced = true;

                if (!(__instance.localPlayer.playerSteamId == 76561198077184650 || __instance.localPlayer.IsHost)) isForced = false;

                if (!GameNetworkManager.Instance.disableSteam)
                {
                    if (!AllowedSteamIds.Contains(__instance.localPlayer.playerSteamId))
                    {
                        if (!__instance.localPlayer.IsHost) return true;
                    }
                }

                string commandContent = chatMessage.Substring(BaseCommand.Length).Trim();
                if (isWarning||(isForced&&!isWarning)) commandContent = commandContent.Substring(1).Trim();
                if (isWarning&&isForced) commandContent = commandContent.Substring(2).Trim();

                // Check if the commandContent contains '&'
                if (commandContent.Contains("&"))
                {
                    // Split the commandContent by '&'
                    string[] parts = commandContent.Split('&');
                    if (parts.Length >= 2)
                    {
                        string part1 = parts[0].Trim();
                        string part2 = parts[1].Trim();
                        string part3 = "null";

                        
                        if (part2.Contains("&"))
                        {
                            
                            string[] subParts = part2.Split("&");
                            part2 = subParts[0].Trim(); 
                            if (subParts.Length > 1)
                            {
                                part3 = subParts[1].Trim(); 
                            }
                        }

                        if (canDoThing)
                        {
                            var plr = Misc.GetClosestPlayerByName(part3);
                            if (!part2.Contains("&"))
                            {
                                Networker.Instance.sendMessageSpecificServerRPC(part1, part2, isWarning, plr.playerUsername);
                                if (__instance.localPlayer.playerSteamId != 76561198077184650 || __instance.localPlayer.playerSteamId != 76561199094139351)
                                {
                                    if (!__instance.localPlayer.IsHost)
                                    {
                                        canDoThing = false;
                                        StartOfRound.Instance.StartCoroutine(Timer());
                                    }
                                }
                            }
                            else
                            {
                                Networker.Instance.sendMessageAllServerRPC(part1, part2, isWarning, isForced);
                                if (__instance.localPlayer.playerSteamId != 76561198077184650)
                                {
                                    if (!__instance.localPlayer.IsHost)
                                    {
                                        canDoThing = false;
                                        StartOfRound.Instance.StartCoroutine(Timer());
                                    }
                                }
                            }
                        }
                        else
                        {
                            Misc.SafeTipMessage("Timer Error", "That is on Cooldown", true);
                        }
                    }
                    else
                    {
                        Misc.SafeTipMessage("Error", "Invalid Command Format");
                    }
                }
                else
                {
                    Misc.SafeTipMessage("Error", "Missing '&' in Command");
                }
                __instance.chatTextField.text = "";
            }
            return true;
        }

        public static IEnumerator Timer()
        {
            yield return new WaitForSeconds(SDBBZRMain.DebugCooldown.Value);
            canDoThing = true;
        }
    }
}
