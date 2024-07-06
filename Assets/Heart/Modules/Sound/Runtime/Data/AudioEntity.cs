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
        [SerializeField] private ERandomFlag randomFlags;

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
        public ERandomFlag RandomFlags => randomFlags;

        public SoundClip PickNewClip(out int index) => clips.PickNewOne(audioPlayMode, Id, out index);
        public SoundClip PickNewClip() => Clips.PickNewOne(AudioPlayMode, Id, out _);

        public bool Validate() { return AudioExtension.Validate(Name.ToWhiteBold(), clips, Id); }
        
        public float GetMasterVolume()
        {
            return GetRandomValue(MasterVolume, RandomFlags);
        }

        public float GetPitch()
        {
            return GetRandomValue(Pitch, ERandomFlag.Pitch);
        }

        public float GetRandomValue(float baseValue, ERandomFlag flag)
        {
            if(!RandomFlags.HasFlagUnsafe(flag)) return baseValue;

            float range = 0f;
            switch (flag)
            {
                case ERandomFlag.Pitch:
                    range = PitchRandomRange;
                    break;
                case ERandomFlag.Volume:
                    range = VolumeRandomRange;
                    break;
                default:
                    Debug.LogError(AudioConstant.LOG_HEADER + "Invalid approach with multiple flags on GetRandomValue()");
                    break;
            }

            float half = range * 0.5f;
            return baseValue + Random.Range(-half, half);
        }

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

        public static class ForEditor
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