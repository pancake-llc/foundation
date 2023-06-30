using Pancake.Tag;
using Pancake.ExLibEditor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pancake.TagEditor
{
    //[CustomEditor(typeof(TagQuery), true)]
    public class TagQueryEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            Undo.undoRedoPerformed += ReBuild;
            return Build();
        }

        private void OnDestroy() => Undo.undoRedoPerformed -= ReBuild;

        VisualElement root = null;
        VisualTreeAsset queryUI;
        VisualTreeAsset queryTagUI;
        StyleEnum<DisplayStyle> visible = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        StyleEnum<DisplayStyle> hidden = new StyleEnum<DisplayStyle>(DisplayStyle.None);


        void ReBuild() => Build();

        VisualElement Build()
        {
            serializedObject.Update();
            if (root == null)
            {
                root = new VisualElement();
                root.style.flexGrow = 1f;
            }
            else
            {
                root.Clear();
            }


            queryUI = ProjectDatabase.FindAssetWithPath<VisualTreeAsset>("TagQueryUI.uxml", TagEditorUtils.RELATIVE_PATH);
            queryTagUI = ProjectDatabase.FindAssetWithPath<VisualTreeAsset>("TagQueryTagUI.uxml", TagEditorUtils.RELATIVE_PATH);
            var uxmlInstance = queryUI.CloneTree();
            uxmlInstance.style.flexGrow = 1f;
            root.Add(uxmlInstance);

            var orPanel = root.Q("Or");
            var andPanel = root.Q("And");
            var notPanel = root.Q("Not");
            var orPanelContent = orPanel.Q("DropRegion");
            var andPanelContent = andPanel.Q("DropRegion");
            var notPanelContent = notPanel.Q("DropRegion");
            var queriesPanel = root.Q("AdditionalQueries");

            orPanel.RegisterCallback<DragEnterEvent>(dee => HandleDragEnter(dee, orPanelContent, InclusionRule.Any, "tag-rule-any"));
            andPanel.RegisterCallback<DragEnterEvent>(dee => HandleDragEnter(dee, andPanelContent, InclusionRule.MustInclude, "tag-rule-include"));
            notPanel.RegisterCallback<DragEnterEvent>(dee => HandleDragEnter(dee, notPanelContent, InclusionRule.MustExclude, "tag-rule-exclude"));

            orPanel.RegisterCallback<DragLeaveEvent>(HandleDragLeave);
            andPanel.RegisterCallback<DragLeaveEvent>(HandleDragLeave);
            notPanel.RegisterCallback<DragLeaveEvent>(HandleDragLeave);
#if UNITY_2020_1_OR_NEWER
            andPanel.Q<Button>().RegisterCallback<ClickEvent>(_ => AddTagWithRule(InclusionRule.MustInclude));
            orPanel.Q<Button>().RegisterCallback<ClickEvent>(_ => AddTagWithRule(InclusionRule.Any));
            notPanel.Q<Button>().RegisterCallback<ClickEvent>(_ => AddTagWithRule(InclusionRule.MustExclude));
#else
            andPanel.Q<Button>().clicked += () => AddTagWithRule(InclusionRule.MustInclude);
            orPanel.Q<Button>().clicked += () => AddTagWithRule(InclusionRule.Any);
            notPanel.Q<Button>().clicked += () => AddTagWithRule(InclusionRule.MustExclude);
#endif

            var tq = target as TagQuery;
            uxmlInstance.RegisterCallback<MouseMoveEvent>(mde => OnMouseMove(mde.localMousePosition));
            uxmlInstance.RegisterCallback<DragUpdatedEvent>(mde => OnMouseMove(mde.localMousePosition));
            uxmlInstance.RegisterCallback<DragLeaveEvent>(_ => StopDragging());
            uxmlInstance.RegisterCallback<DragExitedEvent>(_ => StopDragging());
            uxmlInstance.RegisterCallback<MouseUpEvent>(_ => StopDragging());
            uxmlInstance.RegisterCallback<MouseDownEvent>(_ => StopEditing());
            uxmlInstance.RegisterCallback<DragPerformEvent>(_ => { ReBuild(); });

            for (int i = 0; i < tq.matchingTags.Length; ++i)
            {
                int idx = i;
                var item = queryTagUI.CloneTree(); // new VisualElement();
                item.userData = i;
                item.Q<Label>().text = tq.matchingTags[i].tag == null ? "Null" : tq.matchingTags[i].tag.name;

                item.RegisterCallback<FocusOutEvent>(foe => StopEditing());
                item.RegisterCallback<MouseDownEvent>(mde => StartDragging(mde, item, idx));
                item.Q<Button>("DeleteBtn").clicked += () => DeleteTag(idx);

                var underlyingProperty = new PropertyField(serializedObject.FindProperty("matchingTags").GetArrayElementAtIndex(i).FindPropertyRelative("tag"));
                underlyingProperty.name = "UnderlyingProp";
                item.Q("TagEdit").style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                item.Q("TagEdit").Add(underlyingProperty);
                if (tq.matchingTags[i].rule == InclusionRule.Any)
                {
                    orPanelContent.Add(item);
                    item.AddToClassList("tag-rule-any");
                }
                else if (tq.matchingTags[i].rule == InclusionRule.MustInclude)
                {
                    andPanelContent.Add(item);
                    item.AddToClassList("tag-rule-include");
                }
                else if (tq.matchingTags[i].rule == InclusionRule.MustExclude)
                {
                    notPanelContent.Add(item);
                    item.AddToClassList("tag-rule-exclude");
                }
            }

            uxmlInstance.Bind(serializedObject);

            // Hack to expand parent window so drag & drop isn't arbitrarily cut-off
            root.schedule.Execute(() =>
            {
                var ve = root.parent;
                while (ve != null && !(ve is ScrollView))
                {
                    ve.style.flexGrow = 1f;
                    ve = ve.parent;
                }

                if (ve is ScrollView) ve.Q("unity-content-container").style.flexGrow = 1f;
            });
            return root;
        }

        private void HandleDragLeave(DragLeaveEvent evt)
        {
            if (draggingItemRepresentation == null) return;
            draggingItem.style.opacity = 0.2f;
            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            draggingItemRepresentation.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            draggingItemRepresentation.style.position = new StyleEnum<Position>(Position.Absolute);
            draggingItemRepresentation.Q("Tag").RemoveFromClassList("tag-drop-valid");
            draggingItem.Q("Tag").RemoveFromClassList("tag-drop-valid");
            root.Add(draggingItemRepresentation);
        }

        private void HandleDragEnter(DragEnterEvent evt, VisualElement ve, InclusionRule containerRule, string ruleClass)
        {
            if (draggingItemRepresentation == null) return;
            if (ve == originalContainer)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                draggingItemRepresentation.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                draggingItem.Q("Tag").AddToClassList("tag-drop-valid");
                draggingItem.style.opacity = 1f;
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            }

            dragRule = containerRule;

            var tag = draggingItemRepresentation.Q("Tag");
            draggingItemRepresentation.RemoveFromClassList("tag-rule-include");
            draggingItemRepresentation.RemoveFromClassList("tag-rule-exclude");
            draggingItemRepresentation.RemoveFromClassList("tag-rule-any");
            draggingItemRepresentation.AddToClassList(ruleClass);
            tag.AddToClassList("tag-drop-valid");
            draggingItemRepresentation.style.position = new StyleEnum<Position>(Position.Relative);
            draggingItemRepresentation.transform.position = Vector3.zero;
            ve.Add(draggingItemRepresentation);
        }

        bool canDrag = false;
        int draggingIndex = -1;
        InclusionRule dragRule;
        TemplateContainer draggingItem = null;
        VisualElement originalContainer = null;
        VisualElement draggingItemRepresentation = null;
        TemplateContainer editingItem = null;

        private void StartDragging(MouseDownEvent mde, TemplateContainer item, int idx)
        {
            if (mde.clickCount > 1)
            {
                StopDragging();
                StartEditing(item);
            }
            else
            {
                canDrag = true;
                draggingIndex = idx;
                draggingItem = item;
                draggingItem.style.opacity = 0.2f;
                originalContainer = item.parent;
                DragAndDrop.PrepareStartDrag();
            }

            mde.StopImmediatePropagation();
        }

        private void StartEditing(TemplateContainer item)
        {
            if (editingItem != null)
            {
                var tq = (target as TagQuery);
                int idx = (int) editingItem.userData;
                editingItem.Q<Label>().text = (target as TagQuery).matchingTags[idx].tag == null ? "Null" : tq.matchingTags[idx].tag.name;
                editingItem.Q("Tag").style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                editingItem.Q("TagEdit").style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }

            editingItem = item;
            editingItem.Q("Tag").style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            editingItem.Q("TagEdit").style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        }

        private void StopEditing()
        {
            if (editingItem == null) return;
            editingItem.Q("Tag").style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            editingItem.Q("TagEdit").style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            editingItem = null;
            ReBuild();
        }


        private void OnMouseMove(Vector2 mouseLoc)
        {
            if (canDrag)
            {
                canDrag = false;
                draggingItemRepresentation = queryTagUI.CloneTree();
                draggingItemRepresentation.pickingMode = PickingMode.Ignore;
                draggingItemRepresentation.Q<Label>().text = draggingItem.Q<Label>().text;
                dragRule = (target as TagQuery).matchingTags[(int) draggingItem.userData].rule;
                //draggingItemRepresentation.style.opacity = 0.5f;
                root.Add(draggingItemRepresentation);
                Debug.Log("Started dragging");
                DragAndDrop.StartDrag("Dragging " + draggingItem);
                DragAndDrop.activeControlID = 1;
            }

            if (draggingItemRepresentation != null)
            {
                if (draggingItemRepresentation.parent == root || draggingItemRepresentation.parent == originalContainer)
                {
                    draggingItemRepresentation.style.position = new StyleEnum<Position>(Position.Absolute);
                    draggingItemRepresentation.transform.position = mouseLoc;
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                }
                else
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                }
            }
        }

        private void StopDragging()
        {
            canDrag = false;
            if (draggingItemRepresentation != null)
            {
                if (draggingItemRepresentation.parent == root || draggingItemRepresentation.parent == originalContainer)
                {
                }
                else
                {
                    var arr = (target as TagQuery).matchingTags;
                    if (draggingIndex >= 0 && draggingIndex < arr.Length)
                    {
                        serializedObject.Update();
                        serializedObject.FindProperty("matchingTags").GetArrayElementAtIndex(draggingIndex).FindPropertyRelative("rule").enumValueIndex = (int) dragRule;
                        serializedObject.ApplyModifiedProperties();
                    }

                    DragAndDrop.AcceptDrag();
                }

                draggingItemRepresentation.parent?.Remove(draggingItemRepresentation);
                draggingItemRepresentation = null;
            }

            if (draggingItem != null)
            {
                draggingItem.Q("Tag").RemoveFromClassList("tag-drop-valid");
                draggingItem.style.opacity = 1f;
            }

            draggingIndex = -1;
        }


        private void DeleteTag(int idx)
        {
            serializedObject.Update();
            Undo.RegisterCompleteObjectUndo(target, "Delete Tag");
            var numEntries = serializedObject.FindProperty("matchingTags").arraySize;
            if (numEntries > 1)
            {
                var lastElement = serializedObject.FindProperty("matchingTags").GetArrayElementAtIndex(numEntries - 1);
                serializedObject.FindProperty("matchingTags").GetArrayElementAtIndex(idx).FindPropertyRelative("tag").objectReferenceValue =
                    lastElement.FindPropertyRelative("tag").objectReferenceValue;
                serializedObject.FindProperty("matchingTags").GetArrayElementAtIndex(idx).FindPropertyRelative("rule").enumValueIndex =
                    lastElement.FindPropertyRelative("rule").enumValueIndex;
            }

            serializedObject.FindProperty("matchingTags").arraySize = numEntries - 1;
            serializedObject.ApplyModifiedProperties();
            ReBuild();
        }

        private void AddTagWithRule(InclusionRule rule)
        {
            serializedObject.Update();
            Undo.RegisterCompleteObjectUndo(target, "Add Tag");
            var defaultTag = TagPropertyDrawerBase<TagGroupBase, Tag.Tag>.FindDefault();

            var numEntries = serializedObject.FindProperty("matchingTags").arraySize;
            serializedObject.FindProperty("matchingTags").arraySize = numEntries + 1;
            serializedObject.FindProperty("matchingTags").GetArrayElementAtIndex(numEntries).FindPropertyRelative("tag").objectReferenceValue = defaultTag;
            serializedObject.FindProperty("matchingTags").GetArrayElementAtIndex(numEntries).FindPropertyRelative("rule").enumValueIndex = (int) rule;
            serializedObject.ApplyModifiedProperties();
            ReBuild();
        }
    }
}