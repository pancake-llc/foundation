using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pancake.Database
{
    public class GroupFoldableButton : VisualElement, IGroupButton
    {
        public static GroupFoldableButton CurrentSelected;
        public Foldout FoldoutElement;
        public VisualElement FoldoutCheckmark;
        public Toggle FoldoutToggle;

        public Image PrefixElement;
        public Button ButtonElement;

        public string Title { get => name; set => name = value; }

        public IGroup Group { get; set; }
        public VisualElement MainElement { get; set; }
        public VisualElement InternalElement { get; set; }
        protected bool IsCustomGroup;

        private static Color ColorInactive => EditorGUIUtility.isProSkin ? DarkInactive : LightInactive;
        private static Color ColorActive => EditorGUIUtility.isProSkin ? DarkActive : LightActive;

        // private
        private static readonly Color LightInactive = new Color(0.9f, 0.9f, 0.9f);
        private static readonly Color LightActive = new Color(0.24f, 0.37f, 0.58f);
        private static readonly Color DarkInactive = new Color(0.21f, 0.21f, 0.21f);
        private static readonly Color DarkActive = new Color(0.24f, 0.37f, 0.58f);
        private static readonly Color BumperColor = new Color(0.7f, 0.3f, 0.3f);

        public GroupFoldableButton(IGroup group, Texture icon, bool isCustomGroup)
        {
            name = group.Title;
            viewDataKey = group.Title;
            MainElement = this;
            Group = group;
            IsCustomGroup = isCustomGroup;

            //MainElement.style.left = 3;
            MainElement.style.flexDirection = FlexDirection.Column;
            MainElement.style.alignItems = Align.Stretch;
            MainElement.style.justifyContent = Justify.FlexStart;

            ButtonElement = new Button();
            ButtonElement.text = group.Title;
            ButtonElement.clicked += SetAsCurrent;
            ButtonElement.style.unityTextAlign = TextAnchor.MiddleLeft;
            ButtonElement.style.unityFontStyleAndWeight = FontStyle.Normal;
            ButtonElement.style.flexGrow = 1;
            ButtonElement.style.position = Position.Absolute;
            ButtonElement.style.left = 32;
            ButtonElement.style.width = 350;
            ButtonElement.style.height = 18;

            ButtonElement.style.backgroundColor = ColorInactive;
            ButtonElement.style.borderTopWidth = 0;
            ButtonElement.style.borderBottomWidth = 0;
            ButtonElement.style.borderLeftWidth = 0;
            ButtonElement.style.borderRightWidth = 0;
            ButtonElement.style.borderBottomLeftRadius = 2;
            ButtonElement.style.borderBottomRightRadius = 2;
            ButtonElement.style.borderTopLeftRadius = 2;
            ButtonElement.style.borderTopRightRadius = 2;

            ButtonElement.style.borderTopColor = BumperColor;
            ButtonElement.style.borderBottomColor = BumperColor;
            ButtonElement.style.borderRightColor = BumperColor;
            ButtonElement.style.borderLeftColor = BumperColor;

            FoldoutElement = new Foldout();
            FoldoutElement.style.width = 20;
            FoldoutElement.style.flexGrow = 0;
            FoldoutElement.text = string.Empty;
            FoldoutElement.contentContainer.style.marginLeft = 10;
            FoldoutCheckmark = FoldoutElement.Q<VisualElement>("unity-checkmark");
            FoldoutToggle = FoldoutElement.Q<Toggle>();
            FoldoutToggle.style.marginLeft = 0;

            PrefixElement = new Image();
            PrefixElement.image = icon;
            PrefixElement.style.width = 16;
            PrefixElement.style.height = 16;
            PrefixElement.style.justifyContent = Justify.Center;
            PrefixElement.style.alignSelf = Align.Center;
            PrefixElement.style.unityOverflowClipBox = StyleKeyword.None;
            PrefixElement.style.position = Position.Absolute;
            PrefixElement.style.left = 16;
            PrefixElement.style.top = 2;
            PrefixElement.style.unityTextAlign = TextAnchor.MiddleLeft;

            this.Add(PrefixElement);
            this.Add(ButtonElement);
            this.Add(FoldoutElement);

            InternalElement = FoldoutElement;
            SetShowFoldout(false);
        }

        public virtual void SetIsHighlighted(bool state) { ButtonElement.style.borderLeftWidth = state ? 5 : 0; }
        public virtual void SetIsSelected(bool isActive) { ButtonElement.style.backgroundColor = isActive ? ColorActive : ColorInactive; }
        public void SetShowFoldout(bool show) { FoldoutCheckmark.style.width = show ? 20 : 0; }

        public virtual void SetAsCurrent()
        {
            if (CurrentSelected == this)
            {
                return;
            }

            CurrentSelected?.SetIsSelected(false);
            SetIsSelected(true);
            CurrentSelected = this;

            Dashboard.SetCurrentGroup(Group, IsCustomGroup);
        }
    }
}