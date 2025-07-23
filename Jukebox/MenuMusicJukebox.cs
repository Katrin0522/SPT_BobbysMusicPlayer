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
        private static bool paused = false;
        private static float pausedTime = 0f;
        
        /// <summary>
        /// This method is responsible for handling the "Jukebox" controls of the Main Menu
        /// </summary>
        public static void MenuMusicControls()
        {
            if (!SoundtrackJukebox.soundtrackCalled && AudioManager.menuMusicAudioSource != null)
            {
                if (Input.GetKeyDown(SettingsModel.Instance.PauseTrack.Value.MainKey) && AudioManager.menuMusicAudioSource.isPlaying)
                {
                    AudioManager.menuMusicAudioSource.Pause();
                    StaticManager.Instance.StopCoroutine(menuMusicCoroutine);
                    pausedTime = AudioManager.menuMusicAudioSource.clip.length - AudioManager.menuMusicAudioSource.time;
                    paused = true;
                }
                else if (Input.GetKeyDown(SettingsModel.Instance.PauseTrack.Value.MainKey) && paused)
                {
                    AudioManager.menuMusicAudioSource.UnPause();
                    menuMusicCoroutine = StaticManager.Instance.WaitSeconds(pausedTime, new Action(Singleton<GUISounds>.Instance.method_3));
                    paused = false;
                }
                if (Input.GetKeyDown(SettingsModel.Instance.RestartTrack.Value.MainKey))
                {
                    AudioManager.menuMusicAudioSource.Stop();
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
                    AudioManager.menuMusicAudioSource.Stop();
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
                    AudioManager.menuMusicAudioSource.Stop();
                    StaticManager.Instance.StopCoroutine(menuMusicCoroutine);
                    paused = false;
                    Singleton<GUISounds>.Instance.method_3();
                }
            }
        }
    }
}