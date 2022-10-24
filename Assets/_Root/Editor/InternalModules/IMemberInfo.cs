using System.Reflection;

namespace Pancake.Editor
{
    public interface IMemberInfo
    {
        /// <summary>
        /// Member info of serialized member.
        /// </summary>
        MemberInfo GetMemberInfo();
    }
}