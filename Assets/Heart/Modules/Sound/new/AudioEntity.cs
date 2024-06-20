using System;
using Pancake.Common;
using UnityEngine;

namespace Pancake.Sound
{
    [Serializable]
    public class AudioEntity : IAudioIdentity, IAudioEntity
    {
        [SerializeField] private string name;
        [SerializeField] private int id;
        [SerializeField] private EPlayMode playMode = EPlayMode.Single;
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
        public EPlayMode PlayMode => playMode;
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

        public SoundClip PickNewClip() => Clips.PickNewOne(PlayMode, Id);

        public bool Validate() { return SoundExtension.Validate(Name.TextBold(), Clips, Id); }

#if UNITY_EDITOR
        public enum SeamlessType
        {
            ClipSetting,
            Time,
            Tempo
        }

        [Serializable]
        public struct TempoTransition
        {
            public float bpm;
            public int beats;
        }

        public static class EditorPropertyName
        {
            public static string MulticlipsPlayMode => nameof(playMode);
            public static string SeamlessTransitionType => nameof(seamlessTransitionType);
            public static string TransitionTempo => nameof(transitionTempo);
            public static string SnapToFullVolume => nameof(snapToFullVolume);
        }

        [SerializeField] private SeamlessType seamlessTransitionType = SeamlessType.ClipSetting;
        [SerializeField] private TempoTransition transitionTempo;
        [SerializeField] private bool snapToFullVolume;
#endif
    }
}