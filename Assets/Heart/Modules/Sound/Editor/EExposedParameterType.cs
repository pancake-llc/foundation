namespace PancakeEditor.Sound.Reflection
{
    [System.Flags]
    public enum EExposedParameterType
    {
        None = 0,
        Volume = 1 << 0,
        Pitch = 1 << 1,
        EffectSend = 1 << 2,
        All = Volume | Pitch | EffectSend,
    }
}