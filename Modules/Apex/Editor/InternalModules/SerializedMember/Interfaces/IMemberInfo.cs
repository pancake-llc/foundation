using System.Reflection;

namespace Pancake.ApexEditor
{
    public interface IMemberInfo
    {
        /// <summary>
        /// Member info of serialized member.
        /// </summary>
        MemberInfo GetMemberInfo();
    }
}