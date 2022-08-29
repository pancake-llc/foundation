using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Pancake.Database
{
    public class Builder : IPreprocessBuildWithReport
    {
        /// <summary>
        /// <para>Data will not read types from any assemblies starting with these prefixes.</para>
        /// <para>This is done to improve compile times by ignoring namespaces that will never
        /// contain a Type which inherits from Entity.</para>
        /// <para>Please check your assembly names if you aren't seeing content in the Type List.</para>
        /// </summary>
        public static readonly string[] AssemblyBlacklist =
        {
            "System", "Mono.", "Unity.", "UnityEngine", "UnityEditor", "mscorlib", "SyntaxTree", "netstandard", "nunit", "AssetStoreTools", "ExCSS"
        };

        public int callbackOrder => 100;
        public void OnPreprocessBuild(BuildReport report) { Reload(); }

        [DidReloadScripts]
        public static void CallbackAfterScriptReload()
        {
            if (Data.Database == null)
            {
                Debug.LogWarning("Database not found. Please create via create asset menu 'Pancake/Create Database'");
                return;
            }

            Reload();
        }

        /// <summary>
        /// Rebuild lists of static groups, custom groups, and all data entities.
        /// </summary>
        public static void Reload()
        {
            FindDataEntities();
            FindStaticGroups();

            UnityEditor.EditorUtility.SetDirty(Data.Database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (Dashboard.instance != null) Dashboard.instance.RebuildFull();
        }

        private static void FindStaticGroups()
        {
            Data.Database.ClearStaticGroups();

            var assembly = Assembly.GetAssembly(typeof(Entity));
            var assemblyName = assembly.GetName();
            var processedAssemblyNames = new List<string>();

            // ** ASSEMBLY TOP LEVEL
            foreach (var ab in AppDomain.CurrentDomain.GetAssemblies())
            {
                string assyName = ab.GetName().Name;

                // ignore dynamic, blacklisted and duplicates
                if (ab.IsDynamic || Enumerable.Any(AssemblyBlacklist, ignored => assyName.StartsWith(ignored)) ||
                    Enumerable.Any(processedAssemblyNames, n => n == assyName)) continue;

                // ** TOP ASSEMBLY REFERERENCED LEVEL
                foreach (var referencedAssembly in ab.GetReferencedAssemblies())
                {
                    if (referencedAssembly.Name != assemblyName.Name) continue;

                    // if it does reference it, we can get all the classes inside it and make groups.
                    var validDataClasses = BuildStaticGroupsFromAssembly(ab);

                    // find all of the assets for that group Type and add them into the DB
                    foreach (var x in validDataClasses) Data.Database.SetStaticGroup(x);
                    processedAssemblyNames.Add(referencedAssembly.Name);
                }
            }

            var staticGroups = BuildStaticGroupsFromAssembly(assembly);
            foreach (var x in staticGroups) Data.Database.SetStaticGroup(x);
            processedAssemblyNames.Add(assembly.GetName().Name);
        }

        private static List<StaticGroup> BuildStaticGroupsFromAssembly(Assembly assembly)
        {
            var result = new List<StaticGroup>();

            // ** ASSEMBLY LEVEL
            var groups = assembly.GetExportedTypes().Where(t => t.IsSubclassOf(typeof(Entity)) || t == typeof(Entity)).GroupBy(t => t.Namespace);

            // Find all of the valid types and make group instances for them.
            // ** NAMESPACE LEVEL
            foreach (var namespaceGroup in groups)
            {
                // ** TYPE LEVEL
                foreach (var type in namespaceGroup)
                {
                    var group = new StaticGroup(type) {Type = type, Content = GetAllDataEntitiesOfTypeInProject(type)};
                    result.Add(group);
                }
            }

            // send back the list of types in this assembly
            return result;
        }

        protected static List<CustomGroup> ProcessAssembly(Assembly assy)
        {
            var groupListResult = new List<CustomGroup>();

            // ** ASSEMBLY LEVEL
            var groups = assy.GetExportedTypes().Where(t => t.IsSubclassOf(typeof(Entity)) || t == typeof(Entity)).GroupBy(t => t.Namespace);

            // ** NAMESPACE LEVEL
            foreach (var namespaceGroup in groups)
            {
                // ** TYPE LEVEL
                foreach (var type in namespaceGroup)
                {
                    var group = ScriptableObject.CreateInstance<CustomGroup>();
                    group.Title = type.Name;
                    group.Content = GetAllDataEntitiesOfTypeInProject(type);
                    group.Type = type;
                    groupListResult.Add(group);
                    Debug.Log($"<color=green>Discovered {group.Type} as a static group type</color>");
                }
            }

            return groupListResult;
        }

        private static void FindDataEntities()
        {
            var list = new List<Entity>();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(Entity)}");
            list.AddRange(guids.Select(guid => AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(Entity)) as Entity));

            Data.Database.ClearData();
            foreach (var x in list)
            {
                Data.Database.Add(x, false);
            }

            UnityEditor.EditorUtility.SetDirty(Data.Database);
        }

        [MenuItem("Tools/Pancake/Database/Recreate Id All Entity (DANGER)", priority = 100)]
        public static void ResetIdAllEntity()
        {
            bool proceed = UnityEditor.EditorUtility.DisplayDialog("Reset Data Keys",
                "This will find every Entity in the project and assign a new id to each one.",
                "Proceed",
                "Abort");
            if (!proceed) return;

            if (Data.Database == null)
            {
                Debug.Log("Database not found. Please create via create asset menu 'Pancake/Create Database'");
                return;
            }

            var data = GetAllDataEntitiesOfTypeInProject(typeof(Entity));
            var changed = 0;
            foreach (var x in data)
            {
                x.ID = Ulid.NewUlid().ToString();
                UnityEditor.EditorUtility.SetDirty(x);
                changed++;
            }

            Reload();

            UnityEditor.EditorUtility.DisplayDialog("Complete", $"Changed {changed} entity keys.", "Ok");
        }

        /// <summary>
        /// Forces a refresh of assets serialization.
        /// </summary>
        [MenuItem("Tools/Pancake/Database/Reimport Entities - By Type (Safe)", priority = 100)]
        public static void ReimportAllByType()
        {
            bool confirm = UnityEditor.EditorUtility.DisplayDialog("Reimport Data Asset Files",
                $"Reimport all of the Entity Data Assets?\n\n" +
                $"This reimports all Entity type Assets. Won't fix issues related to mismatching class/file names.\n\n This is generally a safe operation.",
                "Proceed",
                "Abort!");

            if (!confirm) return;

            var count = 0;
            AssetDatabase.StartAssetEditing();
            try
            {
                string storage = EditorUtility.StoragePath();
                if (storage[storage.Length - 1] == '/') storage = storage.Remove(storage.Length - 1);
                string[] files = AssetDatabase.FindAssets("t:Entity", new[] {storage});
                for (var i = 0; i < files.Length; i++)
                {
                    UnityEditor.EditorUtility.DisplayProgressBar("Importing...", AssetDatabase.GUIDToAssetPath(files[i]), (float) i / files.Length);
                    AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(files[i]), ImportAssetOptions.ForceUpdate);
                    Debug.Log($"{AssetDatabase.GUIDToAssetPath(files[i])}");
                    count++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                UnityEditor.EditorUtility.ClearProgressBar();
                UnityEditor.EditorUtility.DisplayDialog("Done reimporting", $"{count} assets were reimported and logged to the console.", "Great");
            }
        }

        /// <summary>
        /// Forces a refresh of assets serialization.
        /// </summary>
        [MenuItem("Tools/Pancake/Database/Reimport Entities - By Name (Safe)", priority = 100)]
        public static void ReimportAllByName()
        {
            bool confirm = UnityEditor.EditorUtility.DisplayDialog("Reimport Data Asset Files",
                "Reimport all of the Data Data Assets?\n\n" +
                "This reimports all files with names including 'Data-' which is the built-in prefix for saved Data Files.\n\n This is generally a safe operation.",
                "Proceed",
                "Abort");

            if (!confirm) return;

            var count = 0;
            AssetDatabase.StartAssetEditing();
            try
            {
                string storage = EditorUtility.StoragePath();
                if (storage[storage.Length - 1] == '/') storage = storage.Remove(storage.Length - 1);
                string[] files = AssetDatabase.FindAssets("Data-", new[] {storage});
                for (var i = 0; i < files.Length; i++)
                {
                    UnityEditor.EditorUtility.DisplayProgressBar("Importing...", AssetDatabase.GUIDToAssetPath(files[i]), (float) i / files.Length);
                    AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(files[i]), ImportAssetOptions.ForceUpdate);
                    Debug.Log($"{AssetDatabase.GUIDToAssetPath(files[i])}");
                    count++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                UnityEditor.EditorUtility.ClearProgressBar();
                UnityEditor.EditorUtility.DisplayDialog("Done reimporting", $"{count} assets were reimported and logged to the console.", "Great");
            }
        }

        [MenuItem("Tools/Pancake/Database/Cleanup Data (Semi-Safe)", priority = 100)]
        public static void CleanupStorageFolder()
        {
            bool confirm = UnityEditor.EditorUtility.DisplayDialog("Cleanup Data",
                "This will check all asset files with the 'Data-' prefix and ensure validity. Invalid files can be deleted. This is primarily for identifying or removing assets which have broken script connections due to class name mismatches or class deletions. \n\n" +
                "You will be able to confirm delete/skip for each file individually.\n\n" +
                "You may NOT want to do this if the data found is broken accidentally and you're trying to restore it. This does not restore data, it validates assets and offers deletion if they are problematic. While this cleans up the project, it does DELETE the data asset file.\n",
                "Proceed",
                "Abort");

            if (!confirm) return;

            var found = 0;
            var deleted = 0;
            var failed = 0;
            var ignored = 0;
            AssetDatabase.StartAssetEditing();
            try
            {
                string storage = EditorUtility.StoragePath();
                if (storage[storage.Length - 1] == '/') storage = storage.Remove(storage.Length - 1);
                string[] files = AssetDatabase.FindAssets("Data-", new[] {storage});
                for (var i = 0; i < files.Length; i++)
                {
                    UnityEditor.EditorUtility.DisplayProgressBar("Scanning...", AssetDatabase.GUIDToAssetPath(files[i]), (float) i / files.Length);

                    string path = AssetDatabase.GUIDToAssetPath(files[i]);
                    var file = AssetDatabase.LoadAssetAtPath<Entity>(AssetDatabase.GUIDToAssetPath(files[i]));
                    if (file == null)
                    {
                        found++;

                        // how the heck do i get the object if we're literally dealing with objects that don't cast.
                        //EditorGUIUtility.PingObject();

                        bool deleteFaulty = UnityEditor.EditorUtility.DisplayDialog("Faulty file found",
                            $"{path}\n\n" + "This file seems to be broken. Please check:\n\n" + "* File is actually a Data Data file.\n" +
                            "* Class file still exists.\n" + "* Class filename matches class name.\n" + "* Assemblies are not black-listed.\n\n" +
                            "What do you want to do?",
                            "Delete file",
                            "Ignore file");

                        if (deleteFaulty)
                        {
                            bool success = AssetDatabase.DeleteAsset(path);
                            if (success) deleted++;
                            else failed++;
                        }
                        else ignored++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                UnityEditor.EditorUtility.ClearProgressBar();
                UnityEditor.EditorUtility.DisplayDialog("Done cleaning.",
                    $"{found} assets were faulty.\n" + $"{deleted} assets were deleted.\n" + $"{failed} assets failed to delete.\n" + $"{ignored} assets were ignored.\n",
                    "Excellent");
            }
        }

        public static List<T> GetAllAssetsInProject<T>() where T : Entity
        {
            var list = new List<T>();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(T));
                list.Add(asset as T);
            }

            return list;
        }

        public static List<Entity> GetAllDataEntitiesOfTypeInProject(Type t)
        {
            var list = new List<Entity>();
            string[] guids = AssetDatabase.FindAssets($"t:{t.Name}");
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = (Entity) AssetDatabase.LoadAssetAtPath(assetPath, typeof(Entity));
                list.Add(asset);
            }

            return list;
        }
    }
}