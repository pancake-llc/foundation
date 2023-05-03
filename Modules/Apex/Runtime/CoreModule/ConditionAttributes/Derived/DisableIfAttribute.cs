namespace Pancake.Apex
{
    public sealed class DisableIfAttribute : ConditionAttribute
    {
        public DisableIfAttribute(string member)
            : base(member)
        {
        }

        public DisableIfAttribute(string member, object comparer)
            : base(member, comparer)
        {
        }
    }
}