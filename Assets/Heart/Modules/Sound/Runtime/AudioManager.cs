using System;
using System.Collections.Generic;
using System.Linq;
using Pancake.Common;
using Pancake.Linq;
using Pancake.Pools;
using UnityEngine;

namespace Pancake.Sound
{
    [EditorIcon("icon_default")]
    public class AudioManager : GameComponent
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private AudioAsset audioAsset;

        private List<AudioHandle> _handles = new();
        private static event Action<float> MusicVolumeChangeEvent;
        private static event Action<float> SfxVolumeChangeEvent;
        internal static Action<AudioId> playEvent;
        internal static Action<AudioId> stopEvent;
        internal static Action<AudioId> pauseEvent;
        internal static Action<AudioId> resumeEvent;
        internal static Action<AudioId, float, float, float> quietOthersEvent;
        internal static Action stopAllEvent;
        internal static Action<EAudioType> stopAllByTypeEvent;
        internal static Action stopAllAndCleanEvent;

        public static float MusicVolume
        {
            get => Data.Load("music_volume", 1f);
            set
            {
                Data.Save("music_volume", value);
                MusicVolumeChangeEvent?.Invoke(value);
            }
        }

        public static float SfxVolume
        {
            get => Data.Load("sfx_volume", 1);
            set
            {
                Data.Save("sfx_volume", value);
                SfxVolumeChangeEvent?.Invoke(value);
            }
        }

        private void OnEnable()
        {
            MusicVolumeChangeEvent += OnMusicVolumeChanged;
            SfxVolumeChangeEvent += OnSfxVolumeChanged;
            playEvent += Play;
            stopEvent += Stop;
            pauseEvent += Pause;
            resumeEvent += Resume;
            stopAllEvent += StopAll;
            stopAllByTypeEvent += StopAll;
            quietOthersEvent += OnQuietOthers;
            stopAllAndCleanEvent += StopAllAndClean;
        }

        private void OnDisable()
        {
            MusicVolumeChangeEvent -= OnMusicVolumeChanged;
            SfxVolumeChangeEvent -= OnSfxVolumeChanged;
            playEvent -= Play;
            stopEvent -= Stop;
            pauseEvent -= Pause;
            resumeEvent -= Resume;
            stopAllEvent -= StopAll;
            stopAllByTypeEvent -= StopAll;
            quietOthersEvent -= OnQuietOthers;
            stopAllAndCleanEvent -= StopAllAndClean;
        }

        private void OnMusicVolumeChanged(float volume)
        {
            foreach (var handle in _handles)
            {
                if (handle.type == EAudioType.Music) handle.player.SetVolume(volume * handle.volume);
            }
        }

        private void OnSfxVolumeChanged(float volume)
        {
            foreach (var handle in _handles)
            {
                if (handle.type == EAudioType.Sfx) handle.player.SetVolume(volume * handle.volume);
            }
        }

        private void Play(AudioId id)
        {
            (var clips, var type, float groupVolume, bool loop) = audioAsset.audios.Filter(data => data.id == id.id).FirstOrDefault().GetClips();

            foreach (var clip in clips)
            {
                float clipVolume = groupVolume * clip.volume;
                var player = playerPrefab.Request<AudioPlayer>();
                float masterVolume = type == EAudioType.Music ? MusicVolume : SfxVolume;
                var handle = new AudioHandle(id.id, type, clipVolume, player);
                player.Play(clip.clip, loop, clipVolume * masterVolume, id.id);
                if (!loop) player.OnCompleted += OnCompletedPlay;
                _handles.Add(handle);
            }
        }

        private void Stop(AudioId id)
        {
            var founds = new List<AudioHandle>();
            foreach (var handle in _handles)
            {
                if (handle.id == id.id)
                {
                    handle.player.OnCompleted -= OnCompletedPlay;
                    handle.player.Stop();
                    handle.player.gameObject.Return();
                    founds.Add(handle);
                }
            }

            for (var i = 0; i < founds.Count; i++)
            {
                var handle = founds[i];
                _handles.Remove(handle);
                founds[i] = null;
            }
        }

        private void Pause(AudioId id)
        {
            foreach (var handle in _handles)
            {
                if (handle.id == id.id) handle.player.Pause();
            }
        }

        private void Resume(AudioId id)
        {
            foreach (var handle in _handles)
            {
                if (handle.id == id.id) handle.player.Resume();
            }
        }

        private void StopAll()
        {
            for (var i = 0; i < _handles.Count; i++)
            {
                var handle = _handles[i];
                handle.player.OnCompleted -= OnCompletedPlay;
                handle.player.Stop();
                handle.player.gameObject.Return();
                _handles[i] = null;
            }

            _handles.Clear();
        }

        private void StopAll(EAudioType type)
        {
            var founds = new List<AudioHandle>();
            foreach (var handle in _handles)
            {
                if (handle.type == type)
                {
                    handle.player.OnCompleted -= OnCompletedPlay;
                    handle.player.Stop();
                    handle.player.gameObject.Return();
                    founds.Add(handle);
                }
            }

            for (var i = 0; i < founds.Count; i++)
            {
                var handle = founds[i];
                _handles.Remove(handle);
                founds[i] = null;
            }
        }

        private void StopAllAndClean()
        {
            StopAll();
            SharedGameObjectPool.Dispose(playerPrefab);
        }

        private void OnQuietOthers(AudioId id, float volume, float fadeTime, float effectiveTime)
        {
            foreach (var handle in _handles)
            {
                if (handle.id != id.id) handle.player.FadeOut(volume, fadeTime, effectiveTime);
            }
        }

        private void OnCompletedPlay(AudioPlayer player)
        {
            player.OnCompleted -= OnCompletedPlay;
            player.gameObject.Return();
            foreach (var handle in _handles.ToList())
            {
                if (handle.id == player.idAudioHandle) _handles.Remove(handle);
            }
        }
    }
}