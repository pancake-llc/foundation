using Pancake.Apex;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [ManipulatorTarget(typeof(AssetOnlyAttribute))]
    public sealed class AssetOnlyManipulator : MemberManipulator, ITypeValidationCallback
    {
        /// <summary>
        /// Called before rendering member GUI.
        /// </summary>
        public override void OnBeforeGUI()
        {
            bool isActive = true;

            Object[] objects = DragAndDrop.objectReferences;
            if (objects != null && objects.Length > 0)
            {
                GameObject go = objects[0] as GameObject;
                if (go != null)
                {
                    isActive = !(PrefabUtility.GetPrefabAssetType(go) == PrefabAssetType.NotAPrefab);
                }
            }

            if (Event.current.type == EventType.DragExited)
            {
                isActive = true;
            }

            EditorGUI.BeginDisabledGroup(!isActive);
        }

        /// <summary>
        /// Called after rendering member GUI.
        /// </summary>
        public override void OnAfterGUI() { EditorGUI.EndDisabledGroup(); }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        /// <param name="label">Display label of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property) { return property.propertyType == SerializedPropertyType.ObjectReference; }
    }
}