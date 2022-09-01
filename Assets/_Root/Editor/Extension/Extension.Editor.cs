using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
        
    }
}