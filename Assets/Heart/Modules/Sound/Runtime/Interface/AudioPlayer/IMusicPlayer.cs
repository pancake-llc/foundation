namespace Pancake.Sound
{
    public interface IMusicPlayer : IEffectDecoratable, IVolumeSettable, IAudioStoppable
    {
        internal IMusicPlayer SetTransition(EAudioTransition transition, EAudioStopMode stopMode, float overrideFade);
    }
}