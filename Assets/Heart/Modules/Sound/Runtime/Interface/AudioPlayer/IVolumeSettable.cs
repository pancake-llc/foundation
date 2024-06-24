namespace Pancake.Sound
{
    public interface IVolumeSettable
    {
        internal IAudioPlayer SetVolume(float vol, float fadeTime);
    }
}