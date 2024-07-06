using Pancake.Sound;

namespace PancakeEditor.Sound
{
    public struct PreviewClip : IClip
    {
        public float StartPosition { get; set; }
        public float EndPosition { get; set; }
        public float FullLength { get; set; }

        public PreviewClip(IClip clip)
        {
            StartPosition = clip.StartPosition;
            EndPosition = clip.EndPosition;
            FullLength = clip.FullLength;
        }

        public PreviewClip(ISoundClip clip)
        {
            StartPosition = clip.StartPosition;
            EndPosition = clip.EndPosition;
            FullLength = clip.AudioClip.length;
        }

        public PreviewClip(float fullLength)
            : this()
        {
            FullLength = fullLength;
        }
    }
}