using System.Reflection;
using Pancake.SOA;
using UnityEditor;
using UnityEngine;
using Type = System.Type;

namespace Pancake.Editor.SOA
{
    [CustomEditor(typeof(GameEventBase<>), true)]
    public class TypedGameEventEditor : BaseGameEventEditor
    {
        private MethodInfo _raiseMethod;
        private SerializedProperty _descriptionProperty;

        protected override void DrawDeveloperDescription()
        {
            EditorGUILayout.LabelField("Developer Description", Uniform.TextImportant);
            _descriptionProperty.stringValue = EditorGUILayout.TextArea(_descriptionProperty.stringValue);
            Uniform.SpaceOneLine();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _descriptionProperty = serializedObject.FindProperty("description");
            _raiseMethod = target.GetType().BaseType.GetMethod("Raise", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
        }
        protected override void DrawRaiseButton()
        {
            SerializedProperty property = serializedObject.FindProperty("_debugValue");

            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                Type debugValueType = GetDebugValueType(property);
                GenericPropertyDrawer.DrawPropertyDrawerLayout(property, debugValueType);

                if (scope.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }

            if (GUILayout.Button("Raise"))
            {
                CallMethod(GetDebugValue(property));
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDeveloperDescription();
            base.OnInspectorGUI();
        }

        private object GetDebugValue(SerializedProperty property)
        {
            Type targetType = property.serializedObject.targetObject.GetType();
            FieldInfo targetField = targetType.GetField("_debugValue", BindingFlags.Instance | BindingFlags.NonPublic);

            return targetField.GetValue(property.serializedObject.targetObject);
        }
        private Type GetDebugValueType(SerializedProperty property)
        {
            Type targetType = property.serializedObject.targetObject.GetType();
            FieldInfo targetField = targetType.GetField("_debugValue", BindingFlags.Instance | BindingFlags.NonPublic);

            return targetField.FieldType;
        }
        private void CallMethod(object value)
        {
            _raiseMethod.Invoke(target, new object[1] { value });
        }
    }
}