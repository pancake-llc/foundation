using Pancake.Scriptable;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Pancake.ScriptableEditor
{
    public static class ScriptableContextMenuCreator
    {
        [MenuItem("CONTEXT/ScriptableBase/Reset")]
        private static void Reset(MenuCommand command) => ResetToDefaultValue(command.context);

        [ContextMenu("Reset To Default Value")]
        private static void ResetToDefaultValue(Object unityObject)
        {
            var reset = unityObject as ScriptableBase;
            if (reset != null) reset.Reset();
            EditorUtility.SetDirty(unityObject);
        }

        [MenuItem("CONTEXT/ScriptableVariableBase/ResetToInitialValue")]
        private static void ResetToInitialValue(MenuCommand command) => ResetToInitialValue(command.context);

        [ContextMenu("Reset To Initial Values")]
        private static void ResetToInitialValue(Object unityObject)
        {
            if (unityObject is IReset reset) reset.ResetToInitialValue();
        }
    }
}