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
        private ScriptableVariableBase _scriptableVariable;
        private static bool repaintFlag;

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            RequireCheck();
            var genericType = _scriptableVariable.GetGenericType;
            bool canBeSerialized = EditorExtend.IsUnityType(genericType) || EditorExtend.IsSerializable(genericType);
            if (!canBeSerialized)
            {
                EditorExtend.DrawSerializationError(genericType);
                return;
            }

            if (ScriptableEditorSetting.IsExist())
            {
                if (ScriptableEditorSetting.DrawMode == EVariableDrawMode.Minimal) DrawMinimal();
                else DrawDefault();
            }
            else
            {
                DrawDefault();
            }

            if (serializedObject.ApplyModifiedProperties()) EditorUtility.SetDirty(target);

            if (!EditorApplication.isPlaying) return;

            var container = (IDrawObjectsInInspector) target;
            var objects = container.GetAllObjects();

            Uniform.DrawLine();

            if (objects.Count > 0) DisplayAll(objects);
        }

        protected virtual void RequireCheck()
        {
            if (_scriptableVariable == null) _scriptableVariable = target as ScriptableVariableBase;
        }

        protected virtual void DrawMinimal() { Uniform.DrawOnlyField(serializedObject, "value", false); }

        private void DrawDefault()
        {
            Uniform.DrawOnlyField(serializedObject, "m_Script", true);
            string[] propertiesToHide = CanShowMinMaxProperty(target)
                ? new[] {"m_Script", "guid", "guidCreateMode"}
                : new[] {"m_Script", "minMax", "guid", "guidCreateMode"};
            DrawDefaultExcept(propertiesToHide);
            
            GUILayout.Space(4);
            var isSaved = serializedObject.FindProperty("saved");
            var resetOn = serializedObject.FindProperty("resetOn");
            if (isSaved.boolValue && resetOn.enumValueIndex == (int) ResetType.ApplicationStarts)
            {
                EditorGUILayout.HelpBox(
                    "When saved equal true \nresetOn should be set to SceneLoaded or AdditiveSceneLoaded \nSo that the value is updated again when using the Restore Data.",
                    MessageType.Warning);
            }

            GUILayout.Space(4);
            if (GUILayout.Button("Reset to initial value"))
            {
                var so = (IReset) target;
                so.ResetToInitialValue();
            }
        }

        protected virtual void DrawDefaultExcept(string[] propertiesToHide) { Uniform.DrawVariableCustomInspector(serializedObject, propertiesToHide); }

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
            if (repaintFlag) return;

            RequireCheck();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            repaintFlag = true;
        }

        private void OnDisable() { EditorApplication.playModeStateChanged -= OnPlayModeStateChanged; }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            if (target == null) return;

            if (obj == PlayModeStateChange.EnteredPlayMode)
            {
                RequireCheck();
                _scriptableVariable.repaintRequest += OnRepaintRequested;
            }
            else if (obj == PlayModeStateChange.ExitingPlayMode) _scriptableVariable.repaintRequest -= OnRepaintRequested;
        }

        private void OnRepaintRequested() => Repaint();

        #endregion
    }
}