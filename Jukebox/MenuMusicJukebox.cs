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
            if (SoundtrackJukebox.soundtrackCalled || audio.menuMusicAudioSource == null) return;
            
            
            if (Input.GetKeyDown(SettingsModel.Instance.PauseTrack.Value.MainKey) && audio.menuMusicAudioSource.isPlaying)
            {
                audio.menuMusicAudioSource.Pause();
                StaticManager.Instance.StopCoroutine(menuMusicCoroutine);
                pausedTime = audio.menuMusicAudioSource.clip.length - audio.menuMusicAudioSource.time;
                paused = true;
            }
            else if (Input.GetKeyDown(SettingsModel.Instance.PauseTrack.Value.MainKey) && paused)
            {
                audio.menuMusicAudioSource.UnPause();
                menuMusicCoroutine = StaticManager.Instance.WaitSeconds(pausedTime, new Action(Singleton<GUISounds>.Instance.method_3));
                paused = false;
            }
            
            if (Input.GetKeyDown(SettingsModel.Instance.RestartTrack.Value.MainKey))
            {
                audio.menuMusicAudioSource.Stop();
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
                audio.menuMusicAudioSource.Stop();
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
                audio.menuMusicAudioSource.Stop();
                StaticManager.Instance.StopCoroutine(menuMusicCoroutine);
                paused = false;
                Singleton<GUISounds>.Instance.method_3();
            }
        }
    }
}