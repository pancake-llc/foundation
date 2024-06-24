using Pancake.Common;
using UnityEngine;

namespace Pancake.Sound
{
    [System.Serializable]
    public class AudioEntity : IAudioIdentity, IAudioEntity
    {
        [SerializeField] private string name;
        [SerializeField] private int id;
        [SerializeField] private EAudioPlayMode audioPlayMode = EAudioPlayMode.Single;
        [SerializeField] private SoundClip[] clips;
        [SerializeField] private float masterVolume;
        [SerializeField] private bool loop;
        [SerializeField] private bool seamlessLoop;
        [SerializeField] private float transitionTime;
        [SerializeField] private SpatialSetting spatialSetting;
        [SerializeField] private int priority;
        [SerializeField] private float pitch;
        [SerializeField] private float pitchRandomRange;
        [SerializeField] private float volumeRandomRange;
        [SerializeField] private ERandomFlags randomFlags;

        public string Name => name;
        public int Id => id;
        public EAudioPlayMode AudioPlayMode => audioPlayMode;
        public SoundClip[] Clips => clips;
        public float MasterVolume => masterVolume;
        public bool Loop => loop;
        public bool SeamlessLoop => seamlessLoop;
        public float TransitionTime => transitionTime;
        public SpatialSetting SpatialSetting => spatialSetting;
        public int Priority => priority;
        public float Pitch => pitch;
        public float PitchRandomRange => pitchRandomRange;
        public float VolumeRandomRange => volumeRandomRange;
        public ERandomFlags RandomFlags => randomFlags;

        public SoundClip PickNewClip() => clips.PickNewOne(audioPlayMode, Id);

        public bool Validate() { return AudioExtension.Validate(Name.ToWhiteBold(), clips, Id); }

#if UNITY_EDITOR
        public enum SeamlessType
        {
            ClipSetting,
            Time,
            Tempo
        }

        [System.Serializable]
        public struct TempoTransition
        {
            public float bpm;
            public int beats;
        }

        public static class EditorPropertyName
        {
            public static string AudioPlayMode => nameof(audioPlayMode);
            public static string SeamlessTransitionType => nameof(seamlessTransitionType);
            public static string TransitionTempo => nameof(transitionTempo);
            public static string SnapToFullVolume => nameof(snapToFullVolume);
            public static string Name => nameof(name);
            public static string Id => nameof(id);
            public static string Clips => nameof(clips);
            public static string MasterVolume => nameof(masterVolume);
            public static string Loop => nameof(loop);
            public static string SeamlessLoop => nameof(seamlessLoop);
            public static string TransitionTime => nameof(transitionTime);
            public static string SpatialSetting => nameof(spatialSetting);
            public static string Priority => nameof(priority);
            public static string Pitch => nameof(pitch);
            public static string PitchRandomRange => nameof(pitchRandomRange);
            public static string VolumeRandomRange => nameof(volumeRandomRange);
            public static string RandomFlags => nameof(randomFlags);
        }

        [SerializeField] private SeamlessType seamlessTransitionType = SeamlessType.ClipSetting;
        [SerializeField] private TempoTransition transitionTempo;
        [SerializeField] private bool snapToFullVolume;

#endif
    }
}