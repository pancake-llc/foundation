namespace Pancake.Sound
{
    public interface IAudioIdentity
    {
        bool Validate();
        int Id { get; }
        string Name { get; }
    }
}