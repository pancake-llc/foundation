using UnityEngine;

namespace Pancake.Sound
{
    public interface ISoundClip
    {
        AudioClip Clip { get; }
        float Volume { get; }
        float Delay { get; }
        float StartPosition { get; }
        float EndPosition { get; }
        float FadeIn { get; }
        float FadeOut { get; }
    }
}