namespace Pancake.Sound
{
	public interface IAudioEntity
	{
		bool Loop { get; }
		bool SeamlessLoop { get; }
		float TransitionTime { get; }
        SpatialSetting SpatialSetting { get; }
		int Priority { get; }
        SoundClip PickNewClip();
        float GetMasterVolume();
        float GetPitch();
        float GetRandomValue(float baseValue, ERandomFlag flags);
    } 
}
