using System.Collections.Generic;
using Pancake;
using Pancake.Common;
using Pancake.Sound;
using PancakeEditor.Common;
using UnityEngine;

namespace PancakeEditor.Sound
{
    [EditorIcon("so_dark_setting")]
    public class AudioEditorSetting : ScriptableSettings<AudioEditorSetting>
    {
        [System.Serializable]
        public struct AudioTypeSetting
        {
            public EAudioType audioType;
            public Color color;
            public EDrawedProperty drawedProperty;

            public AudioTypeSetting(EAudioType audioType, string colorString, EDrawedProperty drawedProperty)
            {
                this.audioType = audioType;
                ColorUtility.TryParseHtmlString(colorString, out color);
                this.drawedProperty = drawedProperty;
            }
            
            public bool CanDraw(EDrawedProperty target) => drawedProperty.HasFlagUnsafe(target);
        }

        public string assetOutputPath;
        public bool showAudioTypeOnSoundId;
        public bool showVuColorOnVolumeSlider;
        public bool showMasterVolumeOnClipListHeader;
        public List<AudioTypeSetting> audioTypeSettings;

        public Color GetAudioTypeColor(EAudioType audioType) { return TryGetAudioTypeSetting(audioType, out var setting) ? setting.color : default; }

        public bool TryGetAudioTypeSetting(EAudioType audioType, out AudioTypeSetting result)
        {
            result = default;

            // For temp asset
            if (audioType == EAudioType.None)
            {
                result = new AudioTypeSetting {audioType = audioType, color = Uniform.SunsetOrange, drawedProperty = EDrawedProperty.All};
                return true;
            }

            if (audioTypeSettings == null) CreateNewAudioTypeSettings();

            // ReSharper disable once PossibleNullReferenceException
            foreach (var setting in audioTypeSettings)
            {
                if (audioType == setting.audioType)
                {
                    result = setting;
                    return true;
                }
            }

            return false;
        }

        public bool WriteAudioTypeSetting(EAudioType audioType, AudioTypeSetting newSetting)
        {
            for (var i = 0; i < audioTypeSettings.Count; i++)
            {
                if (audioType == audioTypeSettings[i].audioType)
                {
                    audioTypeSettings[i] = newSetting;
                    return true;
                }
            }

            return false;
        }

        public void ResetToFactorySettings()
        {
            showVuColorOnVolumeSlider = true;
            showAudioTypeOnSoundId = true;
            showMasterVolumeOnClipListHeader = false;
            CreateNewAudioTypeSettings();
        }

        private void CreateNewAudioTypeSettings()
        {
            audioTypeSettings = new List<AudioTypeSetting>
            {
                new(EAudioType.Music,
                    "#012F874C",
                    EDrawedProperty.Volume | EDrawedProperty.PlaybackPosition | EDrawedProperty.Fade | EDrawedProperty.ClipPreview | EDrawedProperty.MasterVolume |
                    EDrawedProperty.Loop),
                new(EAudioType.UI,
                    "#0E9C884C",
                    EDrawedProperty.Volume | EDrawedProperty.PlaybackPosition | EDrawedProperty.Fade | EDrawedProperty.ClipPreview | EDrawedProperty.MasterVolume),
                new(EAudioType.Ambience,
                    "#00B0284C",
                    EDrawedProperty.Volume | EDrawedProperty.PlaybackPosition | EDrawedProperty.Fade | EDrawedProperty.ClipPreview | EDrawedProperty.MasterVolume |
                    EDrawedProperty.Loop | EDrawedProperty.SpatialSettings),
                new(EAudioType.Sfx,
                    "#FD803D96",
                    EDrawedProperty.Volume | EDrawedProperty.PlaybackPosition | EDrawedProperty.Fade | EDrawedProperty.ClipPreview | EDrawedProperty.MasterVolume |
                    EDrawedProperty.Loop | EDrawedProperty.SpatialSettings | EDrawedProperty.Pitch),
                new(EAudioType.VoiceOver,
                    "#EEC6374C",
                    EDrawedProperty.Volume | EDrawedProperty.PlaybackPosition | EDrawedProperty.Fade | EDrawedProperty.ClipPreview | EDrawedProperty.MasterVolume),
            };
        }

        public static string AssetOutputPath { get => Instance.assetOutputPath; set => Instance.assetOutputPath = value; }
        public static bool ShowAudioTypeOnSoundId { get => Instance.showAudioTypeOnSoundId; set => Instance.showAudioTypeOnSoundId = value; }
        public static bool ShowVuColorOnVolumeSlider { get => Instance.showVuColorOnVolumeSlider; set => Instance.showVuColorOnVolumeSlider = value; }
        public static bool ShowMasterVolumeOnClipListHeader { get => Instance.showMasterVolumeOnClipListHeader; set => Instance.showMasterVolumeOnClipListHeader = value; }
        public static List<AudioTypeSetting> AudioTypeSettings { get => Instance.audioTypeSettings; set => Instance.audioTypeSettings = value; }
    }
}