using System.Linq;
using Pancake.ExLibEditor;
using Pancake.Scriptable;
using UnityEditor;

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
            if (prop.NextVisible(true))
            {
                do
                {
                    if (propertiesToHide.Any(prop.name.Contains)) continue;

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
                } while (prop.NextVisible(false));
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}