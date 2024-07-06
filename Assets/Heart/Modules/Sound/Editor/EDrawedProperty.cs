using System;

namespace PancakeEditor.Sound
{
    [Flags]
    public enum EDrawedProperty
    {
        // Clip
        Volume = 1 << 0,
        PlaybackPosition = 1 << 1,
        Fade = 1 << 2,
        ClipPreview = 1 << 3,

        // Overall (starts from 10)
        MasterVolume = 1 << 10,
        Loop = 1 << 11,
        Priority = 1 << 12,
        SpatialSettings = 1 << 13,
        Pitch = 1 << 14,

        All = Volume | PlaybackPosition | Fade | ClipPreview | MasterVolume | Loop | Priority | SpatialSettings | Pitch,
    }
}