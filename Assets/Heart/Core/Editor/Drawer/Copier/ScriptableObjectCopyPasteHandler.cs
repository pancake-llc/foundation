using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PancakeEditor
{
    public static class ScriptableObjectCopyPasteHandler
    {
        private static Object copiedObject;

        #region MenuItems and ContextMenu

        #region Copy Methods

        [MenuItem("CONTEXT/ScriptableObject/Copy", true)]
        public static bool ValidateCopy()
        {
            return Selection.activeObject is ScriptableObject && Selection.objects.Length == 1;
        }
        
        [MenuItem("CONTEXT/ScriptableObject/Copy")]
        private static void Copy(MenuCommand command) => Copy(command.context);

        private static void Copy(Object unityObject)
        {
            var so = unityObject as ScriptableObject;
            copiedObject = so;
        }

        #endregion

        #region Paste Methods

        private static bool ValidatePaste()
        {
            if (copiedObject == null) return false;

            Type type = copiedObject.GetType();

            foreach (var selectedObject in Selection.objects)
            {
                if (selectedObject.GetType() == type)
                    return true;
            }

            return false;
        }

        [MenuItem("CONTEXT/ScriptableObject/Paste", true)]
        private static bool ValidatePaste(MenuCommand command) => ValidatePaste();

        [MenuItem("CONTEXT/ScriptableObject/Paste")]
        private static void Paste(MenuCommand command) => ScriptableObjectCopyPasteWindow.OpenWindow(copiedObject as ScriptableObject);

        [UnityEngine.ContextMenu("Paste")]
        public static void Paste(Object unityObject)
        {
            ScriptableObjectCopyPasteWindow.OpenWindow(copiedObject as ScriptableObject);
        }

        #endregion

        #endregion

        #region Operation Methods

        public static List<Object> GetEligibleObjects(Object copiedObject)
        {
            List<Object> eligibleObjects = new List<Object>();

            Type type = copiedObject.GetType();
            if (type == null) return eligibleObjects;

            foreach (var selectedObject in Selection.objects)
            {
                if (selectedObject.GetType() == type)
                {
                    eligibleObjects.Add(selectedObject);
                }
            }

            return eligibleObjects;
        }

        public static void PasteSelectedValues(Object copiedObject, Dictionary<FieldInfo, bool> fieldInfoList)
        {
            ScriptableObjectCopyPasteHandler.copiedObject = copiedObject;

            List<Object> eligibleObjects = GetEligibleObjects(copiedObject);
            int counter = 0;
            foreach (var selectedObject in eligibleObjects)
            {
                Undo.RecordObject(selectedObject, "Paste ScriptableObject Values");

                foreach (var fieldInfo in fieldInfoList.Keys)
                {
                    if (fieldInfoList[fieldInfo])
                    {
                        object value = fieldInfo.GetValue(copiedObject);
                        object copiedValue = CopyValue(value);
                        fieldInfo.SetValue(selectedObject, copiedValue);
                    }
                }

                EditorUtility.SetDirty(selectedObject);
                counter++;
            }

            Debug.Log(
                $"<b>Paste operation <color=#00FF00>completed</color> on <color=#FF0000>{counter}</color> objects</b>");
        }

        private static object CopyValue(object original)
        {
            try
            {
                if (original == null)
                {
                    return null;
                }

                Type type = original.GetType();

                if (type.IsValueType || type == typeof(string))
                {
                    return original;
                }
                else if (type.IsArray)
                {
                    Type elementType = type.GetElementType();
                    var array = original as Array;
                    Array copied = Array.CreateInstance(elementType, array.Length);
                    Array.Copy(array, copied, array.Length);
                    return Convert.ChangeType(copied, original.GetType());
                }
                else if (original is Object)
                {
                    return original;
                }
                else if (original is AnimationCurve originalCurve)
                {
                    AnimationCurve copiedCurve = new AnimationCurve(originalCurve.keys);
                    return copiedCurve;
                }
                else if (original is Gradient originalGradient)
                {
                    Gradient copiedGradient = new Gradient();
                    copiedGradient.SetKeys(originalGradient.colorKeys, originalGradient.alphaKeys);
                    return copiedGradient;
                }
                else
                {
                    object copied;
                    try
                    {
                        copied = Activator.CreateInstance(original.GetType());
                    }
                    catch (MissingMethodException)
                    {
                        copied = FormatterServices.GetUninitializedObject(type);
                    }

                    FieldInfo[] fields =
                        type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (FieldInfo field in fields)
                    {
                        object fieldValue = field.GetValue(original);
                        if (fieldValue == null)
                            continue;
                        field.SetValue(copied, CopyValue(fieldValue));
                    }

                    return copied;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error while copying value: " + e);
                return null;
            }
        }

        #endregion
    }
}