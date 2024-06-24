namespace Pancake.Sound
{
    public interface IPlayerEffect : IVolumeSettable, IMusicDecoratable, IAudioStoppable
    {
#if !UNITY_WEBGL
        internal IPlayerEffect QuietOthers(float othersVol, float fadeTime);
        internal IPlayerEffect QuietOthers(float othersVol, Fading fading);
        internal IPlayerEffect LowPassOthers(float freq, float fadeTime);
        internal IPlayerEffect LowPassOthers(float freq, Fading fading);
        internal IPlayerEffect HighPassOthers(float freq, float fadeTime);
        internal IPlayerEffect HighPassOthers(float freq, Fading fading);
#endif
    }
}