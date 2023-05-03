namespace Pancake.Apex
{
    public sealed class ValueDropdownAttribute : ViewAttribute
    {
        public readonly string name;

        public ValueDropdownAttribute(string member) { this.name = member; }
    }
}