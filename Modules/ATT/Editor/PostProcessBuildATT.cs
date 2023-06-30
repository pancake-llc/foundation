#if UNITY_IOS

using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace PancakeEditor
{
    public static class PostProcessBuildATT
    {
        [PostProcessBuild]
        public static void ChangeXcodePlist(BuildTarget buildTarget, string path)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                string plistPath = path + "/Info.plist";
                var plist = new PlistDocument();
                plist.ReadFromFile(plistPath);
                var rootDict = plist.root;
                rootDict.SetString("NSUserTrackingUsageDescription", "This identifier will be used to deliver personalized ads to you.");
                File.WriteAllText(plistPath, plist.WriteToString());
            }
        }


        [PostProcessBuild]
        public static void OnPostProcessBuildAddFirebaseFile(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
                var proj = new PBXProject();
                proj.ReadFromFile(projPath);
                proj.AddFileToBuild(proj.GetUnityMainTargetGuid(), proj.AddFile("GoogleService-Info.plist", "GoogleService-Info.plist"));
                proj.WriteToFile(projPath);
            }
        }
    }
}

#endif