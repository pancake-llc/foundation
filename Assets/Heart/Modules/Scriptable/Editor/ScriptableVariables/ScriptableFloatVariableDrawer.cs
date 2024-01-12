using System.Linq;
using Pancake.ExLibEditor;
using Pancake.Scriptable;
using UnityEditor;

namespace Pancake.ScriptableEditor
{
    [CustomEditor(typeof(FloatVariable), true)]
    public class ScriptableFloatVariableDrawer : ScriptableVariableDrawer
    {
        private FloatVariable _floatVariable;

        protected override void RequireCheck()
        {
            base.RequireCheck();
            if (_floatVariable == null) _floatVariable = target as FloatVariable;
        }

        protected override void DrawMinimal()
        {
            if (_floatVariable.IsClamped)
                Uniform.DrawOnlyFloatField(serializedObject,
                    "value",
                    false,
                    _floatVariable.Min,
                    _floatVariable.Max);
            else base.DrawMinimal();
        }

        protected override void DrawDefaultExcept(string[] propertiesToHide)
        {
            if (_floatVariable.IsClamped) CustomDraw(propertiesToHide);
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
                        Uniform.DrawOnlyFloatField(serializedObject,
                            "value",
                            false,
                            _floatVariable.Min,
                            _floatVariable.Max);
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