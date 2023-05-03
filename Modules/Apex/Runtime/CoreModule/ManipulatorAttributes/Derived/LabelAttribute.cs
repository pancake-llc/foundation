namespace Pancake.Apex
{
    public sealed class LabelAttribute : ManipulatorAttribute
    {
        public readonly string name;

        public LabelAttribute(string name) { this.name = name; }
    }
}