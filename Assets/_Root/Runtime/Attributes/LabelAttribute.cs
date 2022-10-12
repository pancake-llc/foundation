namespace Pancake
{
    public sealed class LabelAttribute : ManipulatorAttribute
    {
        public readonly string name;

        public LabelAttribute(string name) { this.name = name; }
    }
}