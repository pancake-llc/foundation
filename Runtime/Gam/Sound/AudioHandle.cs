using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake
{
    public class AudioHandle : BaseMono
    {
        public AudioState PreviousState { get; set; }
        public AudioState CurrentState { get; set; }
        private AudioSource _source;
        private float _delay;
        private int _loopCount;
        private float _fadeVolume;
        private float _fadeDuration;
        private CoroutineHandle _fadeHandle;
        private CoroutineHandle _playHandle;

        private float _waitTime;
        private float _volume;

        public bool IsPlaying => CurrentState == AudioState.Playing || CurrentState == AudioState.AwaitPlaying;
        public bool IsPause => CurrentState == AudioState.Pause || CurrentState == AudioState.AwaitPause;

        public float Length
        {
            get
            {
                if (CurrentState == AudioState.Stop || CurrentState == AudioState.AwaitStop) return 0;
                return M.Clamp01(_source.time / _source.clip.length);
            }
        }

        public float Volume { get => _volume; set => _volume = M.Clamp01(value); }

        public Action onStart;
        public Action onPlay;
        public Action onPause;
        public Action onCompleted;

        public void Init(Sound sound, bool randomPitch = false)
        {
            if (_source == null) _source = Get<AudioSource>();

            _source.clip = sound.clip;
            _source.outputAudioMixerGroup = sound.output;
            _source.playOnAwake = sound.playOnAwake;
            _source.volume = sound.volume;
            Volume = sound.volume;
            if (!randomPitch) _source.pitch = sound.pitch;
            else _source.pitch = Random.Range(sound.pitchMin, sound.pitchMax);
            _source.spatialBlend = sound.spatialBlend;
            _source.rolloffMode = sound.audioMode;
            _source.maxDistance = sound.maxRange;
            _source.loop = false;
            _loopCount = sound.loop switch
            {
                SoundLoop.None => 0,
                SoundLoop.Loop => -1,
                SoundLoop.Number => sound.loopCount,
                _ => throw new ArgumentOutOfRangeException()
            };

            _fadeDuration = sound.fadeDuration;
            _fadeVolume = sound.fadeVolume;
        }

        public void ChangeVolume() { _source.volume = Volume * _fadeVolume; }

        public void Play(Action<AudioHandle> onPlayCompleted = null)
        {
            gameObject.SetActive(true);
            CurrentState = AudioState.AwaitPlaying;
            ChangeVolume();
            onStart?.Invoke();
            Timing.KillCoroutines(_playHandle);
            Timing.RunCoroutine(IePlayAudio(onPlayCompleted).CancelWith(gameObject));
        }

        public void Pause()
        {
            if (CurrentState != AudioState.Playing && CurrentState != AudioState.AwaitPlaying) return;
            PreviousState = CurrentState;
            CurrentState = AudioState.AwaitPause;
            if (_fadeDuration > 0f)
            {
                FadeOut(_fadeDuration, OnPaused);
                return;
            }

            OnPaused();
        }

        public void Resume()
        {
            if (CurrentState != AudioState.Pause && CurrentState != AudioState.AwaitPause) return;
            CurrentState = PreviousState;
            _source.UnPause();
            FadeIn(_fadeDuration);
        }

        public void Stop(Action action = null)
        {
            if (CurrentState == AudioState.Stop || CurrentState == AudioState.AwaitStop) return;
            CurrentState = AudioState.AwaitStop;
            if (_fadeDuration > 0.0f)
            {
                FadeOut(_fadeDuration,
                    () =>
                    {
                        action?.Invoke();
                        OnStopped();
                    });
                return;
            }

            action?.Invoke();
            OnStopped();
        }

        public void ForcePause()
        {
            if (CurrentState != AudioState.Playing && CurrentState != AudioState.AwaitPlaying) return;
            PreviousState = CurrentState;
            CurrentState = AudioState.Pause;
            OnPaused();
        }

        public void ForceStop()
        {
            if (CurrentState == AudioState.Stop) return;
            CurrentState = AudioState.Stop;
            OnStopped();
        }

        private void OnPaused()
        {
            CurrentState = AudioState.Pause;
            _source.Pause();
        }

        private void OnStopped()
        {
            CurrentState = AudioState.Stop;
            _source.Stop();
            Timing.KillCoroutines(_playHandle);
            Timing.KillCoroutines(_fadeHandle);
            ResetPlayer();
        }

        protected IEnumerator<float> IeWait(float waitTime, Action<float> onNext)
        {
            var time = 0f;
            while (time < waitTime)
            {
                time += Runtime.GetDeltaTime(MagicAudio.TimeMode);
                onNext?.Invoke(time);
                yield return Timing.WaitForOneFrame;
            }
        }

        protected IEnumerator<float> IeWait(float waitTime, Action onNext = null)
        {
            var time = 0f;
            while (time < waitTime)
            {
                time += Runtime.GetDeltaTime(MagicAudio.TimeMode);
                onNext?.Invoke();
                yield return Timing.WaitForOneFrame;
            }
        }

        protected void FadeIn(float time)
        {
            Timing.KillCoroutines(_fadeHandle);
            _fadeHandle = Timing.RunCoroutine(IeFadeIn(time).CancelWith(gameObject));
        }

        protected void FadeOut(float time, Action onCompleted = null)
        {
            Timing.KillCoroutines(_fadeHandle);
            _fadeHandle = Timing.RunCoroutine(IeFadeOut(time, onCompleted).CancelWith(gameObject));
        }

        private IEnumerator<float> IeFadeIn(float fadeTime)
        {
            _fadeVolume = 0;

            yield return Timing.WaitUntilDone(IeWait(fadeTime,
                (_) =>
                {
                    _fadeVolume = Mathf.Clamp01(_ / fadeTime);
                    ChangeVolume();
                }));

            _fadeVolume = 1f;
            ChangeVolume();
        }

        private IEnumerator<float> IeFadeOut(float fadeTime, Action onCompleted)
        {
            yield return Timing.WaitUntilDone(IeWait(fadeTime,
                (_) =>
                {
                    _fadeVolume = 1f - Mathf.Clamp01(_ / fadeTime);
                    ChangeVolume();
                }));

            _fadeVolume = 0;
            ChangeVolume();
            onCompleted?.Invoke();
        }

        private IEnumerator<float> IePlayAudio(Action<AudioHandle> onPlayCompleted)
        {
            yield return Timing.WaitUntilDone(IeWait(_delay));

            CurrentState = AudioState.Playing;
            onPlay?.Invoke();
            _source.time = 0f;
            ChangeVolume();
            if (_fadeDuration > 0f) FadeIn(_fadeDuration);

            do
            {
                _source.Play();
                _waitTime = 0f;
                while (_waitTime < _source.clip.length / _source.pitch)
                {
                    if (CurrentState == AudioState.Pause)
                    {
                        onPause?.Invoke();
                        while (CurrentState == AudioState.Pause)
                        {
                            yield return Timing.WaitForOneFrame;
                        }
                    }

                    _waitTime += Runtime.GetDeltaTime(MagicAudio.TimeMode);
                    yield return Timing.WaitForOneFrame;
                }

                onCompleted?.Invoke();
                if (_loopCount > 0)
                {
                    _loopCount--;
                }
            } while (_loopCount == -1 || _loopCount > 0);

            onPlayCompleted?.Invoke(this);
            MagicAudio.Complete(this);
            ResetPlayer();
        }

        protected void ResetPlayer()
        {
            gameObject.SetActive(false);
            CurrentState = AudioState.Stop;
        }
    }
}