using System.Diagnostics;
using Pancake.ExLibEditor;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;

namespace PancakeEditor
{
    public static class TrackingFirebaseDrawer
    {
        private static string bundleId;
        private static bool customBundleId;

        public static void OnInspectorGUI()
        {
#if PANCAKE_FIREBASE_ANALYTIC
            Uniform.DrawInstalled("analytic 11.5.0");
            EditorGUILayout.Space();
#endif

#if PANCAKE_FIREBASE_REMOTECONFIG
            Uniform.DrawInstalled("remote config 11.5.0");
            EditorGUILayout.Space();
#endif

#if PANCAKE_FIREBASE_MESSAGING
            Uniform.DrawInstalled("messaging 11.5.0");
            EditorGUILayout.Space();
#endif

#if PANCAKE_FIREBASE_CRASHLYTIC
            Uniform.DrawInstalled("crashlytic 11.5.0");
            EditorGUILayout.Space();
#endif

#if !PANCAKE_FIREBASE_ANALYTIC
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Firebase Analytic", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.google.firebase.analytics", "https://github.com/firebase-unity/firebase-analytics.git#11.5.0");
                RegistryManager.Add("com.google.firebase.app", "https://github.com/firebase-unity/firebase-app.git#11.5.0");
                RegistryManager.Add("com.google.external-dependency-manager", "https://github.com/google-unity/external-dependency-manager.git#1.2.177");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif


#if !PANCAKE_FIREBASE_REMOTECONFIG
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Firebase Remote Config", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.google.firebase.remote-config", "https://github.com/firebase-unity/firebase-remote-config.git#11.5.0");
                RegistryManager.Add("com.google.firebase.app", "https://github.com/firebase-unity/firebase-app.git#11.5.0");
                RegistryManager.Add("com.google.external-dependency-manager", "https://github.com/google-unity/external-dependency-manager.git#1.2.177");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif

#if !PANCAKE_FIREBASE_MESSAGING
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Firebase Messaging", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.google.firebase.messaging", "https://github.com/firebase-unity/firebase-messaging.git#11.5.0");
                RegistryManager.Add("com.google.firebase.app", "https://github.com/firebase-unity/firebase-app.git#11.5.0");
                RegistryManager.Add("com.google.external-dependency-manager", "https://github.com/google-unity/external-dependency-manager.git#1.2.177");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif

#if !PANCAKE_FIREBASE_CRASHLYTIC
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install Firebase Crashlytic", GUILayout.MaxHeight(40f)))
            {
                RegistryManager.Add("com.google.firebase.crashlytics", "https://github.com/firebase-unity/firebase-crashlytics.git#11.5.0");
                RegistryManager.Add("com.google.firebase.app", "https://github.com/firebase-unity/firebase-app.git#11.5.0");
                RegistryManager.Add("com.google.external-dependency-manager", "https://github.com/google-unity/external-dependency-manager.git#1.2.177");
                RegistryManager.Resolve();
            }

            GUI.enabled = true;
#endif

            GUILayout.Space(16);

            if (customBundleId)
            {
                GUI.enabled = true;
            }
            else
            {
                bundleId = Application.identifier;
                GUI.enabled = false;
            }

            bundleId = EditorGUILayout.TextField("Bundle Id: ", bundleId);
            GUI.enabled = true;
            customBundleId = EditorGUILayout.Toggle("Custom Bundle: ", customBundleId);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Run Debug View"))
            {
                SetDebugView(bundleId);
            }

            if (GUILayout.Button("Set None"))
            {
                SetDebugView(".none.");
            }

            GUILayout.EndHorizontal();

            void SetDebugView(string package)
            {
                var fileName = $"{AndroidExternalToolsSettings.sdkRootPath}/platform-tools/adb";
                var arguments = $"shell setprop debug.firebase.analytics.app {package}";
                var startInfo = new ProcessStartInfo
                {
                    FileName               = fileName,
                    UseShellExecute        = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow         = true,
                    Arguments              = arguments,
                };

                var process = Process.Start(startInfo);
                process!.WaitForExit();
                UnityEngine.Debug.Log( $"{fileName} {arguments}" );
            }

#if PANCAKE_FIREBASE_ANALYTIC || PANCAKE_FIREBASE_REMOTECONFIG || PANCAKE_FIREBASE_MESSAGING || PANCAKE_FIREBASE_CRASHLYTIC
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall All Firebase Package", GUILayout.MaxHeight(25f)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Firebase", "Are you sure you want to uninstall all firebase package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.google.firebase.analytics");
                    RegistryManager.Remove("com.google.firebase.messaging");
                    RegistryManager.Remove("com.google.firebase.remote-config");
                    RegistryManager.Remove("com.google.firebase.crashlytics");
                    RegistryManager.Remove("com.google.firebase.app");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
#endif
        }
    }
}