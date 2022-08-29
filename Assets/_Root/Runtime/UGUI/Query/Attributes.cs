using System;

namespace Pancake.UIQuery
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class IgnoreTestMemberAttribute : Attribute
    {
    }
}
