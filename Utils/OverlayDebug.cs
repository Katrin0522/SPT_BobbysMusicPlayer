#if DEBUG
using BobbysMusicPlayer.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BobbysMusicPlayer.Utils
{
    public class OverlayDebug: MonoBehaviour
    {
        private static OverlayDebug _instance;
        public static OverlayDebug Instance => _instance ??= new OverlayDebug();
        
        private TextMeshProUGUI _overlayText;
        private GameObject _overlay;
        
        public void Enable()
        {
            _instance = this;
            
            _overlay = new GameObject("[BobbysMusicPlayer] Overlay", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            DontDestroyOnLoad(_overlay);
            var canvas = _overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = _overlay.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            var textObj = new GameObject("[BobbysMusicPlayer] OverlayText", typeof(RectTransform));
            textObj.transform.SetParent(_overlay.transform, false);

            _overlayText = textObj.AddComponent<TextMeshProUGUI>();
            _overlayText.text = "Overlay BobbysMusicPlayer initialized";
            _overlayText.fontSize = SettingsModel.Instance.FontSizeDebug.Value;
            _overlayText.color = Color.white;
            _overlayText.alignment = TextAlignmentOptions.TopLeft;
            _overlayText.enableWordWrapping = false;

            var rectTransform = _overlayText.rectTransform;
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.sizeDelta = new Vector2(800, 200);
            
            SetOverlayPosition(new Vector2(SettingsModel.Instance.PositionXDebug.Value, SettingsModel.Instance.PositionYDebug.Value));
            UpdateOverlay();
        }
        
        public void UpdateOverlay()
        {
            if (_overlayText == null) return;

            var _audio = BobbysMusicPlayerPlugin.Instance.GetAudio();
            
            _overlayText.text = $"InRaid -> {BobbysMusicPlayerPlugin.InRaid}\n" +
                                $"\n" +
                                $"Combat Timer -> {_audio.combatTimer}" + 
                                $"\n" +
                                $"headsetMultiplier -> {_audio.headsetMultiplier}" + 
                                $"\n" +
                                $"Current Env Multiplier -> {_audio.currentEnvironmentMultiplier}" + 
                                $"\n" +
                                $"[CombatLerp Volume Data]\n" +
                                $"CombatAudioSource Volume -> {Mathf.Lerp(0f, _audio.combatMusicVolume, _audio.lerp)}\n" +
                                $"SoundtrackAudioSource Volume -> {Mathf.Lerp(_audio.soundtrackVolume, SettingsModel.Instance.AmbientCombatMultiplier.Value*_audio.soundtrackVolume, _audio.lerp)}\n" +
                                $"SpawnAudioSource Volume -> {Mathf.Lerp(_audio.spawnMusicVolume, SettingsModel.Instance.AmbientCombatMultiplier.Value*_audio.spawnMusicVolume, _audio.lerp)}\n" +
                                $"\n" +
                                $"[VolumeSetter Volume Data]\n" +
                                $"CombatAudioSource Volume -> {_audio.combatMusicVolume}\n" +
                                $"SoundtrackAudioSource Volume -> {_audio.soundtrackVolume}\n" +
                                $"SpawnAudioSource Volume -> {_audio.spawnMusicVolume}\n" +
                                $"\n" +
                                $"[AudioSource is playing?]\n" +
                                $"CombatAudioSource isPlay? -> {_audio.combatAudioSource?.isPlaying}\n" +
                                $"SoundtrackAudioSource isPlay? -> {_audio.soundtrackAudioSource?.isPlaying}\n" +
                                $"SpawnAudioSource isPlay? -> {_audio.spawnAudioSource?.isPlaying}\n";
                                
        }

        public void SetOverlayPosition(Vector2 anchoredPosition)
        {
            if (_overlayText != null)
                _overlayText.rectTransform.anchoredPosition = anchoredPosition;
        }
        
        public void SetFontSize(int size)
        {
            if (_overlayText != null)
                _overlayText.fontSize = size;
        }

        public void Disable()
        {
            Destroy(_overlay);
            Destroy(this);
        }
    }
}
#endif
