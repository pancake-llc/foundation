using Pancake.ExLibEditor;
using Pancake.Scriptable;
using UnityEditor;

namespace Pancake.ScriptableEditor
{
    [CustomEditor(typeof(ScriptableConstant<>), true)]
    public class ScritpableConstantDrawer : Editor
    {
        private ScriptableVariableBase _scriptableVariable = null;

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            //Check for Serializable
            if (_scriptableVariable == null) _scriptableVariable = target as ScriptableVariableBase;
            var genericType = _scriptableVariable.GetGenericType;
            if (!EditorExtend.IsSerializable(genericType)) EditorExtend.DrawSerializationError(genericType);

            Uniform.DrawOnlyField(serializedObject, "value", false);

            if (serializedObject.ApplyModifiedProperties()) EditorUtility.SetDirty(target);
        }
    }
}