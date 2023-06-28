using System;
using System.Collections.Generic;
using Pancake.ExLibEditor;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
#if !UNITY_2020_1_OR_NEWER
using ClickEvent = UnityEngine.UIElements.MouseDownEvent;
#endif

namespace Pancake.BTagEditor
{
    public class BTagDropDownMenuWindow : EditorWindow
    {
        public Action<SerializedProperty, int, string> OnAddCategory;
        public Action<SerializedProperty, int, int, string> OnEditCategory;
        public Action<SerializedProperty, int, int, string> OnDeleteCategory;
        public Action<SerializedProperty, int> OnSelect;
        public Action<SerializedProperty, int> OnEdit;
        public Action<SerializedProperty, int> OnDuplicate;
        public Action<SerializedProperty, int> OnDelete;

        public int SelectedEntry = 0;
        public int WorkingSelectedEntry = 0;

        // Due to the unexpected way Array elements are drawn (one property drawer called multiple times) 
        // we need to pass back this property when invoking events
        public SerializedProperty RelatedProperty;

        List<(string label, int idx, bool editable)> menuEntries;
        List<TemplateContainer>[] categoriesPerEntry;
        Dictionary<(int depth, string key), TemplateContainer> createdCategories = new Dictionary<(int, string), TemplateContainer>();

        const string NewGroupLabel = "New Group";
        VisualTreeAsset menu;
        VisualTreeAsset menuItem;
        VisualTreeAsset menuCategory;
        StyleSheet menuItemUSS;
        ToolbarSearchField searchField;
        ScrollView menuScrollView;
        StyleEnum<DisplayStyle> visible = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        StyleEnum<DisplayStyle> hidden = new StyleEnum<DisplayStyle>(DisplayStyle.None);

        void OnEnable()
        {
            menu = ProjectDatabase.FindAssetWithPath<VisualTreeAsset>("BTagMenuUI.uxml", BTagEditorUtils.RELATIVE_PATH);
            menuCategory = ProjectDatabase.FindAssetWithPath<VisualTreeAsset>("BTagMenuCategoryUI.uxml", BTagEditorUtils.RELATIVE_PATH);
            menuItem = ProjectDatabase.FindAssetWithPath<VisualTreeAsset>("BTagMenuItemUI.uxml", BTagEditorUtils.RELATIVE_PATH);
            menuItemUSS = ProjectDatabase.FindAssetWithPath<StyleSheet>("BTagMenuItemUI.uss", BTagEditorUtils.RELATIVE_PATH);
        }

        // Invoked by the property drawer prior to instantiating this window as a popup
        public void Build(List<(string label, int idx, bool editable)> menuEntries, float windowWidth)
        {
            this.menuEntries = menuEntries;
            rootVisualElement.Clear();

            if (!EditorGUIUtility.isProSkin) rootVisualElement.AddToClassList("unity-light");
            var menuTree = menu.CloneTree();
            menuTree.styleSheets.Add(menuItemUSS);

            menuScrollView = menuTree.Q<ScrollView>();
            if (EditorGUIUtility.isProSkin) menuTree.AddToClassList("pro");

            // Create the search field and register listeners
            searchField = menuTree.Q<ToolbarSearchField>();
            searchField.Q<TextField>().delegatesFocus = true;
            searchField.RegisterCallback<KeyDownEvent>(HandleKeyDown);
            rootVisualElement.RegisterCallback<KeyDownEvent>(HandleKeyDown);
            searchField.RegisterValueChangedCallback(ce => SearchFieldChanged(ce.newValue, rootVisualElement));

            menuScrollView.Clear();
            createdCategories.Clear();
            categoriesPerEntry = new List<TemplateContainer>[this.menuEntries.Count];

            // First entry is always empty - 'None'
            var firstEntry = CreateNestedItem(this.menuEntries, 0, 0);
            menuScrollView.Add(firstEntry);
            VisualElement contentGroup = new VisualElement();

            contentGroup.style.maxWidth = windowWidth - 15f;
            contentGroup.AddToClassList("btag-group");
            menuScrollView.Add(contentGroup);
            // Add all the entries to the drop down menu except first & last
            for (int i = 1; i < this.menuEntries.Count - 1; ++i)
            {
                contentGroup.Add(CreateNestedItem(this.menuEntries, i, 0));
            }

            var separator = new VisualElement();
            separator.style.height = 1f;
            separator.style.backgroundColor = Color.gray;
            menuScrollView.Add(separator);

            var newGroupBtn = new Button();
            newGroupBtn.style.marginBottom = newGroupBtn.style.marginLeft = newGroupBtn.style.marginRight = newGroupBtn.style.marginTop = 10f;
            newGroupBtn.text = this.menuEntries[this.menuEntries.Count - 1].label;
            newGroupBtn.clicked += () => AddCategory(this.menuEntries[this.menuEntries.Count - 1].idx, NewGroupLabel);
            menuScrollView.Add(newGroupBtn);

            rootVisualElement.style.borderBottomColor = rootVisualElement.style.borderTopColor =
                rootVisualElement.style.borderLeftColor = rootVisualElement.style.borderRightColor = Color.black;
            rootVisualElement.style.borderBottomWidth =
                rootVisualElement.style.borderTopWidth = rootVisualElement.style.borderLeftWidth = rootVisualElement.style.borderRightWidth = 1f;
            rootVisualElement.style.marginBottom = rootVisualElement.style.marginTop = rootVisualElement.style.marginLeft = rootVisualElement.style.marginRight = 1f;
            rootVisualElement.Add(menuTree);
            SearchFieldChanged(searchField.value, rootVisualElement);
            searchField.Focus();
            rootVisualElement.schedule.Execute(() => searchField.Q<TextField>().Focus()).ExecuteLater(100);
        }

        private void HandleKeyDown(KeyDownEvent kde)
        {
            int currentMenuEntry = menuEntries.FindIndex(x => x.idx == (WorkingSelectedEntry >= 0 ? WorkingSelectedEntry : SelectedEntry));
            string lowerSearchString = searchField.value.ToLower();
            if (kde.keyCode == KeyCode.DownArrow)
            {
                while (currentMenuEntry < menuEntries.Count - 1)
                {
                    currentMenuEntry++;
                    if (string.IsNullOrWhiteSpace(lowerSearchString) || menuEntries[currentMenuEntry].label.ToLower().Contains(lowerSearchString)) break;
                }

                SelectedEntry = menuEntries[currentMenuEntry].idx;
                //SelectedEntry = menuEntries[ Mathf.Min(menuEntries.Count - 1, currentMenuEntry + 1)].idx;
                WorkingSelectedEntry = -1;
                UpdateSelectedEntry();
                kde.StopImmediatePropagation();
            }
            else if (kde.keyCode == KeyCode.UpArrow)
            {
                while (currentMenuEntry > 0)
                {
                    currentMenuEntry--;
                    if (string.IsNullOrWhiteSpace(lowerSearchString) || menuEntries[currentMenuEntry].label.ToLower().Contains(lowerSearchString)) break;
                }

                SelectedEntry = menuEntries[currentMenuEntry].idx;
                WorkingSelectedEntry = -1;
                UpdateSelectedEntry();
                kde.StopImmediatePropagation();
            }
            else if (kde.keyCode == KeyCode.Return || kde.keyCode == KeyCode.KeypadEnter)
            {
                Select(WorkingSelectedEntry >= 0 ? WorkingSelectedEntry : SelectedEntry);
            }
            else if (kde.keyCode == KeyCode.Escape)
            {
                Close();
            }
        }

        private VisualElement CreateNestedItem(List<(string label, int index, bool editable)> menuEntries, int idx, int depth)
        {
            int menuEntryIndex = menuEntries[idx].index;
            string label = menuEntries[idx].label;
            string[] origSplitLabels = label.Split('/');
            string[] splitLabels = origSplitLabels.Clone() as string[];
            string thisLabel = splitLabels[depth];
            if (string.IsNullOrEmpty(thisLabel)) thisLabel = "--";
            for (int i = 1; i < splitLabels.Length; ++i) splitLabels[i] = splitLabels[i - 1] + "/" + splitLabels[i];
            if (depth < splitLabels.Length - 1)
            {
                string key = splitLabels[depth];

                TemplateContainer existingCategory = null;
                bool alreadyExists = createdCategories.TryGetValue((0, splitLabels[0]), out existingCategory);
                TemplateContainer category;
                Foldout itemFoldout = default;
                if (alreadyExists)
                {
                    for (int i = 1; i <= depth; ++i)
                    {
                        TemplateContainer fetchedCatgeory = null;
                        if (!createdCategories.TryGetValue((i, splitLabels[i]), out fetchedCatgeory) || !existingCategory.Contains(fetchedCatgeory))
                        {
                            alreadyExists = false;
                            break;
                        }

                        existingCategory = fetchedCatgeory;
                    }
                }

                if (alreadyExists)
                {
                    category = createdCategories[(depth, key)];
                    itemFoldout = category.Q<Foldout>();
                    if (origSplitLabels.Length > (depth + 1) && !string.IsNullOrEmpty(origSplitLabels[depth + 1]))
                    {
                        var subItem = CreateNestedItem(menuEntries, idx, depth + 1);
                        itemFoldout.Add(subItem);
                    }
                }
                else
                {
                    category = menuCategory.CloneTree();
                    category.tooltip = splitLabels[depth];
                    itemFoldout = category.Q<Foldout>();
                    itemFoldout.text = thisLabel;
                    category.style.paddingLeft = depth > 0 ? 15 : 0;
                    category.Q<Button>("create").tooltip = "Add new entry to group: " + splitLabels[depth];
                    category.Q<Button>("edit").tooltip = "Rename group: " + splitLabels[depth];
                    category.Q<Button>("delete").tooltip = "Delete group: " + splitLabels[depth];

#if UNITY_2020_1_OR_NEWER
                    category.Q<Button>("create")
                        .RegisterCallback<ClickEvent>(ce =>
                        {
                            ce.StopPropagation();
                            AddCategory(menuEntryIndex, thisLabel);
                        });
                    category.Q<Button>("edit")
                        .RegisterCallback<ClickEvent>(ce =>
                        {
                            ce.StopPropagation();
                            EditCategory(menuEntryIndex, depth, thisLabel);
                        });
                    category.Q<Button>("delete")
                        .RegisterCallback<ClickEvent>(ce =>
                        {
                            ce.StopPropagation();
                            DeleteCategory(menuEntryIndex, depth, thisLabel);
                        });
#else
                    category.Q<Button>("create").clicked += () => { AddCategory(menuEntryIndex, depth == 0 ? string.Empty : key.Substring(key.IndexOf("/")+1)); };
                    category.Q<Button>("edit").clicked += () => { EditCategory(menuEntryIndex, depth, thisLabel); };
                    category.Q<Button>("delete").clicked += () => { DeleteCategory(menuEntryIndex, depth, thisLabel); };
#endif
                    if (origSplitLabels.Length > (depth + 1) && !string.IsNullOrEmpty(origSplitLabels[depth + 1]))
                    {
                        itemFoldout.Add(CreateNestedItem(menuEntries, idx, depth + 1));
                    }

                    int thisDepth = depth;
                    category.RegisterCallback<ClickEvent>(ce =>
                    {
                        if (ce.button == (int) MouseButton.RightMouse && menuEntries[idx].editable)
                        {
                            ShowRightClickMenu(ce, menuEntryIndex, thisLabel, thisDepth);
                            ce.StopPropagation();
                        }
                    });

                    // Close all sub-items that aren't this one
                    itemFoldout.RegisterValueChangedCallback(ce =>
                    {
                        if (ce.newValue)
                        {
                            var itemParent = category.parent;
                            for (int c = 0; c < itemParent.childCount; ++c)
                            {
                                var foldout = itemParent.ElementAt(c).Q<Foldout>();
                                if (foldout != null)
                                {
                                    foldout.SetValueWithoutNotify(false);
                                    foldout.parent.RemoveFromClassList("open");
                                }
                            }

                            itemFoldout.parent.AddToClassList("open");
                            itemFoldout.SetValueWithoutNotify(true);
                            menuScrollView.schedule.Execute(() =>
                                {
                                    if (menuScrollView.verticalScroller.enabledSelf)
                                    {
                                        var pos = menuScrollView.scrollOffset;
                                        pos.y += itemFoldout.worldBound.position.y - 30f;
                                        menuScrollView.scrollOffset = pos;
                                    }
                                })
                                .ExecuteLater(50);
                        }
                        else
                        {
                            itemFoldout.parent.RemoveFromClassList("open");
                        }

                        ce.StopPropagation();
                    });

                    if (createdCategories.ContainsKey((depth, key)))
                    {
                        Debug.LogAssertion("Already contains " + key + " for level " + depth);
                    }

                    ;
                    createdCategories.Add((depth, key), category);
                }

                if (categoriesPerEntry[idx] == null) categoriesPerEntry[idx] = new List<TemplateContainer>();
                if (!categoriesPerEntry[idx].Contains(category)) categoriesPerEntry[idx].Add(category);

                if (SelectedEntry == menuEntryIndex)
                {
                    itemFoldout.value = true;
                    itemFoldout.parent.AddToClassList("open");
                    rootVisualElement.schedule.Execute(() =>
                        {
                            if (menuScrollView.verticalScroller.enabledSelf)
                            {
                                var pos = menuScrollView.scrollOffset;
                                pos.y += itemFoldout.worldBound.position.y - 30f;
                                menuScrollView.scrollOffset = pos;
                            }
                        })
                        .ExecuteLater(50);
                }

                return category;
            }
            else
            {
                var item = menuItem.CloneTree();
                item.tooltip = splitLabels[depth];
#if UNITY_2020_1_OR_NEWER
                item.Q<Label>().text = thisLabel;
#else
                // Label padding doesn't appear to work for 2019
                item.Q<Label>().text = "      " + thisLabel;
#endif
                if (SelectedEntry == menuEntryIndex) item.Q<Label>().AddToClassList("selected");

                item.RegisterCallback<ClickEvent>(ce =>
                {
                    ce.StopPropagation();
                    if (ce.button == (int) MouseButton.LeftMouse)
                    {
                        Select(menuEntryIndex);
                    }
                    else if (ce.button == (int) MouseButton.RightMouse && menuEntries[idx].editable)
                    {
                        ShowRightClickMenu(ce, menuEntryIndex, thisLabel, -1);
                    }
                });

#if UNITY_2020_1_OR_NEWER
                item.Q<Button>("edit")
                    .RegisterCallback<ClickEvent>(ce =>
                    {
                        ce.StopPropagation();
                        Edit(menuEntryIndex);
                    });
                item.Q<Button>("duplicate")
                    .RegisterCallback<ClickEvent>(ce =>
                    {
                        ce.StopPropagation();
                        Duplicate(menuEntryIndex);
                    });
                item.Q<Button>("delete")
                    .RegisterCallback<ClickEvent>(ce =>
                    {
                        ce.StopPropagation();
                        Delete(menuEntryIndex);
                    });
#else
                item.Q<Button>("edit").clicked += () => { Edit(menuEntryIndex); };
                item.Q<Button>("duplicate").clicked += () => { Duplicate(menuEntryIndex); };
                item.Q<Button>("delete").clicked += () => { Delete(menuEntryIndex); };
#endif
                item.Q<Button>("edit").tooltip = "Rename: " + splitLabels[depth];
                item.Q<Button>("duplicate").tooltip = "Duplicate: " + splitLabels[depth];
                item.Q<Button>("delete").tooltip = "Delete: " + splitLabels[depth];
                item.Q<Button>("edit").style.display = menuEntries[idx].editable ? DisplayStyle.Flex : DisplayStyle.None;
                item.Q<Button>("duplicate").style.display = menuEntries[idx].editable ? DisplayStyle.Flex : DisplayStyle.None;
                item.Q<Button>("delete").style.display = menuEntries[idx].editable ? DisplayStyle.Flex : DisplayStyle.None;

                if (categoriesPerEntry[idx] == null) categoriesPerEntry[idx] = new List<TemplateContainer>();
                if (!categoriesPerEntry[idx].Contains(item)) categoriesPerEntry[idx].Add(item);
                return item;
            }
        }

        private void UpdateSelectedEntry()
        {
            menuScrollView.Query<Foldout>()
                .ForEach(x =>
                {
                    x.parent.RemoveFromClassList("open");
                    x.value = false;
                });
            for (int c = 0; c < categoriesPerEntry.Length; ++c)
            {
                int menuEntryIndex = c == 0 ? 0 : menuEntries[c].idx;
                var cats = categoriesPerEntry[c];
                if (cats == null) continue;

                for (int catIdx = 0; catIdx < cats.Count; ++catIdx)
                {
                    var itemFoldout = cats[catIdx].Q<Foldout>();
                    if (itemFoldout != null)
                    {
                        if ((WorkingSelectedEntry >= 0 && WorkingSelectedEntry == menuEntryIndex) || (WorkingSelectedEntry < 0 && SelectedEntry == menuEntryIndex))
                        {
                            itemFoldout.value = true;
                            itemFoldout.parent.AddToClassList("open");
                            rootVisualElement.schedule.Execute(() =>
                                {
                                    if (menuScrollView.verticalScroller.enabledSelf)
                                    {
                                        var pos = menuScrollView.scrollOffset;
                                        pos.y += itemFoldout.worldBound.position.y - 30f;
                                        menuScrollView.scrollOffset = pos;
                                    }
                                })
                                .ExecuteLater(50);
                        }
                    }
                    else
                    {
                        bool isSelectedEntry = (WorkingSelectedEntry >= 0 && WorkingSelectedEntry == menuEntryIndex) ||
                                               (WorkingSelectedEntry < 0 && SelectedEntry == menuEntryIndex);
                        if (isSelectedEntry)
                        {
                            cats[catIdx].Q<Label>().AddToClassList("selected");
                        }
                        else
                        {
                            cats[catIdx].Q<Label>().RemoveFromClassList("selected");
                        }
                    }
                }
            }
        }

        internal void SearchFieldChanged(string searchString, VisualElement root)
        {
            bool showAll = string.IsNullOrWhiteSpace(searchString);
            string lowerSearchString = searchString.ToLower();
            if (showAll)
            {
                if (WorkingSelectedEntry >= 0)
                {
                    WorkingSelectedEntry = -1;
                    UpdateSelectedEntry();
                }

                root.Query<TemplateContainer>().ForEach(x => x.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex));
            }
            else
            {
                for (int i = 1; i < menuEntries.Count - 1; ++i)
                {
                    if (categoriesPerEntry[i] == null) continue;
                    for (var c = 0; c < categoriesPerEntry[i].Count; ++c)
                    {
                        categoriesPerEntry[i][c].style.display = hidden;
                    }
                }

                int shownCount = 0;
                int autoSelectIdx = -1;
                bool selectedEntryIsInResults = false;
                for (int i = 1; i < menuEntries.Count - 1; ++i)
                {
                    if (categoriesPerEntry[i] == null) continue;
                    bool shouldShow = menuEntries[i].label.ToLower().Contains(lowerSearchString);
                    if (shouldShow)
                    {
                        shownCount++;
                        autoSelectIdx = i;
                        if (menuEntries[i].idx == SelectedEntry) selectedEntryIsInResults = true;
                        for (var c = 0; c < categoriesPerEntry[i].Count; ++c)
                        {
                            categoriesPerEntry[i][c].style.display = visible;
                        }
                    }
                }

                WorkingSelectedEntry = -1;
                if (!selectedEntryIsInResults)
                {
                    WorkingSelectedEntry = autoSelectIdx >= 0 ? menuEntries[autoSelectIdx].idx : 0;
                }

                UpdateSelectedEntry();
            }
        }

        private void ShowRightClickMenu(ClickEvent evt, int menuEntryIndex, string lbl, int depth = -1)
        {
            var menu = new GenericMenu();
            var targetElement = evt.target as VisualElement;
            if (targetElement == null) return;

            if (depth >= 0)
            {
                // Category actions
                menu.AddItem(new GUIContent("Add Tag to Group"), false, () => AddCategory(menuEntryIndex, lbl));
                menu.AddItem(new GUIContent("Rename"), false, () => EditCategory(menuEntryIndex, depth, lbl));
                menu.AddItem(new GUIContent("Delete Group "), false, () => DeleteCategory(menuEntryIndex, depth, lbl));
            }
            else
            {
                // Individual Tag actions
                menu.AddItem(new GUIContent("Duplicate "), false, () => Duplicate(menuEntryIndex));
                menu.AddItem(new GUIContent("Rename "), false, () => Edit(menuEntryIndex));
                menu.AddItem(new GUIContent("Delete "), false, () => Delete(menuEntryIndex));
            }

#if UNITY_2020_1_OR_NEWER
            var menuPosition = new Vector2(targetElement.layout.xMin + evt.localPosition.x, evt.localPosition.y);
#else
            var menuPosition = new Vector2(targetElement.layout.xMin + evt.localMousePosition.x, evt.localMousePosition.y);
#endif
            menuPosition = targetElement.LocalToWorld(menuPosition);
            var menuRect = new Rect(menuPosition, Vector2.zero);

            menu.DropDown(menuRect);
        }

        private void AddCategory(int idx, string label) { OnAddCategory?.Invoke(RelatedProperty, idx, label); }
        private void EditCategory(int idx, int depth, string label) { OnEditCategory?.Invoke(RelatedProperty, idx, depth, label); }
        private void DeleteCategory(int idx, int depth, string label) { OnDeleteCategory?.Invoke(RelatedProperty, idx, depth, label); }
        private void Select(int idx) { OnSelect?.Invoke(RelatedProperty, idx); }
        private void Duplicate(int idx) { OnDuplicate?.Invoke(RelatedProperty, idx); }
        private void Delete(int idx) { OnDelete?.Invoke(RelatedProperty, idx); }
        private void Edit(int idx) { OnEdit?.Invoke(RelatedProperty, idx); }
    }
}