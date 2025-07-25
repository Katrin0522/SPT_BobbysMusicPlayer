using BepInEx;
using BepInEx.Logging;
using BobbysMusicPlayer.Data;
using BobbysMusicPlayer.Patches;
using Comfort.Common;
using EFT;
using BobbysMusicPlayer.Jukebox;
using BobbysMusicPlayer.Models;
using BobbysMusicPlayer.Utils;

namespace BobbysMusicPlayer
{
    [BepInPlugin("BobbyRenzobbi.MusicPlayer", "BobbysMusicPlayer", "1.2.4")]
    public class BobbysMusicPlayerPlugin : BaseUnityPlugin
    {
        public static BobbysMusicPlayerPlugin Instance { get; private set; }
        
        private SettingsModel _settings;
        private AudioManager _audio;
        
        internal static ManualLogSource LogSource;
        
        public static bool InRaid { get; set; }
        
        private void Awake()
        {
            Instance = this;
            LogSource = Logger;
            LogSource.LogInfo("Plugin loading...");
            
            //Init config
            _settings = SettingsModel.Create(Config);
            
            GlobalData.EnvironmentDict[EnvironmentType.Indoor] = _settings.IndoorMultiplier.Value;
            
            //Initialization audio side
            _audio = new AudioManager();
            _audio.Init(gameObject);
            
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

        private void Update()
        {
            MenuMusicJukebox.CheckMenuMusicControls();
            
            if (!InRaid)
            {
                if (!MenuMusicPatch.HasReloadedAudio)
                {
                    MenuMusicPatch.LoadAudioClips();
                    UISoundsPatch.LoadUIClips();
                }
                SoundtrackJukebox.soundtrackCalled = false;
                _audio.HasStartedLoadingAudio = false;
                _audio.spawnTrackHasPlayed = false;
                return;
            }
            
            if (Singleton<GameWorld>.Instance.MainPlayer == null || Singleton<GameWorld>.Instance.MainPlayer.Location == "hideout")
            {
                return;
            }
            
            MenuMusicPatch.HasReloadedAudio = false;
            
            _audio.PrepareRaidAudioClips();
            
            if (Singleton<AbstractGame>.Instance.Status != GameStatus.Started)
            {
                return;
            }
            
            _audio.PlaySpawnMusic();
            _audio.VolumeSetter();
            _audio.CombatMusic();
            
            SoundtrackJukebox.CheckSoundtrackControls();
            SoundtrackJukebox.soundtrackCalled = true;
            SoundtrackJukebox.PlaySoundtrack();

        }

        public AudioManager GetAudio() => _audio;
    }
}