namespace Pancake.Sound
{
    public abstract class AudioPlayerDecorator : AudioPlayerInstanceWrapper
    {
        protected AudioPlayerDecorator(AudioPlayer instance)
            : base(instance)
        {
        }
    }
}