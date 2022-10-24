using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Editor
{
    public static partial class InEditor
    {
        /// <summary>
        /// Find all components <typeparamref name="T"/> of root prefab GameObjects
        /// </summary>
        public static List<T> FindAllAssetComponents<T>() where T : Component
        {
            var gos = FindAllAssets<GameObject>();
            return gos.SelectMany(go => go.GetComponents<T>()).ToList();
        }

        /// <summary>
        /// Find all assets of type <typeparamref name="T"/>.
        /// In editor it uses AssetDatabase.
        /// In runtime it uses Resources.FindObjectsOfTypeAll
        /// </summary>
        public static List<T> FindAllAssets<T>() where T : Object
        {
            List<T> l = new List<T>();
#if UNITY_EDITOR
            var typeStr = typeof(T).ToString();
            typeStr = typeStr.Replace("UnityEngine.", "");

            if (typeof(T) == typeof(SceneAsset)) typeStr = "Scene";
            else if (typeof(T) == typeof(GameObject)) typeStr = "gameobject";

            var guids = AssetDatabase.FindAssets("t:" + typeStr);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                T obj = AssetDatabase.LoadAssetAtPath<T>(path);
                if (obj != null) l.Add(obj);
            }
#else
            l.AddRange(Resources.FindObjectsOfTypeAll<T>());
#endif
            return l;
        }

        /// <summary>
        /// Find all asset has type <typeparamref name="T"></typeparamref> in folder <paramref name="path"/>
        /// </summary>
        /// <param name="path">path find asset</param>
        /// <typeparam name="T">type</typeparam>
        /// <returns></returns>
        public static T[] FindAllAssetsWithPath<T>(string path)
        {
            ArrayList al = new ArrayList();
            string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);

            foreach (string fileName in fileEntries)
            {
                int assetPathIndex = fileName.IndexOf("Assets", StringComparison.Ordinal);
                string localPath = fileName.Substring(assetPathIndex);

                Object t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

                if (t != null)
                    al.Add(t);
            }

            T[] result = new T[al.Count];
            for (int i = 0; i < al.Count; i++)
                result[i] = (T) al[i];

            return result;
        }

        public static T FindAssetWithPath<T>(string nameAsset, string relativePath) where T : Object
        {
            string path = AssetInPackagePath(relativePath, nameAsset);
            var t = AssetDatabase.LoadAssetAtPath(path, typeof(T));
            if (t == null) Debug.LogError($"Couldn't load the {nameof(T)} at path :{path}");
            return t as T;
        }

        private static string AssetInPackagePath(string relativePath, string nameAsset, string namePackage = "com.pancake.heart")
        {
            var upmPath = $"Packages/{namePackage}/{relativePath}/{nameAsset}";
            return !File.Exists(Path.GetFullPath(upmPath)) ? $"Assets/_Root/{relativePath}/{nameAsset}" : upmPath;
        }

        /// <summary>
        /// Swap value of <paramref name="keyA"/> and <paramref name="keyB"/>
        /// </summary>
        /// <param name="keyA"></param>
        /// <param name="keyB"></param>
        public static void SwapEditorPrefs<T>(string keyA, string keyB)
        {
            switch (typeof(T))
            {
                // ReSharper disable once ConvertTypeCheckPatternToNullCheck
                case Type intType when intType == typeof(int):
                    int tempInt = EditorPrefs.GetInt(keyA);
                    EditorPrefs.SetInt(keyA, EditorPrefs.GetInt(keyB));
                    EditorPrefs.SetInt(keyB, tempInt);
                    break;
                // ReSharper disable once ConvertTypeCheckPatternToNullCheck
                case Type stringType when stringType == typeof(string):
                    string tempString = EditorPrefs.GetString(keyA);
                    EditorPrefs.SetString(keyA, EditorPrefs.GetString(keyB));
                    EditorPrefs.SetString(keyB, tempString);
                    break;
                // ReSharper disable once ConvertTypeCheckPatternToNullCheck
                case Type floatType when floatType == typeof(float):
                    float tempFloat = EditorPrefs.GetFloat(keyA);
                    EditorPrefs.SetFloat(keyA, EditorPrefs.GetFloat(keyB));
                    EditorPrefs.SetFloat(keyB, tempFloat);
                    break;
            }
        }

        /// <summary>
        /// Current event type is equal repaint or not
        /// </summary>
        public static bool IsRepaint => Event.current.type == EventType.Repaint;

        /// <summary>
        /// Current event type is equal layout or not
        /// </summary>
        public static bool IsLayout => Event.current.type == EventType.Layout;

        /// <summary>
        /// Copy <paramref name="value"/> to clipboard
        /// </summary>
        /// <param name="value"></param>
        public static void CopyToClipboard(string value) { EditorGUIUtility.systemCopyBuffer = value; }

        /// <summary>
        /// Get current value store in clipboard
        /// </summary>
        /// <returns></returns>
        public static string GetClipboardValue() { return EditorGUIUtility.systemCopyBuffer; }

        /// <summary>
        /// Show popup dialog
        /// </summary>
        /// <param name="title">title dialog</param>
        /// <param name="message">message dialog</param>
        /// <param name="strOk">name button ok display</param>
        /// <param name="strCancel">name button cancel display</param>
        /// <param name="actionOk">callback button ok</param>
        /// <returns></returns>
        public static void ShowDialog(string title, string message, string strOk, string strCancel, Action actionOk)
        {
            if (EditorUtility.DisplayDialog(title, message, strOk, strCancel)) actionOk?.Invoke();
        }

        public static void DelayedCall(float delay, Action callback)
        {
            var delayedCall = new DelayedCall(delay, callback);
        }

        /// <summary>
        /// thanks @JoshuaMcKenzie and @Edvard-D
        /// remove all empty object reference elements
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int RemoveEmptyArrayElements(this SerializedProperty list)
        {
            var elementsRemoved = 0;
            if (list == null) return elementsRemoved;

            for (int i = list.arraySize - 1; i >= 0; i--)
            {
                var element = list.GetArrayElementAtIndex(i);
                if (element.propertyType != SerializedPropertyType.ObjectReference) continue;
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    list.RemoveElement(i);
                    elementsRemoved++;
                }
            }

            return elementsRemoved;
        }

        /// <summary>
        /// thanks @JoshuaMcKenzie
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public static void RemoveElement(this SerializedProperty list, int index)
        {
            if (list == null)
                throw new ArgumentNullException();

            if (!list.isArray)
                throw new ArgumentException("Property is not an array");

            if (index < 0 || index >= list.arraySize)
                throw new IndexOutOfRangeException();

            list.GetArrayElementAtIndex(index).SetPropertyValue(null);
            list.DeleteArrayElementAtIndex(index);

            list.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// thanks @JoshuaMcKenzie
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue(this SerializedProperty property, object value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.AnimationCurve:
                    property.animationCurveValue = value as AnimationCurve;
                    break;

                case SerializedPropertyType.ArraySize:
                    property.intValue = Convert.ToInt32(value);
                    break;

                case SerializedPropertyType.Boolean:
                    property.boolValue = Convert.ToBoolean(value);
                    break;

                case SerializedPropertyType.Bounds:
                    property.boundsValue = (Bounds?) value ?? new Bounds();
                    break;

                case SerializedPropertyType.Character:
                    property.intValue = Convert.ToInt32(value);
                    break;

                case SerializedPropertyType.Color:
                    property.colorValue = (Color?) value ?? new Color();
                    break;

                case SerializedPropertyType.Float:
                    property.floatValue = Convert.ToSingle(value);
                    break;

                case SerializedPropertyType.Integer:
                    property.intValue = Convert.ToInt32(value);
                    break;

                case SerializedPropertyType.LayerMask:
                    property.intValue = (value as LayerMask?)?.value ?? Convert.ToInt32(value);
                    break;

                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = value as UnityEngine.Object;
                    break;

                case SerializedPropertyType.Quaternion:
                    property.quaternionValue = (Quaternion?) value ?? Quaternion.identity;
                    break;

                case SerializedPropertyType.Rect:
                    property.rectValue = (Rect?) value ?? new Rect();
                    break;

                case SerializedPropertyType.String:
                    property.stringValue = value as string;
                    break;

                case SerializedPropertyType.Vector2:
                    property.vector2Value = (Vector2?) value ?? Vector2.zero;
                    break;

                case SerializedPropertyType.Vector3:
                    property.vector3Value = (Vector3?) value ?? Vector3.zero;
                    break;

                case SerializedPropertyType.Vector4:
                    property.vector4Value = (Vector4?) value ?? Vector4.zero;
                    break;
            }
        }

        /// <summary>
        /// Get the active directory if it exists or "Assets" returned
        /// </summary>
        public static string ActiveDirectory
        {
            get
            {
                var objs = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

                if (!objs.IsNullOrEmpty())
                {
                    string path;

                    foreach (var obj in objs)
                    {
                        path = AssetDatabase.GetAssetPath(obj);
                        if (Directory.Exists(path) && path.StartsWith("Assets")) return path;
                    }

                    path = AssetDatabase.GetAssetPath(objs[0]);
                    if (path.StartsWith("Assets")) return path.Substring(0, path.LastIndexOf('/'));
                }

                return "Assets";
            }
        }


        /// <summary>
        /// Create asset file.
        /// </summary>
        /// <param name="unityObject"></param>
        /// <param name="assetPath">
        /// A path relative to "Assets", for example: "Assets/MyFolder/MyAsset.asset"
        /// </param>
        /// <param name="autoRename"></param>
        /// <param name="autoCreateDirectory"></param>
        public static void CreateAsset<T>(T unityObject, string assetPath, bool autoRename = true, bool autoCreateDirectory = true) where T : Object
        {
            if (autoCreateDirectory)
            {
                int index = assetPath.LastIndexOf('/');
                if (index >= 0)
                {
                    Directory.CreateDirectory(assetPath.Substring(0, index));
                }
            }

            if (autoRename)
            {
                assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            }

            AssetDatabase.CreateAsset(unityObject, assetPath);
        }


        /// <summary>
        /// Create asset file.
        /// </summary>
        public static void CreateAsset<T>(T unityObject) where T : Object { CreateAsset(unityObject, ActiveDirectory + '/' + typeof(T).Name + ".asset", true, false); }


        /// <summary>
        /// Find asset of specific ease.
        /// </summary>
        public static T FindAsset<T>() where T : Object
        {
            var guids = AssetDatabase.FindAssets("t:" + typeof(T).FullName);
            return !guids.IsNullOrEmpty() ? AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0])) : null;
        }

        /// <summary>
        /// Find all isolated assets of specific ease. An isolated asset is stored in a individual file.
        /// </summary>
        public static List<T> FindIsolatedAssets<T>() where T : Object
        {
            var guids = AssetDatabase.FindAssets("t:" + typeof(T).FullName);
            List<T> results = new List<T>();

            foreach (var guid in guids)
            {
                results.Add(AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)));
            }

            return results;
        }

        /// <summary>
        /// Find all assets of specific ease.
        /// </summary>
        public static List<T> FindAssets<T>() where T : Object
        {
            var guids = AssetDatabase.FindAssets("t:" + typeof(T).FullName);
            List<T> results = new List<T>();

            foreach (var guid in guids)
            {
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(guid)))
                {
                    if (asset is T) results.Add(asset as T);
                }
            }

            return results;
        }

        public static void AddPreloadedAsset(Object assetObject)
        {
            if (!assetObject) return;

            var assets = PlayerSettings.GetPreloadedAssets();

            bool added = false;

            if (assets != null)
            {
                if (ArrayUtility.Contains(assets, assetObject)) return;

                for (int i = 0; i < assets.Length; i++)
                {
                    if (!assets[i])
                    {
                        assets[i] = assetObject;
                        added = true;
                        break;
                    }
                }
            }

            if (!added) ArrayUtility.Add(ref assets, assetObject);

            PlayerSettings.SetPreloadedAssets(assets);
        }

        private const string DEFAULT_RESOURCE_PATH = "Assets/_Root/Resources";
        private const string DEFAULT_ADDRESSABLE_PATH = "Assets/_Root/Storages";

        public static string DefaultResourcesPath()
        {
            if (!DEFAULT_RESOURCE_PATH.DirectoryExists()) DEFAULT_RESOURCE_PATH.CreateDirectory();
            return DEFAULT_RESOURCE_PATH;
        }

        public static string DefaultStoragesPath()
        {
            if (!DEFAULT_ADDRESSABLE_PATH.DirectoryExists()) DEFAULT_ADDRESSABLE_PATH.CreateDirectory();
            return DEFAULT_ADDRESSABLE_PATH;
        }

        // return true if child is childrent of parent
        internal static bool IsChildOfPath(string child, string parent)
        {
            if (child.Equals(parent)) return false;
            var allParent = new List<DirectoryInfo>();
            GetAllParentDirectories(new DirectoryInfo(child), ref allParent);

            foreach (var p in allParent)
            {
                bool check = EqualPath(p, parent);
                if (check) return true;
            }

            return false;
        }

        internal static void GetAllParentDirectories(DirectoryInfo directoryToScan, ref List<DirectoryInfo> directories)
        {
            while (true)
            {
                if (directoryToScan == null || directoryToScan.Name == directoryToScan.Root.Name || !directoryToScan.FullName.Contains("Assets")) return;

                directories.Add(directoryToScan);
                directoryToScan = directoryToScan.Parent;
            }
        }

        internal static void GetAllChildDirectories(string path, ref List<string> directories)
        {
            string[] result = Directory.GetDirectories(path);
            if (result.Length == 0) return;
            foreach (string i in result)
            {
                directories.Add(i);
                GetAllChildDirectories(i, ref directories);
            }
        }

        private static bool EqualPath(FileSystemInfo info, string str)
        {
            string relativePath = info.FullName;
            if (relativePath.StartsWith(Application.dataPath.Replace('/', '\\'))) relativePath = "Assets" + relativePath.Substring(Application.dataPath.Length);
            relativePath = relativePath.Replace('\\', '/');
            return str.Equals(relativePath);
        }

        internal static void ReduceScopeDirectory(ref List<string> source)
        {
            var arr = new string[source.Count];
            source.CopyTo(arr);
            var valueRemove = new List<string>();
            var unique = arr.Distinct().ToList();
            foreach (string u in unique)
            {
                var check = false;
                foreach (string k in unique)
                {
                    if (IsChildOfPath(u, k)) check = true;
                }

                if (check) valueRemove.Add(u);
            }

            foreach (string i in valueRemove)
            {
                unique.Remove(i);
            }

            source = unique;
        }

        public static Rect GetCenterScreen()
        {
            Type containerWindowType = typeof(ScriptableObject).GetAllSubClass().Where(t => t.Name == "ContainerWindow").FirstOrDefault();
            if (containerWindowType == null)
            {
                return Rect.zero;
            }

            FieldInfo showModeField = containerWindowType.GetField("m_ShowMode", BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            PropertyInfo positionProperty = containerWindowType.GetProperty("position", BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (showModeField == null || positionProperty == null)
            {
                return Rect.zero;
            }

            Object[] windows = Resources.FindObjectsOfTypeAll(containerWindowType);
            for (int i = 0; i < windows.Length; i++)
            {
                Object window = windows[i];
                var showmode = (int) showModeField.GetValue(window);
                if (showmode == 4)
                {
                    Rect position = (Rect) positionProperty.GetValue(window, null);
                    return position;
                }
            }

            return Rect.zero;
        }

        public static void MoveToCenter(this EditorWindow window)
        {
            Rect position = window.position;
            Rect mainWindowPosition = InEditor.GetCenterScreen();
            if (mainWindowPosition != Rect.zero)
            {
                float width = (mainWindowPosition.width - position.width) * 0.5f;
                float height = (mainWindowPosition.height - position.height) * 0.5f;
                position.x = mainWindowPosition.x + width;
                position.y = mainWindowPosition.y + height;
                window.position = position;
            }
            else
            {
                window.position = new Rect(new Vector2(Screen.width, Screen.height), new Vector2(position.width, position.height));
            }
        }

        /// <summary>
        /// Find member metadata by path.
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="memberPath">Path to member.</param>
        /// <returns>SerializedMemberData of member.</returns>
        public static MemberData FindMember(this SerializedObject serializedObject, string memberPath) { return new MemberData(serializedObject, memberPath); }

        /// <summary>
        /// Get parent of serialized property.
        /// </summary>
        /// <returns>If serialized property is top, return itself.</returns>
        public static SerializedProperty GetParent(this SerializedProperty property)
        {
            string[] paths = property.propertyPath.Split('.');
            if (paths != null && paths.Length > 1)
            {
                Array.Resize<string>(ref paths, paths.Length - 1);
                string path = string.Join(".", paths);
                return property.serializedObject.FindProperty(path);
            }

            return property;
        }

        /// <summary>
        /// Get visible children of serialized property.
        /// </summary>
        public static IEnumerable<SerializedProperty> GetVisibleChildren(this SerializedProperty serializedProperty)
        {
            SerializedProperty currentProperty = serializedProperty.Copy();
            SerializedProperty nextSiblingProperty = serializedProperty.Copy();
            {
                nextSiblingProperty.NextVisible(false);
            }

            if (currentProperty.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;

                    yield return currentProperty;
                } while (currentProperty.NextVisible(false));
            }
        }
    }
}