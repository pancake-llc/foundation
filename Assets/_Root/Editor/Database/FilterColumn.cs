using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pancake.Database
{
    public class FilterColumn : GroupColumn
    {
        protected static ScrollView ScrollElement;
        public static Foldout CustomGroupsFoldout;
        public static Foldout DefaultGroupsFoldout;
        public List<CustomGroup> CustomGroups = new List<CustomGroup>();
        private List<IGroupButton> m_filteringMustShow = new List<IGroupButton>();

        public static Texture IconBoxGreen;
        public static Texture IconBoxBlue;
        public static Texture IconBoxWireframe;

        public override void Rebuild()
        {
            RebuildPrep();
            RebuildCustomGroups();
            RebuildStaticGroups();
            Dashboard.SetCurrentGroup(Dashboard.CurrentSelectedGroup);
            FilterBySearchBar();
        }

        private void RebuildPrep()
        {
            Clear();
            if (IconBoxGreen == null)
            {
                IconBoxGreen = EditorGUIUtility.IconContent("BoxCollider Icon").image;
                IconBoxBlue = EditorGUIUtility.IconContent("d_Prefab Icon").image;
                IconBoxWireframe = EditorGUIUtility.IconContent("d_GameObject Icon").image;
            }

            if (!isSubscribed)
            {
                Dashboard.onSearchGroups = FilterBySearchBar;
            }

            isSubscribed = true;
            allButtonsCache = new List<IGroupButton>();

            ScrollElement = new ScrollView();
            ScrollElement.style.flexGrow = 1;
            this.Add(ScrollElement);
        }

        private void RebuildCustomGroups()
        {
            CustomGroups = Builder.GetAllAssetsInProject<CustomGroup>();
            CustomGroupsFoldout = new Foldout {text = "Custom Groups"};
            CustomGroupsFoldout.contentContainer.style.marginLeft = -5;
            CustomGroups.Sort((x, y) => string.CompareOrdinal(x.Title, y.Title));

            ScrollElement.Add(CustomGroupsFoldout);

            foreach (CustomGroup g in CustomGroups)
            {
                GroupFoldableButton currentButton = new GroupFoldableButton(g, IconBoxWireframe, true);
                CustomGroupsFoldout.Add(currentButton.MainElement);

                UnityEditor.Editor ed = UnityEditor.Editor.CreateEditor((CustomGroup) currentButton.Group);
                SerializedObject so = ed.serializedObject;
                Button button = currentButton.MainElement.Q<Button>();
                button.bindingPath = "title";
                button.BindProperty(so);
            }
        }

        private void RebuildStaticGroups()
        {
            DefaultGroupsFoldout = new Foldout {text = "Static Groups"};
            DefaultGroupsFoldout.contentContainer.style.marginLeft = -5;
            //DefaultGroupsFoldout.style.marginLeft = 5;

            ScrollElement.Add(DefaultGroupsFoldout);

            foreach (StaticGroup group in Data.Database.GetAllStaticGroups())
            {
                GroupFoldableButton groupButton = new GroupFoldableButton(group, group.Type.IsAbstract ? IconBoxBlue : IconBoxGreen, false);
                allButtonsCache.Add(groupButton);
            }

            allButtonsCache.Sort((x, y) => string.CompareOrdinal(x.Title, y.Title));

            foreach (IGroupButton curButton in allButtonsCache)
            {
                // put any first tier classes directly into the foldout.
                if (curButton.Group.Type.BaseType == typeof(Entity) || curButton.Group.Type == typeof(Entity))
                {
                    DefaultGroupsFoldout.Add(curButton.MainElement);
                }
                else
                {
                    // find parent class button
                    IGroupButton targetParent = allButtonsCache.Find(otherButton => otherButton.Group.Type == curButton.Group.Type.BaseType);
                    if (targetParent == null) return;

                    targetParent.InternalElement.Add(curButton.MainElement);
                    targetParent.SetShowFoldout(true);
                }
            }
        }

        public override GroupFoldableButton SelectButtonByTitle(string title)
        {
            GroupFoldableButton button = ScrollElement.Q<GroupFoldableButton>(title);
            if (button == null) return null;

            ScrollTo(button);
            button.SetAsCurrent();
            return button;
        }

        public override void ScrollTo(VisualElement button) { ScrollElement.ScrollTo(button); }

        public override void Filter(string filter)
        {
            m_filteringMustShow.Clear();
            if (string.IsNullOrEmpty(filter))
            {
                foreach (IGroupButton button in allButtonsCache)
                {
                    button.SetIsHighlighted(false);
                }
            }
            else
            {
                foreach (IGroupButton button in allButtonsCache)
                {
                    // turn it off
                    button.SetIsHighlighted(false);

                    // if there's a name match, turn it back on
                    bool isNameMatch = button.Group.Type.Name.ToLower().Contains(filter.ToLower());
                    if (!isNameMatch) continue;

                    button.SetIsHighlighted(true);
                    FilterUpHierarchy(button);
                }

                foreach (IGroupButton button in m_filteringMustShow)
                {
                    button.SetIsHighlighted(true);
                }
            }
        }

        private void FilterUpHierarchy(IGroupButton button)
        {
            IGroupButton buttonParent = allButtonsCache.Find(x => x.Group.Type == button.Group.Type.BaseType);
            if (buttonParent != null)
            {
                m_filteringMustShow.Add(buttonParent);
                FilterUpHierarchy(buttonParent);
            }
        }

        public override void FilterBySearchBar() { Filter(Dashboard.groupSearch.value); }
    }
}