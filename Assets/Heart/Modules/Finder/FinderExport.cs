using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PancakeEditor.Finder
{
    public static class FinderExport
    {
        private static Dictionary<string, ProcessReplaceData> listReplace;
        private static HashSet<string> cacheSelection;

        public static bool IsMergeProcessing { get; private set; }

        [MenuItem("Assets/Finder/Toggle Ignore", false)]
        private static void Ignore()
        {
            if (!FinderWindowBase.IsCacheReady)
            {
                Debug.LogWarning("Finder cache not yet ready, please open Pancake > Finder and hit scan project!");
                return;
            }

            var actives = Selection.objects;
            for (var i = 0; i < actives.Length; i++)
            {
                string path = AssetDatabase.GetAssetPath(actives[i]);

                if (FinderWindowBase.IgnoreAsset.Contains(path)) FinderWindowBase.RemoveIgnore(path);
                else FinderWindowBase.AddIgnore(path);
            }
        }

        [MenuItem("Assets/Finder/Copy GUID", false)]
        private static void CopyGuid() { EditorGUIUtility.systemCopyBuffer = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject)); }

        [MenuItem("Assets/Finder/Select Dependencies (assets I use)", false)]
        private static void SelectDependencies_wtme()
        {
            if (!FinderWindowBase.IsCacheReady)
            {
                Debug.LogWarning("Finder cache not yet ready, please open Pancake > Finder and hit scan project!");
                return;
            }

            SelectDependencies(false);
        }

        [MenuItem("Assets/Finder/Refresh")]
        public static void ForceRefreshSelection()
        {
            string[] guids = Selection.assetGUIDs;
            if (!FinderWindowBase.IsCacheReady) return; // cache not ready!

            for (var i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];

                if (FinderWindowBase.CacheSetting.assetMap.ContainsKey(guid))
                {
                    FinderWindowBase.CacheSetting.RefreshAsset(guid, true);

                    continue;
                }

                FinderWindowBase.CacheSetting.AddAsset(guid);
            }

            FinderWindowBase.CacheSetting.Check4Work();
        }

        [MenuItem("Assets/Finder/Select Dependencies included me", false)]
        private static void SelectDependencies_wme()
        {
            if (!FinderWindowBase.IsCacheReady)
            {
                Debug.LogWarning("Finder cache not yet ready, please open Pancake > Finder and hit scan project!");
                return;
            }

            SelectDependencies(true);
        }

        [MenuItem("Assets/Finder/Select Used (assets used me)", false)]
        private static void SelectUsed_wtme()
        {
            if (!FinderWindowBase.IsCacheReady)
            {
                Debug.LogWarning("Finder cache not yet ready, please open Pancake > Finder and hit scan project!");
                return;
            }

            SelectUsed(false);
        }

        [MenuItem("Assets/Finder/Select Used included me", false)]
        private static void SelectUsed_wme()
        {
            if (!FinderWindowBase.IsCacheReady)
            {
                Debug.LogWarning("Finder cache not yet ready, please open Pancake > Finder and hit scan project!");
                return;
            }

            SelectUsed(true);
        }

        public static void MergeDuplicate(string guidFile)
        {
            long toFileId = 0;
            string[] stringArr = guidFile.Split('/');
            if (stringArr.Length > 1) toFileId = long.Parse(stringArr[1]);
            string guid = stringArr[0];

            string gPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(gPath) || !gPath.StartsWith("Assets/"))
            {
                Debug.LogWarning("Invalid guid <" + guid + "> in clipboard, can not replace !");
                return;
            }

            string[] temp = FinderUtility.SelectionAssetGUIDs; //cheat refresh selection, DO NOT delete
            var guidsFiles = FinderUtility.selectionAssetGUIDs;

            var realKey = "";
            foreach (string item in guidsFiles)
            {
                if (item.StartsWith(guidFile, StringComparison.Ordinal)) realKey = item;
            }

            if (string.IsNullOrEmpty(realKey))
            {
                Debug.LogWarning("Clipboard guid <" + guid + "> not found in Selection, you may not intentionally replace selection assets by clipboard guid");

                return;
            }

            guidsFiles.Remove(realKey);
            cacheSelection = new HashSet<string>();
            foreach (string item in cacheSelection)
            {
                cacheSelection.Add(item);
            }

            if (guidsFiles.Count == 0)
            {
                Debug.LogWarning("No new asset selected to replace, must select all duplications to replace");
                return;
            }

            //check asset type, only replace same type
#if REPLACE_SAME_TYPE
            var type1 = AssetDatabase.GetMainAssetTypeAtPath(gPath);
            var importType1 = AssetImporter.GetAtPath(gPath);
#endif

            var lstFind = new List<string>();

            foreach (string item in guidsFiles)
            {
                string[] arr = item.Split('/');
                string g = arr[0];

#if REPLACE_SAME_TYPE
                var p2 = AssetDatabase.GUIDToAssetPath(g);
                var type2 = AssetDatabase.GetMainAssetTypeAtPath(p2);

                if(type1 != type2)
                {
                    Debug.LogWarning("Cannot replace asset: " + p2 + " becase difference type");
                    continue;
                }
                if(type1 == typeof(UnityEngine.Texture2D))
                {
                    var importType2 = AssetImporter.GetAtPath(p2) as TextureImporter;
                    var textureImportType1 = importType1 as TextureImporter;
                    if (importType2 == null || textureImportType1 == null)
                    {
                        Debug.LogWarning("Cannot replace asset: " + p2 + " becase difference type");
                        continue;
                    }
                    if(textureImportType1.textureType != importType2.textureType)
                    {
                        Debug.LogWarning("Cannot replace asset: " + p2 + " becase difference type");
                        continue;
                    }
                    if (textureImportType1.textureType == TextureImporterType.Sprite)
                    {
                        if (textureImportType1.spriteImportMode != importType2.spriteImportMode)
                        {
                            Debug.LogWarning("Cannot replace asset: " + p2 + " becase difference type");
                            continue;
                        }
                    }
                    //Debug.Log("import type " + mainImportType);
                }
                //Debug.Log("type: " + mainType);
#endif
                lstFind.Add(g);
            }

            if (lstFind.Count == 0)
            {
                Debug.LogWarning("No new asset selected to replace, must select all duplications to replace");
                return;
            }

            var assetList = FinderWindowBase.CacheSetting.FindAssets(lstFind.ToArray(), false);

            //replace one by one
            listReplace = new Dictionary<string, ProcessReplaceData>();
            for (int i = assetList.Count - 1; i >= 0; i--)
            {
                Debug.Log("Finder Replace GUID : " + assetList[i].guid + " ---> " + guid + " : " + assetList[i].usedByMap.Count + " assets updated");

                string fromId = assetList[i].guid;

                var arr = assetList[i].usedByMap.Values.ToList();
                for (var j = 0; j < arr.Count; j++)
                {
                    var a = arr[j];
                    if (!listReplace.ContainsKey(a.AssetPath)) listReplace.Add(a.AssetPath, new ProcessReplaceData());

                    listReplace[a.AssetPath].datas.Add(new ReplaceData {from = fromId, to = guid, asset = a, toFileId = toFileId});
                }
            }

            IsMergeProcessing = true;
            EditorApplication.update -= ApplicationUpdate;
            EditorApplication.update += ApplicationUpdate;
        }

        private static void ApplicationUpdate()
        {
            var isCompleted = true;
            foreach (var item in listReplace)
            {
                if (item.Value.processed) continue;
                item.Value.processed = true;

                for (var i = 0; i < item.Value.datas.Count; i++)
                {
                    var a = item.Value.datas[i];
                    a.isTerrian = a.asset.type == EFinderAssetType.Terrain;
                    if (a.isTerrian)
                        a.terrainData = AssetDatabase.LoadAssetAtPath(a.asset.AssetPath, typeof(Object)) as TerrainData;
                    a.isSucess = a.asset.ReplaceReference(a.from, a.to, a.toFileId, a.terrainData);

                    if (a.isTerrian)
                    {
                        a.terrainData = null;
                        FinderUtility.UnloadUnusedAssets();
                    }
                }

                isCompleted = false;
                break;
            }

            if (!isCompleted) return;
            foreach (var item in listReplace)
            {
                var lst = item.Value.datas;
                for (var i = 0; i < lst.Count; i++)
                {
                    var data = lst[i];
                    if (!data.isUpdated && data.isSucess)
                    {
                        data.isUpdated = true;
                        if (data.isTerrian)
                        {
                            EditorUtility.SetDirty(data.terrainData);
                            AssetDatabase.SaveAssets();
                            data.terrainData = null;
                            FinderUtility.UnloadUnusedAssets();
                        }
                        else
                            try
                            {
                                AssetDatabase.ImportAsset(data.asset.AssetPath, ImportAssetOptions.Default);
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning(data.asset.AssetPath + "\n" + e);
                            }
                    }
                }
            }

            var guidsRefreshed = new HashSet<string>();
            EditorApplication.update -= ApplicationUpdate;
            foreach (var item in listReplace)
            {
                var lst = item.Value.datas;
                for (var i = 0; i < lst.Count; i++)
                {
                    var data = lst[i];
                    if (data.isSucess && !guidsRefreshed.Contains(data.asset.guid))
                    {
                        guidsRefreshed.Add(data.asset.guid);
                        FinderWindowBase.CacheSetting.RefreshAsset(data.asset.guid, true);
                    }
                }
            }

            // lstThreads = null;
            listReplace = null;
            FinderWindowBase.CacheSetting.RefreshSelection();
            FinderWindowBase.CacheSetting.Check4Work();

            AssetDatabase.Refresh();
            IsMergeProcessing = false;
        }


        //-------------------------- APIs ----------------------

        private static void SelectDependencies(bool includeMe)
        {
            var list = FinderWindowBase.CacheSetting.FindAssets(FinderUtility.SelectionAssetGUIDs, false);
            var dict = new Dictionary<string, Object>();

            if (includeMe) AddToDict(dict, list.ToArray());

            for (var i = 0; i < list.Count; i++)
            {
                AddToDict(dict, FindAsset.FindUsage(list[i]).ToArray());
            }

            Selection.objects = dict.Values.ToArray();
        }

        private static void SelectUsed(bool includeMe)
        {
            var list = FinderWindowBase.CacheSetting.FindAssets(FinderUtility.SelectionAssetGUIDs, false);
            var dict = new Dictionary<string, Object>();

            if (includeMe) AddToDict(dict, list.ToArray());

            for (var i = 0; i < list.Count; i++)
            {
                AddToDict(dict, list[i].usedByMap.Values.ToArray());
            }

            Selection.objects = dict.Values.ToArray();
        }


        //-------------------------- UTILS ---------------------

        internal static void AddToDict(Dictionary<string, Object> dict, params FindAsset[] list)
        {
            for (var j = 0; j < list.Length; j++)
            {
                string guid = list[j].guid;
                if (!dict.ContainsKey(guid))
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    dict.Add(guid, FinderUtility.LoadAssetAtPath<Object>(assetPath));
                }
            }
        }

        private class ProcessReplaceData
        {
            public readonly List<ReplaceData> datas = new();
            public bool processed;
        }

        private class ReplaceData
        {
            public FindAsset asset;
            public string from;
            public bool isSucess;
            public bool isTerrian;
            public bool isUpdated;
            public TerrainData terrainData;
            public string to;

            public long toFileId;
        }
    }
}