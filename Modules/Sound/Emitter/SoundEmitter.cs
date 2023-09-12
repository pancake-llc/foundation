using Pancake.Apex;
using PrimeTween;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Sound
{
    [HideMonoScript]
    [EditorIcon("script_sound")]
    [RequireComponent(typeof(AudioSource))]
    public class SoundEmitter : CacheGameComponent<AudioSource>
    {
        public event UnityAction<SoundEmitter> OnCompleted;
        public event UnityAction<SoundEmitter> OnPaused;
        public event UnityAction<SoundEmitter> OnResumed;
        public event UnityAction<SoundEmitter> OnStopped;

        protected override void Awake()
        {
            base.Awake();
            component.playOnAwake = false;
        }

        /// <summary>
        /// Instructs the AudioSource to play a single clip, with optional looping
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="loop"></param>
        /// <param name="volume"></param>
        internal void PlayAudioClip(AudioClip clip, bool loop, float volume)
        {
            component.clip = clip;
            component.loop = loop;
            component.volume = volume;
            component.time = 0f; //Reset in case this AudioSource is being reused for a short SFX after being used for a long music track
            component.Play();
            if (!loop) this.Delay(clip.length, OnCompletedInvoke);
        }

        private void OnCompletedInvoke()
        {
            OnCompleted?.Invoke(this); // The AudioManager will pick this up
        }

        internal void FadeMusicIn(AudioClip clip, float duration, float volume, float startTime = 0f)
        {
            PlayAudioClip(clip, true, 0);

            //Start the clip at the same time the previous one left, if length allows
            //TODO: find a better way to sync fading songs
            if (startTime <= component.clip.length) component.time = startTime;

            Tween.AudioVolume(component, volume, duration);
        }

        internal float FadeMusicOut(float duration)
        {
            Tween.AudioVolume(component, 0, duration);

            return component.time;
        }

        internal void OnFadeOutCompleted() { OnCompletedInvoke(); }

        public AudioClip GetClip() => component.clip;
        public bool IsPlaying() => component.isPlaying;
        public bool IsLooping() => component.loop;

        /// <summary>
        /// Used when the game is unpaused, to pick up SFX from where they left.
        /// </summary>
        internal void Resume()
        {
            OnResumed?.Invoke(this);
            component.UnPause();
        }

        /// <summary>
        /// Used when the game is paused.
        /// </summary>
        internal void Pause()
        {
            OnPaused?.Invoke(this);
            component.Pause();
        }

        internal void Stop()
        {
            OnStopped?.Invoke(this);
            component.Stop();
        }

        internal void Finish()
        {
            if (!component.loop) return;

            component.loop = false;
            float remainingTime = component.clip.length - component.time;
            this.Delay(remainingTime, OnFadeOutCompleted);
        }
    }
}