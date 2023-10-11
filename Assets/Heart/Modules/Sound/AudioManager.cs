using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Sound
{
    [EditorIcon("csharp")]
    public sealed class AudioManager : GameComponent
    {
        [Header("Sound Emitter Pool")] [SerializeField] private SoundEmitterPool pool;
        [SerializeField] private int initialSize = 10;

        [Header("Listening Channel")] [Tooltip("The SoundManager listens to this event, fired by objects in any scene, to play SFXs")] [SerializeField]
        private ScriptableEventAudio eventPlaySfx;

        [SerializeField] private ScriptableEventAudioHandle eventStopSfx;
        [SerializeField] private ScriptableEventAudioHandle eventPauseSfx;
        [SerializeField] private ScriptableEventAudioHandle eventResumeSfx;
        [SerializeField] private ScriptableEventAudioHandle eventFinishSfx;
        [SerializeField] private ScriptableEventNoParam eventStopAllSfx;


        [Space] [Tooltip("The SoundManager listens to this event, fired by objects in any scene, to play Music")] [SerializeField]
        private ScriptableEventAudio eventPlayMusic;

        [SerializeField] private ScriptableEventAudioHandle eventStopMusic;
        [SerializeField] private ScriptableEventAudioHandle eventPauseMusic;
        [SerializeField] private ScriptableEventAudioHandle eventResumeMusic;

        [Header("Audio Control")] [SerializeField] private FloatVariable musicVolume;
        [SerializeField] private FloatVariable sfxVolume;

        private SoundEmitterVault _sfx;
        private SoundEmitter _music;

        private void Awake()
        {
            _sfx = new SoundEmitterVault();
            pool.Prewarm(initialSize);
            pool.SetParent(transform);
            sfxVolume.OnValueChanged += OnSfxVolumeChanged;
            musicVolume.OnValueChanged += OnMusicVolumeChanged;
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

        protected override void OnEnabled()
        {
            base.OnEnabled();
            eventPlaySfx.OnRaised += PlaySfx;
            eventStopSfx.OnRaised += StopSfx;
            eventPauseSfx.OnRaised += PauseSfx;
            eventResumeSfx.OnRaised += ResumeSfx;
            eventFinishSfx.OnRaised += FinishSfx;
            eventStopAllSfx.OnRaised += StopAllSfx;
            eventPlayMusic.OnRaised += PlayMusic;
            eventStopMusic.OnRaised += StopMusic;
            eventPauseMusic.OnRaised += PauseMusic;
            eventResumeMusic.OnRaised += ResumeMusic;
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            eventPlaySfx.OnRaised -= PlaySfx;
            eventStopSfx.OnRaised -= StopSfx;
            eventPauseSfx.OnRaised -= PauseSfx;
            eventResumeSfx.OnRaised -= ResumeSfx;
            eventFinishSfx.OnRaised -= FinishSfx;
            eventStopAllSfx.OnRaised -= StopAllSfx;
            eventPlayMusic.OnRaised -= PlayMusic;
            eventStopMusic.OnRaised -= StopMusic;
            eventPauseMusic.OnRaised -= PauseMusic;
            eventResumeMusic.OnRaised -= ResumeMusic;
        }

        /// <summary>
        /// Plays an AudioCue by requesting the appropriate number of SoundEmitters from the pool.
        /// </summary>
        private AudioHandle PlaySfx(Audio audio)
        {
            var clipsToPlay = audio.GetClips();
            var soundEmitters = new SoundEmitter[clipsToPlay.Length];

            int nOfClips = clipsToPlay.Length;
            for (int i = 0; i < nOfClips; i++)
            {
                soundEmitters[i] = pool.Request();
                if (soundEmitters[i] != null)
                {
                    soundEmitters[i].PlayAudioClip(clipsToPlay[i], audio.loop, audio.volume * sfxVolume.Value);
                    if (!audio.loop) soundEmitters[i].OnCompleted += OnSoundEmitterFinishedPlaying;
                }
            }

            return _sfx.Add(audio, soundEmitters);
        }

        private void StopSfx(AudioHandle handle)
        {
            bool isFound = _sfx.Get(handle, out var soundEmitters);
            if (!isFound) return;
            foreach (var s in soundEmitters)
            {
                StopAndCleanEmitter(s);
            }

            _sfx.Remove(handle);
        }

        private void PauseSfx(AudioHandle handle)
        {
            bool isFound = _sfx.Get(handle, out var soundEmitters);
            if (!isFound) return;
            foreach (var s in soundEmitters)
            {
                s.Pause();
            }
        }

        private void ResumeSfx(AudioHandle handle)
        {
            bool isFound = _sfx.Get(handle, out var soundEmitters);
            if (!isFound) return;
            foreach (var s in soundEmitters)
            {
                s.Resume();
            }
        }

        private void StopAllSfx()
        {
            foreach (var s in _sfx.GetAll())
            {
                foreach (var soundEmitter in s)
                {
                    soundEmitter.Stop();
                }
            }
        }

        public void FinishSfx(AudioHandle handle)
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
        public void TimelineInterruptsMusic() { StopMusic(AudioHandle.invalid); }

        private AudioHandle PlayMusic(Audio audio)
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

            _music = pool.Request();
            _music.FadeMusicIn(audio.GetClips()[0], 0.2f, audio.volume * musicVolume.Value, startTime);
            _music.OnCompleted += StopMusicEmitter;

            return AudioHandle.invalid;
            //No need to return a valid key for music
        }

        private void StopMusic(AudioHandle handle)
        {
            if (_music != null)
            {
                if ( _music.IsPlaying()) _music.Stop();
                pool.Return(_music);
            }
        }

        private void PauseMusic(AudioHandle handle)
        {
            if (_music != null && _music.IsPlaying()) _music.Pause();
        }

        private void ResumeMusic(AudioHandle handle)
        {
            if (_music != null && !_music.IsPlaying()) _music.Resume();
        }

        private void OnSoundEmitterFinishedPlaying(SoundEmitter soundEmitter) { StopAndCleanEmitter(soundEmitter); }

        private void StopAndCleanEmitter(SoundEmitter soundEmitter)
        {
            if (!soundEmitter.IsLooping()) soundEmitter.OnCompleted -= OnSoundEmitterFinishedPlaying;

            soundEmitter.Stop();
            pool.Return(soundEmitter);

            //TODO: is the above enough?
            //_soundEmitterVault.Remove(audioCueKey); is never called if StopAndClean is called after a Finish event
            //How is the key removed from the vault?
        }

        private void StopMusicEmitter(SoundEmitter soundEmitter)
        {
            soundEmitter.OnCompleted -= StopMusicEmitter;
            pool.Return(soundEmitter);
        }
    }
}