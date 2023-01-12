using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake
{
    public static class MagicAudio
    {
        private static readonly List<Sound> SoundAssets = new List<Sound>();
        private static GameObject prefab;
        public static TimeMode TimeMode { get; private set; } = TimeMode.Normal;

        public static void InstallSoundPreset(SoundPreset preset)
        {
            if (preset == null || !Application.isPlaying) return;

            for (var i = 0; i < preset.Sounds.Count; i++)
            {
                var sound = preset.Sounds[i];
                var exist = false;
                foreach (var soundAsset in SoundAssets)
                {
                    if (soundAsset.ID == sound.ID) exist = true;
                }

                if (!exist) SoundAssets.Add(sound);
            }

            MagicAudio.prefab = preset.Prefab;
        }

        public static void ClearAsset() { SoundAssets.Clear(); }

        public static void ClearPool() { MagicPool.DestroyPool(MagicPool.GetPool(prefab)); }

        public static AudioHandle Play(string id, Action<AudioHandle> onCompleted = null)
        {
            var sound = SoundAssets.Find(_ => _.ID == id);
            if (sound == null) return null;

            var obj = MagicPool.Spawn(prefab);
            var audioHandle = obj.GetComponent<AudioHandle>();
            audioHandle.Init(sound);
            audioHandle.Play(onCompleted);
            return audioHandle;
        }

        internal static void Complete(AudioHandle audio) { MagicPool.Despawn(audio.gameObject); }

        public static void ChangeTimeMode(TimeMode mode) { TimeMode = mode; }
    }
}