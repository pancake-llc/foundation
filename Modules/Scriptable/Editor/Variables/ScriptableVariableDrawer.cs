using System.Collections.Generic;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Scriptable
{
    [CustomEditor(typeof(ScriptableVariableBase), true)]
    public class ScriptableVariableDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            var drawMode = ScriptableEditorSetting.DrawMode;
            if (drawMode == EVariableDrawMode.Default)
            {
                DrawDefault();
            }
            else
            {
                DrawMinimal();
            }

            if (serializedObject.ApplyModifiedProperties()) EditorUtility.SetDirty(target);
            if (!EditorApplication.isPlaying) return;

            var container = (IDrawObjectsInInspector) target;
            var gameObjects = container.GetAllObjects();

            Uniform.DrawLine();
            if (gameObjects.Count > 0) DisplayALl(gameObjects);
        }

        private void DisplayALl(List<Object> gameObjects)
        {
            GUILayout.Space(15);
            var title = $"Objects reacting to OnValueChanged Event : {gameObjects.Count}";
            GUILayout.BeginVertical(title, "window");
            foreach (var obj in gameObjects)
            {
                var text = $"{obj.name}  ({obj.GetType().Name})";
                Uniform.DrawSelectableObject(obj, new[] {text, "Select"});
            }

            GUILayout.EndVertical();
        }

        private void DrawMinimal() { Uniform.DrawOnlyField(serializedObject, "value", false); }

        private void DrawDefault()
        {
            Uniform.DrawOnlyField(serializedObject, "m_Script", true);
            var propertiesToHide = CanShowMinMaxProperty(target) ? new[] {"m_Script"} : new[] {"m_Script", "minMax"};
            Uniform.DrawInspectorExcept(serializedObject, propertiesToHide);

            // hack, weirdly in the wizard, the Vector2 fields are drawn on the line below. 
            // So in order the see them, we need some space. Otherwise they are hidden behind the button.
            if (IsInWizard()) GUILayout.Space(20f);
            
            if (GUILayout.Button("Reset to initial value"))
            {
                var so = (ISave)target;
                so.SetToInitialValue();
            }
        }

        private bool IsInWizard() { return EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() == typeof(Wizard); }

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
    }
}