namespace Pancake.Sound
{
    public interface IPitchSettable
    {
        internal IAudioPlayer SetPitch(float pitch, float fadeTime);
    }
}