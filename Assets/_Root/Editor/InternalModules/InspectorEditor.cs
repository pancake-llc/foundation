using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Object), true)]
    public class InspectorEditor : UnityEditor.Editor
    {
        private List<VisualEntity> visualElements;
        private List<VisualEntity> searchedElements;
        private List<MethodInfo> onInspectorGUIMethods;
        private List<MethodInfo> onInspectorUpdateMethods;
        private EditorHeartSettings _heartSettings;
        private SearchField searchField;
        private string searchText;
        private bool useDefaultEditor;
        private bool ignoreScriptReference;
        private bool isSearchableEditor;
        private bool keepEnable;

        /// <summary>
        /// Called when the object becomes enabled and active.
        /// </summary>
        protected virtual void OnEnable()
        {
            _heartSettings = EditorHeartSettings.Current;

            System.Type type = target.GetType();
            ignoreScriptReference = type.GetCustomAttribute<HideMonoAttribute>() != null;
            isSearchableEditor = type.GetCustomAttribute<SearchableEditorAttribute>() != null;
            useDefaultEditor = type.GetCustomAttribute<UseDefaultEditor>() != null;
            if (!useDefaultEditor)
            {
                System.Func<EditorHeartSettings.ExceptType, bool> predicate = (exceptType) =>
                {
                    do
                    {
                        if (type.Name == exceptType.Name)
                        {
                            return true;
                        }

                        type = type.BaseType;
                    } while (type != null && exceptType.SubClasses);

                    return false;
                };
                EditorHeartSettings.ExceptType[] exceptTypes = _heartSettings.ExceptTypes;
                if (exceptTypes != null)
                {
                    useDefaultEditor = exceptTypes.Any(predicate);
                }
            }

            searchText = string.Empty;
            searchField = new SearchField();
            CreateVisualElements();
            CopyVisualElementsTo(ref searchedElements);
            RegisterCallbacks();
        }

        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            if (keepEnable || (!useDefaultEditor && _heartSettings.Enabled))
            {
                if (!isSearchableEditor)
                {
                    OnVisualElementsGUI();
                }
                else
                {
                    OnSearchableVisualElementGUI();
                }
            }
            else
            {
                base.OnInspectorGUI();
            }

            for (int i = 0; i < onInspectorGUIMethods.Count; i++)
            {
                onInspectorGUIMethods[i].Invoke(target, null);
            }

            for (int i = 0; i < onInspectorUpdateMethods.Count; i++)
            {
                onInspectorUpdateMethods[i].Invoke(target, null);
            }
        }

        /// <summary>
        /// Create all properties, which required to draw GUI.
        /// </summary>
        private void CreateVisualElements()
        {
            visualElements = new List<VisualEntity>();

            // Collect all serialized property.
            int count = 0;
            // try catch to ignore SerializedObjectNotCreatableException
            try
            {
                using (SerializedProperty iterator = serializedObject.GetIterator())
                {
                    if (iterator.NextVisible(true))
                    {
                        do
                        {
                            SerializedField serializedField = new SerializedField(serializedObject, iterator.propertyPath) {Repaint = Repaint};

                            const string MONO_SCRIPT_NAME = "m_Script";
                            if (iterator.name == MONO_SCRIPT_NAME)
                            {
                                serializedField.AddManipulator(new ReadOnlyAttribute());
                                if (ignoreScriptReference)
                                {
                                    continue;
                                }
                            }

                            serializedField.SetOrder(count++);
                            visualElements.Add(serializedField);
                        } while (iterator.NextVisible(false));
                    }
                }
            }
            catch (Exception)
            {
                //
            }
            

            // Collect all buttons with [SerializeMethod] attribute.
            System.Type type = target.GetType();
            foreach (MethodInfo methodInfo in type.AllMethods())
            {
                SerializeMethodAttribute attribute = methodInfo.GetCustomAttribute<SerializeMethodAttribute>();
                if (attribute != null)
                {
                    SerializedMethod serializedMethod = new SerializedMethod(serializedObject, methodInfo.Name) {Repaint = Repaint};

                    serializedMethod.SetOrder(count++);
                    visualElements.Add(serializedMethod);
                }
            }

            visualElements.Sort(OrderComparison);

            // Layout all visual entities.
            LayoutBoxGroups();
            LayoutTabGroups();
            LayoutFoldout();
            LayoutHorizontalGroup();
        }

        private void RegisterCallbacks()
        {
            onInspectorUpdateMethods = new List<MethodInfo>();
            onInspectorGUIMethods = new List<MethodInfo>();

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
                    else if (attribute is OnInspectorUpdateAttribute)
                    {
                        onInspectorUpdateMethods.Add(methodInfo);
                    }
                    else if (attribute is OnInspectorGUIAttribute)
                    {
                        onInspectorGUIMethods.Add(methodInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Default implementation of GUI.
        /// </summary>
        public void OnVisualElementsGUI()
        {
            if (visualElements != null && visualElements.Count > 0)
            {
                serializedObject.Update();
                for (int i = 0; i < visualElements.Count; i++)
                {
                    visualElements[i].DrawLayout();
                }

                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Default implementation of searchable GUI.
        /// </summary>
        public void OnSearchableVisualElementGUI()
        {
            OnSearchField();
            if (searchedElements != null && searchedElements.Count > 0)
            {
                serializedObject.Update();
                for (int i = 0; i < searchedElements.Count; i++)
                {
                    searchedElements[i].DrawLayout();
                }

                serializedObject.ApplyModifiedProperties();
            }
        }

        private void RecursiveSearch(VisualEntity visualElement)
        {
            if (visualElement is IEnumerable<VisualEntity> enumerable && enumerable.Count() > 0)
            {
                IEnumerator<VisualEntity> enumerator = enumerable.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    RecursiveSearch(enumerator.Current);
                }
            }
            else if (visualElement is IMemberLabel elementName)
            {
                if (elementName.GetLabel().text.ToLower().Contains(searchText))
                {
                    searchedElements.Add(visualElement);
                }
            }
        }

        private void OnSearchField()
        {
            Rect reservePosition = GUILayoutUtility.GetRect(0, 21);

            Rect backgroundPosition = new Rect(reservePosition.xMin - 18, reservePosition.y - 4, reservePosition.width + 22, reservePosition.height);
            EditorGUI.DrawRect(backgroundPosition, new Color(0.25f, 0.25f, 0.25f, 1.0f));

            Rect searchPosition = new Rect(backgroundPosition.xMin + 3, backgroundPosition.y + 3, backgroundPosition.width + 10, backgroundPosition.height);
            EditorGUI.BeginChangeCheck();
            searchText = searchField.OnToolbarGUI(searchPosition, searchText);
            if (EditorGUI.EndChangeCheck())
            {
                if (!string.IsNullOrEmpty(searchText))
                {
                    searchedElements.Clear();
                    for (int i = 0; i < visualElements.Count; i++)
                    {
                        searchText = searchText.ToLower();
                        RecursiveSearch(visualElements[i]);
                    }
                }
                else
                {
                    CopyVisualElementsTo(ref searchedElements);
                }
            }
        }

        private void LayoutBoxGroups()
        {
            INTERATION:
            for (int i = 0; i < visualElements.Count; i++)
            {
                VisualEntity visualElement = visualElements[i];

                if (visualElement is SerializedMember serializedMember)
                {
                    GroupAttribute attribute = serializedMember.GetAttribute<GroupAttribute>();
                    if (attribute != null)
                    {
                        GroupContainer boxGroup = visualElements.Where(e => e is GroupContainer bg && bg.GetName() == attribute.Name).FirstOrDefault() as GroupContainer;
                        if (boxGroup != null)
                        {
                            boxGroup.Add(visualElement);
                            visualElements.RemoveAt(i);
                            goto INTERATION;
                        }

                        boxGroup = new GroupContainer(attribute.Name, new List<VisualEntity>() {visualElement});
                        visualElements[i] = boxGroup;
                        goto INTERATION;
                    }
                }
            }
        }

        private void LayoutTabGroups()
        {
            INTERATION_1:
            for (int i = 0; i < visualElements.Count; i++)
            {
                VisualEntity visualElement = visualElements[i];
                if (visualElement is SerializedMember serializedMember)
                {
                    TabGroupAttribute attribute = serializedMember.GetAttribute<TabGroupAttribute>();
                    if (attribute != null)
                    {
                        TabContainer tabGroup = visualElements.Where(e => e is TabContainer tg && tg.GetName() == attribute.Name).FirstOrDefault() as TabContainer;
                        if (tabGroup != null)
                        {
                            tabGroup.AddElement(attribute.Key, visualElement);
                            visualElements.RemoveAt(i);
                            goto INTERATION_1;
                        }

                        tabGroup = new TabContainer(attribute.Name, new Dictionary<string, List<VisualEntity>>());
                        tabGroup.AddElement(attribute.Key, visualElement);
                        visualElements[i] = tabGroup;
                        goto INTERATION_1;
                    }
                }
                else if (visualElement is GroupContainer boxGroup)
                {
                    INTERATION_2:
                    for (int j = 0; j < boxGroup.GetChildrenCount(); j++)
                    {
                        VisualEntity child = boxGroup.GetChild(j);
                        if (child is SerializedMember childMember)
                        {
                            TabGroupAttribute attribute = childMember.GetAttribute<TabGroupAttribute>();
                            if (attribute != null)
                            {
                                TabContainer tabGroup =
                                    boxGroup.GetChildren().Where(e => e is TabContainer tg && tg.GetName() == attribute.Name).FirstOrDefault() as TabContainer;
                                if (tabGroup != null)
                                {
                                    tabGroup.AddElement(attribute.Key, child);
                                    boxGroup.Remove(j);
                                    goto INTERATION_2;
                                }

                                tabGroup = new TabContainer(attribute.Name, new Dictionary<string, List<VisualEntity>>());
                                tabGroup.AddElement(attribute.Key, child);
                                boxGroup.SetChild(j, tabGroup);
                                goto INTERATION_2;
                            }
                        }
                    }
                }
            }
        }

        private void LayoutFoldout()
        {
            INTERATION_1:
            for (int i = 0; i < visualElements.Count; i++)
            {
                VisualEntity visualElement = visualElements[i];
                if (visualElement is SerializedMember serializedMember)
                {
                    FoldoutAttribute attribute = serializedMember.GetAttribute<FoldoutAttribute>();
                    if (attribute != null)
                    {
                        FoldoutContainer foldout =
                            visualElements.Where(e => e is FoldoutContainer f && f.GetName() == attribute.Name).FirstOrDefault() as FoldoutContainer;
                        if (foldout != null)
                        {
                            foldout.Add(visualElement);
                            visualElements.RemoveAt(i);
                            goto INTERATION_1;
                        }

                        foldout = new FoldoutContainer(attribute.Name, attribute.Style, new List<VisualEntity>() {visualElement});
                        visualElements[i] = foldout;
                        goto INTERATION_1;
                    }
                }
                else if (visualElement is TabContainer tabGroup)
                {
                    INTERATION_2:
                    foreach (KeyValuePair<string, List<VisualEntity>> pair in tabGroup.GetSections())
                    {
                        for (int j = 0; j < pair.Value.Count; j++)
                        {
                            VisualEntity child = pair.Value[j];
                            if (child is SerializedMember childMember)
                            {
                                FoldoutAttribute attribute = childMember.GetAttribute<FoldoutAttribute>();
                                if (attribute != null)
                                {
                                    FoldoutContainer foldout =
                                        tabGroup.GetSections()
                                            .Where(d => d.Key == pair.Key)
                                            .SelectMany(m => m.Value)
                                            .Where(e => e is FoldoutContainer f && f.GetName() == attribute.Name)
                                            .FirstOrDefault() as FoldoutContainer;
                                    if (foldout != null)
                                    {
                                        foldout.Add(child);
                                        pair.Value.RemoveAt(j);
                                        goto INTERATION_2;
                                    }

                                    foldout = new FoldoutContainer(attribute.Name, attribute.Style, new List<VisualEntity>() {child});
                                    pair.Value[j] = foldout;
                                    goto INTERATION_2;
                                }
                            }
                        }
                    }
                }
                else if (visualElement is GroupContainer boxGroup)
                {
                    INTERATION_3:
                    for (int k = 0; k < boxGroup.GetChildrenCount(); k++)
                    {
                        VisualEntity child = boxGroup.GetChild(k);
                        if (child is SerializedMember childMember)
                        {
                            FoldoutAttribute attribute = childMember.GetAttribute<FoldoutAttribute>();
                            if (attribute != null)
                            {
                                FoldoutContainer foldout =
                                    boxGroup.GetChildren().Where(e => e is FoldoutContainer f && f.GetName() == attribute.Name).FirstOrDefault() as FoldoutContainer;
                                if (foldout != null)
                                {
                                    foldout.Add(child);
                                    boxGroup.Remove(k);
                                    goto INTERATION_3;
                                }

                                foldout = new FoldoutContainer(attribute.Name, attribute.Style, new List<VisualEntity>() {child});
                                boxGroup.SetChild(k, foldout);
                                goto INTERATION_3;
                            }
                        }
                    }
                }
            }
        }

        private void LayoutHorizontalGroup()
        {
            INTERATION_1:
            for (int i = 0; i < visualElements.Count; i++)
            {
                VisualEntity visualElement = visualElements[i];
                if (visualElement is SerializedMember serializedMember)
                {
                    HorizontalGroupAttribute attribute = serializedMember.GetAttribute<HorizontalGroupAttribute>();
                    if (attribute != null)
                    {
                        HorizontalContainer horizontalGroup =
                            visualElements.Where(e => e is HorizontalContainer h && h.GetName() == attribute.Name).FirstOrDefault() as HorizontalContainer;
                        if (horizontalGroup != null)
                        {
                            horizontalGroup.Add(visualElement);
                            visualElements.RemoveAt(i);
                            goto INTERATION_1;
                        }

                        horizontalGroup = new HorizontalContainer(attribute.Name, new List<VisualEntity>() {visualElement});
                        visualElements[i] = horizontalGroup;
                        goto INTERATION_1;
                    }
                }
                else if (visualElement is TabContainer tabGroup)
                {
                    INTERATION_2:
                    foreach (KeyValuePair<string, List<VisualEntity>> pair in tabGroup.GetSections())
                    {
                        for (int j = 0; j < pair.Value.Count; j++)
                        {
                            VisualEntity child = pair.Value[j];
                            if (child is SerializedMember childMember)
                            {
                                HorizontalGroupAttribute attribute = childMember.GetAttribute<HorizontalGroupAttribute>();
                                if (attribute != null)
                                {
                                    HorizontalContainer horizontalGroup =
                                        tabGroup.GetSections()
                                            .Where(d => d.Key == pair.Key)
                                            .SelectMany(m => m.Value)
                                            .Where(e => e is HorizontalContainer h && h.GetName() == attribute.Name)
                                            .FirstOrDefault() as HorizontalContainer;
                                    if (horizontalGroup != null)
                                    {
                                        horizontalGroup.Add(child);
                                        pair.Value.RemoveAt(j);
                                        goto INTERATION_2;
                                    }

                                    horizontalGroup = new HorizontalContainer(attribute.Name, new List<VisualEntity>() {child});
                                    pair.Value[j] = horizontalGroup;
                                    goto INTERATION_2;
                                }
                            }
                            else if (child is FoldoutContainer foldout)
                            {
                                SUB_INTERATION_1:
                                for (int k = 0; k < foldout.GetChildrenCount(); k++)
                                {
                                    VisualEntity fChild = foldout.GetChild(k);
                                    if (fChild is SerializedMember fChildMember)
                                    {
                                        HorizontalGroupAttribute attribute = fChildMember.GetAttribute<HorizontalGroupAttribute>();
                                        if (attribute != null)
                                        {
                                            HorizontalContainer horizontalGroup =
                                                foldout.GetChildren().Where(e => e is HorizontalContainer h && h.GetName() == attribute.Name).FirstOrDefault() as
                                                    HorizontalContainer;
                                            if (horizontalGroup != null)
                                            {
                                                horizontalGroup.Add(fChild);
                                                foldout.Remove(k);
                                                goto SUB_INTERATION_1;
                                            }

                                            horizontalGroup = new HorizontalContainer(attribute.Name, new List<VisualEntity>() {fChild});
                                            foldout.SetChild(k, horizontalGroup);
                                            goto SUB_INTERATION_1;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (visualElement is GroupContainer boxGroup)
                {
                    INTERATION_3:
                    for (int j = 0; j < boxGroup.GetChildrenCount(); j++)
                    {
                        VisualEntity bgChild = boxGroup.GetChild(j);
                        if (bgChild is SerializedMember bgChildMember)
                        {
                            HorizontalGroupAttribute attribute = bgChildMember.GetAttribute<HorizontalGroupAttribute>();
                            if (attribute != null)
                            {
                                HorizontalContainer horizontalGroup =
                                    boxGroup.GetChildren()
                                        .Where(e => e is HorizontalContainer h && h.GetName() == attribute.Name)
                                        .FirstOrDefault() as HorizontalContainer;
                                if (horizontalGroup != null)
                                {
                                    horizontalGroup.Add(bgChild);
                                    boxGroup.Remove(j);
                                    goto INTERATION_3;
                                }

                                horizontalGroup = new HorizontalContainer(attribute.Name, new List<VisualEntity>() {bgChild});
                                boxGroup.SetChild(j, horizontalGroup);
                                goto INTERATION_3;
                            }
                        }
                        else if (bgChild is FoldoutContainer foldout)
                        {
                            SUB_INTERATION_1:
                            for (int k = 0; k < foldout.GetChildrenCount(); k++)
                            {
                                VisualEntity fChild = foldout.GetChild(k);
                                if (fChild is SerializedMember fChildMember)
                                {
                                    HorizontalGroupAttribute attribute = fChildMember.GetAttribute<HorizontalGroupAttribute>();
                                    if (attribute != null)
                                    {
                                        HorizontalContainer horizontalGroup =
                                            foldout.GetChildren().Where(e => e is HorizontalContainer h && h.GetName() == attribute.Name).FirstOrDefault() as
                                                HorizontalContainer;
                                        if (horizontalGroup != null)
                                        {
                                            horizontalGroup.Add(fChild);
                                            foldout.Remove(k);
                                            goto SUB_INTERATION_1;
                                        }

                                        horizontalGroup = new HorizontalContainer(attribute.Name, new List<VisualEntity>() {fChild});
                                        foldout.SetChild(k, horizontalGroup);
                                        goto SUB_INTERATION_1;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (visualElement is FoldoutContainer foldout)
                {
                    INTERATION_4:
                    for (int j = 0; j < foldout.GetChildrenCount(); j++)
                    {
                        VisualEntity fChild = foldout.GetChild(j);
                        if (fChild is SerializedMember fChildMember)
                        {
                            HorizontalGroupAttribute attribute = fChildMember.GetAttribute<HorizontalGroupAttribute>();
                            if (attribute != null)
                            {
                                HorizontalContainer horizontalGroup =
                                    foldout.GetChildren().Where(e => e is HorizontalContainer h && h.GetName() == attribute.Name).FirstOrDefault() as HorizontalContainer;
                                if (horizontalGroup != null)
                                {
                                    horizontalGroup.Add(fChild);
                                    foldout.Remove(j);
                                    goto INTERATION_4;
                                }

                                horizontalGroup = new HorizontalContainer(attribute.Name, new List<VisualEntity>() {fChild});
                                foldout.SetChild(j, horizontalGroup);
                                goto INTERATION_4;
                            }
                        }
                    }
                }
            }
        }

        protected virtual void OnDisable()
        {
            if (target != null)
            {
                MethodInfo[] methods = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                IEnumerable<MethodInfo> onDisables = methods.Where(m => m.GetCustomAttribute(typeof(OnInspectorDisposeAttribute)) != null);
                foreach (MethodInfo onDisable in onDisables)
                {
                    onDisable.Invoke(target, null);
                }
            }
        }

        public static int OrderComparison(VisualEntity lhs, VisualEntity rhs)
        {
            if (lhs is SerializedMember lhsMember && rhs is SerializedMember rhsMember)
            {
                OrderAttribute lhsOrderAttribute = lhsMember.GetAttribute<OrderAttribute>();
                OrderAttribute rhsOrderAttribute = rhsMember.GetAttribute<OrderAttribute>();
                int lhsOrder = lhsOrderAttribute?.order ?? 0;
                int rhsOrder = rhsOrderAttribute?.order ?? 0;
                if (lhsOrder > rhsOrder)
                    return 1;
                else if (lhsOrder < rhsOrder)
                    return -1;
                else
                    return 0;
            }

            return 0;
        }


        public void CopyVisualElementsTo(ref List<VisualEntity> list) { list = new List<VisualEntity>(visualElements); }

        #region [Static Methods]

        /// <summary>
        /// Find all editor, which use inspector editor and rebuild it all.
        /// </summary>
        public static void RebuildAllInstances(params string[] except)
        {
            InspectorEditor[] editors = Resources.FindObjectsOfTypeAll<InspectorEditor>();
            for (int i = 0; i < editors.Length; i++)
            {
                InspectorEditor editor = editors[i];

                if (except.Any(e => e == editor.target.GetType().Name))
                    continue;

                editor.CreateVisualElements();
                editor.Repaint();
            }
        }

        /// <summary>
        /// Find all editor, which use inspector editor and repaint it all.
        /// </summary>
        public static void RepaintAllInstances()
        {
            InspectorEditor[] editors = Resources.FindObjectsOfTypeAll<InspectorEditor>();
            for (int i = 0; i < editors.Length; i++)
            {
                InspectorEditor editor = editors[i];
                editor.CreateVisualElements();
                editor.Repaint();
            }
        }

        /// <summary>
        /// Find all editor, which use EditorSettings and repaint it all.
        /// </summary>
        public static void RepaintAllInstances(params string[] except)
        {
            InspectorEditor[] editors = Resources.FindObjectsOfTypeAll<InspectorEditor>();
            for (int i = 0; i < editors.Length; i++)
            {
                InspectorEditor editor = editors[i];

                if (except.Any(e => e == editor.target.GetType().Name))
                    continue;

                editor.Repaint();
            }
        }

        #endregion

        #region [Getter / Setter]

        /// <summary>
        /// Set true to enable editor regardless of settings.
        /// </summary>
        public void KeepEnable(bool value) { keepEnable = value; }

        #endregion
    }
}