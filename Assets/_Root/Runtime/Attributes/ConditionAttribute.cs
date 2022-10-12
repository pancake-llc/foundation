using System;

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public abstract class ConditionAttribute : ManipulatorAttribute
    {
        public readonly string member;
        public readonly object comparer;

        public ConditionAttribute(string member)
        {
            this.member = member;
        }

        public ConditionAttribute(string member, object comparer)
        {
            this.member = member;
            this.comparer = comparer;
        }
    }
}
