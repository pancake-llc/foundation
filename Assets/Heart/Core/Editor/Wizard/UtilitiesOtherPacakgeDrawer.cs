using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesOtherPacakgeDrawer
    {
        public static void OnInspectorGUI()
        {
            GUILayout.Space(4);

            #region att

#if PANCAKE_ATT
            Uniform.DrawInstalled("1.2.0");
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall iOS 14 Advertising Support Package", GUILayout.MaxHeight(30f)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall iOS14AdvertisingSupport",
                    "Are you sure you want to uninstall iOS 14 Advertising Support package ?",
                    "Yes",
                    "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.unity.ads.ios-support");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install iOS 14 Advertising Support Package", GUILayout.MaxHeight(30f)))
            {
                RegistryManager.Add("com.unity.ads.ios-support", "1.2.0");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif

            #endregion

            GUILayout.Space(40);

            #region needle console

#if PANCAKE_NEEDLE_CONSOLE
            Uniform.DrawInstalled("2.4.1");
            if (GUILayout.Button("Open Setting", GUILayout.MaxHeight(30f)))
            {
                SettingsService.OpenUserPreferences("Preferences/Needle/Console");
            }
           
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall Needle Console", GUILayout.MaxHeight(30f)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Needle Console", "Are you sure you want to uninstall needle console package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.needle.console");
                    RegistryManager.Remove("com.pancake.demystifier");
                    RegistryManager.Remove("com.pancake.unsafe");
                    RegistryManager.Remove("com.pancake.immutable");
                    RegistryManager.Remove("com.pancake.reflection.metadata");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Needle Console", GUILayout.MaxHeight(30f)))
            {
                RegistryManager.Add("com.needle.console", "https://github.com/pancake-llc/needle-console.git?path=package#2.4.1");
                RegistryManager.Add("com.pancake.demystifier", "https://github.com/pancake-llc/ben-demystifier.git#0.4.1");
                RegistryManager.Add("com.pancake.unsafe", "https://github.com/pancake-llc/system-unsafe.git#5.0.0");
                RegistryManager.Add("com.pancake.immutable", "https://github.com/pancake-llc/system-immutable.git#5.0.0");
                RegistryManager.Add("com.pancake.reflection.metadata", "https://github.com/pancake-llc/reflection-metadata.git#5.0.0");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif

            #endregion

            GUILayout.Space(40);

            #region particle effect ugui

#if PANCAKE_PARTICLE_EFFECT_UGUI
            EditorGUILayout.BeginHorizontal();
            const string label = "Particle Effect For UGUI Installed 4.5.1";
            GUILayout.Label(label, new GUIStyle(Uniform.HeaderLabel) {margin = new RectOffset(0, 0, 6, 0)});
            var lastRect = GUILayoutUtility.GetLastRect();
            var iconRect = new Rect(lastRect.x + label.Length * 6f + 4, lastRect.y, 10, lastRect.height);
            GUI.Label(iconRect, Uniform.IconContent("CollabNew"), Uniform.InstalledIcon);
            GUI.backgroundColor = Uniform.Red;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Uninstall", GUILayout.MaxHeight(30f), GUILayout.MinWidth(120)))
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
            EditorGUILayout.EndHorizontal();

#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Needle Console", GUILayout.MaxHeight(30f)))
            {
                RegistryManager.Add("com.needle.console", "https://github.com/pancake-llc/needle-console.git?path=package#2.4.1");
                RegistryManager.Add("com.pancake.demystifier", "https://github.com/pancake-llc/ben-demystifier.git#0.4.1");
                RegistryManager.Add("com.pancake.unsafe", "https://github.com/pancake-llc/system-unsafe.git#5.0.0");
                RegistryManager.Add("com.pancake.immutable", "https://github.com/pancake-llc/system-immutable.git#5.0.0");
                RegistryManager.Add("com.pancake.reflection.metadata", "https://github.com/pancake-llc/reflection-metadata.git#5.0.0");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif

            #endregion

            GUILayout.Space(40);

            #region selective profile

#if PANCAKE_SELECTIVE_PROFILING
            EditorGUILayout.BeginHorizontal();
            const string labelSelective = "Selective Profiler Installed 1.0.1-pre.1";
            GUILayout.Label(labelSelective, new GUIStyle(Uniform.HeaderLabel) {margin = new RectOffset(0, 0, 6, 0)});
            var lastRectSelective = GUILayoutUtility.GetLastRect();
            var iconRectSelective = new Rect(lastRectSelective.x + labelSelective.Length * 6f + 4, lastRectSelective.y, 10, lastRectSelective.height);
            GUI.Label(iconRectSelective, Uniform.IconContent("CollabNew"), Uniform.InstalledIcon);

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Setting", GUILayout.MaxHeight(30f), GUILayout.MinWidth(120)))
            {
                SettingsService.OpenProjectSettings("Project/Needle/Selective Profiler");
            }
            GUILayout.Space(8);
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall", GUILayout.MaxHeight(30f), GUILayout.MinWidth(120)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Selective Profiling",
                    "Are you sure you want to uninstall selective profiling package ?",
                    "Yes",
                    "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.needle.selective-profiling");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Selective Profiling", GUILayout.MaxHeight(30f)))
            {
                RegistryManager.Add("com.needle.selective-profiling", "https://github.com/pancake-llc/selective-profiling.git?path=package#1.0.1-pre.1");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif

            #endregion

            GUILayout.Space(40);

            #region ui effect

#if PANCAKE_UI_EFFECT
            EditorGUILayout.BeginHorizontal();
            const string labelUiEffect = "UI Effect Installed 4.0.0-preview.10";
            GUILayout.Label(labelUiEffect, new GUIStyle(Uniform.HeaderLabel) {margin = new RectOffset(0, 0, 6, 0)});
            var lastRectUiEffect = GUILayoutUtility.GetLastRect();
            var iconRectUiEffect = new Rect(lastRectUiEffect.x + labelUiEffect.Length * 6f + 12, lastRectUiEffect.y, 10, lastRectUiEffect.height);
            GUI.Label(iconRectUiEffect, Uniform.IconContent("CollabNew"), Uniform.InstalledIcon);
            GUI.backgroundColor = Uniform.Red;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Uninstall", GUILayout.MaxHeight(30f), GUILayout.MinWidth(120)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall UI Effect", "Are you sure you want to uninstall ui effect package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.coffee.ui-effect");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

#else
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install UI Effect", GUILayout.MaxHeight(30f)))
            {
                RegistryManager.Add("com.coffee.ui-effect", "https://github.com/mob-sakai/UIEffect.git#4.0.0-preview.10");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif

            #endregion
        }
    }
}