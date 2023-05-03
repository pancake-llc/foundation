using System;

namespace Pancake.ApexEditor
{
    public interface IMemberType
    {
        /// <summary>
        /// Type of serialized member.
        /// </summary>
        Type GetMemberType();
    }
}