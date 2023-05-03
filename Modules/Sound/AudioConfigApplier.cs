using UnityEngine;

namespace Pancake.Sound
{
    using Pancake;

    /// <summary>
    /// Helper component, to quickly apply the settings from an <c>AudioConfig</c> scriptableObject to an <c>AudioSource</c>.
    /// Useful to add a configuration to the AudioSource that a Timeline is pointing to.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioConfigApplier : CacheGameComponent<AudioSource>
    {
        public AudioConfig config;

        private void Start() { Reset(); }

        private void OnValidate() { Reset(); }

        protected override void Reset()
        {
            base.Reset();
            config.ApplyTo(component);
        }
    }
}