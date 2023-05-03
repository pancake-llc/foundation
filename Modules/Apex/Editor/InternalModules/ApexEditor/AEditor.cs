using System;
using Pancake.Apex;
using Pancake.ExLib.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Vexe.Runtime.Extensions;
using Object = UnityEngine.Object;

namespace Pancake.ApexEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Object), true)]
    public class AEditor : Editor
    {
        private RootContainer rootContainer;

        // Stored search properties.
        private List<VisualEntity> searchEntities;
        private SearchField searchField;
        private string searchText;

        // Stored component flags.
        private bool keepEnable;
        private bool defaultEditor;
        private bool searchableEditor;

        // Stored editor callbacks.
        private MethodCaller<object, object> onInspectorGUI;

        /// <summary>
        /// Called when the editor becomes enabled and active.
        /// </summary>
        protected virtual void OnEnable()
        {
            try
            {
                if (serializedObject.targetObject != null)
                {
                    rootContainer = new RootContainer("Apex Inspector Container", serializedObject, Repaint);

                    searchEntities = new List<VisualEntity>();
                    searchField = new SearchField();
                    searchText = string.Empty;
                    RegisterCallbacks();

                    searchableEditor = target.GetType().GetCustomAttribute<SearchableEditorAttribute>() != null;
                    defaultEditor = target.GetType().GetCustomAttribute<UseDefaultEditor>() != null || ApexUtility.IsExceptType(target.GetType());
                }
            }
            catch (Exception)
            {
                //
            }
        }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (rootContainer != null)
            {
                if (keepEnable || (!defaultEditor && ApexUtility.Enabled))
                {
                    if (searchableEditor)
                    {
                        SearchField();
                    }

                    ApexGUI.IndentLevel = 0;
                    if (!searchableEditor || searchEntities.Count == 0)
                    {
                        HandleMouseMove();
                        rootContainer.DoLayout();
                    }
                    else
                    {
                        OnSearchableEntities();
                    }
                }
                else
                {
                    base.OnInspectorGUI();
                }

                onInspectorGUI.SafeInvoke(target, null);
            }
            else
            {
                GUI.enabled = true;
                GUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox(
                    "Perhaps the script responsible for this component has been deleted from the project. Restore the script or delete this component.",
                    MessageType.Error);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Delete", GUILayout.ExpandHeight(true)))
                {
                    DestroyImmediate(target);
                }

                GUILayout.EndHorizontal();
                GUI.enabled = false;
            }
        }

        /// <summary>
        /// Called when the editor becomes disabled.
        /// </summary>
        protected virtual void OnDisable()
        {
            try
            {
                if (serializedObject.targetObject != null)
                {
                    Type type = target.GetType();
                    foreach (MethodInfo methodInfo in type.AllMethods())
                    {
                        OnInspectorDisposeAttribute attribute = methodInfo.GetCustomAttribute<OnInspectorDisposeAttribute>();
                        if (attribute != null)
                        {
                            if (methodInfo.ReturnType == typeof(void) && methodInfo.GetParameters().Length == 0)
                            {
                                methodInfo.Invoke(target, null);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //
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
        private void OnSearchableEntities()
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
        private void SearchField()
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

        /// <summary>
        /// Find all inspector callbacks.
        /// </summary>
        private void RegisterCallbacks()
        {
            System.Type type = target.GetType();

            foreach (MethodInfo methodInfo in type.AllMethods())
            {
                EditorMethodAttribute attribute = methodInfo.GetCustomAttribute<EditorMethodAttribute>();
                if (attribute != null)
                {
                    if (attribute is OnInspectorInitializeAttribute)
                    {
                        methodInfo.Invoke(target, null);
                    }
                    else if (onInspectorGUI == null && attribute is OnInspectorGUIAttribute && methodInfo.ReturnType == typeof(void) &&
                             methodInfo.GetParameters().Length == 0)
                    {
                        onInspectorGUI = methodInfo.DelegateForCall();
                    }

                    if (attribute is OnRootContainerCreatedAttribute)
                    {
                        methodInfo.Invoke(target, new object[1] {rootContainer});
                    }
                }
            }
        }

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

        #endregion

        #region [Getter / Setter]

        /// <summary>
        /// Set true to enable Apex editor regardless of Apex settings.
        /// </summary>
        public void KeepEnable(bool value) { keepEnable = value; }

        #endregion
    }
}