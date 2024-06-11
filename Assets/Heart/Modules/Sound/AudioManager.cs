using System.Collections.Generic;
using Pancake.Common;
using Pancake.Linq;
using UnityEngine;
using VContainer.Unity;

namespace Pancake.Sound
{
    public sealed class AudioManager : IStartable
    {
        private readonly List<SoundEmitter> _bgm = new();
        private readonly List<SoundEmitter> _sfx = new();
        private GameObject _source;

        public float MusicVolume
        {
            get => Data.Load("user_music_volume", 1f);
            set
            {
                Data.Save("user_music_volume", value);
                foreach (var s in _bgm)
                {
                    s.component.volume = value;
                }
            }
        }

        public float SfxVolume
        {
            get => Data.Load("user_sfx_volume", 1f);
            set
            {
                Data.Save("user_sfx_volume", value);
                foreach (var s in _sfx)
                {
                    s.component.volume = value;
                }
            }
        }

        /// <summary>
        /// Plays an Audio by requesting the appropriate number of SoundEmitters from the pool.
        /// </summary>
        public AudioHandle PlaySfx(Audio audio)
        {
            var clipsToPlay = audio.GetClips().Filter(s => s != null);
            var soundEmitters = new SoundEmitter[clipsToPlay.Length];
            var handle = new AudioHandle(audio);
            int length = clipsToPlay.Length;
            for (var i = 0; i < length; i++)
            {
                soundEmitters[i] = _source.Request<SoundEmitter>();
                soundEmitters[i].Sync(handle);
                soundEmitters[i].PlayAudioClip(clipsToPlay[i], audio.loop, audio.volume * SfxVolume);
                if (!audio.loop) soundEmitters[i].OnCompleted += OnSoundEmitterFinishedPlaying;
            }

            _sfx.AddRange(soundEmitters);

            return handle;
        }

        public void StopSfx(AudioHandle handle)
        {
            var result = _sfx.Filter(e => e.Id.Equals(handle.id));
            foreach (var e in result)
            {
                StopAndCleanEmitter(e);
                _sfx.Remove(e);
            }
        }

        public void PauseSfx(AudioHandle handle)
        {
            var result = _sfx.Filter(e => e.Id.Equals(handle.id));
            foreach (var e in result)
            {
                e.Pause();
            }
        }

        public void ResumeSfx(AudioHandle handle)
        {
            var result = _sfx.Filter(e => e.Id.Equals(handle.id));
            foreach (var e in result)
            {
                e.Resume();
            }
        }

        public void StopAllSfx()
        {
            foreach (var e in _sfx)
            {
                StopAndCleanEmitter(e);
            }

            _sfx.Clear();
        }

        public void FinishSfx(AudioHandle handle)
        {
            var result = _sfx.Filter(e => e.Id.Equals(handle.id));

            foreach (var e in result)
            {
                e.Finish();
                e.OnCompleted += OnSoundEmitterFinishedPlaying;
            }
        }

        public AudioHandle PlayMusic(Audio audio)
        {
            const float fadeDuration = 2f;
            var startTime = 0f;
            foreach (var emitter in _bgm)
            {
                emitter.FadeMusicOut(fadeDuration);
                var go = emitter.gameObject;
                emitter.OnCompleted -= StopMusicEmitter;
                App.Delay(emitter, fadeDuration, () => go.Return());
            }

            _bgm.Clear();

            var clipsToPlay = audio.GetClips().Filter(s => s != null);
            var emitters = new SoundEmitter[clipsToPlay.Length];
            var handle = new AudioHandle(audio);
            int length = clipsToPlay.Length;
            for (var i = 0; i < length; i++)
            {
                emitters[i] = _source.Request<SoundEmitter>();
                emitters[i].Sync(handle);
                emitters[i].FadeMusicIn(clipsToPlay[i], 0.2f, audio.volume * MusicVolume, startTime);
                emitters[i].OnCompleted += StopMusicEmitter;
            }

            _bgm.AddRange(emitters);

            return handle;
        }

        public void StopMusic(AudioHandle handle)
        {
            var result = _bgm.Filter(e => e.Id.Equals(handle.id));
            foreach (var e in result)
            {
                e.Stop();
                StopMusicEmitter(e);
                _bgm.Remove(e);
            }
        }

        public void PauseMusic(AudioHandle handle)
        {
            var result = _bgm.Filter(e => e.Id.Equals(handle.id));
            foreach (var e in result)
            {
                e.Pause();
            }
        }

        public void ResumeMusic(AudioHandle handle)
        {
            var result = _bgm.Filter(e => e.Id.Equals(handle.id));
            foreach (var e in result)
            {
                e.Resume();
            }
        }

        public void StopAllMusic()
        {
            foreach (var e in _bgm)
            {
                e.Stop();
                StopMusicEmitter(e);
            }

            _bgm.Clear();
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

        public void Start()
        {
            _source = new GameObject("SoundEmitter", typeof(AudioSource), typeof(Poolable), typeof(SoundEmitter));
            _source.SetActive(false);
            App.DontDestroy(_source); // move out launcher scene
        }
    }
}