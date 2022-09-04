using System;
using System.IO;
using UnityEngine;

namespace Pancake.Editor.Finder
{
    [Serializable]
    public class ProjectFinderData
    {
        private const string JSON_PATH = "ProjectSettings/ProjectFinderSettings.json";

        [SerializeField] private bool isUpToDate = false;
        public static bool IsUpToDate { get => Instance.isUpToDate; set => Instance.isUpToDate = value; }

        [SerializeField] private AssetInfo[] assetInfos;
        public static AssetInfo[] AssetInfos { get => Instance.assetInfos ?? (Instance.assetInfos = new AssetInfo[0]); set => Instance.assetInfos = value; }

        private static ProjectFinderData instance;

        public static ProjectFinderData Instance
        {
            get
            {
                if (instance == null)
                {
                    if (File.Exists(JSON_PATH))
                    {
                        instance = JsonUtility.FromJson<ProjectFinderData>(File.ReadAllText(JSON_PATH));
                    }
                    else
                    {
                        instance = new ProjectFinderData();
                        File.WriteAllText(JSON_PATH, JsonUtility.ToJson(instance));
                    }
                }

                return instance;
            }
        }

        public static void Save() { File.WriteAllText(JSON_PATH, JsonUtility.ToJson(Instance)); }
    }
}