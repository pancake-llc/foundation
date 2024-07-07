using System;
using LitMotion;
using LitMotion.Extensions;
using Pancake.Common;
using Pancake.Pools;
using UnityEngine;

namespace Pancake.Sound
{
    [RequireComponent(typeof(AudioSource)), AddComponentMenu("")]
    [EditorIcon("icon_blue_audiosource")]
    public class AudioPlayer : GameComponent, IPoolCallbackReceiver
    {
        [SerializeField] private AudioSource audioSource;

        public event Action<AudioPlayer> OnCompleted;
        public event Action<AudioPlayer> OnPaused;
        public event Action<AudioPlayer> OnResumed;
        public event Action<AudioPlayer> OnStopped;

        private DelayHandle _delayHandle;
        internal string idAudioHandle;

        public AudioClip GetClip() => audioSource.clip;
        public bool IsPlaying() => audioSource.isPlaying;
        public bool IsLooping() => audioSource.loop;
        internal void SetVolume(float volume) => audioSource.volume = volume;

        protected override void OnLoadComponents() { audioSource = GetComponent<AudioSource>(); }

        internal void Play(AudioClip clip, bool loop, float volume, string idHandle)
        {
            idAudioHandle = idHandle;
            audioSource.clip = clip;
            audioSource.loop = loop;
            audioSource.volume = volume;
            audioSource.time = 0f;
            audioSource.Play();
            if (!loop) _delayHandle = this.Delay(clip.length, OnCompletedInvoke);
        }

        private void OnCompletedInvoke() { OnCompleted?.Invoke(this); }

        internal void PlayWithFadeIn(AudioClip clip, float duration, float volume, string idHandle, float startTime = 0f)
        {
            Play(clip, true, 0, idHandle);

            //Start the clip at the same time the previous one left, if length allows
            if (startTime <= audioSource.clip.length) audioSource.time = startTime;

            LMotion.Create(0, volume, duration).WithEase(Ease.Linear).BindToVolume(audioSource).AddTo(gameObject);
        }

        internal void FadeIn(float volume, float duration)
        {
            LMotion.Create(audioSource.volume, volume, duration).WithEase(Ease.Linear).BindToVolume(audioSource).AddTo(gameObject);
        }

        internal void FadeOut(float volume, float duration, float effectiveTime)
        {
            float originalVolume = audioSource.volume;
            LMotion.Create(audioSource.volume, volume, duration).WithEase(Ease.Linear).BindToVolume(audioSource).AddTo(gameObject);

            if (effectiveTime >= 0) this.Delay(effectiveTime, () => FadeIn(originalVolume, duration));
        }

        /// <summary>
        /// Used when the game is unpaused, to pick up SFX from where they left.
        /// </summary>
        internal void Resume()
        {
            OnResumed?.Invoke(this);
            audioSource.UnPause();
        }

        /// <summary>
        /// Used when the game is paused.
        /// </summary>
        internal void Pause()
        {
            OnPaused?.Invoke(this);
            audioSource.Pause();
        }

        internal void Stop()
        {
            OnStopped?.Invoke(this);
            audioSource.Stop();
        }

        internal void Finish()
        {
            if (!audioSource.loop) return;

            audioSource.loop = false;
            float remainingTime = audioSource.clip.length - audioSource.time;
            this.Delay(remainingTime, OnCompletedInvoke);
        }

        public void OnRequest() { }

        public void OnReturn()
        {
            OnCompleted = null;
            OnPaused = null;
            OnResumed = null;
            OnStopped = null;

            audioSource.clip = null;
            audioSource.loop = false;
            audioSource.volume = 1;
            audioSource.time = 0f;
            App.StopAndClean(ref _delayHandle);
        }
    }
}