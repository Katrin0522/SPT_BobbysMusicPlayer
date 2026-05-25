using System.Collections.Generic;
using System.IO;
using EFT;
using EFT.UI;
using UnityEngine;

namespace BobbysMusicPlayer.Data
{
    public static class GlobalData
    {
        public static Dictionary<string, string[]> MapDictionary = new()
        {
            ["RezervBase"] = PathData.GetFilesOrEmpty(PathData.MapSpecialDir + "\\reserve"),
            ["bigmap"] = PathData.GetFilesOrEmpty(PathData.MapSpecialDir + "\\customs"),
            ["factory4_night"] = PathData.GetFilesOrEmpty(PathData.MapSpecialDir + "\\factory"),
            ["factory4_day"] = PathData.GetFilesOrEmpty(PathData.MapSpecialDir + "\\factory"),
            ["Interchange"] = PathData.GetFilesOrEmpty(PathData.MapSpecialDir + "\\interchange"),
            ["laboratory"] = PathData.GetFilesOrEmpty(PathData.MapSpecialDir + "\\labs"),
            ["Shoreline"] = PathData.GetFilesOrEmpty(PathData.MapSpecialDir + "\\shoreline"),
            ["Sandbox"] = PathData.GetFilesOrEmpty(PathData.MapSpecialDir + "\\ground_zero"),
            ["Sandbox_high"] = PathData.GetFilesOrEmpty(PathData.MapSpecialDir + "\\ground_zero"),
            ["Woods"] = PathData.GetFilesOrEmpty(PathData.MapSpecialDir + "\\woods"),
            ["Lighthouse"] = PathData.GetFilesOrEmpty(PathData.MapSpecialDir + "\\lighthouse"),
            ["TarkovStreets"] = PathData.GetFilesOrEmpty(PathData.MapSpecialDir + "\\streets")
        };
    
        /// <summary>
        /// UISoundsDir makes it easy to get the contents of each subfolder of UISounds.
        /// BobbysMusicPlayerPlugin.Awake uses it in a foreach loop.
        /// </summary>
        public static string[] UISoundsDir = new string[8] {
            "QuestCompleted", "QuestFailed", "QuestFinished", "QuestStarted", "QuestSubtaskComplete", "DeathSting", "ErrorSound", "TradeSound"
        };
    
        public static Dictionary<EUISoundType, int> UISoundDictionary = new() {
            [EUISoundType.QuestCompleted] = 0,
            [EUISoundType.QuestFailed] = 1,
            [EUISoundType.QuestFinished] = 2,
            [EUISoundType.QuestStarted] = 3,
            [EUISoundType.QuestSubTrackComplete] = 4,
            [EUISoundType.PlayerIsDead] = 5,
            [EUISoundType.ErrorMessage] = 6,
            [EUISoundType.TradeOperationComplete] = 7
        };

        public static Dictionary<string, AudioType> AudioTypes = new() {
            [".wav"] = AudioType.WAV,
            [".ogg"] = AudioType.OGGVORBIS,
            [".mp2"] = AudioType.MPEG,
            [".mp3"] = AudioType.MPEG,
            [".aiff"] = AudioType.AIFF,
            [".s3m"] = AudioType.S3M,
            [".it"] = AudioType.IT,
            [".mod"] = AudioType.MOD,
            [".xm"] = AudioType.XM,
            [".xma"] = AudioType.XMA,
            [".vag"] = AudioType.VAG
        };
    
        public static Dictionary<EnvironmentType, float> EnvironmentDict = new() {
            { EnvironmentType.Outdoor, 1f },
            { EnvironmentType.Indoor, 1f }
        };
        
        public static List<EDamageType> DamageTypeList = new()
        {
            EDamageType.Explosion,
            EDamageType.Blunt,
            EDamageType.Sniper,
            EDamageType.Bullet,
            EDamageType.Melee,
            EDamageType.Landmine
        };
    }
}
