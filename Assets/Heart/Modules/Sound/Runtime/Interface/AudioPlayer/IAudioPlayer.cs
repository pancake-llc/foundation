namespace Pancake.Sound
{
    public interface IAudioPlayer : IEffectDecoratable, IVolumeSettable, IMusicDecoratable, IAudioStoppable
    {
        /// <summary>
        /// The SoundId of the player is playing
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Returns true if the player is playing
        /// </summary>
        bool IsPlaying { get; }
    }
}