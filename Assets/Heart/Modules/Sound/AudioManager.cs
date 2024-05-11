using System;
using Pancake.Common;
using Pancake.Linq;
using UnityEngine;

namespace Pancake.Sound
{
    [EditorIcon("icon_default")]
    public sealed class AudioManager : GameComponent
    {
        [Header("Sound Emitter Pool")] [SerializeField] private GameObject prefab;
        [SerializeField] private int prewarmSize = 10;

        [Header("Audio Control")] [SerializeField] private float initMusicVolume = 1f;
        [SerializeField] private float initSfxVolume = 1f;

        private SoundEmitterVault _sfx;
        private SoundEmitter _music;
        private static event Func<Audio, AudioHandle> PlaySfxEvent;
        private static event Action<AudioHandle> StopSfxEvent;
        private static event Action<AudioHandle> PauseSfxEvent;
        private static event Action<AudioHandle> ResumeSfxEvent;
        private static event Action<AudioHandle> FinishSfxEvent;
        private static event Action StopSfxAllEvent;
        private static event Func<Audio, AudioHandle> PlayMusicEvent;
        private static event Action<AudioHandle> StopMusicEvent;
        private static event Action<AudioHandle> PauseMusicEvent;
        private static event Action<AudioHandle> ResumeMusicEvent;
        private static event Action<float> ChangeSfxVolumeEvent;
        private static event Action<float> ChangeMusicVolumeEvent;

        public static float MusicVolume
        {
            get => Data.Load("user_music_volume", 1f);
            set
            {
                Data.Save("user_music_volume", value);
                ChangeMusicVolumeEvent?.Invoke(value);
            }
        }

        public static float SfxVolume
        {
            get => Data.Load("user_sfx_volume", 1f);
            set
            {
                Data.Save("user_sfx_volume", value);
                ChangeSfxVolumeEvent?.Invoke(value);
            }
        }

        private void Awake()
        {
            _sfx = new SoundEmitterVault();
            prefab.Populate(prewarmSize, true);

            MusicVolume = initMusicVolume;
            SfxVolume = initSfxVolume;
        }

        private void OnMusicVolumeChanged(float volume)
        {
            if (_music != null) _music.component.volume = volume;
        }

        private void OnSfxVolumeChanged(float volume)
        {
            foreach (var s in _sfx.GetAll())
            {
                foreach (var soundEmitter in s)
                {
                    soundEmitter.component.volume = volume;
                }
            }
        }

        private void OnEnable()
        {
            PlaySfxEvent += OnPlaySfx;
            StopSfxEvent += OnStopSfx;
            PauseSfxEvent += OnPauseSfx;
            ResumeSfxEvent += OnResumeSfx;
            FinishSfxEvent += OnFinishSfx;
            StopSfxAllEvent += OnStopAllSfx;
            PlayMusicEvent += OnOnPlayMusic;
            StopMusicEvent += OnStopMusic;
            PauseMusicEvent += OnPauseMusic;
            ResumeMusicEvent += OnResumeMusic;
            ChangeSfxVolumeEvent += OnSfxVolumeChanged;
            ChangeMusicVolumeEvent += OnMusicVolumeChanged;
        }

        private void OnDisable()
        {
            PlaySfxEvent -= OnPlaySfx;
            StopSfxEvent -= OnStopSfx;
            PauseSfxEvent -= OnPauseSfx;
            ResumeSfxEvent -= OnResumeSfx;
            FinishSfxEvent -= OnFinishSfx;
            StopSfxAllEvent -= OnStopAllSfx;
            PlayMusicEvent -= OnOnPlayMusic;
            StopMusicEvent -= OnStopMusic;
            PauseMusicEvent -= OnPauseMusic;
            ResumeMusicEvent -= OnResumeMusic;
            ChangeSfxVolumeEvent -= OnSfxVolumeChanged;
            ChangeMusicVolumeEvent -= OnMusicVolumeChanged;
        }

        /// <summary>
        /// Plays an AudioCue by requesting the appropriate number of SoundEmitters from the pool.
        /// </summary>
        private AudioHandle OnPlaySfx(Audio audio)
        {
            var clipsToPlay = audio.GetClips().Filter(s => s != null);
            var soundEmitters = new SoundEmitter[clipsToPlay.Length];

            int nOfClips = clipsToPlay.Length;
            for (int i = 0; i < nOfClips; i++)
            {
                soundEmitters[i] = prefab.Request<SoundEmitter>();
                if (soundEmitters[i] != null)
                {
                    soundEmitters[i].PlayAudioClip(clipsToPlay[i], audio.loop, audio.volume * SfxVolume);
                    if (!audio.loop) soundEmitters[i].OnCompleted += OnSoundEmitterFinishedPlaying;
                }
            }

            return _sfx.Add(audio, soundEmitters);
        }

        private void OnStopSfx(AudioHandle handle)
        {
            bool isFound = _sfx.Get(handle, out var soundEmitters);
            if (!isFound) return;
            foreach (var s in soundEmitters)
            {
                StopAndCleanEmitter(s);
            }

            _sfx.Remove(handle);
        }

        private void OnPauseSfx(AudioHandle handle)
        {
            bool isFound = _sfx.Get(handle, out var soundEmitters);
            if (!isFound) return;
            foreach (var s in soundEmitters)
            {
                s.Pause();
            }
        }

        private void OnResumeSfx(AudioHandle handle)
        {
            bool isFound = _sfx.Get(handle, out var soundEmitters);
            if (!isFound) return;
            foreach (var s in soundEmitters)
            {
                s.Resume();
            }
        }

        private void OnStopAllSfx()
        {
            foreach (var s in _sfx.GetAll())
            {
                foreach (var soundEmitter in s)
                {
                    StopAndCleanEmitter(soundEmitter);
                }
            }

            _sfx.ClearAll();
        }

        public void OnFinishSfx(AudioHandle handle)
        {
            bool isFound = _sfx.Get(handle, out SoundEmitter[] soundEmitters);

            if (isFound)
            {
                for (int i = 0; i < soundEmitters.Length; i++)
                {
                    soundEmitters[i].Finish();
                    soundEmitters[i].OnCompleted += OnSoundEmitterFinishedPlaying;
                }
            }
            else
            {
                Debug.LogWarning("Finishing an Audio was requested, but the Audio was not found.");
            }
        }

        /// <summary>
        /// Only used by the timeline to stop the gameplay music during cutscenes.
        /// Called by the SignalReceiver present on this same GameObject.
        /// </summary>
        public void TimelineInterruptsMusic() { OnStopMusic(AudioHandle.invalid); }

        private AudioHandle OnOnPlayMusic(Audio audio)
        {
            const float fadeDuration = 2f;
            var startTime = 0f;

            if (_music != null && _music.IsPlaying())
            {
                AudioClip songToPlay = audio.GetClips()[0];
                if (_music.GetClip() == songToPlay) return AudioHandle.invalid;

                //Music is already playing, need to fade it out
                startTime = _music.FadeMusicOut(fadeDuration);
            }

            _music = prefab.Request<SoundEmitter>();
            _music.FadeMusicIn(audio.GetClips()[0], 0.2f, audio.volume * MusicVolume, startTime);
            _music.OnCompleted += StopMusicEmitter;

            return AudioHandle.invalid;
            //No need to return a valid key for music
        }

        private void OnStopMusic(AudioHandle handle)
        {
            if (_music != null)
            {
                if (_music.IsPlaying()) _music.Stop();
                _music.gameObject.Return();
            }
        }

        private void OnPauseMusic(AudioHandle handle)
        {
            if (_music != null && _music.IsPlaying()) _music.Pause();
        }

        private void OnResumeMusic(AudioHandle handle)
        {
            if (_music != null && !_music.IsPlaying()) _music.Resume();
        }

        private void OnSoundEmitterFinishedPlaying(SoundEmitter soundEmitter) { StopAndCleanEmitter(soundEmitter); }

        private void StopAndCleanEmitter(SoundEmitter soundEmitter)
        {
            if (!soundEmitter.IsLooping()) soundEmitter.OnCompleted -= OnSoundEmitterFinishedPlaying;

            soundEmitter.Stop();
            soundEmitter.gameObject.Return();

            //TODO: is the above enough?
            //_soundEmitterVault.Remove(audioCueKey); is never called if StopAndClean is called after a Finish event
            //How is the key removed from the vault?
        }

        private void StopMusicEmitter(SoundEmitter soundEmitter)
        {
            soundEmitter.OnCompleted -= StopMusicEmitter;
            soundEmitter.gameObject.Return();
        }

        internal static AudioHandle PlaySfx(Audio audio) { return PlaySfxEvent?.Invoke(audio) ?? AudioHandle.invalid; }
        internal static void StopSfx(AudioHandle handle) { StopSfxEvent?.Invoke(handle); }
        internal static void PauseSfx(AudioHandle handle) { PauseSfxEvent?.Invoke(handle); }
        internal static void ResumeSfx(AudioHandle handle) { ResumeSfxEvent?.Invoke(handle); }
        internal static void FinishSfx(AudioHandle handle) { FinishSfxEvent?.Invoke(handle); }
        internal static void StopAllSfx() { StopSfxAllEvent?.Invoke(); }
        internal static AudioHandle PlayMusic(Audio audio) { return PlayMusicEvent?.Invoke(audio) ?? AudioHandle.invalid; }
        internal static void StopMusic(AudioHandle handle) { StopMusicEvent?.Invoke(handle); }
        internal static void PauseMusic(AudioHandle handle) { PauseMusicEvent?.Invoke(handle); }
        internal static void ResumeMusic(AudioHandle handle) { ResumeMusicEvent?.Invoke(handle); }
    }
}