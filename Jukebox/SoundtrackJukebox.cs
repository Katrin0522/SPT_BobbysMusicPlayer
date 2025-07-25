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
            if (!soundtrackCalled || audio.SoundtrackAudioSource.isPlaying || paused || audio.SpawnAudioSource.isPlaying || audio.AmbientTrackArray.IsNullOrEmpty() || !audio.HasFinishedLoadingAudio)
            {
                return;
            }
            
            if (audio.AmbientTrackArray.Count == 1)
            {
                trackCounter = 0;
            }
            
            audio.SoundtrackAudioSource.clip = audio.AmbientTrackArray[trackCounter];
            audio.SoundtrackAudioSource.Play();
            BobbysMusicPlayerPlugin.LogSource.LogInfo("Playing " + audio.AmbientTrackNamesArray[trackCounter]);
            trackCounter++;
            soundtrackCoroutine = StaticManager.Instance.WaitSeconds(audio.SoundtrackAudioSource.clip.length, PlaySoundtrack);
            
            if (trackCounter >= audio.AmbientTrackArray.Count)
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
            if (audio.SpawnAudioSource.isPlaying) return;
            
            if (Input.GetKeyDown(SettingsModel.Instance.PauseTrack.Value.MainKey) && audio.SoundtrackAudioSource.isPlaying)
            {
                audio.SoundtrackAudioSource.Pause();
                StaticManager.Instance.StopCoroutine(soundtrackCoroutine);
                pausedTime = audio.SoundtrackAudioSource.clip.length - audio.SoundtrackAudioSource.time;
                paused = true;
            }
            else if (Input.GetKeyDown(SettingsModel.Instance.PauseTrack.Value.MainKey) && paused)
            {
                audio.SoundtrackAudioSource.UnPause();
                soundtrackCoroutine = StaticManager.Instance.WaitSeconds(pausedTime, PlaySoundtrack);
                paused = false;
            }
            
            if (Input.GetKeyDown(SettingsModel.Instance.RestartTrack.Value.MainKey))
            {
                audio.SoundtrackAudioSource.Stop();
                
                if (trackCounter != 0)
                {
                    trackCounter--;
                }
                else
                {
                    trackCounter = audio.AmbientTrackArray.Count - 1;
                }
                
                StaticManager.Instance.StopCoroutine(soundtrackCoroutine);
                paused = false;
                PlaySoundtrack();
            }
            
            if (Input.GetKeyDown(SettingsModel.Instance.PreviousTrack.Value.MainKey))
            {
                audio.SoundtrackAudioSource.Stop();
                trackCounter -= 2;
                
                if (trackCounter < 0)
                {
                    trackCounter = audio.AmbientTrackArray.Count + (trackCounter);
                }
                
                StaticManager.Instance.StopCoroutine(soundtrackCoroutine);
                paused = false;
                PlaySoundtrack();
            }
            
            if (Input.GetKeyDown(SettingsModel.Instance.SkipTrack.Value.MainKey))
            {
                audio.SoundtrackAudioSource.Stop();
                StaticManager.Instance.StopCoroutine(soundtrackCoroutine);
                paused = false;
                PlaySoundtrack();
            }
        }
    }
}