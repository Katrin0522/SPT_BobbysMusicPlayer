using BobbysMusicPlayer.Models;
using BobbysMusicPlayer.Utils;
using EFT;
using UnityEngine;

namespace BobbysMusicPlayer.Jukebox
{
    public class SoundtrackJukebox : MonoBehaviour
    {
        private static Coroutine soundtrackCoroutine;
        
        internal static bool soundtrackCalled = false;
        private static bool paused;
        
        private static int trackCounter;
        private static float pausedTime;
        
        public static void PlaySoundtrack()
        {
            AudioManager audio = BobbysMusicPlayerPlugin.Instance.GetAudio();
            if (!soundtrackCalled || audio.soundtrackAudioSource.isPlaying || paused || audio.spawnAudioSource.isPlaying || audio.ambientTrackArray.IsNullOrEmpty() || !audio.HasFinishedLoadingAudio)
            {
                return;
            }
            
            if (audio.ambientTrackArray.Count == 1)
            {
                trackCounter = 0;
            }
            
            audio.soundtrackAudioSource.clip = audio.ambientTrackArray[trackCounter];
            audio.soundtrackAudioSource.Play();
            BobbysMusicPlayerPlugin.LogSource.LogInfo("Playing " + audio.ambientTrackNamesArray[trackCounter]);
            trackCounter++;
            soundtrackCoroutine = StaticManager.Instance.WaitSeconds(audio.soundtrackAudioSource.clip.length, PlaySoundtrack);
            
            if (trackCounter >= audio.ambientTrackArray.Count)
            {
                trackCounter = 0;
            }
        }
        
        /// <summary>
        /// This method is responsible for handling the "Jukebox" controls of the Ambient Soundtrack
        /// </summary>
        public static void CheckSoundtrackControls()
        {
            AudioManager audio = BobbysMusicPlayerPlugin.Instance.GetAudio();
            if (audio.spawnAudioSource.isPlaying) return;
            
            if (Input.GetKeyDown(SettingsModel.Instance.PauseTrack.Value.MainKey) && audio.soundtrackAudioSource.isPlaying)
            {
                audio.soundtrackAudioSource.Pause();
                StaticManager.Instance.StopCoroutine(soundtrackCoroutine);
                pausedTime = audio.soundtrackAudioSource.clip.length - audio.soundtrackAudioSource.time;
                paused = true;
            }
            else if (Input.GetKeyDown(SettingsModel.Instance.PauseTrack.Value.MainKey) && paused)
            {
                audio.soundtrackAudioSource.UnPause();
                soundtrackCoroutine = StaticManager.Instance.WaitSeconds(pausedTime, PlaySoundtrack);
                paused = false;
            }
            
            if (Input.GetKeyDown(SettingsModel.Instance.RestartTrack.Value.MainKey))
            {
                audio.soundtrackAudioSource.Stop();
                
                if (trackCounter != 0)
                {
                    trackCounter--;
                }
                else
                {
                    trackCounter = audio.ambientTrackArray.Count - 1;
                }
                
                StaticManager.Instance.StopCoroutine(soundtrackCoroutine);
                paused = false;
                PlaySoundtrack();
            }
            
            if (Input.GetKeyDown(SettingsModel.Instance.PreviousTrack.Value.MainKey))
            {
                audio.soundtrackAudioSource.Stop();
                trackCounter -= 2;
                
                if (trackCounter < 0)
                {
                    trackCounter = audio.ambientTrackArray.Count + (trackCounter);
                }
                
                StaticManager.Instance.StopCoroutine(soundtrackCoroutine);
                paused = false;
                PlaySoundtrack();
            }
            
            if (Input.GetKeyDown(SettingsModel.Instance.SkipTrack.Value.MainKey))
            {
                audio.soundtrackAudioSource.Stop();
                StaticManager.Instance.StopCoroutine(soundtrackCoroutine);
                paused = false;
                PlaySoundtrack();
            }
        }
    }
}