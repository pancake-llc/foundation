using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;
using System;
using Pancake.BTag;

namespace Pancake.BTagEditor
{
    public static class BTagEditorUtils
    {
        internal const string RELATIVE_PATH = "Modules/Tag/Editor";

        [MenuItem("Assets/Group Selected Scriptable Objects", true)]
        private static bool GroupSelectedValidation()
        {
            var hasGroup = Selection.objects.Any(x => (x is BTagGroupBase));
            var hasAsset = Selection.objects.Any(x => (x is ScriptableBTag));
            return hasAsset || hasGroup;
        }

        [MenuItem("Assets/Group Selected Scriptable Objects %g", false, 18)]
        public static void GroupSelected()
        {
            //Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Group Selected ScriptableObjects");
            bool deleteUnusedGroups = true;
            BTagGroupBase group = default;
            var sortedCollections = GetSortedSelection();
            var allGroups = sortedCollections.allGroups;
            var mainAssetSOs = sortedCollections.mainAssetSOs;
            var subAssetSOs = sortedCollections.subAssetSOs;
            if (allGroups.Count > 0) group = allGroups[0];

            if (allGroups.Count > 1)
            {
                var option = EditorUtility.DisplayDialogComplex("Combine Groups",
                    "Merging multiple groups. Woud you like to delete unused Groups after merging them?",
                    "Yes",
                    "No",
                    "Cancel");
                if (option == 2) return;
                deleteUnusedGroups = (option == 0);
                for (int i = 1; i < allGroups.Count; ++i)
                {
                    var subs = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(allGroups[i]));
                    for (int s = 0; s < subs.Length; ++s)
                    {
                        if (subs[s] is ScriptableBTag) subAssetSOs.Add(subs[s] as ScriptableBTag);
                    }
                }
            }
            else if (allGroups.Count == 1 && mainAssetSOs.Count == 0 && subAssetSOs.Count == 0)
            {
                var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(allGroups[0]));
                if (subAssets.Length > 0 && EditorUtility.DisplayDialog("Ungroup?",
                        "Are you sure you wish to ungroup these assets?",
                        "Yes",
                        "No",
                        DialogOptOutDecisionType.ForThisMachine,
                        "btag_ungroup_optout"))
                {
                    for (int i = 0; i < subAssets.Length; ++i)
                    {
                        ChangeSubAssetToMainAsset(subAssets[i] as ScriptableBTag, allGroups[0], subAssets[i].name, true);
                    }

                    return;
                }
            }

            if (mainAssetSOs.Count < 1 && subAssetSOs.Count < 1) return;
            Undo.RecordObjects(mainAssetSOs.ToArray(), "Main assets");
            Undo.RecordObjects(subAssetSOs.ToArray(), "SubAssets");
            Undo.RecordObjects(allGroups.ToArray(), "Groups");

            // If no group, create one
            if (allGroups.Count < 1)
            {
                if (subAssetSOs.Count > 0 && mainAssetSOs.Count == 0)
                {
                    var mainAsset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(subAssetSOs[0]));
                    if (mainAsset != null && (mainAsset is BTagGroupBase) && EditorUtility.DisplayDialog("Separate Group?",
                            "Create a sub-group within " + mainAsset + " or place in new Group?",
                            "Sub-Group",
                            "New Group"))
                    {
                        for (int i = 0; i < subAssetSOs.Count; ++i)
                        {
                            mainAsset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(subAssetSOs[i]));
                            subAssetSOs[i].name = "Group/" + subAssetSOs[i].name;
                            ForceRefresh(mainAsset as BTagGroupBase);
                        }

                        return;
                    }
                }

                ScriptableBTag firstSO = mainAssetSOs.Count > 0 ? mainAssetSOs[0] : subAssetSOs[0];
                group = CreateGroupForType(firstSO.GetType(), AssetDatabase.GetAssetPath(firstSO), string.Empty).group;
                if (group == null) return;
            }

            for (int i = 0; i < mainAssetSOs.Count; ++i)
            {
                var clone = ScriptableObject.Instantiate(mainAssetSOs[i]);
                clone.name = mainAssetSOs[i].name;
                AssetDatabase.AddObjectToAsset(clone, group);
                ScriptableBTagRegistry.ReplaceSOUsage(mainAssetSOs[i], SearchRegistryOption.FullRefresh, clone);
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(mainAssetSOs[i]));
            }

            for (int i = 0; i < subAssetSOs.Count; ++i)
            {
                var clone = ScriptableObject.Instantiate(subAssetSOs[i]);
                clone.name = subAssetSOs[i].name;
                AssetDatabase.AddObjectToAsset(clone, group);
                ScriptableBTagRegistry.ReplaceSOUsage(subAssetSOs[i], SearchRegistryOption.FullRefresh, clone);
                var mainAsset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(subAssetSOs[i]));
                AssetDatabase.RemoveObjectFromAsset(subAssetSOs[i]);
                if (mainAsset != null)
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(mainAsset));
                    if (!allGroups.Contains(mainAsset)) allGroups.Add(mainAsset as BTagGroupBase);
                }
            }

            for (int i = 0; i < allGroups.Count; ++i) ForceRefresh(allGroups[i]);
            ForceRefresh(group);
            EditorApplication.delayCall += () => DeleteUnusedGroups(allGroups, group);
        }

        [MenuItem("Assets/UnGroup Selected Scriptable Objects", true)]
        private static bool UnGroupSelectedValidation()
        {
            var hasAsset = Selection.objects.Any(x => (x is ScriptableBTag) && !AssetDatabase.IsMainAsset(x));
            return hasAsset;
        }

        [MenuItem("Assets/UnGroup Selected Scriptable Objects %#g", false, 19)]
        public static void UnGroupSelected()
        {
            Undo.SetCurrentGroupName("UnGroup Selected ScriptableObjects");
            var sortedCollections = GetSortedSelection(true);
            var subAssetSOs = sortedCollections.subAssetSOs;
            for (int i = 0; i < subAssetSOs.Count; ++i)
            {
                var subAssetPath = AssetDatabase.GetAssetPath(subAssetSOs[i]);
                var mainAsset = AssetDatabase.LoadMainAssetAtPath(subAssetPath);
                var clone = ScriptableObject.Instantiate(subAssetSOs[i]);
                clone.name = subAssetSOs[i].name;
                AssetDatabase.CreateAsset(clone, subAssetPath.Replace(mainAsset.name, clone.name));
                ScriptableBTagRegistry.ReplaceSOUsage(subAssetSOs[i], SearchRegistryOption.FullRefresh, clone);
                AssetDatabase.RemoveObjectFromAsset(subAssetSOs[i]);
            }

            for (int i = 0; i < sortedCollections.allGroups.Count; ++i)
            {
                if (sortedCollections.allGroups[i] == null)
                {
                    Debug.LogWarning("Group " + i + " was null");
                }
                else
                {
                    ForceRefresh(sortedCollections.allGroups[i]);
                }
            }

            EditorApplication.delayCall += () => DeleteUnusedGroups(sortedCollections.allGroups, null);
        }

        public static void DeleteUnusedGroups(List<BTagGroupBase> allGroups, BTagGroupBase groupToIgnore)
        {
            if (allGroups.Count > 0)
            {
                for (int i = 0; i < allGroups.Count; ++i)
                {
                    var groupPath = AssetDatabase.GetAssetPath(allGroups[i]);
                    if (allGroups[i] != groupToIgnore && AssetDatabase.LoadAllAssetRepresentationsAtPath(groupPath).Length < 1)
                    {
                        var g = allGroups[i];
                        allGroups[i] = null;
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(g));
                    }
                }

                allGroups.Clear();
                allGroups.Add(groupToIgnore);
            }

            Undo.FlushUndoRecordObjects();
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        public static T ChangeSubAssetToMainAsset<T>(T subAsset, BTagGroupBase currentGroup, string newName, bool deleteGroupIfEmpty = true) where T : ScriptableBTag
        {
            if (subAsset == null) return subAsset;
            T clone = ScriptableObject.Instantiate(subAsset);
            if (string.IsNullOrEmpty(newName)) newName = subAsset.name;
            var currentGroupPath = AssetDatabase.GetAssetPath(currentGroup);
            var destPath = Path.GetDirectoryName(currentGroupPath);
            destPath = destPath + Path.DirectorySeparatorChar + GetNextAvailableName(destPath, newName) + ".asset";
            clone.name = newName;

            AssetDatabase.CreateAsset(clone, destPath);
            AssetDatabase.SetMainObject(clone, destPath);

            ScriptableBTagRegistry.ReplaceSOUsage(subAsset, SearchRegistryOption.FullRefresh, clone);
            if (deleteGroupIfEmpty && AssetDatabase.LoadAllAssetRepresentationsAtPath(currentGroupPath).Length == 1)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(currentGroup));
            }
            else
            {
                AssetDatabase.RemoveObjectFromAsset(subAsset);
            }

            ForceRefresh(currentGroup);
            return clone;
        }

        internal static void ShowAddToGroupPopup(BTagGroupBase group, string newName, Action<ScriptableBTag> onCreated)
        {
            if (group == default)
            {
                Debug.LogWarning("No group passed.");
                return;
            }

            var groupType = group.GetType();
            Type[] matches = new Type[0];
            Type foundType = default;
            while (matches.Length < 1 && groupType != default)
            {
                var argumentTypes = groupType.GenericTypeArguments;
                if (argumentTypes.Length > 0)
                {
                    var soTypeForGroup = groupType.GenericTypeArguments[0];
                    matches = TypeCache.GetTypesDerivedFrom(soTypeForGroup).Where(x => !x.IsGenericTypeDefinition).ToArray();
                    if (matches.Length > 0)
                    {
                        foundType = matches[0];
                    }
                    else
                    {
                        if (soTypeForGroup.IsSubclassOf(typeof(ScriptableBTag))) foundType = soTypeForGroup;
                    }
                }

                groupType = groupType.BaseType;
            }

            // If there's only one possible SO to add, just add it, otherwise offer menu
            if (matches.Length == 1 || (foundType != default && matches.Length < 1))
            {
                HandleAddSO((group, newName, foundType, onCreated));
            }
            else
            {
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < matches.Length; ++i)
                {
                    menu.AddItem(new GUIContent(matches[i].Name), false, HandleAddSO, (group, newName, matches[i], onCreated));
                }

                menu.ShowAsContext();
            }
        }

        private static (List<BTagGroupBase> allGroups, List<ScriptableBTag> mainAssetSOs, List<ScriptableBTag> subAssetSOs) GetSortedSelection(
            bool addGroupsFromSelection = false)
        {
            List<BTagGroupBase> allGroups = new List<BTagGroupBase>();
            List<ScriptableBTag> mainAssetSOs = new List<ScriptableBTag>();
            List<ScriptableBTag> subAssetSOs = new List<ScriptableBTag>();
            for (int i = 0; i < Selection.objects.Length; ++i)
            {
                if (Selection.objects[i] is BTagGroupBase)
                {
                    if (!allGroups.Contains(Selection.objects[i])) allGroups.Add(Selection.objects[i] as BTagGroupBase);
                }
                else if (Selection.objects[i] is ScriptableBTag)
                {
                    if (AssetDatabase.IsMainAsset(Selection.objects[i]))
                    {
                        mainAssetSOs.Add(Selection.objects[i] as ScriptableBTag);
                    }
                    else
                    {
                        if (addGroupsFromSelection)
                        {
                            BTagGroupBase mainAss = (BTagGroupBase) AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(Selection.objects[i]));
                            if (mainAss != null && !allGroups.Contains(mainAss)) allGroups.Add(mainAss);
                        }

                        subAssetSOs.Add(Selection.objects[i] as ScriptableBTag);
                    }
                }
            }

            return (allGroups, mainAssetSOs, subAssetSOs);
        }

        public static void HandleAddSO(object data)
        {
            var groupAndType = ((BTagGroupBase group, String newName, System.Type soType, Action<ScriptableBTag> onCreated)) data;
            var closedGeneric = typeof(BTagPropertyDrawerBase<,>).MakeGenericType(groupAndType.group.GetType(), groupAndType.soType);
            MethodInfo genericMethod = closedGeneric.GetMethod("CreateAssetInGroup", BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod);
            var uniqueName = GetNextAvailableName(AssetDatabase.GetAssetPath(groupAndType.group),
                string.IsNullOrEmpty(groupAndType.newName) ? "New" : groupAndType.newName);
            var result = genericMethod.Invoke(null, new[] {groupAndType.group, (object) uniqueName, true}) as ScriptableBTag;
            groupAndType.onCreated?.Invoke(result);
        }

        public static (BTagGroupBase group, string path) CreateGroupForType<T>(string soPath, string groupName) => CreateGroupForType(typeof(T), soPath, groupName);

        public static (BTagGroupBase group, string path) CreateGroupForType(System.Type soType, string soPath, string groupName)
        {
            if (soType == default) return (default, soPath);

            var origType = soType;
            var gDef = typeof(BTagGroup<>).GetGenericTypeDefinition();
            if (gDef == null) return (default, soPath);

            var concrete = gDef.MakeGenericType(soType);
            if (concrete == null) return (default, soPath);

            var derivedGroups = TypeCache.GetTypesDerivedFrom(concrete);
            while (soType.BaseType != default && derivedGroups.Count < 1)
            {
                soType = soType.BaseType;
                concrete = gDef.MakeGenericType(soType);
                derivedGroups = TypeCache.GetTypesDerivedFrom(concrete);
            }

            if (soType == default)
            {
                Debug.LogAssertion("Unable to find an appropriate group that supports assets of type: " + origType);
                return (null, soPath);
            }

            if (derivedGroups.Count > 0)
            {
                var newGroup = ScriptableObject.CreateInstance(derivedGroups[0]) as BTagGroupBase;
                if (newGroup == null)
                {
                    Debug.LogWarning("Unable to create group but found: " + string.Join(",", derivedGroups));
                }
                else
                {
                    var groupPath = soPath;
                    if (string.IsNullOrEmpty(groupName))
                    {
                        var firstSOName = Path.GetFileNameWithoutExtension(groupPath);
                        newGroup.name = firstSOName + "_Group";
                    }
                    else
                    {
                        newGroup.name = groupName;
                    }

                    var extensionlessGroupPath = Path.GetDirectoryName(groupPath) + Path.DirectorySeparatorChar + newGroup.name;
                    groupPath = extensionlessGroupPath + ".asset";
                    int attempt = 1;
                    while (File.Exists(groupPath) && attempt < 10000) groupPath = extensionlessGroupPath + attempt++ + ".asset";
                    if (attempt < 10000)
                    {
                        AssetDatabase.CreateAsset(newGroup, groupPath);
                        ForceRefresh(newGroup);
                        return (newGroup, groupPath);
                    }
                }
            }
            else
            {
                Debug.LogWarning("Didn't find any group that derives from " + concrete + ". Please create a class that extends from " + concrete + ".");
            }

            return (null, soPath);
        }


        public static (ScriptableBTag asset, string path) CreateSOAsMainAssetForType<T>(string soPath, string soName) =>
            CreateSOAsMainAssetForType(typeof(T), soPath, soName);

        public static (ScriptableBTag asset, string path) CreateSOAsMainAssetForType(System.Type soType, string soPath, string soName)
        {
            if (soType == default)
            {
                Debug.LogAssertion("Unable to find an appropriate group that supports assets of type: " + soType);
                return (null, soPath);
            }

            var newSO = ScriptableObject.CreateInstance(soType) as ScriptableBTag;
            if (string.IsNullOrEmpty(soName)) soName = "New";
            var extensionlessAssetPath = Path.GetDirectoryName(soPath) + Path.DirectorySeparatorChar + soName;
            soPath = extensionlessAssetPath + ".asset";
            int attempt = 1;
            while (File.Exists(soPath) && attempt < 10000) soPath = extensionlessAssetPath + attempt++ + ".asset";
            if (attempt < 10000)
            {
                AssetDatabase.CreateAsset(newSO, soPath);
                return (newSO, soPath);
            }

            return (null, soPath);
        }


        internal static string GetNextAvailableName(string groupPath, string defaultName)
        {
            string[] existingNames;
            var group = AssetDatabase.LoadMainAssetAtPath(groupPath);
            if (group != null)
            {
                UnityEngine.Object[] allAssets;
                allAssets = AssetDatabase.LoadAllAssetsAtPath(groupPath);
                existingNames = new string[allAssets.Length];
                for (int a = 0; a < allAssets.Length; ++a)
                {
                    existingNames[a] = allAssets[a].name;
                }
            }
            else
            {
                if (groupPath.StartsWith("Assets/")) groupPath = groupPath.Substring(7);
                string absolutePath = Application.dataPath + Path.DirectorySeparatorChar + groupPath;
                absolutePath = Path.GetDirectoryName(absolutePath);
                existingNames = Directory.GetFiles(absolutePath);
                for (int i = 0; i < existingNames.Length; ++i) existingNames[i] = Path.GetFileNameWithoutExtension(existingNames[i]);
            }

            var uniqueName = ObjectNames.GetUniqueName(existingNames, defaultName);
            return uniqueName;
        }

        /// <summary>
        /// Arghhhh Unity
        /// Only way I've found to redraw project window after changing subasset. 
        /// Things that don't work (in any permutation):
        ///  - AssetDatabase.Import(, ForceUpdate)
        ///  - AssetDatabase.SaveAssets()
        ///  - AssetDatabase.Refresh()
        ///  - Renaming main asset sometimes works but can't name it back on same or next frame and 
        ///     can interfere with BTag processing if asset changes name mid-way through
        /// </summary>
        /// <param name="group"></param>
        public static void ForceRefresh(BTagGroupBase group)
        {
            if (group == null) return;
            var groupPath = AssetDatabase.GetAssetPath(group);
            AssetDatabase.SetMainObject(group, groupPath);
            // 2020.2 warning if we don't save all assets.. this really shouldn't be necessary
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(groupPath);
            var selection = Selection.objects;
            EditorApplication.delayCall += () =>
            {
                ProjectWindowUtil.ShowCreatedAsset(group);
                Selection.objects = selection;
            };
        }
    }

    public class BTagEditorUtils<TGroup, T> where TGroup : BTagGroupBase where T : ScriptableBTag
    {
        internal static List<T> GetSOsForGroup(TGroup group)
        {
            List<T> results = new List<T>();
            if (group != null)
            {
                var potentialTags = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(group));
                for (int i = 0; i < potentialTags.Length; ++i)
                {
                    if (potentialTags[i] is T) results.Add(potentialTags[i] as T);
                }
            }

            return results;
        }

        internal static T Create(TGroup group, string tagName, bool createIfAlreadyExists = false)
        {
            return BTagPropertyDrawerBase<TGroup, T>.CreateAssetInGroup(group, tagName, createIfAlreadyExists);
        }

        internal static T Create(string groupPath, string tagName, bool createIfAlreadyExists = false)
        {
            return BTagPropertyDrawerBase<TGroup, T>.CreateAssetInGroupAtPath(groupPath, tagName, createIfAlreadyExists);
        }
    }
}