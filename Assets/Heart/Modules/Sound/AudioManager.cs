using Pancake.Linq;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Sound
{
    [EditorIcon("csharp")]
    public sealed class AudioManager : GameComponent
    {
        [Header("Sound Emitter Pool")] [SerializeField] private GameObject prefab;
        [SerializeField] private int prewarmSize = 10;

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
        [SerializeField] private ScriptableEventNoParam eventStopAllMusic;
        
        [Header("Audio Control")] [SerializeField] private FloatVariable musicVolume;
        [SerializeField] private FloatVariable sfxVolume;

        private SoundEmitterVault _sfx;
        private SoundEmitterVault _music;

        private void Awake()
        {
            _sfx = new SoundEmitterVault();
            _music = new SoundEmitterVault();
            prefab.Populate(prewarmSize, true);
            sfxVolume.OnValueChanged += OnSfxVolumeChanged;
            musicVolume.OnValueChanged += OnMusicVolumeChanged;
        }

        private void OnMusicVolumeChanged(float volume)
        {
            foreach (var m in _music.GetAll())
            {
                foreach (var soundEmitter in m) soundEmitter.component.volume = volume;
            }
        }

        private void OnSfxVolumeChanged(float volume)
        {
            foreach (var s in _sfx.GetAll())
            {
                foreach (var soundEmitter in s) soundEmitter.component.volume = volume;
            }
        }

        private void OnEnable()
        {
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
            eventStopAllMusic.OnRaised += StopAllMusic;
        }

        private void OnDisable()
        {
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
            eventStopAllMusic.OnRaised -= StopAllMusic;
        }

        /// <summary>
        /// Plays an AudioCue by requesting the appropriate number of SoundEmitters from the pool.
        /// </summary>
        private AudioHandle PlaySfx(Audio audio)
        {
            var clipsToPlay = audio.GetClips().Filter(s => s != null);
            var soundEmitters = new SoundEmitter[clipsToPlay.Length];

            int nOfClips = clipsToPlay.Length;
            for (int i = 0; i < nOfClips; i++)
            {
                soundEmitters[i] = prefab.Request<SoundEmitter>();
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
                    StopAndCleanEmitter(soundEmitter);
                }
            }

            _sfx.ClearAll();
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
            const float fadeDuration = 1f;
            var startTime = 0f;

            //Music is already playing, need to fade it out
            var allPrevious = _music.GetAll();
            foreach (var all in allPrevious)
            {
                foreach (var emitter in all)
                {
                    emitter.FadeMusicOut(fadeDuration);
                    var go = emitter.gameObject;
                    emitter.OnCompleted -= StopMusicEmitter;
                    App.Delay(emitter, fadeDuration, () => go.Return());
                }
            }

            _music.ClearAll();

            var songToPlay = audio.GetClips().Filter(s => s != null);
            var soundEmitters = new SoundEmitter[songToPlay.Length];
            int nOfClips = songToPlay.Length;
            for (int i = 0; i < nOfClips; i++)
            {
                soundEmitters[i] = prefab.Request<SoundEmitter>();
                soundEmitters[i].FadeMusicIn(audio.GetClips()[0], 0.2f, audio.volume * musicVolume.Value, startTime);
                soundEmitters[i].OnCompleted += StopMusicEmitter;
            }

            return _music.Add(audio, soundEmitters);
        }

        private void StopMusic(AudioHandle handle)
        {
            bool isFound = _music.Get(handle, out var soundEmitters);
            if (!isFound) return;
            foreach (var s in soundEmitters)
            {
                if (s.IsPlaying()) s.Stop();
                StopMusicEmitter(s);
            }

            _music.Remove(handle);
        }

        private void PauseMusic(AudioHandle handle)
        {
            bool isFound = _music.Get(handle, out var soundEmitters);
            if (!isFound) return;
            foreach (var s in soundEmitters)
            {
                if (s.IsPlaying()) s.Pause();
            }
        }

        private void ResumeMusic(AudioHandle handle)
        {
            bool isFound = _music.Get(handle, out var soundEmitters);
            if (!isFound) return;
            foreach (var s in soundEmitters)
            {
                if (!s.IsPlaying()) s.Resume();
            }
        }

        private void StopAllMusic()
        {
            foreach (var s in _music.GetAll())
            {
                foreach (var soundEmitter in s)
                {
                    if (soundEmitter.IsPlaying()) soundEmitter.Stop();
                    StopMusicEmitter(soundEmitter);
                }
            }

            _music.ClearAll();
        }

        private void OnSoundEmitterFinishedPlaying(SoundEmitter soundEmitter) { StopAndCleanEmitter(soundEmitter); }

        private void StopAndCleanEmitter(SoundEmitter soundEmitter)
        {
            if (!soundEmitter.IsLooping()) soundEmitter.OnCompleted -= OnSoundEmitterFinishedPlaying;

            soundEmitter.Stop();
            soundEmitter.gameObject.Return();
        }

        private void StopMusicEmitter(SoundEmitter soundEmitter)
        {
            soundEmitter.OnCompleted -= StopMusicEmitter;
            soundEmitter.gameObject.Return();
        }
    }
}