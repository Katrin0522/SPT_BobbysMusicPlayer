using System;

namespace BobbysMusicPlayer.Data
{
    public static class PathData
    {
        private static string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        
        public static string MapSpecialDir = baseDirectory + "\\BepInEx\\plugins\\BobbysMusicPlayer\\Soundtrack\\map_specific_soundtrack";
        public static string CustomMenuMusicSounds = baseDirectory + "\\BepInEx\\plugins\\BobbysMusicPlayer\\CustomMenuMusic\\sounds";
        public static string SoundtrackDefault = baseDirectory + "\\BepInEx\\plugins\\BobbysMusicPlayer\\Soundtrack\\default_soundtrack";
        public static string SoundtrackCombat = baseDirectory + "\\BepInEx\\plugins\\BobbysMusicPlayer\\Soundtrack\\combat_music";
        public static string SoundtrackSpawn = baseDirectory + "\\BepInEx\\plugins\\BobbysMusicPlayer\\Soundtrack\\spawn_music";
        public static string SoundtrackDeath = baseDirectory + "\\BepInEx\\plugins\\BobbysMusicPlayer\\DeathMusic";
        public static string SoundtrackExtract = baseDirectory + "\\BepInEx\\plugins\\BobbysMusicPlayer\\ExtractMusic";
        public static string SoundtrackUI = baseDirectory + "\\BepInEx\\plugins\\BobbysMusicPlayer\\UISounds\\";
    
        //Paths if player wrong moved OST pack 
        public static string CustomMenuMusicSoundsMissing = baseDirectory + "\\BepInEx\\plugins\\CustomMenuMusic\\sounds";
        public static string SoundtrackSoundsMissing = baseDirectory + "\\BepInEx\\plugins\\Soundtrack\\sounds";
    }
}