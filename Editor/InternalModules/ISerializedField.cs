using UnityEditor;

namespace Pancake.Editor
{
    public interface ISerializedField
    {
        /// <summary>
        /// Target serialized property of serialized field.
        /// </summary>
        SerializedProperty GetSerializedProperty();
    }
}