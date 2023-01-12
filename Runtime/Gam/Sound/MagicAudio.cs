using System.Collections.Generic;
using UnityEngine;

namespace Pancake
{
    public static class MagicAudio
    {
        public static readonly List<AudioHandle> Handles = new List<AudioHandle>();
        private static readonly List<Sound> SoundAssets = new List<Sound>();

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
        }

        public static void Reset()
        {
            
        }

        public static void Play(string id)
        {
            var sound = SoundAssets.Find(_ => _.ID == id);
            if (sound ==  null) return;
           
            
        }
    }
}