using UnityEngine;

namespace Pancake.Sound
{
    [System.Serializable]
    public class SoundClip : ISoundClip
    {
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private float volume;
        [SerializeField] private float delay;
        [SerializeField] private float startPosition;
        [SerializeField] private float endPosition;
        [SerializeField] private float fadeIn;
        [SerializeField] private float fadeOut;
        [SerializeField] private int weight;

        public AudioClip AudioClip => audioClip;
        public float Volume => volume;
        public float Delay => delay;
        public float StartPosition => startPosition;
        public float EndPosition => endPosition;
        public float FadeIn => fadeIn;
        public float FadeOut => fadeOut;
        public int Weight => weight;
        public bool IsNull() => audioClip == null;

#if UNITY_EDITOR
        public static class EditorPropertyName
        {
            public static string AudioClip => nameof(audioClip);
            public static string Volume => nameof(volume);
            public static string Delay => nameof(delay);
            public static string StartPosition => nameof(startPosition);
            public static string EndPosition => nameof(endPosition);
            public static string FadeIn => nameof(fadeIn);
            public static string FadeOut => nameof(fadeOut);
            public static string Weight => nameof(weight);
        }
#endif
    }

    public interface ISoundClip
    {
        AudioClip AudioClip { get; }
        float Volume { get; }
        float Delay { get; }
        float StartPosition { get; }
        float EndPosition { get; }
        float FadeIn { get; }
        float FadeOut { get; }
    }
}