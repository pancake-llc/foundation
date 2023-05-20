using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class HorizontalLineAttribute : DecoratorAttribute
    {
        public LineStyle Style { get; set; }
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public HorizontalLineAttribute()
        {
            Style = LineStyle.Thin;
            R = G = B = 0.10196f;
            A = 1f;
        }
    }
}