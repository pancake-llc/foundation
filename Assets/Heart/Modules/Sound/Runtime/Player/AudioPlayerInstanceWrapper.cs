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

        public int Id => Instance ? Instance.Id : -1;

        public bool IsActive => Instance && Instance.IsActive;

        public bool IsPlaying => Instance && Instance.IsPlaying;

        IMusicPlayer IMusicDecoratable.AsBGM() => Instance ? Instance.AsBGM() : null;

#if !UNITY_WEBGL
        IPlayerEffect IEffectDecoratable.AsDominator() => Instance ? Instance.AsDominator() : null;
#endif
        IAudioPlayer IVolumeSettable.SetVolume(float vol, float fadeTime) => Instance ? Instance.SetVolume(vol, fadeTime) : null;

        protected override void LogInstanceIsNull()
        {
            if (AudioSettings.LogAccessRecycledPlayerWarning)
                Debug.LogWarning(AudioConstant.LOG_HEADER + "Invalid operation. The audio player you're accessing has finished playing and has been recycled.");
        }

        public void Stop() => Instance?.Stop();
        public void Stop(Action onFinished) => Instance?.Stop(onFinished);
        public void Stop(float fadeOut) => Instance?.Stop(fadeOut);
        public void Stop(float fadeOut, Action onFinished) => Instance?.Stop(fadeOut, onFinished);
        public void Pause() => Instance?.Pause();
        public void Pause(float fadeOut) => Instance?.Pause(fadeOut);
    }
}