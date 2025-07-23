using BepInEx;
using BepInEx.Logging;
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
        private SettingsModel _settings;
        private AudioManager _audio;
        
        internal static ManualLogSource LogSource;
        
        public static bool InRaid { get; set; }
        
        internal void Awake()
        {
            LogSource = Logger;
            LogSource.LogInfo("Plugin loading...");
            
            //Init config
            _settings = SettingsModel.Create(Config);

            //Initialization audio side
            _audio = new AudioManager();
            _audio.Init(gameObject);
            
            GlobalData.EnvironmentDict[EnvironmentType.Indoor] = _settings.IndoorMultiplier.Value;
            
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
            MenuMusicJukebox.MenuMusicControls();
            if (!InRaid)
            {
                if (!MenuMusicPatch.HasReloadedAudio)
                {
                    MenuMusicPatch.LoadAudioClips();
                    UISoundsPatch.LoadUIClips();
                }
                SoundtrackJukebox.soundtrackCalled = false;
                AudioManager.HasStartedLoadingAudio = false;
                AudioManager.spawnTrackHasPlayed = false;
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

            #region UpdateMethods
            
            _audio.CombatMusic();
            _audio.VolumeSetter();
            _audio.PlaySpawnMusic();
            
            SoundtrackJukebox.SoundtrackControls();
            SoundtrackJukebox.soundtrackCalled = true;
            SoundtrackJukebox.PlaySoundtrack();
            
            #endregion
        }
        
        
    }
}