using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesParticleEffectForUGUIDrawer
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_PARTICLE_EFFECT_UGUI
            Uniform.DrawInstalled("4.2.1");
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall Particle Effect For UGUI", GUILayout.MaxHeight(25f)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Particle Effect For UGUI",
                    "Are you sure you want to uninstall particle effect for ugui package ?",
                    "Yes",
                    "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.coffee.ui-particle");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Particle Effect For UGUI", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.coffee.ui-particle", "https://github.com/mob-sakai/ParticleEffectForUGUI.git#4.2.1");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif
        }
    }
}