using SPT.Reflection.Patching;
using EFT.UI;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BobbysMusicPlayer.Utils;
using UnityEngine;
using HarmonyLib;
using static UnityEngine.Random;

namespace BobbysMusicPlayer.Patches
{
    public class RaidEndMusicPatch : ModulePatch
    {
        internal static List<string> deathMusicList = new();
        internal static List<string> extractMusicList = new();
        private static Dictionary<EEndGameSoundType, List<string>> raidEndDictionary = new()
        {
            [EEndGameSoundType.ArenaLose] = deathMusicList,
            [EEndGameSoundType.ArenaWin] = extractMusicList
        };
        
        private static AudioClip raidEndClip;

        //TODO: Move all audio action into separate class
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(UISoundsWrapper), nameof(UISoundsWrapper.GetEndGameClip));
        }

        private static void LoadNextTrack(EEndGameSoundType soundType)
        {
            string raidEndTrack = raidEndDictionary[soundType][Range(0, raidEndDictionary[soundType].Count)];
            raidEndClip = AudioManager.RequestAudioClip(raidEndTrack);
            
            string trackName = Path.GetFileName(raidEndTrack);
            Logger.LogInfo(trackName + " assigned to " + soundType);
        }

        [PatchPrefix]
        static bool Prefix(ref AudioClip __result, EEndGameSoundType soundType)
        {
            if (raidEndDictionary[soundType].IsNullOrEmpty())
            {
                return true;
            }
            
            LoadNextTrack(soundType);
            
            if (raidEndClip != null)
            {
                __result = raidEndClip;
                return false;
            }
            
            return true;
        }
    }
}