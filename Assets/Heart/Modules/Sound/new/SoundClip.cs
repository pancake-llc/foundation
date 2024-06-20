using System;
using UnityEngine;

namespace Pancake.Sound
{
    [Serializable]
    public class SoundClip : ISoundClip
    {
        [SerializeField] private AudioClip clip;
        [SerializeField] private float volume;
        [SerializeField] private float delay;
        [SerializeField] private float startPosition;
        [SerializeField] private float endPosition;
        [SerializeField] private float fadeIn;
        [SerializeField] private float fadeOut;
        [SerializeField] private int weight;

        public AudioClip Clip => clip;
        public float Volume => volume;
        public float Delay => delay;
        public float StartPosition => startPosition;
        public float EndPosition => endPosition;
        public float FadeIn => fadeIn;
        public float FadeOut => fadeOut;
        public int Weight => weight;
        public bool IsNull() => clip == null;
    }
}