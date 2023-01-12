//#if PANCAKE_GAM

using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Pancake
{
    [CreateAssetMenu(menuName = "Pancake/Audio/Sound Asset", fileName = "SoundAsset")]
    public class Sound : ScriptableObject
    {
        [SerializeField, TextArea] private string developerDescription;
        [SerializeField, Ulid] private string id;
        [ValidateInput("ValidateSoundName")] public string soundName;

        [Header("Clip and Output")] public AudioClip clip = null;
        public AudioMixerGroup output = null;
        public SoundLoop loop;
        [ShowIf(nameof(loop), SoundLoop.Number)] public int loopCount = 1;
        public bool playOnAwake = false;

        [Header("Volume and Pitch")] [Range(0f, 1f)] public float volume = 0.5f;
        [Range(-3f, 3f)] public float pitch = 1f;

        public float pitchMin;
        public float pitchMax;

        [Header("3D Sound")] [Range(0, 1f)] public float spatialBlend = 1f;

        public AudioRolloffMode audioMode;
        public float maxRange = 3000f;

        public string ID => id;

#if UNITY_EDITOR
        private ValidationResult ValidateSoundName()
        {
            return string.IsNullOrEmpty(soundName) ? ValidationResult.Error("Sound name can not be empty") : ValidationResult.Valid;
        }
#endif
    }
}

//#endif