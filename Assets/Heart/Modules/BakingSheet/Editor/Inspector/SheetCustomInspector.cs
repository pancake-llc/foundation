using Pancake.BakingSheet.Unity;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.BakingSheet
{
    [CustomEditor(typeof(SheetScriptableObject))]
    public class SheetCustomInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            base.OnInspectorGUI();
            GUI.enabled = true;
        }
    }
}