#if PANCAKE_ADS
using System.Collections.Generic;
using System.IO;
using Pancake.Editor;
using UnityEditor;
using UnityEngine;

namespace Pancake.Monetization.Editor
{
    public class AdsEditorUtil
    {
        public const string SCRIPTING_DEFINITION_ADMOB = "PANCAKE_ADMOB_ENABLE";
        public const string SCRIPTING_DEFINITION_APPLOVIN = "PANCAKE_MAX_ENABLE";
        public const string SCRIPTING_DEFINITION_MULTIPLE_DEX = "PANCAKE_MULTIPLE_DEX";
        private const string KEY_FORCE_GRADLE = "KEY_FORCE_GRADLE";
        public const string DEFAULT_FILTER_ADMOB_DLL = "l:gvhp_exportpath-GoogleMobileAds/GoogleMobileAds.dll";
        public const string DEFAULT_FILTER_MAX_MAXSDK = "l:al_max_export_path-MaxSdk/Scripts/MaxSdk.cs";

        private const string MAINTEMPALTE_GRADLE_PATH = "Assets/Plugins/Android/mainTemplate.gradle";
        private const string GRADLETEMPALTE_PROPERTIES_PATH = "Assets/Plugins/Android/gradleTemplate.properties";
        private const string PROGUARD_PATH = "Assets/Plugins/Android/proguard-user.txt";

        public static void CreateMainTemplateGradle()
        {
            const string androidPath = "Assets/Plugins/Android/";
            if (!androidPath.DirectoryExists()) androidPath.CreateDirectory();
            if (File.Exists(MAINTEMPALTE_GRADLE_PATH)) return;
            var maintemplate = (TextAsset) Resources.Load("mainTemplate", typeof(TextAsset));
            string maintemplateData = maintemplate.text;
            var writer = new StreamWriter(MAINTEMPALTE_GRADLE_PATH, false);
            writer.Write(maintemplateData);
            writer.Close();
            AssetDatabase.ImportAsset(MAINTEMPALTE_GRADLE_PATH);
        }

        public static void AddAlgorixSettingGradle(MaxNetwork network)
        {
            GradleConfig config = new GradleConfig(MAINTEMPALTE_GRADLE_PATH);
            foreach (var rootChild in config.ROOT.CHILDREN)
            {
                if (rootChild.NAME.Equals("dependencies"))
                {
                    if (!rootChild.CHILDREN.Exists(_ => _.NAME.Contains("compile(name: 'alx.")))
                    {
                        rootChild.CHILDREN.Insert(1, new GradleContentNode($"compile(name: 'alx.{network.LatestVersions.Android}', ext: 'aar')", rootChild));
                    }
                }
            }

            config.Save(MAINTEMPALTE_GRADLE_PATH);
        }

        public static void RemoveAlgorixSettingGradle()
        {
            GradleConfig config = new GradleConfig(MAINTEMPALTE_GRADLE_PATH);
            foreach (var rootChild in config.ROOT.CHILDREN)
            {
                if (rootChild.NAME.Equals("dependencies"))
                {
                    for (int i = 0; i < rootChild.CHILDREN.Count; i++)
                    {
                        if (rootChild.CHILDREN[i].NAME.Contains("compile(name: 'alx."))
                        {
                            rootChild.CHILDREN.RemoveAt(i);
                        }
                    }
                }
            }

            config.Save(MAINTEMPALTE_GRADLE_PATH);
        }

        public static void CreateGradleTemplateProperties()
        {
            const string androidPath = "Assets/Plugins/Android/";
            if (!androidPath.DirectoryExists()) androidPath.CreateDirectory();
            if (File.Exists(GRADLETEMPALTE_PROPERTIES_PATH)) return;
            var gradleTemplate = (TextAsset) Resources.Load("gradleTemplate", typeof(TextAsset));
            string maintemplateData = gradleTemplate.text;
            var writer = new StreamWriter(GRADLETEMPALTE_PROPERTIES_PATH, false);
            writer.Write(maintemplateData);
            writer.Close();
            AssetDatabase.ImportAsset(GRADLETEMPALTE_PROPERTIES_PATH);
        }

        public static void DeleteMainTemplateGradle()
        {
            if (!File.Exists(MAINTEMPALTE_GRADLE_PATH)) return;
            FileUtil.DeleteFileOrDirectory(MAINTEMPALTE_GRADLE_PATH);
            FileUtil.DeleteFileOrDirectory(MAINTEMPALTE_GRADLE_PATH + ".meta");
            AssetDatabase.Refresh();
        }

        public static void DeleteGradleTemplateProperties()
        {
            if (!File.Exists(GRADLETEMPALTE_PROPERTIES_PATH)) return;
            FileUtil.DeleteFileOrDirectory(GRADLETEMPALTE_PROPERTIES_PATH);
            FileUtil.DeleteFileOrDirectory(GRADLETEMPALTE_PROPERTIES_PATH + ".meta");
            AssetDatabase.Refresh();
        }

        public static void SetDeleteGradleState(bool state) { EditorPrefs.SetBool(KEY_FORCE_GRADLE, state); }

        public static bool StateDeleteGradle() => EditorPrefs.GetBool(KEY_FORCE_GRADLE, false);

        public static void CreateProguardFile()
        {
            if (File.Exists(PROGUARD_PATH)) return;
            var writer = new StreamWriter(PROGUARD_PATH, false);
            writer.Write("");
            writer.Close();
            AssetDatabase.ImportAsset(PROGUARD_PATH);
        }

        public static void AddSettingProguardFile(List<string> settings)
        {
            if (!File.Exists(PROGUARD_PATH)) CreateProguardFile();

            var writer = new StreamWriter(PROGUARD_PATH, true);
            for (int i = 0; i < settings.Count; i++)
            {
                writer.Write(settings[i]);
                writer.Write("\n");
            }

            writer.Close();
            AssetDatabase.ImportAsset(PROGUARD_PATH);
        }

        public static void DeleteProguardFile()
        {
            if (!File.Exists(PROGUARD_PATH)) return;
            FileUtil.DeleteFileOrDirectory(PROGUARD_PATH);
            FileUtil.DeleteFileOrDirectory(PROGUARD_PATH + ".meta");
            AssetDatabase.Refresh();
        }
    }
}
#endif