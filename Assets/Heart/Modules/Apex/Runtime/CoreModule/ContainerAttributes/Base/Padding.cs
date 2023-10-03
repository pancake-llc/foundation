using UnityEngine;

namespace Pancake.Apex
{
    public struct Padding
    {
        public float top;
        public float bottom;
        public float left;
        public float right;
        public float xOffset;
        public float yOffset;
        public float space;

        public Padding(float top, float bottom, float left, float right)
        {
            this.top = top;
            this.bottom = bottom;
            this.left = left;
            this.right = right;
            this.xOffset = 0.0f;
            this.yOffset = 0.0f;
            this.space = 0.0f;
        }

        public Padding(float top, float bottom, float left, float right, float xOffset, float yOffset)
            : this(top, bottom, left, right)
        {
            this.xOffset = xOffset;
            this.yOffset = yOffset;
            this.space = 0.0f;
        }

        public Padding(float top, float bottom, float left, float right, float xOffset, float yOffset, float space)
            : this(top,
                bottom,
                left,
                right,
                xOffset,
                yOffset)
        {
            this.space = space;
        }

        public Rect PaddingRect(Rect source)
        {
            return new Rect(source.x + left + xOffset, source.y + top + yOffset, source.width - right - left, source.height - bottom - top);
        }

        public float GetHeightDifference() { return space + bottom + yOffset; }
    }
}