using System;

namespace Pancake.Editor
{
    public interface IMemberType
    {
        /// <summary>
        /// Type of serialized member.
        /// </summary>
        Type GetMemberType();
    }
}