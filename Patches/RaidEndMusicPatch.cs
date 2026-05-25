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
        internal static List<string> DeathMusicList = new();
        internal static List<string> ExtractMusicList = new();
        private static Dictionary<EEndGameSoundType, List<string>> raidEndDictionary = new() {
            [EEndGameSoundType.ArenaWin] = ExtractMusicList,
            [EEndGameSoundType.ArenaLose] = DeathMusicList
        };
        
        private static AudioClip raidEndClip;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(UISoundsWrapper), nameof(UISoundsWrapper.GetEndGameClip));
        }

        [PatchPrefix]
        static bool Prefix(ref AudioClip __result, EEndGameSoundType soundType)
        {
            if (!raidEndDictionary.TryGetValue(soundType, out List<string> tracks) || tracks.IsNullOrEmpty())
            {
                return true;
            }
            
            LoadNextTrack(soundType, tracks);
            
            if (raidEndClip != null)
            {
                __result = raidEndClip;
                return false;
            }
            
            return true;
        }
        
        private static void LoadNextTrack(EEndGameSoundType soundType, List<string> tracks)
        {
            string raidEndTrack = tracks[Range(0, tracks.Count)];
            raidEndClip = AudioManager.RequestAudioClip(raidEndTrack);
            
            string trackName = Path.GetFileName(raidEndTrack);
            BobbysMusicPlayerPlugin.LogSource.LogInfo(trackName + " assigned to " + soundType);
        }
    }
}
