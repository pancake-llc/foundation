namespace Pancake.Sound
{
    public interface IMusicPlayer : IEffectDecoratable, IVolumeSettable, IAudioStoppable
    {
        SoundId Id { get; }
        internal IMusicPlayer SetTransition(EAudioTransition transition, EAudioStopMode stopMode, float overrideFade);
    }
}