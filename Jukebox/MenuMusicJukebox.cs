using System;
using BobbysMusicPlayer.Models;
using BobbysMusicPlayer.Patches;
using BobbysMusicPlayer.Utils;
using Comfort.Common;
using EFT;
using EFT.UI;
using UnityEngine;

namespace BobbysMusicPlayer.Jukebox
{
    public class MenuMusicJukebox : MonoBehaviour
    {
        internal static Coroutine menuMusicCoroutine;
        private static bool paused;
        private static float pausedTime;
        
        /// <summary>
        /// This method is responsible for handling the "Jukebox" controls of the Main Menu
        /// </summary>
        public static void CheckMenuMusicControls()
        {
            AudioManager audio = BobbysMusicPlayerPlugin.Instance.GetAudio();
            if (SoundtrackJukebox.soundtrackCalled || audio.MenuMusicAudioSource == null) return;
            
            
            if (Input.GetKeyDown(SettingsModel.Instance.PauseTrack.Value.MainKey) && audio.MenuMusicAudioSource.isPlaying)
            {
                audio.MenuMusicAudioSource.Pause();
                StaticManager.Instance.StopCoroutine(menuMusicCoroutine);
                pausedTime = audio.MenuMusicAudioSource.clip.length - audio.MenuMusicAudioSource.time;
                paused = true;
            }
            else if (Input.GetKeyDown(SettingsModel.Instance.PauseTrack.Value.MainKey) && paused)
            {
                audio.MenuMusicAudioSource.UnPause();
                menuMusicCoroutine = StaticManager.Instance.WaitSeconds(pausedTime, new Action(Singleton<GUISounds>.Instance.method_3));
                paused = false;
            }
            
            if (Input.GetKeyDown(SettingsModel.Instance.RestartTrack.Value.MainKey))
            {
                audio.MenuMusicAudioSource.Stop();
                if (MenuMusicPatch.trackCounter != 0)
                {
                    MenuMusicPatch.trackCounter--;
                }
                else
                {
                    MenuMusicPatch.trackCounter = MenuMusicPatch.trackArray.Count - 1;
                }
                StaticManager.Instance.StopCoroutine(menuMusicCoroutine);
                paused = false;
                Singleton<GUISounds>.Instance.method_3();
            }
            
            if (Input.GetKeyDown(SettingsModel.Instance.PreviousTrack.Value.MainKey))
            {
                audio.MenuMusicAudioSource.Stop();
                MenuMusicPatch.trackCounter -= 2;
                if (MenuMusicPatch.trackCounter < 0)
                {
                    MenuMusicPatch.trackCounter = MenuMusicPatch.trackArray.Count + (MenuMusicPatch.trackCounter);
                }
                StaticManager.Instance.StopCoroutine(menuMusicCoroutine);
                paused = false;
                Singleton<GUISounds>.Instance.method_3();
            }
            
            if (Input.GetKeyDown(SettingsModel.Instance.SkipTrack.Value.MainKey))
            {
                audio.MenuMusicAudioSource.Stop();
                StaticManager.Instance.StopCoroutine(menuMusicCoroutine);
                paused = false;
                Singleton<GUISounds>.Instance.method_3();
            }
        }
    }
}