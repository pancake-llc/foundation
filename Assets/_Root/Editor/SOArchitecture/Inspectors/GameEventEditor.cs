using Pancake.SOA;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor.SOA
{
    [CustomEditor(typeof(GameEventBase), true)]
    public sealed class GameEventEditor : BaseGameEventEditor
    {
        private SerializedProperty _descriptionProperty;
        private GameEvent Target { get { return (GameEvent)target; } }

        protected override void OnEnable()
        {
            base.OnEnable();
            _descriptionProperty = serializedObject.FindProperty("description");
        }

        protected override void DrawRaiseButton()
        {
            DrawDeveloperDescription();
            if (GUILayout.Button("Raise"))
            {
                Target.Raise();
            }
        }

        protected override void DrawDeveloperDescription()
        {
            EditorGUILayout.LabelField("Developer Description", Uniform.TextImportant);
            _descriptionProperty.stringValue = EditorGUILayout.TextArea(_descriptionProperty.stringValue);
            Uniform.SpaceOneLine();
        }
    } 
}