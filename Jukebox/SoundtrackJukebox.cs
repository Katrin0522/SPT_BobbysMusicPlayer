using System;
using BobbysMusicPlayer.Models;
using EFT;
using UnityEngine;

namespace BobbysMusicPlayer.Jukebox
{
    public class SoundtrackJukebox : MonoBehaviour
    {
        internal static bool soundtrackCalled = false;
        internal static bool inRaid = false; 
        private static int trackCounter = 0;
        internal static Coroutine soundtrackCoroutine;
        private static bool paused = false;
        private static float pausedTime = 0f;
        public static void PlaySoundtrack()
        {
            if (!soundtrackCalled || Audio.soundtrackAudioSource.isPlaying || paused || Audio.spawnAudioSource.isPlaying || BobbysMusicPlayerPlugin.ambientTrackArray.IsNullOrEmpty() || !BobbysMusicPlayerPlugin.HasFinishedLoadingAudio)
            {
                return;
            }
            if (BobbysMusicPlayerPlugin.ambientTrackArray.Count == 1)
            {
                trackCounter = 0;
            }
            Audio.soundtrackAudioSource.clip = BobbysMusicPlayerPlugin.ambientTrackArray[trackCounter];
            Audio.soundtrackAudioSource.Play();
            BobbysMusicPlayerPlugin.LogSource.LogInfo("Playing " + BobbysMusicPlayerPlugin.ambientTrackNamesArray[trackCounter]);
            trackCounter++;
            soundtrackCoroutine = StaticManager.Instance.WaitSeconds(Audio.soundtrackAudioSource.clip.length, new Action(PlaySoundtrack));
            if (trackCounter >= BobbysMusicPlayerPlugin.ambientTrackArray.Count)
            {
                trackCounter = 0;
            }
        }
        // This method is responsible for handling the "Jukebox" controls of the Ambient Soundtrack
        public static void SoundtrackControls()
        {
            if (Audio.spawnAudioSource.isPlaying)
            {
                return;
            }
            if (Input.GetKeyDown(SettingsModel.Instance.PauseTrack.Value.MainKey) && Audio.soundtrackAudioSource.isPlaying)
            {
                Audio.soundtrackAudioSource.Pause();
                StaticManager.Instance.StopCoroutine(soundtrackCoroutine);
                pausedTime = Audio.soundtrackAudioSource.clip.length - Audio.soundtrackAudioSource.time;
                paused = true;
            }
            else if (Input.GetKeyDown(SettingsModel.Instance.PauseTrack.Value.MainKey) && paused)
            {
                Audio.soundtrackAudioSource.UnPause();
                soundtrackCoroutine = StaticManager.Instance.WaitSeconds(pausedTime, new Action(PlaySoundtrack));
                paused = false;
            }
            if (Input.GetKeyDown(SettingsModel.Instance.RestartTrack.Value.MainKey))
            {
                Audio.soundtrackAudioSource.Stop();
                if (trackCounter != 0)
                {
                    trackCounter--;
                }
                else
                {
                    trackCounter = BobbysMusicPlayerPlugin.ambientTrackArray.Count - 1;
                }
                StaticManager.Instance.StopCoroutine(soundtrackCoroutine);
                paused = false;
                PlaySoundtrack();
            }
            if (Input.GetKeyDown(SettingsModel.Instance.PreviousTrack.Value.MainKey))
            {
                Audio.soundtrackAudioSource.Stop();
                trackCounter -= 2;
                if (trackCounter < 0)
                {
                    trackCounter = BobbysMusicPlayerPlugin.ambientTrackArray.Count + (trackCounter);
                }
                StaticManager.Instance.StopCoroutine(soundtrackCoroutine);
                paused = false;
                PlaySoundtrack();
            }
            if (Input.GetKeyDown(SettingsModel.Instance.SkipTrack.Value.MainKey))
            {
                Audio.soundtrackAudioSource.Stop();
                StaticManager.Instance.StopCoroutine(soundtrackCoroutine);
                paused = false;
                PlaySoundtrack();
            }
        }
    }
}