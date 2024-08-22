using Pancake.BakingSheet.Unity;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.BakingSheet
{
    [CustomEditor(typeof(SheetContainerScriptableObject))]
    public class SheetContainerCustomInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            base.OnInspectorGUI();
            GUI.enabled = true;
        }
    }
}