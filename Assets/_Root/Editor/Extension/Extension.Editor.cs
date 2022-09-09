using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Pancake.Tween;
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

        [MenuItem("GameObject/Pancake/Self Filling", false, 1)]
        private static void AnchorFillinSelectedUIObjects()
        {
            foreach (var obj in Selection.gameObjects)
            {
                var rectTransform = obj.GetComponent<RectTransform>();
                if (rectTransform) rectTransform.SelfFilling();
            }
        }

        [MenuItem("GameObject/Pancake/Self Filling", true, 1)]
        private static bool AnchorFillinSelectedUIObjectsValidate()
        {
            bool flag = false;
            foreach (var obj in Selection.gameObjects)
            {
                var rectTransform = obj.GetComponent<RectTransform>();
                flag = rectTransform != null;
            }

            return flag;
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
    }
}