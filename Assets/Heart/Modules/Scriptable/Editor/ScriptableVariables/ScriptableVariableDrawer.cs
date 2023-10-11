using System;
using System.Collections.Generic;
using Pancake.ExLibEditor;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.ScriptableEditor
{
    [CustomEditor(typeof(ScriptableVariableBase), true)]
    public class ScriptableVariableDrawer : UnityEditor.Editor
    {
        private ScriptableBase _scriptableBase = null;
        private ScriptableVariableBase _scriptableVariable = null;
        private static bool repaintFlag;

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            //Check for Serializable
            if (_scriptableVariable == null) _scriptableVariable = target as ScriptableVariableBase;
            var genericType = _scriptableVariable.GetGenericType;
            if (!EditorExtend.IsSerializable(genericType)) EditorExtend.DrawSerializationError(genericType);

            if (ScriptableEditorSetting.DrawMode == EVariableDrawMode.Minimal) DrawMinimal();
            else DrawDefault();

            if (serializedObject.ApplyModifiedProperties()) EditorUtility.SetDirty(target);

            if (!EditorApplication.isPlaying) return;

            var container = (IDrawObjectsInInspector) target;
            var objects = container.GetAllObjects();

            Uniform.DrawLine();

            if (objects.Count > 0) DisplayAll(objects);
        }

        private void DrawMinimal() { Uniform.DrawOnlyField(serializedObject, "value", false); }

        private void DrawDefault()
        {
            Uniform.DrawOnlyField(serializedObject, "m_Script", true);
            var propertiesToHide = CanShowMinMaxProperty(target) ? new[] {"m_Script"} : new[] {"m_Script", "minMax"};
            Uniform.DrawInspectorExcept(serializedObject, propertiesToHide);

            if (GUILayout.Button("Reset to initial value"))
            {
                var so = (IReset) target;
                so.ResetToInitialValue();
            }
        }

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
                try
                {
                    if (obj == null) continue;

                    var text = $"{obj.name}  ({obj.GetType().Name})";
                    Uniform.DrawSelectableObject(obj, new[] {text, "Select"});
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            GUILayout.EndVertical();
        }

        #region Repaint

        private void OnEnable()
        {
            if (repaintFlag)
                return;

            _scriptableBase = target as ScriptableBase;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            repaintFlag = true;
        }

        private void OnDisable() { EditorApplication.playModeStateChanged -= OnPlayModeStateChanged; }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            if (target == null) return;

            if (obj == PlayModeStateChange.EnteredPlayMode)
            {
                if (_scriptableBase == null) _scriptableBase = (ScriptableBase) target;
                _scriptableBase.repaintRequest += OnRepaintRequested;
            }
            else if (obj == PlayModeStateChange.ExitingPlayMode) _scriptableBase.repaintRequest -= OnRepaintRequested;
        }

        private void OnRepaintRequested() => Repaint();

        #endregion
    }
}