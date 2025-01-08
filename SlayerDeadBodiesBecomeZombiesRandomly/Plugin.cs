﻿using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
namespace SlayerDeadBodiesBecomeZombiesRandomly
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("ainavt.lc.lethalconfig", BepInDependency.DependencyFlags.SoftDependency)]
    public class SDBBZRMain : BaseUnityPlugin
    {
        private const string modGUID = "Slayer6409.NightOfTheLivingMimic";
        private const string modName = "NightOfTheLivingMimic";
        private const string modVersion = "1.0.5";
        private readonly Harmony harmony = new Harmony(modGUID);
        public static ManualLogSource CustomLogger;
        public static bool LethalConfigPresent = false;
        public static ConfigFile BepInExConfig = null;
        public static ConfigEntry<float> percentChance;
        public static ConfigEntry<int> timer;
        public static ConfigEntry<bool> continuous;
        public static ConfigEntry<int> chanceDecrease;
        public static ConfigEntry<bool> maskTurn;
        public static ConfigEntry<bool> funnyMode;
        public static ConfigEntry<bool> chaosMode;
        public static AssetBundle LoadedAssets;
        public static GameObject NetworkerPrefab;

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
            funnyMode = BepInExConfig.Bind(
                "Zombie",
                "Funny Mode",
                false,
                "Do Funny Mask Animation");
            chaosMode = BepInExConfig.Bind(
                "Zombie",
                "Chaos",
                false,
                "Don't do this");
        }

        private void Awake()
        {
            Logger.LogFatal($@"
                                              
           ..                   ..           
         .#@@%###*+-.    .-+*%%%@%*.         
        =%@@@@@@@@#@@@@@@@@@@@@@@@##:        
       =#@@@@@@@@@@@@@@@@@@@@@@@@@%#+:       
      :*%%@@@@@@@%%%*#@@@@@@@%%@@@@#*+.      
      +#@@*.     -@@@@@@@@@:     .##*+:      
      +#@%        -@@@@@@@-       .**+:      
     .+#@++@@@@%+: #@@@@@@.=@@@@@@++*+-      
      =#@@@@@@@@@@@@@@@@%@@@@@@@@@@#++:      
      -####%%#%Glitch is Bald@%#**#**+.      
      .+++++++=-=*@@@@@@@@%*=-*%#+++=-       
      .+*##%@@%%#*+*%@@%#*==*@@@@%*++:       
       =#%@%@@%#%%*-+-===*+*@@@@%*++=.       
       :#@%#####%@@@@*-=+@@@@@@%#*-+=        
       .*##**##=%@@@@%@@@@@@%-%%#+-=:        
        =######* :#%@@%@@%#: .###+==.        
        :*#####@.    ..:.   .####+=-         
        .+#####@.           .####+=.         
         :*++*+*.           +*+#++-.         
          =*++##+**%#%%%@##**+**++:          
           :++*+##@#%@@@@%##*+++=:           
             :+++##%@@@@@%#++++:             
               .-=+*****#*+-=-.              
                  .:-=====:..                
                               
                 
"); 
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig"))
                LethalConfigPresent = true;
            BepInExConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, "NightOfTheLivingMimic.cfg"), true);
            ModConfig();
            NetcodeWeaver(); 
            LoadedAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "sdbbzr"));
            NetworkerPrefab = LoadedAssets.LoadAsset<GameObject>("Networker");
            NetworkerPrefab.AddComponent<Networker>();
            harmony.PatchAll();
            if (LethalConfigPresent) ConfigManager.setupLethalConfig();

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