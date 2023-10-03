using System;
using Pancake.Apex;
using Pancake.ExLib.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using System.Collections;
using UnityEditor.Callbacks;
using Object = UnityEngine.Object;

namespace Pancake.ApexEditor
{
    [CustomEditor(typeof(Object), true)]
    [CanEditMultipleObjects]
    public class AEditor : Editor
    {
        private RootContainer rootContainer;

        // Stored search properties.
        private string searchText;
        private SearchField searchField;
        private List<VisualEntity> searchEntities;

        // Stored component flags.
        private bool keepEnable;
        private bool defaultEditor;
        private bool searchableEditor;

        // Stored editor callbacks.
        private bool valueDealyCallRegistered;
        private bool guiDealyCallRegistered;
        private List<Action> onInspectorGUICalls;
        private List<Action> onInspectorDisposeCalls;
        private List<Action> onObjectChangedCalls;
        private List<Action> onObjectChangedDelayCalls;
        private List<Action> onObjectGUIChangedCalls;
        private List<Action> onObjectGUIChangedDelayCalls;

        /// <summary>
        /// Called when the editor becomes enabled and active.
        /// </summary>
        protected virtual void OnEnable()
        {
            try
            {
                if (serializedObject.targetObject == null)
                {
                    return;
                }
            }
            catch (Exception)
            {
                return;
            }

            if (!ApexUtility.Enabled && !keepEnable)
            {
                defaultEditor = true;
                return;
            }

            Type type = target.GetType();
            if (type.BaseType == null || type.BaseType == typeof(Component) || type.BaseType == typeof(Behaviour))
            {
                defaultEditor = true;
            }
            else
            {
                try
                {
                    rootContainer = new RootContainer("Apex Inspector Container", serializedObject, Repaint);

                    searchEntities = new List<VisualEntity>();
                    searchField = new SearchField();
                    searchText = string.Empty;

                    RegisterCallbacks();

                    searchableEditor = target.GetType().GetCustomAttribute<SearchableEditorAttribute>() != null;
                    defaultEditor = target.GetType().GetCustomAttribute<UseDefaultEditor>() != null || ApexUtility.IsExceptType(target.GetType());
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Exception when creating root container in {type.Name} editor!\nMessage: {ex.Message}");
                    defaultEditor = true;
                }
            }
        }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (serializedObject.targetObject != null)
            {
                if (defaultEditor)
                {
                    base.OnInspectorGUI();
                }
                else
                {
                    if (searchableEditor)
                    {
                        DrawSearchField();
                    }

                    ApexGUI.IndentLevel = 0;
                    if (!searchableEditor || searchEntities.Count == 0)
                    {
                        HandleMouseMove();

                        rootContainer.DoLayout();

                        if (rootContainer.HasObjectChanged())
                        {
                            SafeInvokeObjectChanged();
                        }

                        if (rootContainer.HasGUIChanged())
                        {
                            SafeInvokeObjectGUIChanged();
                        }
                    }
                    else
                    {
                        OnSearchGUI();
                    }

                    SafeInvokeInspectorGUI();
                }
            }
            else
            {
                OnMissingScriptGUI();
            }
        }

        /// <summary>
        /// Implement this method to draw GUI when target object is missing.
        /// </summary>
        protected virtual void OnMissingScriptGUI()
        {
            GUI.enabled = true;
            GUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("Perhaps the script responsible for this component has been deleted from the project. Restore the script or delete this component.",
                MessageType.Error);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Delete", GUILayout.ExpandHeight(true)))
            {
                if (AssetDatabase.IsSubAsset(target))
                {
                    string assetPath = AssetDatabase.GetAssetPath(target);
                    AssetDatabase.RemoveObjectFromAsset(target);
                    DestroyImmediate(target);
                    AssetDatabase.ImportAsset(assetPath);
                }
                else
                {
                    DestroyImmediate(target);
                }
            }

            GUILayout.EndHorizontal();
            GUI.enabled = false;
        }

        /// <summary>
        /// Called when the editor becomes disabled.
        /// </summary>
        protected virtual void OnDisable() { SafeInvokeInspectorDispose(); }

        /// <summary>
        /// Safe invoke all [OnObjectChanged] methods of target object.
        /// </summary>
        public void SafeInvokeObjectChanged()
        {
            if (onObjectChangedCalls != null)
            {
                for (int i = 0; i < onObjectChangedCalls.Count; i++)
                {
                    onObjectChangedCalls[i].Invoke();
                }
            }

            if (!valueDealyCallRegistered && onObjectChangedDelayCalls != null)
            {
                EditorApplication.delayCall += OnObjectChangedDelayCall;
                valueDealyCallRegistered = true;
            }
        }

        /// <summary>
        /// Safe invoke all [OnObjectGUIChanged] methods of target object.
        /// </summary>
        public void SafeInvokeObjectGUIChanged()
        {
            if (onObjectGUIChangedCalls != null)
            {
                for (int i = 0; i < onObjectGUIChangedCalls.Count; i++)
                {
                    onObjectGUIChangedCalls[i].Invoke();
                }
            }

            if (!guiDealyCallRegistered && onObjectGUIChangedDelayCalls != null)
            {
                EditorApplication.delayCall += OnObjectGUIChangedDelayCall;
                guiDealyCallRegistered = true;
            }
        }

        /// <summary>
        /// Safe invoke all [OnInspectorGUI] methods of target object.
        /// </summary>
        private void SafeInvokeInspectorGUI()
        {
            if (onInspectorGUICalls != null)
            {
                for (int i = 0; i < onInspectorGUICalls.Count; i++)
                {
                    onInspectorGUICalls[i].Invoke();
                }
            }
        }

        /// <summary>
        /// Safe invoke all [OnInspectorDispose] methods of target object.
        /// </summary>
        private void SafeInvokeInspectorDispose()
        {
            try
            {
                if (serializedObject.targetObject != null && onInspectorDisposeCalls != null)
                {
                    for (int i = 0; i < onInspectorDisposeCalls.Count; i++)
                    {
                        onInspectorDisposeCalls[i].Invoke();
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// Find and register all callbacks.
        /// </summary>
        private void RegisterCallbacks()
        {
            Type type = target.GetType();
            Type limitDescendant = target is MonoBehaviour ? typeof(MonoBehaviour) : typeof(Object);

            foreach (MethodInfo methodInfo in type.AllMethods(limitDescendant))
            {
                EditorMethodAttribute attribute = methodInfo.GetCustomAttribute<EditorMethodAttribute>();
                if (attribute != null && methodInfo.ReturnType == typeof(void) && methodInfo.GetParameters().Length == 0)
                {
                    if (attribute is OnInspectorInitializeAttribute)
                    {
                        methodInfo.Invoke(target, null);
                    }
                    else if (attribute is OnRootContainerCreatedAttribute)
                    {
                        methodInfo.Invoke(target, new object[1] {rootContainer});
                    }
                    else if (attribute is OnInspectorGUIAttribute)
                    {
                        if (onInspectorGUICalls == null)
                        {
                            onInspectorGUICalls = new List<Action>(1);
                        }

                        onInspectorGUICalls.Add((Action) methodInfo.CreateDelegate(typeof(Action), target));
                    }
                    else if (attribute is OnInspectorDisposeAttribute)
                    {
                        if (onInspectorDisposeCalls == null)
                        {
                            onInspectorDisposeCalls = new List<Action>(1);
                        }

                        onInspectorDisposeCalls.Add((Action) methodInfo.CreateDelegate(typeof(Action), target));
                    }
                    else if (attribute is OnObjectChangedAttribute objectChangedAttribute)
                    {
                        if (objectChangedAttribute.DelayCall)
                        {
                            if (onObjectChangedDelayCalls == null)
                            {
                                onObjectChangedDelayCalls = new List<Action>(1);
                            }

                            onObjectChangedDelayCalls.Add((Action) methodInfo.CreateDelegate(typeof(Action), target));
                        }
                        else
                        {
                            if (onObjectChangedCalls == null)
                            {
                                onObjectChangedCalls = new List<Action>(1);
                            }

                            onObjectChangedCalls.Add((Action) methodInfo.CreateDelegate(typeof(Action), target));
                        }
                    }
                    else if (attribute is OnObjectGUIChangedAttribute objectGUIChangedAttribute)
                    {
                        if (objectGUIChangedAttribute.DelayCall)
                        {
                            if (onObjectGUIChangedDelayCalls == null)
                            {
                                onObjectGUIChangedDelayCalls = new List<Action>(1);
                            }

                            onObjectGUIChangedDelayCalls.Add((Action) methodInfo.CreateDelegate(typeof(Action), target));
                        }
                        else
                        {
                            if (onObjectGUIChangedCalls == null)
                            {
                                onObjectGUIChangedCalls = new List<Action>(1);
                            }

                            onObjectGUIChangedCalls.Add((Action) methodInfo.CreateDelegate(typeof(Action), target));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Receive MouseMove events in the GUI in this Editor.
        /// </summary>
        private void HandleMouseMove()
        {
            if (InspectorWindow == null)
            {
                EditorWindow window = EditorWindow.focusedWindow;
                if (window != null && window.GetType().FullName == "UnityEditor.InspectorWindow")
                {
                    InspectorWindow = window;
                    InspectorWindow.wantsMouseMove = true;
                }
            }
        }

        /// <summary>
        /// Show all entities whose names match the text entered in the search field.
        /// </summary>
        private void OnSearchGUI()
        {
            for (int i = 0; i < searchEntities.Count; i++)
            {
                searchEntities[i].DrawLayout();
            }

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Show search field.
        /// </summary>
        private void DrawSearchField()
        {
            Rect position = GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight);

            position.x -= 18;
            position.y -= 5;
            position.width += 22;
            position.height += 4;
            EditorGUI.DrawRect(position, new Color(0.25f, 0.25f, 0.25f, 1.0f));

            position.x += 4;
            position.y += 3;
            position.width -= 5;
            position.height -= 2;

            EditorGUI.BeginChangeCheck();
            searchText = searchField.OnToolbarGUI(position, searchText);
            if (EditorGUI.EndChangeCheck())
            {
                searchEntities.Clear();
                if (!string.IsNullOrEmpty(searchText))
                {
                    foreach (VisualEntity entity in rootContainer.Entities)
                    {
                        RecursiveSearch(entity);
                    }
                }
            }
        }

        /// <summary>
        /// Recursive search whose names match the text entered in the search field.
        /// </summary>
        /// <param name="visualEntity">Start visual entity.</param>
        private void RecursiveSearch(VisualEntity visualEntity)
        {
            if (visualEntity is Container container)
            {
                foreach (VisualEntity entity in container.Entities)
                {
                    RecursiveSearch(entity);
                }
            }
            else if (visualEntity is IMemberLabel iMember)
            {
                if (iMember.GetLabel().text.ToLower().Contains(searchText))
                {
                    searchEntities.Add(visualEntity);
                }
            }
        }

        #region [Event Actions]

        /// <summary>
        /// Editor application delayCall event for calling OnObjectChanged methods with delayCall parameter.
        /// </summary>
        private void OnObjectChangedDelayCall()
        {
            for (int i = 0; i < onObjectChangedDelayCalls.Count; i++)
            {
                onObjectChangedDelayCalls[i].Invoke();
            }

            valueDealyCallRegistered = false;
        }

        /// <summary>
        /// Editor application delayCall event for calling OnObjectGUIChanged methods with delayCall parameter.
        /// </summary>
        private void OnObjectGUIChangedDelayCall()
        {
            for (int i = 0; i < onObjectGUIChangedDelayCalls.Count; i++)
            {
                onObjectGUIChangedDelayCalls[i].Invoke();
            }

            guiDealyCallRegistered = false;
        }

        #endregion

        #region [Static Members]

        /// <summary>
        /// Find all active Apex editors and rebuild it.
        /// </summary>
        public static void RebuildAll(params string[] except)
        {
            ActiveEditorTracker tracker = ActiveEditorTracker.sharedTracker;
            for (int i = 0; i < tracker.activeEditors.Length; i++)
            {
                Editor editor = tracker.activeEditors[i];
                if (editor is AEditor aEditor && !except.Any(t => t == editor.GetType().Name))
                {
                    aEditor.OnEnable();
                    aEditor.Repaint();
                }
            }
        }

        /// <summary>
        /// Find all active Apex editors and repaint it.
        /// </summary>
        public static void RepaintAll(params string[] except)
        {
            ActiveEditorTracker tracker = ActiveEditorTracker.sharedTracker;
            for (int i = 0; i < tracker.activeEditors.Length; i++)
            {
                Editor editor = tracker.activeEditors[i];
                if (editor is AEditor aEditor && !except.Any(t => t == editor.GetType().Name))
                {
                    aEditor.Repaint();
                }
            }
        }

        /// <summary>
        /// Internal reference to default Unity Inspector editor window.
        /// </summary>
        private static EditorWindow InspectorWindow;

        /// <summary>
        /// Overrides the internal library of the Unity editor to make the Apex editor uniform for all editors, regardless of the user editors.
        /// </summary>
        [DidReloadScripts]
        private static void OverrideUnityEditor()
        {
            if (ApexUtility.Enabled && ApexUtility.Master)
            {
                Assembly assembly = Assembly.GetAssembly(typeof(CustomEditor));
                if (assembly != null)
                {
                    Type type = assembly.GetType("UnityEditor.CustomEditorAttributes");
                    if (type != null)
                    {
#if UNITY_2023_1_OR_NEWER
                        FieldInfo fieldInfo = type.GetField("k_Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                        if (fieldInfo != null)
                        {
                            object instance = fieldInfo.GetValue(null);
                            PropertyInfo instanceValueProperty = instance.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                            if (instanceValueProperty != null)
                            {
                                object instanceValue = instanceValueProperty.GetValue(instance);
                                FieldInfo cacheField = type.GetField("m_Cache", BindingFlags.NonPublic | BindingFlags.Instance);
                                if (cacheField != null)
                                {
                                    object cache = cacheField.GetValue(instanceValue);

                                    Type cacheType = cache.GetType();
                                    FieldInfo editorCacheField = cacheType.GetField("m_CustomEditorCache", BindingFlags.NonPublic | BindingFlags.Instance);
                                    if (editorCacheField != null)
                                    {
                                        List<object> invalidKeys = new List<object>();

                                        IDictionary dictionary = editorCacheField.GetValue(cache) as IDictionary;
                                        foreach (DictionaryEntry entry in dictionary)
                                        {
                                            Type keyType = (Type)entry.Key;
                                            if (IsExtraType(keyType))
                                            {
                                                invalidKeys.Add(entry.Key);
                                            }
                                            else if (keyType == typeof(Object))
                                            {
                                                Type editorStorageType = entry.Value.GetType();
                                                FieldInfo editorsField = editorStorageType.GetField("customEditors", BindingFlags.Public | BindingFlags.Instance);
                                                if (editorsField != null)
                                                {
                                                    List<int> invalidItems = new List<int>();
                                                    IList list = editorsField.GetValue(entry.Value) as IList;

                                                    int count = 0;
                                                    foreach (object item in list)
                                                    {
                                                        if (item != null)
                                                        {
                                                            Type itemType = item.GetType();
                                                            FieldInfo itemField = itemType.GetField("inspectorType", BindingFlags.Public | BindingFlags.Instance);
                                                            if (itemField != null)
                                                            {
                                                                Type inspectorType = (Type)itemField.GetValue(item);
                                                                if (inspectorType != typeof(AEditor))
                                                                {
                                                                    invalidItems.Add(count);
                                                                }
                                                            }
                                                        }
                                                        count++;
                                                    }

                                                    for (int i = 0; i < invalidItems.Count; i++)
                                                    {
                                                        list.RemoveAt(invalidItems[i]);
                                                    }
                                                }

                                                FieldInfo editorsMultiField =
 editorStorageType.GetField("customEditorsMultiEdition", BindingFlags.Public | BindingFlags.Instance);
                                                if (editorsMultiField != null)
                                                {
                                                    List<int> invalidItems = new List<int>();
                                                    IList list = editorsMultiField.GetValue(entry.Value) as IList;

                                                    int count = 0;
                                                    foreach (object item in list)
                                                    {
                                                        if (item != null)
                                                        {
                                                            Type itemType = item.GetType();
                                                            FieldInfo itemField = itemType.GetField("inspectorType", BindingFlags.Public | BindingFlags.Instance);
                                                            if (itemField != null)
                                                            {
                                                                Type inspectorType = (Type)itemField.GetValue(item);
                                                                if (inspectorType != typeof(AEditor))
                                                                {
                                                                    invalidItems.Add(count);
                                                                }
                                                            }
                                                        }
                                                        count++;
                                                    }

                                                    for (int i = 0; i < invalidItems.Count; i++)
                                                    {
                                                        list.RemoveAt(invalidItems[i]);
                                                    }

                                                }
                                            }
                                        }

                                        for (int i = 0; i < invalidKeys.Count; i++)
                                        {
                                            dictionary.Remove(invalidKeys[i]);
                                        }
                                    }

                                }
                            }
                        }
                    }
#else
                        FieldInfo fieldInfo = type.GetField("kSCustomEditors", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                        if (fieldInfo != null)
                        {
                            List<object> invalidKeys = new List<object>();
                            IDictionary dictionary = (IDictionary) fieldInfo.GetValue(null);
                            foreach (DictionaryEntry entry in dictionary)
                            {
                                Type keyType = (Type) entry.Key;
                                if (IsExtraType(keyType))
                                {
                                    invalidKeys.Add(entry.Key);
                                }
                                else if (keyType == typeof(Object))
                                {
                                    int count = 0;
                                    List<int> invalidItems = new List<int>();
                                    IList list = (IList) entry.Value;
                                    foreach (object item in list)
                                    {
                                        if (item != null)
                                        {
                                            Type itemType = item.GetType();
                                            FieldInfo itemField = itemType.GetField("m_InspectorType", BindingFlags.Public | BindingFlags.Instance);
                                            if (itemField != null)
                                            {
                                                Type inspectedType = (Type) itemField.GetValue(item);
                                                if (inspectedType != typeof(AEditor))
                                                {
                                                    invalidItems.Add(count);
                                                }
                                            }
                                        }

                                        count++;
                                    }

                                    for (int i = 0; i < invalidItems.Count; i++)
                                    {
                                        list.RemoveAt(invalidItems[i]);
                                    }
                                }
                            }

                            for (int i = 0; i < invalidKeys.Count; i++)
                            {
                                dictionary.Remove(invalidKeys[i]);
                            }
                        }
                    }
#endif
                }

                bool IsExtraType(Type keyType)
                {
                    return keyType == typeof(MonoBehaviour) || keyType == typeof(Behaviour) || keyType == typeof(Component) || keyType == typeof(ScriptableObject);
                }
            }
        }

        /// <summary>
        /// Overrides the internal library of the Unity editor to make the Apex editor uniform for all editors, regardless of the user editors.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void LaunchOverrideUnityEditor()
        {
            const string guid = "ApexInternal.UnityEditorLaunched";
            if (!SessionState.GetBool(guid, false))
            {
                EditorApplication.delayCall += OverrideUnityEditor;
                SessionState.SetBool(guid, true);
            }
        }

        #endregion

        #region [Getter / Setter]

        /// <summary>
        /// Root container of apex editor.
        /// </summary>
        /// <returns></returns>
        public RootContainer GetRootContainer() { return rootContainer; }

        /// <summary>
        /// Set true to enable Apex editor regardless of Apex settings.
        /// </summary>
        public void KeepEnable(bool value)
        {
            keepEnable = value;
            OnEnable();
        }

        public bool IsDefaultEditor() { return defaultEditor; }

        public void IsDefaultEditor(bool value) { defaultEditor = value; }

        public bool IsSearchableEditor() { return searchableEditor; }

        public void IsSearchableEditor(bool value) { searchableEditor = value; }

        #endregion
    }
}