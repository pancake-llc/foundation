using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesInAppReviewDrawer
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_IN_APP_REVIEW
            Uniform.DrawInstalled("in-app-review 1.8.1");
            EditorGUILayout.Space();
#endif

#if PANCAKE_GOOGLE_EDM
            Uniform.DrawInstalled("google edm 1.2.177");
            EditorGUILayout.Space();
#endif

#if !PANCAKE_IN_APP_REVIEW
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install In App Review", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.google.play.review", "https://github.com/google-unity/in-app-review.git#1.8.1");
                RegistryManager.Add("com.google.play.core", "https://github.com/google-unity/google-play-core.git#1.8.1");
                RegistryManager.Add("com.google.play.common", "https://github.com/google-unity/google-play-common.git#1.8.1");
                RegistryManager.Add("com.google.android.appbundle", "https://github.com/google-unity/android-app-bundle.git#1.8.0");
                RegistryManager.Add("com.google.external-dependency-manager", "https://github.com/google-unity/external-dependency-manager.git#1.2.175");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif

#if !PANCAKE_GOOGLE_EDM
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Google External Dependency Manager", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.google.external-dependency-manager", "https://github.com/google-unity/external-dependency-manager.git#1.2.177");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif

#if PANCAKE_IN_APP_REVIEW
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall In App Review", GUILayout.MaxHeight(25f)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall In App Review", "Are you sure you want to uninstall in-app-review package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.google.play.review");
                    RegistryManager.Remove("com.google.play.core");
                    RegistryManager.Remove("com.google.play.common");
                    RegistryManager.Remove("com.google.android.appbundle");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
#endif

#if PANCAKE_GOOGLE_EDM
            EditorGUILayout.Space();
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall Google External Dependency Manager", GUILayout.MaxHeight(25f)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall EDM4U", "Are you sure you want to uninstall EDM4U package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.google.external-dependency-manager");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
#endif
        }
    }
}