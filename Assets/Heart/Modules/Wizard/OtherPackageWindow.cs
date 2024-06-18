using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

// ReSharper disable UnusedMember.Local
namespace PancakeEditor
{
    internal static class OtherPackageWindow
    {
        public static void OnInspectorGUI()
        {
            GUILayout.Space(4);

#if PANCAKE_ATT
            UninstallAtt();
#else
            InstallAtt();
#endif

            GUILayout.Space(4);

#if PANCAKE_PROFILE_ANALYZER
            UninstallProfileAnalyzer();
#else
            InstallProfileAnalyzer();
#endif

            GUILayout.Space(4);

#if PANCAKE_NOTIFICATION
            EditorGUILayout.BeginHorizontal();
            Uniform.DrawInstalled("Notification 2.3.2", new RectOffset(0, 0, 6, 0));


            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Open Setting", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                SettingsService.OpenProjectSettings("Project/Mobile Notifications");
            }

            if (GUILayout.Button("See Wiki", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                Application.OpenURL("https://github.com/pancake-llc/heart/wiki/notification");
            }

            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT), GUILayout.MinWidth(80)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Notification", $"Are you sure you want to uninstall Notification package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.unity.mobile.notifications");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
#else
            InstallNotification();
#endif

#if PANCAKE_TEST_PERFORMANCE
            Uninstall("Test Performance 3.0.3", "com.unity.test-framework.performance");
#else
            InstallTestPerformance();
#endif

            GUILayout.Space(4);

#if PANCAKE_PARTICLE_EFFECT_UGUI
            Uninstall("Particle Effect For UGUI 4.6.8", "com.coffee.ui-particle");
#else
            InstallParticleEffectUGUI();
#endif

            GUILayout.Space(4);

#if PANCAKE_UI_EFFECT
            Uninstall("UI Effect 4.0.0-preview.10", "com.coffee.ui-effect");
#else
            InstallUIEffect();
#endif

            GUILayout.Space(4);

#if PANCAKE_IN_APP_REVIEW
            EditorGUILayout.BeginHorizontal();
            Uniform.DrawInstalled("In-App-Review 1.8.1");

            GUI.backgroundColor = Uniform.Red;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Uninstall", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT), GUILayout.MinWidth(80)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall In-App-Review", "Are you sure you want to uninstall In-App-Review package ?", "Yes", "No");
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
            EditorGUILayout.EndHorizontal();
#else
            InstallUIEffect();
#endif
        }

        private static void InstallAtt()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install iOS 14 Advertising Support Package", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
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

            if (GUILayout.Button("Uninstall", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT), GUILayout.MinWidth(100)))
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

        private static void InstallParticleEffectUGUI()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Particle Effect For UGUI", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                RegistryManager.Add("com.coffee.ui-particle", "https://github.com/mob-sakai/ParticleEffectForUGUI.git#4.6.8");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
        }

        private static void InstallUIEffect()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install UI Effect", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                RegistryManager.Add("com.coffee.ui-effect", "https://github.com/mob-sakai/UIEffect.git#4.0.0-preview.10");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
        }

        private static void InstallProfileAnalyzer()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Profiler Analyzer", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                RegistryManager.Add("com.unity.performance.profile-analyzer", "1.2.2");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
        }

        private static void UninstallProfileAnalyzer()
        {
            EditorGUILayout.BeginHorizontal();
            Uniform.DrawInstalled("Profile Analyzer 1.2.2", new RectOffset(0, 0, 6, 0));

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Dashboard", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT), GUILayout.MinWidth(100)))
            {
                EditorApplication.ExecuteMenuItem("Window/Analysis/Profile Analyzer");
            }

            GUILayout.Space(8);
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT), GUILayout.MinWidth(100)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Profile Analyzer",
                    "Are you sure you want to uninstall ProfileAnalyzer package ?",
                    "Yes",
                    "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.unity.performance.profile-analyzer");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private static void InstallTestPerformance()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Test Performance", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                RegistryManager.Add("com.unity.test-framework.performance", "3.0.3");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
        }

        private static void InstallNotification()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Unity Local Notification", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                RegistryManager.Add("com.unity.mobile.notifications", "2.3.2");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
        }

        private static void InstallInAppReview()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install In-App-Review", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                RegistryManager.Add("com.google.play.review", "https://github.com/google-unity/in-app-review.git#1.8.1");
                RegistryManager.Add("com.google.play.core", "https://github.com/google-unity/google-play-core.git#1.8.1");
                RegistryManager.Add("com.google.play.common", "https://github.com/google-unity/google-play-common.git#1.8.1");
                RegistryManager.Add("com.google.android.appbundle", "https://github.com/google-unity/android-app-bundle.git#1.8.0");
                RegistryManager.Add("com.google.external-dependency-manager", "https://github.com/google-unity/external-dependency-manager.git#1.2.175");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
        }

        private static void Uninstall(string namePackage, string bundle)
        {
            EditorGUILayout.BeginHorizontal();
            Uniform.DrawInstalled(namePackage, new RectOffset(0, 0, 6, 0));

            GUI.backgroundColor = Uniform.Red;
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Uninstall", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT), GUILayout.MinWidth(80)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog($"Uninstall {namePackage}", $"Are you sure you want to uninstall {namePackage} package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove(bundle);
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }
    }
}