using System;
using System.Collections.Generic;
using System.IO;
using EFT.UI;
using UnityEngine;

namespace BobbysMusicPlayer;

public static class GlobalData
{
    private static string MapSpecialDir = AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\BobbysMusicPlayer\\Soundtrack\\map_specific_soundtrack";
    
    public static Dictionary<string, string[]> MapDictionary = new Dictionary<string, string[]>
    {
        ["RezervBase"] = Directory.GetFiles(MapSpecialDir + "\\reserve"),
        ["bigmap"] = Directory.GetFiles(MapSpecialDir + "\\customs"),
        ["factory4_night"] = Directory.GetFiles(MapSpecialDir + "\\factory"),
        ["factory4_day"] = Directory.GetFiles(MapSpecialDir + "\\factory"),
        ["Interchange"] = Directory.GetFiles(MapSpecialDir + "\\interchange"),
        ["laboratory"] = Directory.GetFiles(MapSpecialDir + "\\labs"),
        ["Shoreline"] = Directory.GetFiles(MapSpecialDir + "\\shoreline"),
        ["Sandbox"] = Directory.GetFiles(MapSpecialDir + "\\ground_zero"),
        ["Sandbox_high"] = Directory.GetFiles(MapSpecialDir + "\\ground_zero"),
        ["Woods"] = Directory.GetFiles(MapSpecialDir + "\\woods"),
        ["Lighthouse"] = Directory.GetFiles(MapSpecialDir + "\\lighthouse"),
        ["TarkovStreets"] = Directory.GetFiles(MapSpecialDir + "\\streets")
    };
    
    /// <summary>
    /// UISoundsDir makes it easy to get the contents of each subfolder of UISounds.
    /// BobbysMusicPlayerPlugin.Awake uses it in a foreach loop.
    /// </summary>
    public static string[] UISoundsDir = new string[8]
    {
        "QuestCompleted", "QuestFailed", "QuestFinished", "QuestStarted", "QuestSubtaskComplete", "DeathSting", "ErrorSound", "TradeSound"
    };
    
    public static Dictionary<EUISoundType, int> UISoundDictionary = new Dictionary<EUISoundType, int>
    {
        [EUISoundType.QuestCompleted] = 0,
        [EUISoundType.QuestFailed] = 1,
        [EUISoundType.QuestFinished] = 2,
        [EUISoundType.QuestStarted] = 3,
        [EUISoundType.QuestSubTrackComplete] = 4,
        [EUISoundType.PlayerIsDead] = 5,
        [EUISoundType.ErrorMessage] = 6,
        [EUISoundType.TradeOperationComplete] = 7
    };

    public static Dictionary<string, AudioType> AudioTypes = new Dictionary<string, AudioType>
    {
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
    
    public static Dictionary<EnvironmentType, float> EnvironmentDict = new()
    {
        { EnvironmentType.Outdoor, 1f },
        { EnvironmentType.Indoor, 1f }
    };
}