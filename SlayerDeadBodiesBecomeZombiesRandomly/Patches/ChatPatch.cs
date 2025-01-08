using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlayerDeadBodiesBecomeZombiesRandomly.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class ChatPatch
    {
        private const string BaseCommand = "=="; 
        private static readonly HashSet<ulong> AllowedSteamIds = new HashSet<ulong>
        {
            76561198077184650,
            76561199094139351,
            76561198164429786,
            76561198984467725
        };

        [HarmonyPatch("SubmitChat_performed")]
        [HarmonyPrefix]
        private static bool ChatStuff(HUDManager __instance)
        {

            // Get the current chat text from the HUDManager's chat input field
            string chatMessage = __instance.chatTextField.text;
            bool isWarning = false;
            // Check if the message starts with the BaseCommand
            if (chatMessage.StartsWith(BaseCommand))
            {
                if (chatMessage.StartsWith(BaseCommand + "!")) isWarning = true;
                if (!GameNetworkManager.Instance.disableSteam)
                {
                    if (!AllowedSteamIds.Contains(__instance.localPlayer.playerSteamId))
                        return true; 
                }
                    
                string commandContent = chatMessage.Substring(BaseCommand.Length).Trim();
                if (isWarning) commandContent = commandContent.Substring(1).Trim();
                // Check if the commandContent contains '&'
                if (commandContent.Contains("&"))
                {
                    // Split the commandContent by '&'
                    string[] parts = commandContent.Split('&');
                    if (parts.Length == 2)
                    {
                        string part1 = parts[0].Trim();
                        string part2 = parts[1].Trim();

                        Networker.Instance.sendMessageAllServerRPC(part1, part2, isWarning);
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
    }
}
