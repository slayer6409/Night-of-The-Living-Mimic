using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using LethalConfig;

namespace SlayerDeadBodiesBecomeZombiesRandomly
{
    internal class ConfigManager
    {
        public static void SetupLethalConfig()
        {
            var percentChanceSlider = new FloatSliderConfigItem(SDBBZRMain.percentChance, new FloatSliderOptions
            {
                Min = 0.1f,
                Max = 100
            });

            var timerIntField = new IntInputFieldConfigItem(SDBBZRMain.timer, false);

            var continuousBoolField = new BoolCheckBoxConfigItem(SDBBZRMain.continuous, false);

            var chanceDecreaseIntField = new IntInputFieldConfigItem(SDBBZRMain.chanceDecrease, false);

            var maskTurnBoolField = new BoolCheckBoxConfigItem(SDBBZRMain.maskTurn, false);

            var chaosModeBoolField = new BoolCheckBoxConfigItem(SDBBZRMain.chaosMode, false);

            var showDebugChatboxesBoolField = new BoolCheckBoxConfigItem(SDBBZRMain.ShowDebugChatboxes, false);

            LethalConfigManager.AddConfigItem(percentChanceSlider);
            LethalConfigManager.AddConfigItem(timerIntField);
            LethalConfigManager.AddConfigItem(continuousBoolField);
            LethalConfigManager.AddConfigItem(chanceDecreaseIntField);
            LethalConfigManager.AddConfigItem(maskTurnBoolField);
            LethalConfigManager.AddConfigItem(showDebugChatboxesBoolField);
            LethalConfigManager.AddConfigItem(chaosModeBoolField);

            //var funnyModeBoolField = new BoolCheckBoxConfigItem(SDBBZRMain.funnyMode, false);
            //LethalConfigManager.AddConfigItem(funnyModeBoolField);

            if (SDBBZRMain.TwitchChatAPIPresent)
            {
                var enableSubBoolField = new BoolCheckBoxConfigItem(TwitchHandler.enableSubs);
                var t1SubIntField = new IntInputFieldConfigItem(TwitchHandler.t1sub);
                var t2SubIntField = new IntInputFieldConfigItem(TwitchHandler.t2sub);
                var t3SubIntField = new IntInputFieldConfigItem(TwitchHandler.t3sub);
                var enableCheerBoolField = new BoolCheckBoxConfigItem(TwitchHandler.enableCheer);
                var cheerMinIntField = new IntInputFieldConfigItem(TwitchHandler.cheerMin);
                var enableRaidBoolField = new BoolCheckBoxConfigItem(TwitchHandler.enableRaid);
                var enableChatBoolField = new BoolCheckBoxConfigItem(TwitchHandler.enableChatEvents);
                var twitchChatEveryoneBoolField = new BoolCheckBoxConfigItem(TwitchHandler.twitchChatEveryone);

                LethalConfigManager.AddConfigItem(enableSubBoolField);
                LethalConfigManager.AddConfigItem(t1SubIntField);
                LethalConfigManager.AddConfigItem(t2SubIntField);
                LethalConfigManager.AddConfigItem(t3SubIntField);
                LethalConfigManager.AddConfigItem(enableCheerBoolField);
                LethalConfigManager.AddConfigItem(cheerMinIntField);
                LethalConfigManager.AddConfigItem(enableRaidBoolField);
                LethalConfigManager.AddConfigItem(enableChatBoolField);
                LethalConfigManager.AddConfigItem(twitchChatEveryoneBoolField);
            }
        }


    }
}
