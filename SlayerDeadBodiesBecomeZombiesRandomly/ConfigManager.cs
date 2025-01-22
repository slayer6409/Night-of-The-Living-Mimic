using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;
using LethalConfig;
using BepInEx.Configuration;

namespace SlayerDeadBodiesBecomeZombiesRandomly
{
    internal class ConfigManager
    {
        public static void setupLethalConfig()
        {
            var percentChanceSlider = new FloatSliderConfigItem(SDBBZRMain.percentChance, new FloatSliderOptions
            {
                Min = 0.1f,
                Max = 100
            });

            var TimerIntField = new IntInputFieldConfigItem(SDBBZRMain.timer, false);

            var continuousBoolField = new BoolCheckBoxConfigItem(SDBBZRMain.continuous, false);

            var chanceDecreaseIntField = new IntInputFieldConfigItem(SDBBZRMain.chanceDecrease, false);

            var maskTurnBoolField = new BoolCheckBoxConfigItem(SDBBZRMain.maskTurn, false);

            //var funnyModeBoolField = new BoolCheckBoxConfigItem(SDBBZRMain.funnyMode, false);

            var chaosModeBoolField = new BoolCheckBoxConfigItem(SDBBZRMain.chaosMode, false);

            LethalConfigManager.AddConfigItem(percentChanceSlider);
            LethalConfigManager.AddConfigItem(TimerIntField);
            LethalConfigManager.AddConfigItem(continuousBoolField);
            LethalConfigManager.AddConfigItem(chanceDecreaseIntField);
            LethalConfigManager.AddConfigItem(maskTurnBoolField);
            //LethalConfigManager.AddConfigItem(funnyModeBoolField);
            LethalConfigManager.AddConfigItem(chaosModeBoolField);
        }


    }
}
