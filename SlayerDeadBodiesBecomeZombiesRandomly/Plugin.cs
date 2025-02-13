using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
namespace SlayerDeadBodiesBecomeZombiesRandomly
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("ainavt.lc.lethalconfig", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.github.zehsteam.TwitchChatAPI", BepInDependency.DependencyFlags.SoftDependency)]
    public class SDBBZRMain : BaseUnityPlugin
    {
        private const string modGUID = "Slayer6409.NightOfTheLivingMimic";
        private const string modName = "NightOfTheLivingMimic";
        private const string modVersion = "1.1.8";
        private readonly Harmony harmony = new Harmony(modGUID);
        public static ManualLogSource CustomLogger;
        public static bool LethalConfigPresent = false;
        public static bool TwitchChatAPIPresent = false;
        public static ConfigFile BepInExConfig = null;
        public static ConfigEntry<float> percentChance;
        public static ConfigEntry<int> timer;
        public static ConfigEntry<int> DebugCooldown;
        public static ConfigEntry<bool> continuous;
        public static ConfigEntry<bool> ShowDebugChatboxes;
        public static ConfigEntry<int> chanceDecrease;
        public static ConfigEntry<bool> maskTurn;
        //public static ConfigEntry<bool> funnyMode;
        public static ConfigEntry<bool> chaosMode;
        public static AssetBundle LoadedAssets;
        public static GameObject NetworkerPrefab;
        public static ConfigEntry<string> CursedPlayers;
        public static ConfigEntry<string> allowedChatUsersConfig;
        public static ConfigEntry<bool> twitchBlacklistMode; 
        public static ConfigEntry<string> allowedTwitchUsersConfig;
        public static List<ulong> CursedPlayersList;
        public static bool DoubleCurseGlitch = false; 
        public static bool canDoTwitch = false;

        
        public static HashSet<ulong> allowedTwitchUsers = new HashSet<ulong>
        {
            76561198077184650, //Me
            76561199094139351, //Lizzie
            76561198984467725 //Glitch
        };
        public static HashSet<ulong> deniedTwitchUsers = new HashSet<ulong>
        {
        };
        public static HashSet<ulong> allowedChatUsers = new HashSet<ulong>
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
        public static readonly HashSet<ulong> SuperCursedIDS = new HashSet<ulong>
        {
            76561198984467725, //Glitch
            76561198086325047, //lunxara
            76561198383757792, //Wesley
            76561198044500483  //S1ckboy
        };

        public static void ModConfig()
        {
            
            percentChance = BepInExConfig.Bind(
                "Zombie",
                "Percent Chance",
                7.5f,
                "What percent chance will it change into a zombie");
            continuous = BepInExConfig.Bind(
                "Zombie",
                "Continuous",
                true,
                "If a body can become a zombie after killing the zombie again");
            timer = BepInExConfig.Bind(
                "Zombie",
                "Timer",
                10,
                "How often does it try to become a zombie");
            chanceDecrease = BepInExConfig.Bind(
                "Zombie",
                "Chance Decrease",
                1,
                "Decreases the chance of spawning additional zombies from the same body by this percent");
            maskTurn = BepInExConfig.Bind(
                "Zombie",
                "Masked Turn Back",
                true,
                "If Masked enemies also come back alive");
            //funnyMode = BepInExConfig.Bind(
            //    "Zombie",
            //    "Funny Mode",
            //    false,
            //    "Do Funny Mask Animation");
            chaosMode = BepInExConfig.Bind(
                "Zombie",
                "Chaos",
                false,
                "Don't do this");
            CursedPlayers = BepInExConfig.Bind(
                "Misc",
                "Cursed Players",
                "",
                "Steam IDs to curse players separated by a comma");
            ShowDebugChatboxes = BepInExConfig.Bind(
                "Misc",
                "Show Debug Chatboxes",
                true,
                "Makes it to where you can see the Debug Chatboxes or not");
            DebugCooldown = BepInExConfig.Bind(
                "Misc",
                "Debug Cooldown",
                10,
                "How long the cooldown for the debug tool is in seconds");
            allowedChatUsersConfig = BepInExConfig.Bind(
                "Misc",
                "Allowed Chat Steam IDs",
                "",
                "SteamIDs of users who are allowed to use the chat function");
            allowedTwitchUsersConfig = BepInExConfig.Bind(
                "Twitch",
                "Trusted Steam UserIDs for Twitch Integration",
                "",
                "Put Steam IDs of users who are allowed to use the Twitch Integration here.\nThis exists to prevent trolling and free uses of all Mimic commands (Host Only)\nHost will still be able to use Twitch Integration even if they are not in this config.");
            twitchBlacklistMode = BepInExConfig.Bind(
                "Twitch",
                "Blacklist Mode",
                false,
                "Turn the Whitelist above into a blacklist");
        }

        private void Awake()
        {
            CustomLogger = base.Logger;
            Logger.LogFatal($@"
                                              
           ..                   ..           
         .#@@@%###*+-.    .-+*%%%@@%*.         
        =%@@@@@@@@#@@@@@@@@@@@@@@@@@##:        
       =#@@@@@@@@@@@@@@@@@@@@@@@@@@@%#+:       
      :*%%@@@@@@@%%%*#@@@@@@@@@%%@@@@#*+.      
      +#@@@*.     -@@@@@@@@@:     .###*+:      
      +#@@%        -@@@@@@@@-       .***+:      
     .+#@++@@@@@%+: #@@@@@@@.=@@@@@@@++*+-      
      =#@@@@@@@@@@@@@@@@@@%@@@@@@@@@@@#++:      
      -######%%#%Glitch is Bald%##***#**+.      
      .++++++++=-=*@@@@@@@@@%*=-*%#++++#=-       
      .+*##%@@@%%#*+*%@@@%#*==*@@@@@%*#++:       
       =#%@%@@@%#%%*-+-====*+*@@@@@%*#++=.       
       :#@%######%@@@@@*-=+@@@@@@@%##*-+=        
       .*##**###=%@@@@@%@@@@@@@%-%%##+-=:        
        =#######* :#%@@%@@@%#:  .####+==.        
        :*######@.     ..:.    .#####+=-         
        .+#######@.           .#####+=.         
         :*++++*++*.         .+*+##++-.         
          =*++###+**%#%%%@###**+**++:          
           :++*+###@#%@@@@%###*+++=:           
             :+++###%@@@@@%##++++:             
               .-=+=*****##*+-=-.              
                  .:-=======:..                
                               
                 
"); 
            
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig"))
                LethalConfigPresent = true;
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.github.zehsteam.TwitchChatAPI"))
                TwitchChatAPIPresent = true;
            BepInExConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, "NightOfTheLivingMimic.cfg"), true);
            ModConfig();
            if (TwitchChatAPIPresent) TwitchHandler.doConfigStuff();
            NetcodeWeaver(); 
            LoadedAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "sdbbzr"));
            NetworkerPrefab = LoadedAssets.LoadAsset<GameObject>("Networker");
            NetworkerPrefab.AddComponent<Networker>();
            harmony.PatchAll();
            if (LethalConfigPresent) ConfigManager.SetupLethalConfig();
            CursedPlayersList = CursedPlayers.Value
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(id => ulong.TryParse(id.Trim(), out var steamId) ? steamId : (ulong?)null)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();
            var users = allowedChatUsersConfig.Value
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(id => ulong.TryParse(id.Trim(), out var steamId) ? steamId : (ulong?)null)
                .Where(id => id.HasValue)
                .Select(id => id.Value);
            allowedChatUsers.UnionWith(users);
            var twitchUsers = allowedTwitchUsersConfig.Value
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(id => ulong.TryParse(id.Trim(), out var steamId) ? steamId : (ulong?)null)
                .Where(id => id.HasValue)
                .Select(id => id.Value);

            if (twitchBlacklistMode.Value)
            {
                deniedTwitchUsers.UnionWith(twitchUsers);
            }
            else
            {
                allowedTwitchUsers.UnionWith(twitchUsers);
            }
            //if (CursedPlayersList.Contains(76561198077184650) || CursedPlayersList.Contains(76561198984467725))
            //{
            //    if(CursedPlayersList.Contains(76561198984467725))
            //    {
            //        Logger.LogWarning("Why did you double curse yourself? Are you Bald?");
            //        DoubleCurseGlitch = true;
            //    }
            //    if (CursedPlayersList.Contains(76561198077184650))
            //    {
            //        Logger.LogWarning("Why did you add me to the cursed players, now you're double cursed. And you are bald");
            //        DoubleCurseGlitch = true;
            //    }
            //}
            foreach(var player in SuperCursedIDS)
            {
                if(!CursedPlayersList.Contains((ulong)player)) CursedPlayersList.Add((ulong)player);
            }
            if (SDBBZRMain.TwitchChatAPIPresent) TwitchHandler.Initialize();
        }
        private static void NetcodeWeaver()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }
}