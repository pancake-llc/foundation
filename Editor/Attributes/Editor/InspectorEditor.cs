using UnityEditor;
using UnityEngine;

namespace Pancake.AttributeDrawer
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), editorForChildClasses: true, isFallback = true)]
    internal sealed class InspectorMonoBehaviourEditor : InspectorEditor
    {
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptableObject), editorForChildClasses: true, isFallback = true)]
    internal sealed class InspectorScriptableObjectEditor : InspectorEditor
    {
    }

    public class InspectorEditor : UnityEditor.Editor
    {
        private PropertyTreeForSerializedObject _inspector;

        private void OnDisable()
        {
            _inspector?.Dispose();
            _inspector = null;
        }

        public override void OnInspectorGUI()
        {
            if (serializedObject.targetObjects.Length == 0)
            {
                return;
            }

            if (serializedObject.targetObject == null)
            {
                EditorGUILayout.HelpBox("Script is missing", MessageType.Warning);
                return;
            }

            if (GuiHelper.IsEditorTargetPushed(serializedObject.targetObject))
            {
                GUILayout.Label("Recursive inline editors not supported");
                return;
            }

            if (_inspector == null)
            {
                _inspector = new PropertyTreeForSerializedObject(serializedObject);
            }

            serializedObject.UpdateIfRequiredOrScript();

            _inspector.Update();

            if (_inspector.ValidationRequired)
            {
                _inspector.RunValidation();
            }

            using (GuiHelper.PushEditorTarget(target))
            {
                _inspector.Draw();
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                _inspector.RequestValidation();
            }

            if (_inspector.RepaintRequired)
            {
                Repaint();
            }
        }
    }
}