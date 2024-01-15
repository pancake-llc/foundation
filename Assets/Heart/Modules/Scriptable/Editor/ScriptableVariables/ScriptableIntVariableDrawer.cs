using System.Linq;
using Pancake.ExLibEditor;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace Pancake.ScriptableEditor
{
    [CustomEditor(typeof(IntVariable), true)]
    public class ScriptableIntVariableDrawer : ScriptableVariableDrawer
    {
        private IntVariable _intVariable;

        protected override void RequireCheck()
        {
            base.RequireCheck();
            if (_intVariable == null) _intVariable = target as IntVariable;
        }

        protected override void DrawMinimal()
        {
            if (_intVariable.IsClamped)
                Uniform.DrawOnlyIntField(serializedObject,
                    "value",
                    false,
                    _intVariable.Min,
                    _intVariable.Max);
            else base.DrawMinimal();
        }

        protected override void DrawDefaultExcept(string[] propertiesToHide)
        {
            if (_intVariable.IsClamped) CustomDraw(propertiesToHide);
            else base.DrawDefaultExcept(propertiesToHide);
        }

        private void CustomDraw(string[] propertiesToHide)
        {
            serializedObject.Update();
            var prop = serializedObject.GetIterator();
            var propertyCount = 0;

            if (prop.NextVisible(true))
            {
                do
                {
                    if (propertiesToHide.Any(skipProp => prop.name == skipProp)) continue;

                    if (prop.name == "value")
                    {
                        Uniform.DrawOnlyIntField(serializedObject,
                            "value",
                            false,
                            _intVariable.Min,
                            _intVariable.Max);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(prop.name), true);
                    }

                    propertyCount++;

                    if (propertyCount != 4) continue;

                    //Draw save properties
                    var isSaved = serializedObject.FindProperty("saved");
                    if (!isSaved.boolValue) continue;
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal();
                    var guidProperty = serializedObject.FindProperty("guid");
                    var guidCreateMode = serializedObject.FindProperty("guidCreateMode");
                    int index = guidCreateMode.enumValueIndex;
                    EditorGUILayout.PropertyField(guidCreateMode, true);
                    if (index == 0)
                    {
                        GUI.enabled = false;
                        EditorGUILayout.TextField(guidProperty.stringValue);
                        GUI.enabled = true;
                    }
                    else
                    {
                        guidProperty.stringValue = EditorGUILayout.TextField(guidProperty.stringValue);
                    }

                    EditorGUILayout.EndHorizontal();
                    EditorGUI.indentLevel--;
                } while (prop.NextVisible(false));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}