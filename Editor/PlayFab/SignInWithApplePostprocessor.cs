#if PANCAKE_PLAYFAB
#if UNITY_IOS || UNITY_TVOS
#define UNITY_XCODE_EXTENSIONS_AVAILABLE
#endif

#if PANCAKE_APPLE_SIGNIN
using AppleAuth.Editor;
#endif
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_XCODE_EXTENSIONS_AVAILABLE
using UnityEditor.iOS.Xcode;
#endif

namespace Pancake.Editor
{
    public static class SignInWithApplePostprocessor
    {
        private const int CallOrder = 1;

        [PostProcessBuild(CallOrder)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (target == BuildTarget.iOS || target == BuildTarget.tvOS)
            {
#if UNITY_XCODE_EXTENSIONS_AVAILABLE
                    var projectPath = PBXProject.GetPBXProjectPath(path);
                    var project = new PBXProject();
                    project.ReadFromString(System.IO.File.ReadAllText(projectPath));
                    var manager = new ProjectCapabilityManager(projectPath, "Entitlements.entitlements", null, project.GetUnityMainTargetGuid());
                    manager.AddSignInWithAppleWithCompatibility();
                    manager.WriteToFile();
#endif
            }
            else if (target == BuildTarget.StandaloneOSX)
            {
#if PANCAKE_APPLE_SIGNIN
                AppleAuthMacosPostprocessorHelper.FixManagerBundleIdentifier(target, path);        
#endif
            }
        }
    }
}
#endif