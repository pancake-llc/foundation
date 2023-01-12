using UnityEngine;

namespace Pancake
{
    public class SoundEntry : BaseMono
    {
        [SerializeField] private SoundPreset soundPreset;

        private void Awake() { MagicAudio.InstallSoundPreset(soundPreset); }

        private void OnDestroy() { MagicAudio.Reset(); }
    }
}