using SPT.Reflection.Patching;
using EFT.UI;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace BobbysMusicPlayer.Patches
{
    public class UISoundsPatch : ModulePatch
    {
        internal static List<string>[] uiSounds = new List<string>[8];
        internal static List<AudioClip>[] uiSoundsClips = new List<AudioClip>[8];
        private static BobbysMusicPlayerPlugin bobbysMusicPlayerPlugin = new BobbysMusicPlayerPlugin();
        
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(UISoundsWrapper), nameof(UISoundsWrapper.GetUIClip));
        }
        
        [PatchPrefix]
        static bool Prefix(ref AudioClip __result, EUISoundType soundType)
        {
            if (!GlobalData.UISoundDictionary.ContainsKey(soundType))
            {
                return true;
            }
            var audioClipArray = uiSoundsClips[GlobalData.UISoundDictionary[soundType]];
            if (audioClipArray.IsNullOrEmpty())
            {
                return true;
            }
            
            // The sound that plays in game will be a randomly selected sound from the corresponding folder
            __result = audioClipArray[BobbysMusicPlayerPlugin.rand.Next(audioClipArray.Count)];
            return false;
        }
        
        /// <summary>
        /// Each element of uiSoundsClips is a List of AudioClips since we want players to be able to import as many sounds as they want per folder.
        /// </summary>
        internal static async void LoadUIClips()
        {
            int counter = 0;
            foreach (List<string> list in uiSounds)
            {
                uiSoundsClips[counter] = new List<AudioClip>();
                foreach (string track in list)
                {
                    uiSoundsClips[counter].Add(await bobbysMusicPlayerPlugin.AsyncRequestAudioClip(track));
                    BobbysMusicPlayerPlugin.LogSource.LogInfo(Path.GetFileName(track) + " assigned to " + GlobalData.UISoundsDir[counter]);
                }
                counter++;
            }
        }
    }
}