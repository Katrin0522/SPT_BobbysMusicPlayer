using SPT.Reflection.Patching;
using EFT.UI;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace BobbysMusicPlayer.Patches
{
    public class RaidEndMusicPatch : ModulePatch
    {
        internal static List<string> deathMusicList = new List<string>();
        internal static List<string> extractMusicList = new List<string>();
        private static Dictionary<EEndGameSoundType, List<string>> raidEndDictionary = new Dictionary<EEndGameSoundType, List<string>> 
        {
            [EEndGameSoundType.ArenaLose] = deathMusicList,
            [EEndGameSoundType.ArenaWin] = extractMusicList
        };
        private static AudioClip raidEndClip;
        private static BobbysMusicPlayerPlugin bobbysMusicPlayerPlugin = new BobbysMusicPlayerPlugin();
        //TODO: Move all audio action into separate class

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(UISoundsWrapper), nameof(UISoundsWrapper.GetEndGameClip));
        }

        private static void LoadNextTrack(EEndGameSoundType soundType)
        {
            string raidEndTrack = raidEndDictionary[soundType][BobbysMusicPlayerPlugin.rand.Next(raidEndDictionary[soundType].Count)];
            raidEndClip = bobbysMusicPlayerPlugin.RequestAudioClip(raidEndTrack);
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