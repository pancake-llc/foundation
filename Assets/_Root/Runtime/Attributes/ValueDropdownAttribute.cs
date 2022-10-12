namespace Pancake
{
    public sealed class ValueDropdownAttribute : ViewAttribute
    {
        public readonly string member;

        public ValueDropdownAttribute(string member) { this.member = member; }
    }
}