namespace Pancake.Sound
{
    public enum EPlayMode
    {
        Single, // Always play the first clip
        Sequence, // Play clips sequentially
        Random, // Play clips randomly with the given weight
    }

    [System.Flags]
    public enum ERandomFlags
    {
        None = 0,
        Pitch = 1 << 0,
        Volume = 1 << 1,
    }

    [System.Flags]
    public enum ESoundType
    {
        None = 0,

        Music = 1 << 0,
        UI = 1 << 1,
        Ambience = 1 << 2,
        Sfx = 1 << 3,
        VoiceOver = 1 << 4,

        All = Music | UI | Ambience | Sfx | VoiceOver,
    }

    public enum ESpatialPropertyType
    {
        StereoPan,
        DopplerLevel,
        MinDistance,
        MaxDistance,
        SpatialBlend,
        ReverbZoneMix,
        Spread,
        CustomRolloff,
        RolloffMode,
    }
}