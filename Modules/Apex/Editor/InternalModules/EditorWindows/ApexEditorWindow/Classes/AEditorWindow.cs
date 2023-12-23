using Pancake.Apex;
using Pancake.ExLib.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public abstract class AEditorWindow : EditorWindow
    {
        private SerializedObject serializedObject;
        private List<VisualEntity> visualElements;
        private List<VisualEntity> searchedElements;
        private SearchField searchField;
        private Vector2 scrollPosition;
        private string searchText;
        private bool ignoreScriptReference;
        private bool isSearchableEditor;
        private bool isScrollable;
        private bool keepEnable;

        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);
            ignoreScriptReference = GetType().GetCustomAttribute<HideMonoScriptAttribute>() != null;
            isSearchableEditor = GetType().GetCustomAttribute<SearchableEditorAttribute>() != null;
            isScrollable = GetType().GetCustomAttribute<ScrollableWindowAttribute>() != null;
            searchText = string.Empty;
            searchField = new SearchField();
            CreateVisualElements();
            CopyVisualElementsTo(ref searchedElements);
            OnInitialize();
        }

        /// <summary>
        /// Called once when window being opened.
        /// <br>Implement this method to make some initializations.</br>
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// Implement this function to make a custom GUI.
        /// </summary>
        public virtual void OnGUI()
        {
            EditorGUI.indentLevel = 0;
            if (isScrollable)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            {
                GUILayout.BeginVertical();
                GUILayout.Space(4);
                {
                    if (true || keepEnable)
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
                }
                GUILayout.Space(4);
                GUILayout.EndVertical();
            }
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
            if (isScrollable)
            {
                EditorGUILayout.EndScrollView();
            }
        }

        /// <summary>
        /// Create all apex properties, which required to draw Apex GUI.
        /// </summary>
        private void CreateVisualElements()
        {
            visualElements = new List<VisualEntity>();

            // Collect all serialized property.
            int count = 0;
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

            // Collect all buttons with [Button] attribute.
            foreach (MethodInfo methodInfo in GetType().AllMethods())
            {
                MethodButtonAttribute methodButtonAttribute = methodInfo.GetCustomAttribute<MethodButtonAttribute>();
                if (methodButtonAttribute != null)
                {
                    if (methodButtonAttribute is ButtonAttribute)
                    {
                        try
                        {
                            MethodButton button = new Button(serializedObject, methodInfo.Name) {Repaint = Repaint};

                            button.SetOrder(count++);
                            visualElements.Add(button);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(
                                $"Failed to create a method button <b>{methodInfo.Name}</b> of the {serializedObject.targetObject.GetType().Name} object. (Button has been ignored)\n<b><color=red>Exception: {ex.Message}</color></b>\n\nStacktrace:");
                        }
                    }
                    else if (methodButtonAttribute is RepeatButtonAttribute)
                    {
                        try
                        {
                            MethodButton button = new RepeatButton(serializedObject, methodInfo.Name) {Repaint = Repaint};

                            button.SetOrder(count++);
                            visualElements.Add(button);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(
                                $"Failed to create a method button <b>{methodInfo.Name}</b> of the {serializedObject.targetObject.GetType().Name} object. (Button has been ignored)\n<b><color=red>Exception: {ex.Message}</color></b>\n\nStacktrace:");
                        }
                    }
                    else if (methodButtonAttribute is ToggleButtonAttribute)
                    {
                        try
                        {
                            MethodButton button = new ToggleButton(serializedObject, methodInfo.Name) {Repaint = Repaint};

                            button.SetOrder(count++);
                            visualElements.Add(button);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(
                                $"Failed to create a method button <b>{methodInfo.Name}</b> of the {serializedObject.targetObject.GetType().Name} object. (Button has been ignored)\n<b><color=red>Exception: {ex.Message}</color></b>\n\nStacktrace:");
                        }
                    }
                }
            }

            // Collect all fields with [SerializeCsProperty] attribute.
            foreach (PropertyInfo propertyInfo in GetType().AllProperties())
            {
                SerializePropertyAttribute attribute = propertyInfo.GetCustomAttribute<SerializePropertyAttribute>();
                if (attribute != null)
                {
                    SerializedCsProperty serializedCsProperty = new SerializedCsProperty(serializedObject, propertyInfo.Name) {Repaint = Repaint};

                    serializedCsProperty.SetOrder(count++);
                    visualElements.Add(serializedCsProperty);
                }
            }

            visualElements.Sort();

            // Layout all visual entities.
            LayoutBoxGroups();
            LayoutTabGroups();
            LayoutFoldout();
            LayoutHorizontalGroup();
        }

        /// <summary>
        /// Default implementation of apex GUI.
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

                if (serializedObject.hasModifiedProperties)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        /// <summary>
        /// Default implementation of searchable apex GUI.
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

                if (serializedObject.hasModifiedProperties)
                {
                    serializedObject.ApplyModifiedProperties();
                }
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

            Rect backgroundPosition = new Rect(reservePosition.xMin - 4, reservePosition.y - 4, reservePosition.width + 8, reservePosition.height);
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
                            boxGroup.AddEntity(visualElement);
                            visualElements.RemoveAt(i);
                            goto INTERATION;
                        }

                        boxGroup = new GroupContainer(attribute.Name, new List<VisualEntity>() {visualElement}) {Repaint = Repaint};
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
                            tabGroup.AddEntity(attribute.Key, visualElement);
                            visualElements.RemoveAt(i);
                            goto INTERATION_1;
                        }

                        tabGroup = new TabContainer(attribute.Name, new List<TabContainer.Tab>()) {Repaint = Repaint};
                        tabGroup.AddEntity(attribute.Key, visualElement);
                        visualElements[i] = tabGroup;
                        goto INTERATION_1;
                    }
                }
                else if (visualElement is GroupContainer boxGroup)
                {
                    INTERATION_2:
                    for (int j = 0; j < boxGroup.GetEntityCount(); j++)
                    {
                        VisualEntity child = boxGroup.GetEntity(j);
                        if (child is SerializedMember childMember)
                        {
                            TabGroupAttribute attribute = childMember.GetAttribute<TabGroupAttribute>();
                            if (attribute != null)
                            {
                                TabContainer tabGroup =
                                    boxGroup.GetEntities().Where(e => e is TabContainer tg && tg.GetName() == attribute.Name).FirstOrDefault() as TabContainer;
                                if (tabGroup != null)
                                {
                                    tabGroup.AddEntity(attribute.Key, child);
                                    boxGroup.RemoveEntity(j);
                                    goto INTERATION_2;
                                }

                                tabGroup = new TabContainer(attribute.Name, new List<TabContainer.Tab>()) {Repaint = Repaint};
                                tabGroup.AddEntity(attribute.Key, child);
                                boxGroup.SetEntity(j, tabGroup);
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
                            foldout.AddEntity(visualElement);
                            visualElements.RemoveAt(i);
                            goto INTERATION_1;
                        }

                        foldout = new FoldoutContainer(attribute.Name, attribute.Style, new List<VisualEntity>() {visualElement}) {Repaint = Repaint};
                        visualElements[i] = foldout;
                        goto INTERATION_1;
                    }
                }
                else if (visualElement is TabContainer tabGroup)
                {
                    INTERATION_2:
                    foreach (TabContainer.Tab pair in tabGroup.GetTabs())
                    {
                        for (int j = 0; j < pair.entities.Count; j++)
                        {
                            VisualEntity child = pair.entities[j];
                            if (child is SerializedMember childMember)
                            {
                                FoldoutAttribute attribute = childMember.GetAttribute<FoldoutAttribute>();
                                if (attribute != null)
                                {
                                    FoldoutContainer foldout =
                                        tabGroup.GetTabs()
                                            .Where(d => d.name == pair.name)
                                            .SelectMany(m => m.entities)
                                            .Where(e => e is FoldoutContainer f && f.GetName() == attribute.Name)
                                            .FirstOrDefault() as FoldoutContainer;
                                    if (foldout != null)
                                    {
                                        foldout.AddEntity(child);
                                        pair.entities.RemoveAt(j);
                                        goto INTERATION_2;
                                    }

                                    foldout = new FoldoutContainer(attribute.Name, attribute.Style, new List<VisualEntity>() {child}) {Repaint = Repaint};
                                    pair.entities[j] = foldout;
                                    goto INTERATION_2;
                                }
                            }
                        }
                    }
                }
                else if (visualElement is GroupContainer boxGroup)
                {
                    INTERATION_3:
                    for (int k = 0; k < boxGroup.GetEntityCount(); k++)
                    {
                        VisualEntity child = boxGroup.GetEntity(k);
                        if (child is SerializedMember childMember)
                        {
                            FoldoutAttribute attribute = childMember.GetAttribute<FoldoutAttribute>();
                            if (attribute != null)
                            {
                                FoldoutContainer foldout =
                                    boxGroup.GetEntities().Where(e => e is FoldoutContainer f && f.GetName() == attribute.Name).FirstOrDefault() as FoldoutContainer;
                                if (foldout != null)
                                {
                                    foldout.AddEntity(child);
                                    boxGroup.RemoveEntity(k);
                                    goto INTERATION_3;
                                }

                                foldout = new FoldoutContainer(attribute.Name, attribute.Style, new List<VisualEntity>() {child}) {Repaint = Repaint};
                                boxGroup.SetEntity(k, foldout);
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
                            horizontalGroup.AddEntity(visualElement);
                            visualElements.RemoveAt(i);
                            goto INTERATION_1;
                        }

                        horizontalGroup = new HorizontalContainer(attribute.Name, new List<VisualEntity>() {visualElement}) {Repaint = Repaint};
                        visualElements[i] = horizontalGroup;
                        goto INTERATION_1;
                    }
                }
                else if (visualElement is TabContainer tabGroup)
                {
                    INTERATION_2:
                    foreach (TabContainer.Tab pair in tabGroup.GetTabs())
                    {
                        for (int j = 0; j < pair.entities.Count; j++)
                        {
                            VisualEntity child = pair.entities[j];
                            if (child is SerializedMember childMember)
                            {
                                HorizontalGroupAttribute attribute = childMember.GetAttribute<HorizontalGroupAttribute>();
                                if (attribute != null)
                                {
                                    HorizontalContainer horizontalGroup =
                                        tabGroup.GetTabs()
                                            .Where(d => d.name == pair.name)
                                            .SelectMany(m => m.entities)
                                            .Where(e => e is HorizontalContainer h && h.GetName() == attribute.Name)
                                            .FirstOrDefault() as HorizontalContainer;
                                    if (horizontalGroup != null)
                                    {
                                        horizontalGroup.AddEntity(child);
                                        pair.entities.RemoveAt(j);
                                        goto INTERATION_2;
                                    }

                                    horizontalGroup = new HorizontalContainer(attribute.Name, new List<VisualEntity>() {child}) {Repaint = Repaint};
                                    pair.entities[j] = horizontalGroup;
                                    goto INTERATION_2;
                                }
                            }
                            else if (child is FoldoutContainer foldout)
                            {
                                SUB_INTERATION_1:
                                for (int k = 0; k < foldout.GetEntityCount(); k++)
                                {
                                    VisualEntity fChild = foldout.GetEntity(k);
                                    if (fChild is SerializedMember fChildMember)
                                    {
                                        HorizontalGroupAttribute attribute = fChildMember.GetAttribute<HorizontalGroupAttribute>();
                                        if (attribute != null)
                                        {
                                            HorizontalContainer horizontalGroup =
                                                foldout.GetEntities().Where(e => e is HorizontalContainer h && h.GetName() == attribute.Name).FirstOrDefault() as
                                                    HorizontalContainer;
                                            if (horizontalGroup != null)
                                            {
                                                horizontalGroup.AddEntity(fChild);
                                                foldout.RemoveEntity(k);
                                                goto SUB_INTERATION_1;
                                            }

                                            horizontalGroup = new HorizontalContainer(attribute.Name, new List<VisualEntity>() {fChild}) {Repaint = Repaint};
                                            foldout.SetEntity(k, horizontalGroup);
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
                    for (int j = 0; j < boxGroup.GetEntityCount(); j++)
                    {
                        VisualEntity bgChild = boxGroup.GetEntity(j);
                        if (bgChild is SerializedMember bgChildMember)
                        {
                            HorizontalGroupAttribute attribute = bgChildMember.GetAttribute<HorizontalGroupAttribute>();
                            if (attribute != null)
                            {
                                HorizontalContainer horizontalGroup =
                                    boxGroup.GetEntities()
                                        .Where(e => e is HorizontalContainer h && h.GetName() == attribute.Name)
                                        .FirstOrDefault() as HorizontalContainer;
                                if (horizontalGroup != null)
                                {
                                    horizontalGroup.AddEntity(bgChild);
                                    boxGroup.RemoveEntity(j);
                                    goto INTERATION_3;
                                }

                                horizontalGroup = new HorizontalContainer(attribute.Name, new List<VisualEntity>() {bgChild}) {Repaint = Repaint};
                                boxGroup.SetEntity(j, horizontalGroup);
                                goto INTERATION_3;
                            }
                        }
                        else if (bgChild is FoldoutContainer foldout)
                        {
                            SUB_INTERATION_1:
                            for (int k = 0; k < foldout.GetEntityCount(); k++)
                            {
                                VisualEntity fChild = foldout.GetEntity(k);
                                if (fChild is SerializedMember fChildMember)
                                {
                                    HorizontalGroupAttribute attribute = fChildMember.GetAttribute<HorizontalGroupAttribute>();
                                    if (attribute != null)
                                    {
                                        HorizontalContainer horizontalGroup =
                                            foldout.GetEntities().Where(e => e is HorizontalContainer h && h.GetName() == attribute.Name).FirstOrDefault() as
                                                HorizontalContainer;
                                        if (horizontalGroup != null)
                                        {
                                            horizontalGroup.AddEntity(fChild);
                                            foldout.RemoveEntity(k);
                                            goto SUB_INTERATION_1;
                                        }

                                        horizontalGroup = new HorizontalContainer(attribute.Name, new List<VisualEntity>() {fChild}) {Repaint = Repaint};
                                        foldout.SetEntity(k, horizontalGroup);
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
                    for (int j = 0; j < foldout.GetEntityCount(); j++)
                    {
                        VisualEntity fChild = foldout.GetEntity(j);
                        if (fChild is SerializedMember fChildMember)
                        {
                            HorizontalGroupAttribute attribute = fChildMember.GetAttribute<HorizontalGroupAttribute>();
                            if (attribute != null)
                            {
                                HorizontalContainer horizontalGroup =
                                    foldout.GetEntities().Where(e => e is HorizontalContainer h && h.GetName() == attribute.Name).FirstOrDefault() as HorizontalContainer;
                                if (horizontalGroup != null)
                                {
                                    horizontalGroup.AddEntity(fChild);
                                    foldout.RemoveEntity(j);
                                    goto INTERATION_4;
                                }

                                horizontalGroup = new HorizontalContainer(attribute.Name, new List<VisualEntity>() {fChild}) {Repaint = Repaint};
                                foldout.SetEntity(j, horizontalGroup);
                                goto INTERATION_4;
                            }
                        }
                    }
                }
            }
        }

        public void CopyVisualElementsTo(ref List<VisualEntity> list) { list = new List<VisualEntity>(visualElements); }

        public IEnumerable<VisualEntity> Entities { get { return visualElements; } }

        #region [Getter / Setter]

        /// <summary>
        /// Set true to enable Apex editor regardless of Apex settings.
        /// </summary>
        public void KeepEnable(bool value) { keepEnable = value; }

        #endregion
    }
}