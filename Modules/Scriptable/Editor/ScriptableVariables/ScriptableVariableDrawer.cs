using System.Collections.Generic;
using PancakeEditor;
using UnityEditor;
using UnityEngine;

namespace Obvious.Soap.Editor
{
    [CustomEditor(typeof(ScriptableVariableBase), true)]
    public class ScriptableVariableDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            if (ScriptableEditorSetting.DrawMode == EVariableDrawMode.Minimal)
                DrawMinimal();
            else
                DrawDefault();

            if (serializedObject.ApplyModifiedProperties())
                EditorUtility.SetDirty(target);

            if (!EditorApplication.isPlaying)
                return;

            var container = (IDrawObjectsInInspector) target;
            var objects = container.GetAllObjects();

            Uniform.DrawLine();

            if (objects.Count > 0)
                DisplayAll(objects);
        }

        private void DrawMinimal() { Uniform.DrawOnlyField(serializedObject, "_value", false); }

        private void DrawDefault()
        {
            Uniform.DrawOnlyField(serializedObject, "m_Script", true);
            var propertiesToHide = CanShowMinMaxProperty(target) ? new[] {"m_Script"} : new[] {"m_Script", "_minMax"};
            Uniform.DrawInspectorExcept(serializedObject, propertiesToHide);

            //hack, weirdly in the wizard, the Vector2 fields are drawn on the line below. 
            //So in order the see them, we need some space. Otherwise they are hidden behind the button.
            if (IsInSoapWizard())
                GUILayout.Space(20f);

            if (GUILayout.Button("Reset to initial value"))
            {
                var so = (ISave) target;
                so.SetToInitialValue();
            }
        }

        private bool IsInSoapWizard() { return EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() == typeof(SoapWizardWindow); }

        private bool CanShowMinMaxProperty(Object targetObject) { return IsIntClamped(targetObject) || IsFloatClamped(targetObject); }

        private bool IsIntClamped(Object targetObject)
        {
            var intVariable = targetObject as IntVariable;
            return intVariable != null && intVariable.IsClamped;
        }

        private bool IsFloatClamped(Object targetObject)
        {
            var floatVariable = targetObject as FloatVariable;
            return floatVariable != null && floatVariable.IsClamped;
        }

        private void DisplayAll(List<Object> objects)
        {
            GUILayout.Space(15);
            var title = $"Objects reacting to OnValueChanged Event : {objects.Count}";
            GUILayout.BeginVertical(title, "window");
            foreach (var obj in objects)
            {
                var text = $"{obj.name}  ({obj.GetType().Name})";
                Uniform.DrawSelectableObject(obj, new[] {text, "Select"});
            }

            GUILayout.EndVertical();
        }
    }
}