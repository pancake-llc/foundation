namespace Pancake.Apex
{
    public sealed class HideIfAttribute : ConditionAttribute
    {
        public HideIfAttribute(string member)
            : base(member)
        {
        }

        public HideIfAttribute(string member, object comparer)
            : base(member, comparer)
        {
        }
    }
}