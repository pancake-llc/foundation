using UnityEditor;

namespace Pancake.ApexEditor
{
    public interface ISerializedMember
    {
        /// <summary>
        /// Target serialized object reference of serialized member.
        /// </summary>
        SerializedObject GetSerializedObject();
    }
}