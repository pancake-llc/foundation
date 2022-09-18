using System;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [InitializeOnLoad]
    internal class AutoRunFirstTime
    {
        private static readonly string[] NameFiles = {"AssetContainer.asset"};
        private static readonly Type[] Types = {typeof(AssetContainer)};
        private static int internalIndex = 0;
        static AutoRunFirstTime()
        {
            if (!EditorPrefs.GetBool($"__firsttime__{PlayerSettings.productGUID}", false))
            {
                Run();
            }
        }

        [MenuItem("Tools/Pancake/Create Necessary SO", priority = 32000)]
        private static void Run()
        {
            internalIndex = 0;
            Setup();
        }

        private static void Setup()
        {
            var str = NameFiles[internalIndex];
            UnityEditor.EditorUtility.DisplayProgressBar("Creating the necessary settings", $"Creating {str}...", internalIndex / (float)NameFiles.Length);
            var resourcePath = InEditor.DefaultResourcesPath();
            if (!$"{resourcePath}/{NameFiles}".FileExists())
            {
                CreateInstance(Complete);
            }
            else
            {
                Complete();
            }
            
            void Complete()
            {
                if (internalIndex < NameFiles.Length - 1)
                {
                    internalIndex++;
                    Setup();
                }
                else
                {
                    Debug.Log("Finish creating the necessary settings");
                    EditorPrefs.SetBool($"__firsttime__{PlayerSettings.productGUID}", true);
                    UnityEditor.EditorUtility.ClearProgressBar();
                }
            }
        }
        
        private static void CreateInstance(Action onComplete)
        {
            switch (internalIndex)
            {
                case 0:
                   var instance = UnityEngine.ScriptableObject.CreateInstance<AssetContainer>();
                   AssetDatabase.CreateAsset(instance, $"{InEditor.DefaultResourcesPath()}/{NameFiles[internalIndex]}");
                   AssetDatabase.SaveAssets();
                   onComplete?.Invoke();
                   break;
            }
        }
    }
}