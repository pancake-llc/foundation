using UnityEditor;

namespace Pancake.Editor
{
    public interface ISerializedMember
    {
        /// <summary>
        /// Target serialized object reference of serialized member.
        /// </summary>
        SerializedObject GetSerializedObject();
    }
}