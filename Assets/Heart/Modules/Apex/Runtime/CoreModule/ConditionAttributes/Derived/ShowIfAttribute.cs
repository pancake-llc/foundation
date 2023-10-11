namespace Pancake.Apex
{
    public sealed class ShowIfAttribute : ConditionAttribute
    {
        public ShowIfAttribute(string member)
            : base(member)
        {
        }

        public ShowIfAttribute(string member, object comparer)
            : base(member, comparer)
        {
        }
    }
}