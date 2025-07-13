using BepInEx;
using BepInEx.Logging;
using BobbysMusicPlayer.Patches;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BobbysMusicPlayer.Jukebox;
using BobbysMusicPlayer.Models;
using UnityEngine;
using UnityEngine.Networking;
using HeadsetClass = HeadphonesItemClass;

namespace BobbysMusicPlayer
{
    [BepInPlugin("BobbyRenzobbi.MusicPlayer", "BobbysMusicPlayer", "1.3.0")]
    public class BobbysMusicPlayerPlugin : BaseUnityPlugin
    {
        private SettingsModel _settings;
        
        internal static ManualLogSource LogSource;
        internal static System.Random rand = new();
        
        public static bool InRaid { get; set; } = false;
        
        private static float lerp = 0f;
        internal static float combatTimer = 0f;
        private static float soundtrackVolume = 0f;
        private static float spawnMusicVolume = 0f;
        private static float combatMusicVolume = 0f;
        internal static float headsetMultiplier = 1f;
        
        internal static bool HasFinishedLoadingAudio = false;
        private static bool HasStartedLoadingAudio = false;
        private static bool spawnTrackHasPlayed = false;
        
        private static Dictionary<EnvironmentType, float> environmentDict = new Dictionary<EnvironmentType, float>();
        
        private static List<string> combatMusicTrackList = new List<string>();
        private static List<string> ambientTrackListToPlay = new List<string>();
        private static List<string> spawnTrackList = new List<string>();
        private static List<string> defaultTrackList = new List<string>();
        
        private static List<AudioClip> combatMusicClipList = new List<AudioClip>();
        private static List<AudioClip> spawnTrackClipList = new List<AudioClip>();
        internal static List<string> ambientTrackNamesArray = new List<string>();
        internal static List<AudioClip> ambientTrackArray = new List<AudioClip>();
        
        internal void Awake()
        {
            LogSource = Logger;
            LogSource.LogInfo("Plugin loading...");
            
            //Init config
            _settings = SettingsModel.Create(Config);

            //Moved music loading into a separate method
            LoadMusic();
            
            environmentDict[EnvironmentType.Outdoor] = 1f;
            
            new MenuMusicPatch().Enable();
            new RaidEndMusicPatch().Enable();
            new UISoundsPatch().Enable();
            new ShotAtPatch().Enable();
            new PlayerFiringPatch().Enable();
            new DamageTakenPatch().Enable();
            new ShotFiredNearPatch().Enable();
            new GrenadePatch().Enable();
            new MenuMusicMethod8Patch().Enable();
            new StopMenuMusicPatch().Enable();
            new OnGameWorldStartPatch().Enable();
            new OnGameWorldDisposePatch().Enable();
            MenuMusicPatch.LoadAudioClips();
            UISoundsPatch.LoadUIClips();
            
            LogSource.LogInfo("Plugin loaded!");
        }

        internal void Update()
        {
            MenuMusicJukebox.MenuMusicControls();
            if (!InRaid)
            {
                if (!MenuMusicPatch.HasReloadedAudio)
                {
                    MenuMusicPatch.LoadAudioClips();
                    UISoundsPatch.LoadUIClips();
                }
                SoundtrackJukebox.soundtrackCalled = false;
                HasStartedLoadingAudio = false;
                spawnTrackHasPlayed = false;
                return;
            }
            
            if (Singleton<GameWorld>.Instance.MainPlayer == null || Singleton<GameWorld>.Instance.MainPlayer.Location == "hideout")
            {
                return;
            }
            
            if (Audio.soundtrackAudioSource == null)
            {
                Audio.soundtrackAudioSource = gameObject.AddComponent<AudioSource>();
                Audio.spawnAudioSource = gameObject.AddComponent<AudioSource>();
                Audio.combatAudioSource = gameObject.AddComponent<AudioSource>();
                LogSource.LogWarning("AudioSources added to game");
            }
            MenuMusicPatch.HasReloadedAudio = false;
            PrepareRaidAudioClips();
            
            if (Singleton<AbstractGame>.Instance.Status != GameStatus.Started)
            {
                return;
            }
            CombatMusic();
            VolumeSetter();
            PlaySpawnMusic();
            SoundtrackJukebox.SoundtrackControls();
            SoundtrackJukebox.soundtrackCalled = true;
            SoundtrackJukebox.PlaySoundtrack();
        }

        /// <summary>
        /// Load paths for all music 
        /// </summary>
        private void LoadMusic()
        {
            MenuMusicPatch.menuTrackList.AddRange(Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\BobbysMusicPlayer\\CustomMenuMusic\\sounds"));
            
            //:Bobby hint:
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
        /// Async load audio file as AudioClip
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal async Task<AudioClip> AsyncRequestAudioClip(string path)
        {
            string extension = Path.GetExtension(path);
            Dictionary<string, AudioType> audioType = new Dictionary<string, AudioType>
            {
                [".wav"] = AudioType.WAV,
                [".ogg"] = AudioType.OGGVORBIS,
                [".mp2"] = AudioType.MPEG,
                [".mp3"] = AudioType.MPEG,
                [".aiff"] = AudioType.AIFF,
                [".s3m"] = AudioType.S3M,
                [".it"] = AudioType.IT,
                [".mod"] = AudioType.MOD,
                [".xm"] = AudioType.XM,
                [".xma"] = AudioType.XMA,
                [".vag"] = AudioType.VAG
            };
            UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, audioType[extension.ToLower()]);
            UnityWebRequestAsyncOperation sendWeb = uwr.SendWebRequest();

            while (!sendWeb.isDone)
                await Task.Yield();
            
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Logger.LogError($"Soundtrack: Failed To Fetch Audio Clip by path -> '{path}'");
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
        internal AudioClip RequestAudioClip(string path)
        {
            string extension = Path.GetExtension(path);
            Dictionary<string, AudioType> audioType = new Dictionary<string, AudioType>
            {
                [".wav"] = AudioType.WAV,
                [".ogg"] = AudioType.OGGVORBIS,
                [".mp2"] = AudioType.MPEG,
                [".mp3"] = AudioType.MPEG,
                [".aiff"] = AudioType.AIFF,
                [".s3m"] = AudioType.S3M,
                [".it"] = AudioType.IT,
                [".mod"] = AudioType.MOD,
                [".xm"] = AudioType.XM,
                [".xma"] = AudioType.XMA,
                [".vag"] = AudioType.VAG
            };
            UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, audioType[extension.ToLower()]);
            UnityWebRequestAsyncOperation sendWeb = uwr.SendWebRequest();

            while (!sendWeb.isDone)
                if (uwr.isNetworkError || uwr.isHttpError)
                {
                    Logger.LogError("Soundtrack: Failed To Fetch Audio Clip");
                    return null;
                }
            AudioClip audioclip = DownloadHandlerAudioClip.GetContent(uwr);
            return audioclip;
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
            LogSource.LogInfo("Map is " + Singleton<GameWorld>.Instance.MainPlayer.Location + ".");
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
                int nextRandom = rand.Next(ambientTrackListToPlay.Count);
                string track = ambientTrackListToPlay[nextRandom];
                string trackName = Path.GetFileName(track);
                AudioClip unityAudioClip = await AsyncRequestAudioClip(track);
                ambientTrackArray.Add(unityAudioClip);
                ambientTrackNamesArray.Add(trackName);
                ambientTrackListToPlay.Remove(track);
                // Adding the length of each track to totalLength makes sure that the mod loads the minimum number of random tracks to meet the target length.
                totalLength += ambientTrackArray.Last().length;
                LogSource.LogInfo(trackName + " has been loaded and added to playlist");
            }
            HasFinishedLoadingAudio = true;
        }
        
        /// <summary>
        /// Process combat music lerp side 
        /// </summary>
        private void CombatLerp()
        {
            Audio.AdjustVolume(Audio.combatAudioSource, Mathf.Lerp(0f, combatMusicVolume, lerp));
            Audio.AdjustVolume(Audio.soundtrackAudioSource, Mathf.Lerp(soundtrackVolume, SettingsModel.Instance.AmbientCombatMultiplier.Value*soundtrackVolume, lerp));
            Audio.AdjustVolume(Audio.spawnAudioSource, Mathf.Lerp(spawnMusicVolume, SettingsModel.Instance.AmbientCombatMultiplier.Value*spawnMusicVolume, lerp));
        }
        
        /// <summary>
        /// Process combat music side 
        /// </summary>
        private void CombatMusic()
        {
            if (!combatMusicTrackList.IsNullOrEmpty())
            {
                if (combatTimer > 0)
                {
                    if (!Audio.combatAudioSource.isPlaying && Audio.combatAudioSource.loop == false)
                    {
                        Audio.combatAudioSource.loop = true;
                        Audio.combatAudioSource.Play();
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
                    if (Audio.combatAudioSource.isPlaying)
                    {
                        combatTimer = 0f;
                        CombatLerp();
                        lerp -= Time.deltaTime / SettingsModel.Instance.CombatOutFader.Value;
                        if (lerp <= 0)
                        {
                            Audio.combatAudioSource.loop = false;
                            Audio.combatAudioSource.Stop();
                            // The combat AudioSource's clip will be randomly selected each time the combat music stops
                            Audio.combatAudioSource.clip = combatMusicClipList[rand.Next(combatMusicClipList.Count)];
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Dynamic adjust volume
        /// </summary>
        private void VolumeSetter()
        {
            // Next two lines are taken from Fontaine's Realism Mod. Credit to him
            CompoundItem headwear = Singleton<GameWorld>.Instance.MainPlayer.Equipment.GetSlot(EquipmentSlot.Headwear).ContainedItem as CompoundItem;
            HeadsetClass headset = Singleton<GameWorld>.Instance.MainPlayer.Equipment.GetSlot(EquipmentSlot.Earpiece).ContainedItem as HeadsetClass ?? ((headwear != null) ? headwear.GetAllItemsFromCollection().OfType<HeadsetClass>().FirstOrDefault<HeadsetClass>() : null);
            if (headset != null)
            {
                headsetMultiplier = SettingsModel.Instance.HeadsetMultiplier.Value;
            }
            else
            {
                headsetMultiplier = 1f;
            }
            // Each of the in-raid AudioSources' volumes are calculated by multiplying their configurable volumes with the indoor multiplier and the headset multiplier.
            environmentDict[EnvironmentType.Indoor] = SettingsModel.Instance.IndoorMultiplier.Value;
            soundtrackVolume = SettingsModel.Instance.SoundtrackVolume.Value * environmentDict[Singleton<GameWorld>.Instance.MainPlayer.Environment] * headsetMultiplier;
            spawnMusicVolume = SettingsModel.Instance.SpawnMusicVolume.Value * environmentDict[Singleton<GameWorld>.Instance.MainPlayer.Environment] * headsetMultiplier;
            combatMusicVolume = SettingsModel.Instance.CombatMusicVolume.Value * environmentDict[Singleton<GameWorld>.Instance.MainPlayer.Environment] * headsetMultiplier;

            // We check if the combat AudioSource is playing so that the CombatLerp method can do its job adjusting the ambient soundtrack and spawn music AudioSources
            if (!Audio.combatAudioSource.isPlaying)
            {
                    Audio.AdjustVolume(Audio.soundtrackAudioSource, soundtrackVolume);
                    Audio.AdjustVolume(Audio.spawnAudioSource, spawnMusicVolume);
            }
            if (lerp >= 1)
            {
                Audio.AdjustVolume(Audio.combatAudioSource, combatMusicVolume);
            }   
        }
        
        /// <summary>
        /// This method gets called once when loading into a raid. It makes sure every AudioClip is ready to play in the raid
        /// </summary>
        private async void PrepareRaidAudioClips()
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
                        LogSource.LogInfo("RequestAudioClip called for spawnTrackClip");
                    }
                    spawnTrackHasPlayed = false;
                }
                if (!combatMusicTrackList.IsNullOrEmpty())
                {
                    LogSource.LogWarning("Load music to combat");
                    // The next 4 lines prevent any issues that could be caused by exiting a raid before the combat timer ends
                    combatTimer = 0f;
                    lerp = 0;
                    Audio.combatAudioSource.Stop();
                    Audio.combatAudioSource.loop = false;
                    combatMusicClipList.Clear();
                    foreach (var track in combatMusicTrackList)
                    {
                        combatMusicClipList.Add(await AsyncRequestAudioClip(track));
                    }
                    Audio.combatAudioSource.clip = combatMusicClipList[rand.Next(combatMusicClipList.Count)];
                    LogSource.LogWarning($"Music in combat loaded! {Audio.combatAudioSource.clip.length}");
                }
            }
        }

        /// <summary>
        /// Play sound when spawn
        /// </summary>
        private void PlaySpawnMusic()
        {
            if (!spawnTrackHasPlayed && !spawnTrackClipList.IsNullOrEmpty())
            {
                Audio.spawnAudioSource.clip = spawnTrackClipList[rand.Next(spawnTrackClipList.Count)];
                LogSource.LogInfo("spawnAudioSource.clip assigned to spawnTrackClip");
                Audio.spawnAudioSource.Play();
                LogSource.LogInfo("spawnAudioSource playing");
                spawnTrackHasPlayed = true;
            }
        }
    }
    
    public class Audio : MonoBehaviour
    {
        public static AudioSource soundtrackAudioSource;
        public static AudioSource spawnAudioSource;
        public static AudioSource combatAudioSource;
        public static AudioSource menuMusicAudioSource;
        public static void SetClip(AudioSource audiosource, AudioClip clip)
        {
            audiosource.clip = clip;
        }
        public static void AdjustVolume(AudioSource audiosource, float volume)
        {
            audiosource.volume = volume;
        }
    }
    
    public enum ESoundtrackPlaylist
    {
        MapSpecificPlaylistOnly,
        CombinedPlaylists,
        DefaultPlaylistOnly
    }
}