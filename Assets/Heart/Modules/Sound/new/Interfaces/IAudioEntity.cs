namespace Pancake.Sound
{
    public interface IAudioEntity
    {
        float MasterVolume { get; }
        bool Loop { get; }
        bool SeamlessLoop { get; }
        float TransitionTime { get; }
        SpatialSetting SpatialSetting { get; }
        int Priority { get; }
        float Pitch { get; }
        ERandomFlags RandomFlags { get; }
        float PitchRandomRange { get; }
        float VolumeRandomRange { get; }
        SoundClip PickNewClip();
    }
}