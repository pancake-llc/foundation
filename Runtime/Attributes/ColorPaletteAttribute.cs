namespace Pancake
{
    public sealed class ColorPaletteAttribute : ViewAttribute
    {
        public readonly string member;

        public ColorPaletteAttribute(string member) { this.member = member; }
    }
}