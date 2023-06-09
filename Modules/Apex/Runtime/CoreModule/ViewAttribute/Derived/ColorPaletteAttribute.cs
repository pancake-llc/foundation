using System;

namespace Pancake.Apex
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ColorPaletteAttribute : ViewAttribute
    {
        public readonly string member;

        public ColorPaletteAttribute(string member) { this.member = member; }
    }
}