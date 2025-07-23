using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BobbysMusicPlayer.Models;
using BobbysMusicPlayer.Patches;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.Random;

namespace BobbysMusicPlayer.Utils
{
    public class AudioManager
    {
        public static AudioSource soundtrackAudioSource;
        public static AudioSource spawnAudioSource;
        public static AudioSource combatAudioSource;
        public static AudioSource menuMusicAudioSource;
        
        public static bool HasStartedLoadingAudio = false;
        public static bool HasFinishedLoadingAudio = false;
        public static bool spawnTrackHasPlayed = false;
        
        private static List<string> combatMusicTrackList = new List<string>();
        private static List<string> ambientTrackListToPlay = new List<string>();
        private static List<string> spawnTrackList = new List<string>();
        private static List<string> defaultTrackList = new List<string>();
        
        private static List<AudioClip> combatMusicClipList = new List<AudioClip>();
        private static List<AudioClip> spawnTrackClipList = new List<AudioClip>();
        internal static List<string> ambientTrackNamesArray = new List<string>();
        internal static List<AudioClip> ambientTrackArray = new List<AudioClip>();
        
        internal static float lerp = 0f;
        internal static float combatTimer = 0f;
        internal static float soundtrackVolume = 0f;
        internal static float spawnMusicVolume = 0f;
        internal static float combatMusicVolume = 0f;
        private static float headsetMultiplier = 1f;

        public void Init(GameObject mainObj)
        {
            if (soundtrackAudioSource == null)
            {
                soundtrackAudioSource = mainObj.AddComponent<AudioSource>();
                spawnAudioSource = mainObj.AddComponent<AudioSource>();
                combatAudioSource = mainObj.AddComponent<AudioSource>();
                BobbysMusicPlayerPlugin.LogSource.LogWarning("AudioSources added to game");
            }

            LoadMusic();
        }

        public void Update()
        {
            
        }
        
        /// <summary>
        /// Load paths for all music 
        /// </summary>
        private void LoadMusic()
        {
            MenuMusicPatch.menuTrackList.AddRange(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\BobbysMusicPlayer\\CustomMenuMusic\\sounds"));
            
            //This if statement exists just in case some people install outdated music packs by mistake
            if (MenuMusicPatch.menuTrackList.IsNullOrEmpty() && Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\CustomMenuMusic\\sounds"))
            {
                MenuMusicPatch.menuTrackList.AddRange(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\CustomMenuMusic\\sounds"));
            }
            
            defaultTrackList.AddRange(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\BobbysMusicPlayer\\Soundtrack\\default_soundtrack"));
            if (defaultTrackList.IsNullOrEmpty() && Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\Soundtrack\\sounds"))
            {
                defaultTrackList.AddRange(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\Soundtrack\\sounds"));
            }
            
            combatMusicTrackList.AddRange(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\BobbysMusicPlayer\\Soundtrack\\combat_music"));
            
            spawnTrackList.AddRange(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\BobbysMusicPlayer\\Soundtrack\\spawn_music"));
            
            RaidEndMusicPatch.deathMusicList.AddRange(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\BobbysMusicPlayer\\DeathMusic"));
            RaidEndMusicPatch.extractMusicList.AddRange(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\BobbysMusicPlayer\\ExtractMusic"));
            
            int counter = 0;
            foreach (var dir in GlobalData.UISoundsDir)
            {
                // Each element of uiSounds is a List of strings so that users can add as few or as many sounds as they want to a given folder
                UISoundsPatch.uiSounds[counter] = new List<string>();
                UISoundsPatch.uiSounds[counter].AddRange(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\BobbysMusicPlayer\\UISounds\\" + dir));
                counter++;
            }
        }
        
        /// <summary>
        /// This method gets called once when loading into a raid. It makes sure every AudioClip is ready to play in the raid
        /// </summary>
        public async void PrepareRaidAudioClips()
        {
            if (!HasStartedLoadingAudio)
            {
                HasStartedLoadingAudio = true;
                
                if (!defaultTrackList.IsNullOrEmpty())
                {
                    LoadAmbientSoundtrackClips();
                }
                
                if (!spawnTrackList.IsNullOrEmpty())
                {
                    spawnTrackClipList.Clear();
                    foreach (var track in spawnTrackList)
                    {
                        spawnTrackClipList.Add(await AsyncRequestAudioClip(track));
                        BobbysMusicPlayerPlugin.LogSource.LogInfo("RequestAudioClip called for spawnTrackClip");
                    }
                    spawnTrackHasPlayed = false;
                }
                
                if (!combatMusicTrackList.IsNullOrEmpty())
                {
                    BobbysMusicPlayerPlugin.LogSource.LogInfo("Load music to combat");
                    
                    // The next 4 lines prevent any issues that could be caused by exiting a raid before the combat timer ends
                    combatTimer = 0f;
                    lerp = 0;
                    combatAudioSource.Stop();
                    combatAudioSource.loop = false;
                    
                    combatMusicClipList.Clear();
                    foreach (var track in combatMusicTrackList)
                    {
                        combatMusicClipList.Add(await AsyncRequestAudioClip(track));
                    }
                    
                    combatAudioSource.clip = combatMusicClipList[Range(0, combatMusicClipList.Count)];
                    BobbysMusicPlayerPlugin.LogSource.LogInfo($"Music in combat loaded! {combatAudioSource.clip.length}");
                }
            }
        }
        
        public void SetClip(AudioSource audiosource, AudioClip clip)
        {
            audiosource.clip = clip;
        }
        
        public void AdjustVolume(AudioSource audiosource, float volume)
        {
            audiosource.volume = volume;
        }
        
        /// <summary>
        /// Dynamic adjust volume
        /// </summary>
        public void VolumeSetter()
        {
            // Next two lines are taken from Fontaine's Realism Mod. Credit to him
            CompoundItem headwear = Singleton<GameWorld>.Instance.MainPlayer.Equipment.GetSlot(EquipmentSlot.Headwear).ContainedItem as CompoundItem;
            HeadphonesItemClass headset = Singleton<GameWorld>.Instance.MainPlayer.Equipment.GetSlot(EquipmentSlot.Earpiece).ContainedItem as HeadphonesItemClass ?? ((headwear != null) ? headwear.GetAllItemsFromCollection().OfType<HeadphonesItemClass>().FirstOrDefault<HeadphonesItemClass>() : null);
            if (headset != null)
            {
                headsetMultiplier = SettingsModel.Instance.HeadsetMultiplier.Value;
            }
            else
            {
                headsetMultiplier = 1f;
            }
            
            // Each of the in-raid AudioSources' volumes are calculated by multiplying their configurable volumes with the indoor multiplier and the headset multiplier.
            // TODO: Fix sharp switching of Environment
            soundtrackVolume = SettingsModel.Instance.SoundtrackVolume.Value * GlobalData.EnvironmentDict[Singleton<GameWorld>.Instance.MainPlayer.Environment] * headsetMultiplier;
            spawnMusicVolume = SettingsModel.Instance.SpawnMusicVolume.Value * GlobalData.EnvironmentDict[Singleton<GameWorld>.Instance.MainPlayer.Environment] * headsetMultiplier;
            combatMusicVolume = SettingsModel.Instance.CombatMusicVolume.Value * GlobalData.EnvironmentDict[Singleton<GameWorld>.Instance.MainPlayer.Environment] * headsetMultiplier;

            // We check if the combat AudioSource is playing so that the CombatLerp method can do its job adjusting the ambient soundtrack and spawn music AudioSources
            if (!combatAudioSource.isPlaying)
            {
                    AdjustVolume(soundtrackAudioSource, soundtrackVolume);
                    AdjustVolume(spawnAudioSource, spawnMusicVolume);
            }
            if (lerp >= 1)
            {
                AdjustVolume(combatAudioSource, combatMusicVolume);
            }   
        }
        
        /// <summary>
        /// Process combat music side 
        /// </summary>
        public void CombatMusic()
        {
            if (!combatMusicTrackList.IsNullOrEmpty())
            {
                if (combatTimer > 0)
                {
                    if (!combatAudioSource.isPlaying && combatAudioSource.loop == false)
                    {
                        combatAudioSource.loop = true;
                        combatAudioSource.Play();
                    }
                    if (lerp <= 1)
                    {
                        CombatLerp();
                        lerp += Time.deltaTime / SettingsModel.Instance.CombatInFader.Value;
                    }
                    combatTimer -= Time.deltaTime;
                }
                // When the combat timer runs out, the AudioSource will only stop playing once it's done fading out.
                // If the player re-enters combat in the middle of fading out, the music will smoothly fade back in from the volume it faded out to.
                else if (combatTimer <= 0)
                {
                    if (combatAudioSource.isPlaying)
                    {
                        combatTimer = 0f;
                        CombatLerp();
                        lerp -= Time.deltaTime / SettingsModel.Instance.CombatOutFader.Value;
                        if (lerp <= 0)
                        {
                            combatAudioSource.loop = false;
                            combatAudioSource.Stop();
                            // The combat AudioSource's clip will be randomly selected each time the combat music stops
                            combatAudioSource.clip = combatMusicClipList[Range(0, combatMusicClipList.Count)];
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Play sound when spawn
        /// </summary>
        public void PlaySpawnMusic()
        {
            if (!spawnTrackHasPlayed && !spawnTrackClipList.IsNullOrEmpty())
            {
                spawnAudioSource.clip = spawnTrackClipList[Range(0, spawnTrackClipList.Count)];
                BobbysMusicPlayerPlugin.LogSource.LogInfo("spawnAudioSource.clip assigned to spawnTrackClip");
                spawnAudioSource.Play();
                BobbysMusicPlayerPlugin.LogSource.LogInfo("spawnAudioSource playing");
                spawnTrackHasPlayed = true;
            }
        }
        
        /// <summary>
        /// Process combat music lerp side 
        /// </summary>
        public void CombatLerp()
        {
            AdjustVolume(combatAudioSource, Mathf.Lerp(0f, combatMusicVolume, lerp));
            AdjustVolume(soundtrackAudioSource, Mathf.Lerp(soundtrackVolume, SettingsModel.Instance.AmbientCombatMultiplier.Value*soundtrackVolume, lerp));
            AdjustVolume(spawnAudioSource, Mathf.Lerp(spawnMusicVolume, SettingsModel.Instance.AmbientCombatMultiplier.Value*spawnMusicVolume, lerp));
        }
        
        /// <summary>
        /// Load ambient OST
        /// </summary>
        private async void LoadAmbientSoundtrackClips()
        {
            float totalLength = 0f;
            HasFinishedLoadingAudio = false;
            ambientTrackArray.Clear();
            ambientTrackNamesArray.Clear();
            ambientTrackListToPlay.Clear();
            float targetLength = 60f * SettingsModel.Instance.SoundtrackLength.Value;
            BobbysMusicPlayerPlugin.LogSource.LogInfo("Map is " + Singleton<GameWorld>.Instance.MainPlayer.Location + ".");
            if (GlobalData.MapDictionary[Singleton<GameWorld>.Instance.MainPlayer.Location].IsNullOrEmpty() || SettingsModel.Instance.SoundtrackPlaylist.Value == ESoundtrackPlaylist.DefaultPlaylistOnly)
            {
                ambientTrackListToPlay.AddRange(defaultTrackList);
            }
            else if (SettingsModel.Instance.SoundtrackPlaylist.Value == ESoundtrackPlaylist.CombinedPlaylists)
            {
                ambientTrackListToPlay.AddRange(defaultTrackList);
                ambientTrackListToPlay.AddRange(GlobalData.MapDictionary[Singleton<GameWorld>.Instance.MainPlayer.Location]);
            }
            else if (SettingsModel.Instance.SoundtrackPlaylist.Value == ESoundtrackPlaylist.MapSpecificPlaylistOnly)
            {
                ambientTrackListToPlay.AddRange(GlobalData.MapDictionary[Singleton<GameWorld>.Instance.MainPlayer.Location]);
            }
            while ((totalLength < targetLength) && (!ambientTrackListToPlay.IsNullOrEmpty()))
            {
                int nextRandom = Range(0, ambientTrackListToPlay.Count);
                string track = ambientTrackListToPlay[nextRandom];
                string trackName = Path.GetFileName(track);
                AudioClip unityAudioClip = await AsyncRequestAudioClip(track);
                ambientTrackArray.Add(unityAudioClip);
                ambientTrackNamesArray.Add(trackName);
                ambientTrackListToPlay.Remove(track);
                
                // Adding the length of each track to totalLength makes sure that the mod loads the minimum number of random tracks to meet the target length.
                totalLength += ambientTrackArray.Last().length;
                BobbysMusicPlayerPlugin.LogSource.LogInfo(trackName + " has been loaded and added to playlist");
            }
            HasFinishedLoadingAudio = true;
        }
        
        /// <summary>
        /// Async load audio file as AudioClip
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static async Task<AudioClip> AsyncRequestAudioClip(string path)
        {
            string extension = Path.GetExtension(path);
            
            UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, GlobalData.AudioTypes[extension.ToLower()]);
            UnityWebRequestAsyncOperation sendWeb = uwr.SendWebRequest();

            while (!sendWeb.isDone)
                await Task.Yield();
            
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                BobbysMusicPlayerPlugin.LogSource.LogError($"Soundtrack: Failed To Fetch Audio Clip by path -> '{path}'");
                return null;
            }

            AudioClip audioclip = DownloadHandlerAudioClip.GetContent(uwr);
            return audioclip;
        }
        
        /// <summary>
        /// Sync load audio file as AudioClip
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static AudioClip RequestAudioClip(string path)
        {
            string extension = Path.GetExtension(path);
            
            UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, GlobalData.AudioTypes[extension.ToLower()]);
            UnityWebRequestAsyncOperation sendWeb = uwr.SendWebRequest();

            while (!sendWeb.isDone)
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    BobbysMusicPlayerPlugin.LogSource.LogError("Soundtrack: Failed To Fetch Audio Clip");
                    return null;
                }
            AudioClip audioclip = DownloadHandlerAudioClip.GetContent(uwr);
            return audioclip;
        }
    }
}