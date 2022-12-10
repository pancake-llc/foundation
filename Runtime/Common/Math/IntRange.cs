using System;

namespace Pancake
{
    /// <summary>A value range between two values <see cref="start"/> and <see cref="end"/></summary>
    [Serializable]
    public readonly struct IntRange
    {
        /// <summary>The start of this range</summary>
        public readonly int start;
        /// <summary>The end of this range</summary>
        public readonly int end;

        public IntRange(int start, int end)
        {
            this.start = start;
            this.end = end;
        }
    }
}