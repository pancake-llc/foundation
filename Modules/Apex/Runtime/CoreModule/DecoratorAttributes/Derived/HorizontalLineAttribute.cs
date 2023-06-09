using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class HorizontalLineAttribute : DecoratorAttribute
    {
        public readonly float height;
        public readonly float r;
        public readonly float g;
        public readonly float b;
        public readonly float a;

        public HorizontalLineAttribute()
        {
            this.height = 1.0f;
            this.r = 0.5f;
            this.g = 0.5f;
            this.b = 0.5f;
            this.a = 1.0f;
            Space = 0f;
        }

        public HorizontalLineAttribute(float width)
            : this()
        {
            this.height = width;
        }

        public HorizontalLineAttribute(float width, float r, float g, float b, float a)
            : this(width)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        #region [Optional]

        public float Space { get; set; }

        #endregion
    }
}