using Pancake.Attribute;
using UnityEngine.Audio;

namespace Pancake.Sound
{
    using UnityEngine;

    //TODO: Check which settings we really need at this level
    [Searchable]
    [EditorIcon("scriptable_sfx")]
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Pancake/Misc/AudioConfig")]
    public class AudioConfig : ScriptableObject
    {
        private enum PriorityLevel
        {
            Highest = 0,
            High = 64,
            Standard = 128,
            Low = 194,
            VeryLow = 256,
        }

        public AudioMixerGroup output;

        [SerializeField] private PriorityLevel priorityLevel = PriorityLevel.Standard;

        public int Priority { get => (int) priorityLevel; set => priorityLevel = (PriorityLevel) value; }

        [Header("Properties")] public bool mute = false;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(-3f, 3f)] public float pitch = 1f;
        [Range(-1f, 1f)] public float panStereo = 0f;
        [Range(0f, 1.1f)] public float reverbZoneMix = 1f;

        [Header("Spatialisation")] [Range(0f, 1f)] public float spatialBlend = 1f;

        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
        [Range(0.01f, 5f)] public float minDistance = 0.1f;
        [Range(5f, 100f)] public float maxDistance = 50f;
        [Range(0, 360)] public int spread = 0;
        [Range(0f, 5f)] public float dopplerLevel = 1f;

        [Header("Ignores")] public bool bypassEffects = false;
        public bool bypassListenerEffects = false;
        public bool bypassReverbZones = false;
        public bool ignoreListenerVolume = false;
        public bool ignoreListenerPause = false;

        public void ApplyTo(AudioSource audioSource)
        {
            audioSource.outputAudioMixerGroup = output;
            audioSource.mute = mute;
            audioSource.bypassEffects = bypassEffects;
            audioSource.bypassListenerEffects = bypassListenerEffects;
            audioSource.bypassReverbZones = bypassReverbZones;
            audioSource.priority = Priority;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.panStereo = panStereo;
            audioSource.spatialBlend = spatialBlend;
            audioSource.reverbZoneMix = reverbZoneMix;
            audioSource.dopplerLevel = dopplerLevel;
            audioSource.spread = spread;
            audioSource.rolloffMode = rolloffMode;
            audioSource.minDistance = minDistance;
            audioSource.maxDistance = maxDistance;
            audioSource.ignoreListenerVolume = ignoreListenerVolume;
            audioSource.ignoreListenerPause = ignoreListenerPause;
        }
    }
}