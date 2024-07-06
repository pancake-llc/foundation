namespace Pancake.Sound
{
    public class AudioTypePlaybackPreference : IAudioPlaybackPreference
    {
        // Each audio type will only have one instance
        public float Volume { get; set; } = AudioConstant.FULL_VOLUME;
        public float Pitch { get; set; } = AudioConstant.DEFAULT_PITCH;
        public EEffectType EffectType { get; set; }
    }

    public interface IAudioPlaybackPreference
    {
        float Volume { get; }
        float Pitch { get; }
        EEffectType EffectType { get; }
    }
}