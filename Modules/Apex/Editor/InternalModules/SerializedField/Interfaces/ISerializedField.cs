using UnityEditor;

namespace Pancake.ApexEditor
{
    public interface ISerializedField
    {
        /// <summary>
        /// Target serialized property of serialized field.
        /// </summary>
        SerializedProperty GetSerializedProperty();
    }
}