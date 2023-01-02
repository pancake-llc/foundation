#if PANCAKE_ADS
#if UNITY_EDITOR && UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Pancake.Monetization.Editor
{
    public class BuildPostProcessorInfo
    {
        [PostProcessBuild]
        public static void OnPostBuildProcessInfo(BuildTarget target, string pathXcode)
        {
            if (target == BuildTarget.iOS)
            {
                var infoPlistPath = pathXcode + "/Info.plist";

                PlistDocument document = new PlistDocument();
                document.ReadFromString(File.ReadAllText(infoPlistPath));


                PlistElementDict elementDict = document.root;

                elementDict.SetString("NSUserTrackingUsageDescription", "This identifier will be used to deliver personalized ads to you.");

                File.WriteAllText(infoPlistPath, document.WriteToString());
            }
        }
    }
}

#endif
#endif