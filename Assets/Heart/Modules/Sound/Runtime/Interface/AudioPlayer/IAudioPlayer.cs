using System;

namespace Pancake.Sound
{
    public interface IAudioPlayer : IEffectDecoratable, IVolumeSettable, IMusicDecoratable, IAudioStoppable, IPitchSettable
    {
        /// <summary>
        /// The SoundId of the player is playing
        /// </summary>
        SoundId Id { get; }

        /// <summary>
        /// Returns true if the player is playing
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Triggered when the audio player has finished playing
        /// </summary>
        event Action<SoundId> OnEndPlaying;
    }
}