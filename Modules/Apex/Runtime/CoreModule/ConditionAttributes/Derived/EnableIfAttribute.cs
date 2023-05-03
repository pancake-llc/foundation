namespace Pancake.Apex
{
    public sealed class EnableIfAttribute : ConditionAttribute
    {
        public EnableIfAttribute(string member)
            : base(member)
        {
        }

        public EnableIfAttribute(string member, object comparer)
            : base(member, comparer)
        {
        }
    }
}