using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

// ReSharper disable UnusedMember.Local
namespace PancakeEditor
{
    public static class UtilitiesOtherPacakgeDrawer
    {
        public static void OnInspectorGUI()
        {
            GUILayout.Space(8);
#if PANCAKE_ATT
            UninstallAtt();
#else
            InstallAtt();
#endif


            GUILayout.Space(30);
#if PANCAKE_NEEDLE_CONSOLE
            UninstallNeedleConsole();
#else
            InstallNeedleConsole();
#endif


            GUILayout.Space(30);
#if PANCAKE_SELECTIVE_PROFILING
            UninstallSelectiveProfiler();
#else
            InstallSelectiveProfiler();
#endif


            GUILayout.Space(30);
#if PANCAKE_PARTICLE_EFFECT_UGUI
            UninstallParticleEffectUGUI();
#else
            InstallParticleEffectUGUI();
#endif


            GUILayout.Space(30);
#if PANCAKE_UI_EFFECT
            UninstallUIEffect();
#else
            InstallUIEffect();
#endif
        }

        private static void InstallAtt()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install iOS 14 Advertising Support Package", GUILayout.MaxHeight(30f)))
            {
                RegistryManager.Add("com.unity.ads.ios-support", "1.2.0");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
        }

        private static void UninstallAtt()
        {
            EditorGUILayout.BeginHorizontal();
            Uniform.DrawInstalled("ATT 1.2.0", new RectOffset(0, 0, 6, 0));

            GUI.backgroundColor = Uniform.Red;
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Uninstall", GUILayout.MaxHeight(30f), GUILayout.MinWidth(120)))
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
            EditorGUILayout.EndHorizontal();
        }

        private static void InstallNeedleConsole()
        {
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
        }

        private static void UninstallNeedleConsole()
        {
            EditorGUILayout.BeginHorizontal();
            Uniform.DrawInstalled("Needle Console 4.5.1", new RectOffset(0, 0, 6, 0));

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Setting", GUILayout.MaxHeight(30f), GUILayout.MinWidth(120)))
            {
                SettingsService.OpenUserPreferences("Preferences/Needle/Console");
            }

            GUILayout.Space(8);
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall", GUILayout.MaxHeight(30f), GUILayout.MinWidth(120)))
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
            EditorGUILayout.EndHorizontal();
        }

        private static void InstallSelectiveProfiler()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Selective Profiling", GUILayout.MaxHeight(30f)))
            {
                RegistryManager.Add("com.needle.selective-profiling", "https://github.com/pancake-llc/selective-profiling.git?path=package#1.0.1-pre.1");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
        }

        private static void UninstallSelectiveProfiler()
        {
            EditorGUILayout.BeginHorizontal();
            Uniform.DrawInstalled("Selective Profiler 1.0.1-pre.1", labelMargin: new RectOffset(0, 0, 6, 0));

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
        }

        private static void InstallParticleEffectUGUI()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Particle Effect For UGUI", GUILayout.MaxHeight(30f)))
            {
                RegistryManager.Add("com.coffee.ui-particle", "https://github.com/mob-sakai/ParticleEffectForUGUI.git#4.5.1");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
        }

        private static void UninstallParticleEffectUGUI()
        {
            EditorGUILayout.BeginHorizontal();
            Uniform.DrawInstalled("Particle Effect For UGUI 4.5.1", new RectOffset(0, 0, 6, 0));

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
        }

        private static void InstallUIEffect()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install UI Effect", GUILayout.MaxHeight(30f)))
            {
                RegistryManager.Add("com.coffee.ui-effect", "https://github.com/mob-sakai/UIEffect.git#4.0.0-preview.10");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
        }

        private static void UninstallUIEffect()
        {
            EditorGUILayout.BeginHorizontal();
            Uniform.DrawInstalled("UI Effect 4.0.0-preview.10", new RectOffset(0, 0, 6, 0));

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
        }
    }
}