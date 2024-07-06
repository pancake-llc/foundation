using System;
using UnityEngine;

namespace Pancake.Sound
{
    /// <summary>
    /// To keep tracking the instance of an AudioPlayer
    /// </summary>
    public class AudioPlayerInstanceWrapper : InstanceWrapper<AudioPlayer>, IAudioPlayer
    {
        public AudioPlayerInstanceWrapper(AudioPlayer instance)
            : base(instance)
        {
        }

        public event Action<SoundId> OnEndPlaying
        {
            add
            {
                if (IsAvailable()) Instance.OnEndPlaying += value;
            }
            remove
            {
                if (IsAvailable()) Instance.OnEndPlaying -= value;
            }
        }

        public SoundId Id => Instance ? Instance.Id : SoundId.Invalid;

        public bool IsActive => IsAvailable() && Instance.IsActive;

        public bool IsPlaying => IsAvailable() && Instance.IsPlaying;

        IMusicPlayer IMusicDecoratable.AsBGM() => Instance ? Instance.AsBGM() : null;

        IPlayerEffect IEffectDecoratable.AsDominator() => Instance ? Instance.AsDominator() : null;
        
        IAudioPlayer IVolumeSettable.SetVolume(float vol, float fadeTime) => Instance ? Instance.SetVolume(vol, fadeTime) : null;
        
        IAudioPlayer IPitchSettable.SetPitch(float pitch, float fadeTime) => Instance ? Instance.SetPitch(pitch, fadeTime) : null;

        protected override void LogInstanceIsNull()
        {
            if (AudioSettings.LogAccessRecycledPlayerWarning)
                Debug.LogWarning(AudioConstant.LOG_HEADER + "Invalid operation. The audio player you're accessing has finished playing and has been recycled.");
        }

        void IAudioStoppable.Stop() => Instance?.Stop();
        void IAudioStoppable.Stop(Action onFinished) => Instance?.Stop(onFinished);
        void IAudioStoppable.Stop(float fadeOut) => Instance?.Stop(fadeOut);
        void IAudioStoppable.Stop(float fadeOut, Action onFinished) => Instance?.Stop(fadeOut, onFinished);
        void IAudioStoppable.Pause() => Instance?.Pause();
        void IAudioStoppable.Pause(float fadeOut) => Instance?.Pause(fadeOut);
    }
}