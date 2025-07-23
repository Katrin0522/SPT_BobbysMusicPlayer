using System;
using BobbysMusicPlayer.Models;
using BobbysMusicPlayer.Utils;
using EFT;
using UnityEngine;

namespace BobbysMusicPlayer.Jukebox
{
    public class SoundtrackJukebox : MonoBehaviour
    {
        internal static bool soundtrackCalled = false;
        internal static bool inRaid = false; 
        private static int trackCounter;
        internal static Coroutine soundtrackCoroutine;
        private static bool paused;
        private static float pausedTime;
        
        public static void PlaySoundtrack()
        {
            if (!soundtrackCalled || AudioManager.soundtrackAudioSource.isPlaying || paused || AudioManager.spawnAudioSource.isPlaying || AudioManager.ambientTrackArray.IsNullOrEmpty() || !AudioManager.HasFinishedLoadingAudio)
            {
                return;
            }
            
            if (AudioManager.ambientTrackArray.Count == 1)
            {
                trackCounter = 0;
            }
            
            AudioManager.soundtrackAudioSource.clip = AudioManager.ambientTrackArray[trackCounter];
            AudioManager.soundtrackAudioSource.Play();
            BobbysMusicPlayerPlugin.LogSource.LogInfo("Playing " + AudioManager.ambientTrackNamesArray[trackCounter]);
            trackCounter++;
            soundtrackCoroutine = StaticManager.Instance.WaitSeconds(AudioManager.soundtrackAudioSource.clip.length, PlaySoundtrack);
            
            if (trackCounter >= AudioManager.ambientTrackArray.Count)
            {
                trackCounter = 0;
            }
        }
        
        /// <summary>
        /// This method is responsible for handling the "Jukebox" controls of the Ambient Soundtrack
        /// </summary>
        public static void SoundtrackControls()
        {
            if (AudioManager.spawnAudioSource.isPlaying) return;
            
            if (Input.GetKeyDown(SettingsModel.Instance.PauseTrack.Value.MainKey) && AudioManager.soundtrackAudioSource.isPlaying)
            {
                AudioManager.soundtrackAudioSource.Pause();
                StaticManager.Instance.StopCoroutine(soundtrackCoroutine);
                pausedTime = AudioManager.soundtrackAudioSource.clip.length - AudioManager.soundtrackAudioSource.time;
                paused = true;
            }
            else if (Input.GetKeyDown(SettingsModel.Instance.PauseTrack.Value.MainKey) && paused)
            {
                AudioManager.soundtrackAudioSource.UnPause();
                soundtrackCoroutine = StaticManager.Instance.WaitSeconds(pausedTime, PlaySoundtrack);
                paused = false;
            }
            
            if (Input.GetKeyDown(SettingsModel.Instance.RestartTrack.Value.MainKey))
            {
                AudioManager.soundtrackAudioSource.Stop();
                
                if (trackCounter != 0)
                {
                    trackCounter--;
                }
                else
                {
                    trackCounter = AudioManager.ambientTrackArray.Count - 1;
                }
                
                StaticManager.Instance.StopCoroutine(soundtrackCoroutine);
                paused = false;
                PlaySoundtrack();
            }
            
            if (Input.GetKeyDown(SettingsModel.Instance.PreviousTrack.Value.MainKey))
            {
                AudioManager.soundtrackAudioSource.Stop();
                trackCounter -= 2;
                
                if (trackCounter < 0)
                {
                    trackCounter = AudioManager.ambientTrackArray.Count + (trackCounter);
                }
                
                StaticManager.Instance.StopCoroutine(soundtrackCoroutine);
                paused = false;
                PlaySoundtrack();
            }
            
            if (Input.GetKeyDown(SettingsModel.Instance.SkipTrack.Value.MainKey))
            {
                AudioManager.soundtrackAudioSource.Stop();
                StaticManager.Instance.StopCoroutine(soundtrackCoroutine);
                paused = false;
                PlaySoundtrack();
            }
        }
    }
}