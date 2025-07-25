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
        private AudioManager audio;
        private MenuMusicJukebox menuMusicJukebox;
        private SoundtrackJukebox soundtrackJukebox;
        
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
            audio = new AudioManager();
            audio.Init(gameObject);

            soundtrackJukebox = new SoundtrackJukebox();
            soundtrackJukebox.Init(audio);

            menuMusicJukebox = new MenuMusicJukebox();
            menuMusicJukebox.Init(audio, soundtrackJukebox);
            
            
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
            if (_settings.KeyBind.Value.IsDown())
            {
                audio.PlaySpawnMusic(false);
            }
            
            menuMusicJukebox.CheckMenuMusicControls();
            
            if (!InRaid)
            {
                if (!MenuMusicPatch.HasReloadedAudio)
                {
                    MenuMusicPatch.LoadAudioClips();
                    UISoundsPatch.LoadUIClips();
                }
                soundtrackJukebox.SoundtrackCalled = false;
                audio.HasStartedLoadingAudio = false;
                audio.SpawnTrackHasPlayed = false;
                return;
            }
            
            if (Singleton<GameWorld>.Instance.MainPlayer == null || Singleton<GameWorld>.Instance.MainPlayer.Location == "hideout")
            {
                return;
            }
            
            MenuMusicPatch.HasReloadedAudio = false;
            
            audio.PrepareRaidAudioClips();
#if DEBUG
            OverlayDebug.Instance.UpdateOverlay();
#endif
            
            audio.PlaySpawnMusic();
            audio.VolumeSetter();
            audio.CombatMusic();
            
            soundtrackJukebox.CheckSoundtrackControls();
            soundtrackJukebox.SoundtrackCalled = true;
            soundtrackJukebox.PlaySoundtrack();
        }

        public AudioManager GetAudio() => audio;
        public MenuMusicJukebox GetMenuMusicJukeBox() => menuMusicJukebox;
    }
}