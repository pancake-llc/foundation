using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Pancake;
using PancakeEditor.Common;

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

namespace PancakeEditor.Hierarchy
{
    public class ErrorComponent : BaseHierarchy
    {
        private int _errorCount;
        private StringBuilder _errorStringBuilder;
        private readonly List<string> _targetPropertiesNames = new List<string>(10);

        public ErrorComponent() { rect.width = 7; }

        protected override bool Enabled => HierarchyEditorSetting.EnabledError;
        protected override bool ShowComponentDuringPlayMode => true;

        public override HierarchyLayoutStatus Layout(GameObject gameObject, Rect selectionRect, ref Rect currentRect, float maxWidth)
        {
            if (maxWidth < 7) return HierarchyLayoutStatus.Failed;
            
            rect.x = currentRect.x - 7;
            rect.y = currentRect.y;
            return HierarchyLayoutStatus.Success;
        }

        public override void Draw(GameObject gameObject, Rect selectionRect)
        {
            bool errorFound = FindError(gameObject, gameObject.GetComponents<MonoBehaviour>());

            if (errorFound)
            {
                GUI.color = HierarchyEditorSetting.AdditionalActiveColor.Get();
                GUI.DrawTexture(rect, EditorResources.IconWarning);
                GUI.color = Color.white;
            }
            else if (HierarchyEditorSetting.ShowIconOnParent)
            {
                errorFound = FindError(gameObject, gameObject.GetComponentsInChildren<MonoBehaviour>(true));
                if (errorFound)
                {
                    GUI.color = HierarchyEditorSetting.AdditionalInactiveColor.Get();
                    GUI.DrawTexture(rect, EditorResources.IconWarning);
                    GUI.color = Color.white;
                }
            }
        }

        public override void EventHandler(GameObject gameObject, Event currentEvent)
        {
            if (currentEvent.isMouse && currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
            {
                currentEvent.Use();

                _errorCount = 0;
                _errorStringBuilder = new StringBuilder();
                FindError(gameObject, gameObject.GetComponents<MonoBehaviour>(), true);

                if (_errorCount > 0)
                {
                    EditorUtility.DisplayDialog(_errorCount + (_errorCount == 1 ? " error was found" : " errors were found"), _errorStringBuilder.ToString(), "OK");
                }
            }
        }

        private bool FindError(GameObject gameObject, MonoBehaviour[] components, bool printError = false)
        {
            if (HierarchyEditorSetting.ShowWhenTagOrLayerIsUndefined)
            {
                try
                {
                    gameObject.tag.CompareTo(null);
                }
                catch
                {
                    if (printError)
                    {
                        AppendErrorLine("Tag is undefined");
                    }
                    else
                    {
                        return true;
                    }
                }

                if (LayerMask.LayerToName(gameObject.layer).Equals(""))
                {
                    if (printError)
                    {
                        AppendErrorLine("Layer is undefined");
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            for (var i = 0; i < components.Length; i++)
            {
                var monoBehaviour = components[i];
                if (monoBehaviour == null)
                {
                    if (HierarchyEditorSetting.ShowScriptMissing)
                    {
                        if (printError) AppendErrorLine("Component #" + i + " is missing");
                        else return true;
                    }
                }
                else
                {
                    if (HierarchyEditorSetting.ShowMissingEventMethod)
                    {
                        if (monoBehaviour.gameObject.activeSelf || HierarchyEditorSetting.ShowForDisabledComponents)
                        {
                            try
                            {
                                if (IsUnityEventsNullOrMissing(monoBehaviour, printError))
                                    if (!printError)
                                        return true;
                            }
                            catch (Exception)
                            {
                                //ignored
                            }
                        }
                    }

                    if (HierarchyEditorSetting.ShowReferenceNull || HierarchyEditorSetting.ShowReferenceIsMissing)
                    {
                        if (!monoBehaviour.enabled && !HierarchyEditorSetting.ShowForDisabledComponents) continue;
                        if (!monoBehaviour.gameObject.activeSelf && !HierarchyEditorSetting.ShowForDisabledGameObjects) continue;

                        var type = monoBehaviour.GetType();

                        while (type != null)
                        {
                            var bf = BindingFlags.Instance | BindingFlags.Public;
                            if (!type.FullName.Contains("UnityEngine"))
                                bf |= BindingFlags.NonPublic;
                            var fieldArray = type.GetFields(bf);

                            for (var j = 0; j < fieldArray.Length; j++)
                            {
                                var field = fieldArray[j];

                                if (Attribute.IsDefined(field, typeof(HideInInspector))
                                    || Attribute.IsDefined(field, typeof(HierarchyNullableAttribute))
                                    || Attribute.IsDefined(field, typeof(NonSerializedAttribute)) ||
                                    field.IsStatic) continue;

                                if (field.IsPrivate || !field.IsPublic)
                                {
                                    if (!Attribute.IsDefined(field, typeof(SerializeField))) continue;
                                }

                                object value = field.GetValue(monoBehaviour);

                                try
                                {
                                    if (HierarchyEditorSetting.ShowReferenceIsMissing && value != null && value is Component && (Component) value == null)
                                    {
                                        if (printError)
                                        {
                                            AppendErrorLine(monoBehaviour.GetType().Name + "." + field.Name + ": Reference is missing");
                                            continue;
                                        }

                                        return true;
                                    }
                                }
                                catch
                                {
                                    // ignored
                                }

                                try
                                {
                                    if (HierarchyEditorSetting.ShowReferenceNull && (value == null || value.Equals(null)))
                                    {
                                        if (printError)
                                        {
                                            AppendErrorLine(monoBehaviour.GetType().Name + "." + field.Name + ": Reference is null");
                                            continue;
                                        }

                                        return true;
                                    }
                                }
                                catch
                                {
                                    // ignored
                                }

                                try
                                {
                                    if (HierarchyEditorSetting.ShowReferenceNull && value != null && value is IEnumerable)
                                    {
                                        foreach (var item in (IEnumerable) value)
                                        {
                                            if (item == null || item.Equals(null))
                                            {
                                                if (printError)
                                                {
                                                    AppendErrorLine(monoBehaviour.GetType().Name + "." + field.Name + ": IEnumerable has value with null reference");
                                                    continue;
                                                }

                                                return true;
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                    // ignored
                                }
                            }

                            type = type.BaseType;
                        }
                    }
                }
            }

            return false;
        }

        private bool IsUnityEventsNullOrMissing(MonoBehaviour monoBehaviour, bool printError)
        {
            _targetPropertiesNames.Clear();
            var fieldArray = monoBehaviour.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            for (int i = fieldArray.Length - 1; i >= 0; i--)
            {
                var field = fieldArray[i];
                if (field.FieldType == typeof(UnityEventBase) || field.FieldType.IsSubclassOf(typeof(UnityEventBase)))
                {
                    _targetPropertiesNames.Add(field.Name);
                }
            }

            if (_targetPropertiesNames.Count > 0)
            {
                var serializedMonoBehaviour = new SerializedObject(monoBehaviour);
                for (int i = _targetPropertiesNames.Count - 1; i >= 0; i--)
                {
                    string targetProperty = _targetPropertiesNames[i];

                    var property = serializedMonoBehaviour.FindProperty(targetProperty);
                    var propertyRelativeArrray = property.FindPropertyRelative("m_PersistentCalls.m_Calls");

                    for (int j = propertyRelativeArrray.arraySize - 1; j >= 0; j--)
                    {
                        var arrayElementAtIndex = propertyRelativeArrray.GetArrayElementAtIndex(j);

                        var propertyTarget = arrayElementAtIndex.FindPropertyRelative("m_Target");
                        if (propertyTarget.objectReferenceValue == null)
                        {
                            if (printError)
                            {
                                AppendErrorLine(monoBehaviour.GetType().Name + ": Event object reference is null");
                            }
                            else
                            {
                                return true;
                            }
                        }

                        var propertyMethodName = arrayElementAtIndex.FindPropertyRelative("m_MethodName");
                        if (string.IsNullOrEmpty(propertyMethodName.stringValue))
                        {
                            if (printError)
                            {
                                AppendErrorLine(monoBehaviour.GetType().Name + ": Event handler function is not selected");
                                continue;
                            }

                            return true;
                        }

                        string argumentAssemblyTypeName = arrayElementAtIndex.FindPropertyRelative("m_Arguments")
                            .FindPropertyRelative("m_ObjectArgumentAssemblyTypeName")
                            .stringValue;
                        Type argumentAssemblyType;
                        if (!string.IsNullOrEmpty(argumentAssemblyTypeName))
                            argumentAssemblyType = Type.GetType(argumentAssemblyTypeName, false) ?? typeof(UnityEngine.Object);
                        else argumentAssemblyType = typeof(UnityEngine.Object);

                        UnityEventBase dummyEvent;
                        var propertyTypeName = Type.GetType(property.FindPropertyRelative("m_TypeName").stringValue, false);
                        if (propertyTypeName == null) dummyEvent = new UnityEvent();
                        else dummyEvent = Activator.CreateInstance(propertyTypeName) as UnityEventBase;

                        if (!UnityEventDrawer.IsPersistantListenerValid(dummyEvent,
                                propertyMethodName.stringValue,
                                propertyTarget.objectReferenceValue,
                                (PersistentListenerMode) arrayElementAtIndex.FindPropertyRelative("m_Mode").enumValueIndex,
                                argumentAssemblyType))
                        {
                            if (printError)
                            {
                                AppendErrorLine(monoBehaviour.GetType().Name + ": Event handler function is missing");
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private void AppendErrorLine(string error)
        {
            _errorCount++;
            _errorStringBuilder.Append(_errorCount.ToString());
            _errorStringBuilder.Append(") ");
            _errorStringBuilder.AppendLine(error);
        }
    }
}