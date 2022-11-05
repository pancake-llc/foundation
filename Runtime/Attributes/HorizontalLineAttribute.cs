using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class HorizontalLineAttribute : DecoratorAttribute
    {
        public readonly float width;
        public readonly float space;
        public readonly float r;
        public readonly float g;
        public readonly float b;
        public readonly float a;

        public HorizontalLineAttribute()
        {
            this.width = 1.0f;
            this.space = 1.0f;
            this.r = 0.5f;
            this.g = 0.5f;
            this.b = 0.5f;
            this.a = 1.0f;
        }

        public HorizontalLineAttribute(float width)
            : this()
        {
            this.width = width;
        }

        public HorizontalLineAttribute(float width, float space)
            : this(width)
        {
            this.space = space;
        }

        public HorizontalLineAttribute(float width, float space, float r, float g, float b, float a)
            : this(width, space)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }
}