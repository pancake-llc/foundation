using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [CustomEditor(typeof(ApexSettings))]
    sealed class ApexSettingsEditor : AEditor
    {
        private bool changed;

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GetRootContainer() != null && GetRootContainer().HasObjectChanged())
            {
                changed = true;
            }

            if (changed && GUILayout.Button("Save"))
            {
                ApexSettings settings = target as ApexSettings;
                settings.Save();
                RebuildAll();
                changed = false;
            }
        }
    }
}