using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Pancake.Editor;
#endif

namespace Pancake
{
    /// <summary>
    /// Use on Quaternion
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class EulerAnglesAttribute : PropertyAttribute
    {
#if UNITY_EDITOR

        private Quaternion _quaternion = Quaternion.identity;
        private Vector3 _eulerAngles = Vector3.zero;


        [CustomPropertyDrawer(typeof(EulerAnglesAttribute))]
        private class EulerAnglesDrawer : BasePropertyDrawer<EulerAnglesAttribute>
        {
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                if (EditorGUIUtility.wideMode)
                {
                    return EditorGUIUtility.singleLineHeight;
                }
                else
                {
                    return EditorGUIUtility.singleLineHeight * 2;
                }
            }


            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                if (attribute._quaternion != property.quaternionValue)
                {
                    property.quaternionValue = attribute._quaternion = property.quaternionValue.normalized;
                    attribute._eulerAngles = attribute._quaternion.eulerAngles;
                }

                using (var scope = ChangeCheckScope.New())
                {
                    attribute._eulerAngles = EditorGUI.Vector3Field(position, label, attribute._eulerAngles);
                    if (scope.changed)
                    {
                        property.quaternionValue = attribute._quaternion = Quaternion.Euler(attribute._eulerAngles).normalized;
                    }
                }
            }
        } // class EulerAnglesDrawer

#endif // UNITY_EDITOR
    } // class EulerAnglesAttribute
} // namespace Pancake