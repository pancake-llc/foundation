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
            Uninstall("ATT 1.2.0", "com.unity.ads.ios-support");
#else
            InstallAtt();
#endif

            GUILayout.Space(4);

            var analyzer = RegistryManager.IsInstalled("com.unity.performance.profile-analyzer");
            if (analyzer.Item1) UninstallProfileAnalyzer(analyzer.Item2);
            else InstallProfileAnalyzer();

            GUILayout.Space(4);

            var noti = RegistryManager.IsInstalled("com.unity.mobile.notifications");
            if (noti.Item1) UninstallNotification(noti.Item2);
            else InstallNotification();

            GUILayout.Space(4);

            var testFramework = RegistryManager.IsInstalled("com.unity.test-framework.performance");
            if (testFramework.Item1) UninstallTestPerformance(true, testFramework.Item2);
            else InstallTestPerformance();

            GUILayout.Space(4);

            var particle = RegistryManager.IsInstalled("com.coffee.ui-particle");
            if (particle.Item1)
            {
                string version = RegistryManager.GetVersionByPackageJson("com.coffee.ui-particle");
                Uninstall($"Particle Effect For UGUI {version}", "com.coffee.ui-particle");
            }
            else InstallParticleEffectUGUI();

            GUILayout.Space(4);

            var uiEffect = RegistryManager.IsInstalled("com.coffee.ui-effect");
            if (uiEffect.Item1)
            {
                string version = RegistryManager.GetVersionByPackageJson("com.coffee.ui-effect");
                Uninstall($"UI Effect {version}", "com.coffee.ui-effect");
            }
            else InstallUIEffect();

            GUILayout.Space(4);

#if PANCAKE_IN_APP_REVIEW
            UninstallInAppReview();
#else
            InstallInAppReview();
#endif
        }

        private static void InstallAtt()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install iOS 14 Advertising Support Package", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                RegistryManager.AddPackage("com.unity.ads.ios-support", "1.2.0");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
        }

        private static void InstallParticleEffectUGUI()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Particle Effect For UGUI", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                RegistryManager.AddPackage("com.coffee.ui-particle", "https://github.com/mob-sakai/ParticleEffectForUGUI.git");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
        }

        private static void InstallUIEffect()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install UI Effect", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                RegistryManager.AddPackage("com.coffee.ui-effect", "https://github.com/mob-sakai/UIEffect.git?path=Packages/src");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
        }

        private static async void InstallProfileAnalyzer()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Profiler Analyzer", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                await RegistryManager.InstallLastVersionForPacakge("com.unity.performance.profile-analyzer");
            }

            GUI.enabled = true;
        }

        private static void UninstallProfileAnalyzer(string version)
        {
            EditorGUILayout.BeginHorizontal();
            Uniform.DrawInstalled($"Profile Analyzer {version}", new RectOffset(0, 0, 6, 0));

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Dashboard", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT), GUILayout.MinWidth(100)))
            {
                EditorApplication.ExecuteMenuItem("Window/Analysis/Profile Analyzer");
            }

            GUILayout.Space(8);
            GUI.backgroundColor = Uniform.Red_500;
            if (GUILayout.Button("Uninstall", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT), GUILayout.MinWidth(100)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Profile Analyzer",
                    "Are you sure you want to uninstall ProfileAnalyzer package ?",
                    "Yes",
                    "No");
                if (confirmDelete)
                {
                    RegistryManager.RemovePackage("com.unity.performance.profile-analyzer");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private static async void InstallTestPerformance()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Test Performance", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                await RegistryManager.InstallLastVersionForPacakge("com.unity.test-framework.performance");
            }

            GUI.enabled = true;
        }

        private static void UninstallTestPerformance(bool status, string version)
        {
            EditorGUILayout.BeginHorizontal();
            if (status) Uniform.DrawInstalled($"Test Performance {version}", new RectOffset(0, 0, 6, 0));
            else
            {
                if (RegistryManager.IsInstalled("com.unity.collections").Item1)
                {
                    Uniform.DrawInstalled($"Test Performance 3.0.3", new RectOffset(0, 0, 6, 0));
                }
            }

            GUI.backgroundColor = Uniform.Red_500;
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Uninstall", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT), GUILayout.MinWidth(80)))
            {
                bool confirmDelete =
                    EditorUtility.DisplayDialog("Uninstall Test Performance", $"Are you sure you want to uninstall Test Performance package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.RemovePackage("com.unity.test-framework.performance");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private static async void InstallNotification()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Unity Local Notification", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                await RegistryManager.InstallLastVersionForPacakge("com.unity.mobile.notifications");
            }

            GUI.enabled = true;
        }

        private static void UninstallNotification(string version)
        {
            EditorGUILayout.BeginHorizontal();

            Uniform.DrawInstalled($"Notification {version}", new RectOffset(0, 0, 6, 0));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Open Setting", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                SettingsService.OpenProjectSettings("Project/Mobile Notifications");
            }

            if (GUILayout.Button("See Wiki", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                Application.OpenURL("https://github.com/pancake-llc/heart/wiki/notification");
            }

            GUI.backgroundColor = Uniform.Red_500;
            if (GUILayout.Button("Uninstall", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT), GUILayout.MinWidth(80)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Notification", $"Are you sure you want to uninstall Notification package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.RemovePackage("com.unity.mobile.notifications");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private static void InstallInAppReview()
        {
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install In-App-Review", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                RegistryManager.AddPackage("com.google.play.review", "https://github.com/pancake-llc/in-app-review.git#1.8.3");
                RegistryManager.AddPackage("com.google.play.core", "https://github.com/pancake-llc/google-play-core.git#1.8.5");
                RegistryManager.AddPackage("com.google.play.common", "https://github.com/pancake-llc/google-play-common.git#1.9.2");
                RegistryManager.AddPackage("com.google.external-dependency-manager", "https://github.com/googlesamples/unity-jar-resolver.git?path=upm#v1.2.183");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
        }

        private static void UninstallInAppReview()
        {
            EditorGUILayout.BeginHorizontal();
            Uniform.DrawInstalled("In-App-Review 1.8.3");

            GUI.backgroundColor = Uniform.Red_500;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Uninstall", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT), GUILayout.MinWidth(80)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall In-App-Review", "Are you sure you want to uninstall In-App-Review package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.RemovePackage("com.google.play.review");
                    RegistryManager.RemovePackage("com.google.play.core");
                    RegistryManager.RemovePackage("com.google.play.common");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private static void Uninstall(string namePackage, string bundle)
        {
            EditorGUILayout.BeginHorizontal();
            Uniform.DrawInstalled(namePackage, new RectOffset(0, 0, 6, 0));

            GUI.backgroundColor = Uniform.Red_500;
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Uninstall", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT), GUILayout.MinWidth(80)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog($"Uninstall {namePackage}", $"Are you sure you want to uninstall {namePackage} package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.RemovePackage(bundle);
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }
    }
}