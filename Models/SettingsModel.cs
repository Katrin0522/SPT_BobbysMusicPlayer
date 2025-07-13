using BepInEx.Configuration;
using BobbysMusicPlayer.Utils;
using UnityEngine;

namespace BobbysMusicPlayer.Models
{
	/// <summary>
	/// Model with config fields
	/// </summary>
	public class SettingsModel
	{
		public static SettingsModel Instance { get; private set; }
		
		public ConfigEntry<float> SoundtrackVolume { get; set; }
		public ConfigEntry<float> SpawnMusicVolume { get; set; }
		public ConfigEntry<int> SoundtrackLength { get; set; }
		public ConfigEntry<int> CustomMenuMusicLength { get; set; }
		public ConfigEntry<ESoundtrackPlaylist> SoundtrackPlaylist;
		public ConfigEntry<KeyboardShortcut> RestartTrack { get; set; }
		public ConfigEntry<KeyboardShortcut> SkipTrack { get; set; }
		public ConfigEntry<KeyboardShortcut> PreviousTrack { get; set; }
		public ConfigEntry<KeyboardShortcut> PauseTrack { get; set; }
		public ConfigEntry<float> AmbientCombatMultiplier { get; set; }
		public ConfigEntry<float> CombatAttackedEntryTime { get; set; }
		public ConfigEntry<float> CombatDangerEntryTime { get; set; }
		public ConfigEntry<float> CombatGrenadeEntryTime { get; set; }
		public ConfigEntry<float> CombatFireEntryTime { get; set; }
		public ConfigEntry<float> CombatHitEntryTime { get; set; }
		public ConfigEntry<float> CombatMusicVolume { get; set; }
		public ConfigEntry<float> CombatInFader { get; set; }
		public ConfigEntry<float> CombatOutFader { get; set; }
		public ConfigEntry<float> ShotNearCutoff { get; set; }
		public ConfigEntry<float> GrenadeNearCutoff { get; set; }
		public ConfigEntry<float> IndoorMultiplier { get; set; }
		public ConfigEntry<float> HeadsetMultiplier { get; set; }
		
#if DEBUG
		public ConfigEntry<KeyboardShortcut> KeyBind;
		public ConfigEntry<float> PositionXDebug;
		public ConfigEntry<float> PositionYDebug;
		public ConfigEntry<int> FontSizeDebug;
#endif
		private SettingsModel(ConfigFile configFile)
		{
#if DEBUG
			#region Debug
			
			KeyBind = configFile.Bind(
				"DEBUG", 
				"Test key bind", 
				new KeyboardShortcut(KeyCode.LeftArrow), 
				new ConfigDescription(
					"Just keybind for tests")); 

			PositionXDebug = configFile.Bind(
				"DEBUG",
				"PositionX",
				10f,
				new ConfigDescription("X Position", new AcceptableValueRange<float>(-2000f, 2000f)));

			PositionYDebug = configFile.Bind(
				"DEBUG",
				"PositionY",
				-10f,
				new ConfigDescription("Y Position", new AcceptableValueRange<float>(-2000f, 2000f)));
			
			FontSizeDebug = configFile.Bind(
				"DEBUG",
				"FontSizeDebug",
				28,
				new ConfigDescription("FontSizeDebug", new AcceptableValueRange<int>(0, 200)));
			
			PositionXDebug.SettingChanged += (_, __) =>
			{
				OverlayDebug.Instance.SetOverlayPosition(new Vector2(PositionXDebug.Value, PositionYDebug.Value));
			};
			
			PositionYDebug.SettingChanged += (_, __) =>
			{
				OverlayDebug.Instance.SetOverlayPosition(new Vector2(PositionXDebug.Value, PositionYDebug.Value));
			};

			FontSizeDebug.SettingChanged += (_, __) =>
			{
				OverlayDebug.Instance.SetFontSize(FontSizeDebug.Value);
			};
			
			#endregion	
#endif
			//Names for sections config
			string generalSettings = "1. General Settings";
			string ambientSoundtrackSettings = "3. Ambient Soundtrack Settings";
			string customMenuMusicSettings = "2. Custom Menu Music Settings";
			string dynamicSoundtrackSettings = "4. Dynamic Soundtrack Settings";
			
			SoundtrackVolume = configFile.Bind(
                ambientSoundtrackSettings,
                "Ambient Soundtrack volume",
                0.05f,
                new ConfigDescription("Volume of the Ambient Soundtrack",
	                new AcceptableValueRange<float>(0f, 1f),
	                new ConfigurationManagerAttributes { Order = 3 }));
			
            SpawnMusicVolume = configFile.Bind(
                ambientSoundtrackSettings,
                "Spawn music volume",
                0.05f,
                new ConfigDescription("Volume of the music played on spawn",
	                new AcceptableValueRange<float>(0f, 1f),
	                new ConfigurationManagerAttributes { Order = 2 }));
            
            SoundtrackPlaylist = configFile.Bind(
                ambientSoundtrackSettings,
                "Ambient Soundtrack playlist selection",
                ESoundtrackPlaylist.CombinedPlaylists,
                new ConfigDescription("- Map Specific Playlist Only: Playlist will only use music from the map's soundtrack folder. If it is empty, the default soundtrack folder will be used instead.\n- Combined Playlists: Playlist will combine music from the map's soundtrack folder and the default soundtrack folder.\n- Default Playlist Only: Playlist will only use music from the default soundtrack folder.",
	                null,
	                new ConfigurationManagerAttributes { Order = 1 }));
            
            SoundtrackLength = configFile.Bind(
                ambientSoundtrackSettings,
                "Ambient Soundtrack playlist length (Minutes)",
                50,
                new ConfigDescription("The length of the Ambient playlist created for each raid.\nYou should keep this around 50 unless you have modified raid times.", 
                new AcceptableValueRange<int>(0, 600), 
                new ConfigurationManagerAttributes { Order = 0, IsAdvanced = true }));
            
            CustomMenuMusicLength = configFile.Bind(
                customMenuMusicSettings,
                "Menu Music playlist length (Minutes)", 60,
                new ConfigDescription("The length of the playlist created for the main menu.\nNote: This setting's changes will take place either on game restart, or after a raid.", 
                new AcceptableValueRange<int>(0, 600), 
                new ConfigurationManagerAttributes { Order = 0, IsAdvanced = true }));
            
            RestartTrack = configFile.Bind(
                generalSettings,
                "Restart track button", 
                new KeyboardShortcut(KeyCode.Keypad2));
            
            SkipTrack = configFile.Bind(
                generalSettings,
                "Skip track button", 
                new KeyboardShortcut(KeyCode.Keypad6));
            
            PreviousTrack = configFile.Bind(
                generalSettings,
                "Previous track button", 
                new KeyboardShortcut(KeyCode.Keypad4));
            
            PauseTrack = configFile.Bind(
                generalSettings,
                "Pause track button", 
                new KeyboardShortcut(KeyCode.Keypad5));
            
            CombatAttackedEntryTime = configFile.Bind(
                dynamicSoundtrackSettings,
                "Combat duration when shot at (Seconds)",
                12f,
                new ConfigDescription("The duration of the combat state when the player is shot at\nMake sure this is set less than \"Combat duration when hit (Seconds)\"",
	                new AcceptableValueRange<float>(0f, 600f)));
            
            CombatDangerEntryTime = configFile.Bind(
                dynamicSoundtrackSettings,
                "Combat duration when a shot is fired closeby (Seconds)", 12f,
                new ConfigDescription("The duration of the combat state when a gun is fired within the distance set by \"Shot distance combat trigger (meters)\"", 
                new AcceptableValueRange<float>(0f, 600f)));
            
            CombatGrenadeEntryTime = configFile.Bind(
                dynamicSoundtrackSettings,
                "Combat duration when a grenade explodes closeby (Seconds)", 12f,
                new ConfigDescription("The duration of the combat state when a gun is fired within the distance set by \"Shot distance combat trigger (meters)\"", 
                new AcceptableValueRange<float>(0f, 600f)));
            
            CombatHitEntryTime = configFile.Bind(
                dynamicSoundtrackSettings,
                "Combat duration when hit (Seconds)", 20f,
                new ConfigDescription("The duration of the combat state when the player is hit\nMake sure this is greater than \"Combat duration when shot at (Seconds)\"", 
                new AcceptableValueRange<float>(0f, 600f)));
            
            CombatFireEntryTime = configFile.Bind(
                dynamicSoundtrackSettings,
                "Combat duration when firing (Seconds)", 8f,
                new ConfigDescription("The duration of the combat state when the player fires their gun\nIf using a door breaching mod, consider reducing this value potentially down to 0", 
                new AcceptableValueRange<float>(0f, 600f)));
            
            CombatMusicVolume = configFile.Bind(
                dynamicSoundtrackSettings,
                "Combat music volume", 0.05f,
                new ConfigDescription("Volume of the music played in combat", 
                new AcceptableValueRange<float>(0f, 1f), 
                new ConfigurationManagerAttributes { Order = 8 }));
            
            CombatInFader = configFile.Bind(
                dynamicSoundtrackSettings,
                "Combat entry fader", 4f,
                new ConfigDescription("The transition time from normal soundtrack to combat music", 
                new AcceptableValueRange<float>(0.1f, 30f), 
                new ConfigurationManagerAttributes {IsAdvanced = true}));
            
            CombatOutFader = configFile.Bind(
                dynamicSoundtrackSettings,
                "Combat exit fader", 8f,
                new ConfigDescription("The transition time from combat music to normal soundtrack\nNote: This transition begins after the combat state ends", 
                new AcceptableValueRange<float>(0.1f, 120f), 
                new ConfigurationManagerAttributes {IsAdvanced = true}));
            
            ShotNearCutoff = configFile.Bind(
                dynamicSoundtrackSettings,
                "Shot distance combat trigger (meters)", 15f,
                new ConfigDescription("If an enemy fires within this distance, it will trigger a combat state", 
                new AcceptableValueRange<float>(0f, 150f)));
            
            GrenadeNearCutoff = configFile.Bind(
                dynamicSoundtrackSettings,
                "Explosion distance combat trigger (meters)", 20f,
                new ConfigDescription("If a grenade explodes within this distance, it will trigger a combat state", 
                new AcceptableValueRange<float>(0f, 150f)));
            
            AmbientCombatMultiplier = configFile.Bind(
                dynamicSoundtrackSettings,
                "Ambient Soundtrack volume multiplier during combat", 0f,
                new ConfigDescription("During combat, the Ambient Soundtrack's volume will be multiplied by this value\nSetting this to 0 means your Ambient Soundtrack will be muted in combat.\nSetting this to 1 means that your Ambient Soundtrack volume is independent of your combat state.\nSpawn music volume is also affected", 
                new AcceptableValueRange<float>(0f, 2f), 
                new ConfigurationManagerAttributes { Order = 0 }));
            
            IndoorMultiplier = configFile.Bind(
                generalSettings,
                "In-Raid Soundtrack volume - Indoor multiplier", 0.75f,
                new ConfigDescription("When indoors, all in-raid music volume will be multiplied by this value.\nI recommend setting this somewhere between 0 and 1, since the game is much noisier outdoors than indoors", 
                new AcceptableValueRange<float>(0f, 2f)));
            
            HeadsetMultiplier = configFile.Bind(
                generalSettings,
                "In-Raid Soundtrack volume - Active headset multiplier", 
                0.75f,
                new ConfigDescription("When wearing an active headset, all in-raid music volume will be multiplied by this value.\nI recommend setting this somewhere between 0 and 1, since the game is much noisier without an active headset", 
	                new AcceptableValueRange<float>(0f, 2f)));
		}
		
		/// <summary>
		/// Init configs model
		/// </summary>
		/// <param name="configFile"></param>
		/// <returns></returns>
		public static SettingsModel Create(ConfigFile configFile)
		{
			if (Instance != null)
			{
				return Instance;
			}
			return Instance = new SettingsModel(configFile);
		}
	}
}