using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Common
{
    public static class EditorResources
    {
        private static readonly Dictionary<string, Texture2D> TextureCached = new();

        public static Texture2D BoxContentDark
        {
            get
            {
                TextureCached.TryGetValue(nameof(BoxContentDark), out var tex);

                if (tex != null) return tex;
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQBAMAAADt3eJSAAAAD1BMVEVEREQlJSUmJiYtLS04ODhTxJsAAAAAJklEQVQI12NQYIAAGIMZxmBiEIAwGKnOQFhhDKaBlLOiIBAImQAAdGYChuP2NCcAAAAASUVORK5CYII=");
                TextureCached.Add(nameof(BoxContentDark), tex);
                return tex;
            }
        }

        public static Texture2D BoxBackgroundDark
        {
            get
            {
                TextureCached.TryGetValue(nameof(BoxBackgroundDark), out var tex);

                if (tex != null) return tex;
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQBAMAAADt3eJSAAAAD1BMVEVEREQlJSUtLS0mJiY4ODhE6guiAAAAJ0lEQVQI12NwMhQEAmEVBiUGEABSBhAGM4MAhMFIdQbcCrilcGcAAP3gA3XIoRcnAAAAAElFTkSuQmCC");
                TextureCached.Add(nameof(BoxBackgroundDark), tex);
                return tex;
            }
        }

        public static Texture2D EvenBackground
        {
            get
            {
                TextureCached.TryGetValue(nameof(EvenBackground), out var tex);

                if (tex != null) return tex;
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFAQMAAAC3obSmAAAAA1BMVEVEREQ1TRdOAAAACklEQVQI12OAAwAACgABaQY5MgAAAABJRU5ErkJggg==");
                TextureCached.Add(nameof(EvenBackground), tex);
                return tex;
            }
        }

        public static Texture2D EvenBackgroundBlue
        {
            get
            {
                TextureCached.TryGetValue(nameof(EvenBackgroundBlue), out var tex);

                if (tex != null) return tex;
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFAQMAAAC3obSmAAAAA1BMVEUsXYfq5LfEAAAACklEQVQI12OAAwAACgABaQY5MgAAAABJRU5ErkJggg==");
                TextureCached.Add(nameof(EvenBackgroundBlue), tex);
                return tex;
            }
        }

        public static Texture2D EvenBackgroundDark
        {
            get
            {
                TextureCached.TryGetValue(nameof(EvenBackgroundDark), out var tex);

                if (tex != null) return tex;
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFAQMAAAC3obSmAAAAA1BMVEU3Nzeu5Mv0AAAACklEQVQI12OAAwAACgABaQY5MgAAAABJRU5ErkJggg==");
                TextureCached.Add(nameof(EvenBackgroundDark), tex);
                return tex;
            }
        }

        public static Texture2D ReorderableArrayNormal(string theme)
        {
            TextureCached.TryGetValue($"{nameof(ReorderableArrayNormal)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIBAMAAAA2IaO4AAAAD1BMVEUZGRlMTExFRUVPT09ISEjRaqlGAAAAIUlEQVQI12MAA2ZjYwMGRkFBAQjB4uLiwMCkpKQAIUAAADJQAphR8G2mAAAAAElFTkSuQmCC");
            }
            else
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIBAMAAAA2IaO4AAAAD1BMVEVVVVXT09PMzMzW1tbQ0NB/bCfkAAAAIUlEQVQI12MAA2ZjYwMGRkFBAQjB4uLiwMCkpKQAIUAAADJQAphR8G2mAAAAAElFTkSuQmCC");
            }

            TextureCached.Add($"{nameof(ReorderableArrayNormal)}{theme}", tex);
            return tex;
        }

        public static Texture2D ReorderableArrayEntryBackground(string theme)
        {
            TextureCached.TryGetValue($"{nameof(ReorderableArrayEntryBackground)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEUyMjIZGRmBil2CAAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }
            else
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEWxsbFVVVVKIpF1AAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }

            TextureCached.Add($"{nameof(ReorderableArrayEntryBackground)}{theme}", tex);
            return tex;
        }

        public static Texture2D ReorderableArrayPress(string theme)
        {
            TextureCached.TryGetValue($"{nameof(ReorderableArrayPress)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAgMAAAC5YVYYAAAADFBMVEUZGRlJYoFGX31DXHpK0zOCAAAAF0lEQVQI12MAAtEQENJaAUL2f4AIKAYAL9YE7Xs/oqYAAAAASUVORK5CYII=");
            }
            else
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIBAMAAAA2IaO4AAAAD1BMVEVVVVXx8fHq6ur09PTt7e2ZzNamAAAAIUlEQVQI12MAA2ZjYwMGRkFBAQjB4uLiwMCkpKQAIUAAADJQAphR8G2mAAAAAElFTkSuQmCC");
            }

            TextureCached.Add($"{nameof(ReorderableArrayPress)}{theme}", tex);
            return tex;
        }

        public static Texture2D ReorderableArrayHover(string theme)
        {
            TextureCached.TryGetValue($"{nameof(ReorderableArrayHover)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIBAMAAAA2IaO4AAAAD1BMVEUZGRlRUVFKSkpUVFRNTU320S0gAAAAIUlEQVQI12MAA2ZjYwMGRkFBAQjB4uLiwMCkpKQAIUAAADJQAphR8G2mAAAAAElFTkSuQmCC");
            }
            else
            {
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIBAMAAAA2IaO4AAAAD1BMVEVVVVXi4uLb29vl5eXe3t53x1gGAAAAIUlEQVQI12MAA2ZjYwMGRkFBAQjB4uLiwMCkpKQAIUAAADJQAphR8G2mAAAAAElFTkSuQmCC");
            }

            TextureCached.Add($"{nameof(ReorderableArrayHover)}{theme}", tex);
            return tex;
        }

        public static Texture2D ReorderableArrayEntryEven(string theme)
        {
            TextureCached.TryGetValue($"{nameof(ReorderableArrayEntryEven)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEU3NzcZGRkuebNzAAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }
            else
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEXDw8NVVVVO7OYHAAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }

            TextureCached.Add($"{nameof(ReorderableArrayEntryEven)}{theme}", tex);
            return tex;
        }

        public static Texture2D ReorderableArrayEntryFocus(string theme)
        {
            TextureCached.TryGetValue($"{nameof(ReorderableArrayEntryFocus)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEVGUFAZGRlj5AnwAAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }
            else
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEXS3NxVVVVsrHCjAAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }

            TextureCached.Add($"{nameof(ReorderableArrayEntryFocus)}{theme}", tex);
            return tex;
        }

        public static Texture2D ReorderableArrayEntryOdd(string theme)
        {
            TextureCached.TryGetValue($"{nameof(ReorderableArrayEntryOdd)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEUyMjIZGRmBil2CAAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }
            else
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEWxsbFVVVVKIpF1AAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }

            TextureCached.Add($"{nameof(ReorderableArrayEntryOdd)}{theme}", tex);
            return tex;
        }

        public static Texture2D ReorderableArrayEntryActive(string theme)
        {
            TextureCached.TryGetValue($"{nameof(ReorderableArrayEntryActive)}{theme}", out var tex);

            if (tex != null) return tex;
            if (theme.Equals("DarkTheme"))
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEU8RkYZGRlTj002AAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }
            else
            {
                tex = Editor.ConvertToTexture("iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEXI0tJVVVWRDpgMAAAAD0lEQVQI12P4z9CIBP8DACgwBQWtv/itAAAAAElFTkSuQmCC");
            }

            TextureCached.Add($"{nameof(ReorderableArrayEntryActive)}{theme}", tex);
            return tex;
        }


        private const string RELATIVE_PATH = "Modules/Apex/ExLib/Core/Editor/Misc/Icons";
        private const string RELATIVE_COMPONENT_HEADER_PATH = "Modules/Apex/ExLib/Core/Editor/Misc/Icons/ComponentHeader";
        private const string RELATIVE_REORDERABLE_ARRAY_PATH = "Modules/Apex/ExLib/Core/Editor/Misc/Icons/ReorderableArray";
        private const string RELATIVE_TEMPLATE_PATH = "Modules/Apex/ExLib/Core/Editor/Misc/Templates";
        
        public static Texture2D ScriptableEvent => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_event.png", RELATIVE_PATH);
        public static Texture2D ScriptableEventListener => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_event_listener.png", RELATIVE_PATH);
        public static Texture2D ScriptableList => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_list.png", RELATIVE_PATH);
        public static Texture2D ScriptableVariable => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_variable.png", RELATIVE_PATH);
        public static Texture2D Dreamblale => ProjectDatabase.FindAssetWithPath<Texture2D>("dreamblade.png", RELATIVE_PATH);
        public static Texture2D StarEmpty => ProjectDatabase.FindAssetWithPath<Texture2D>("star_empty.png", RELATIVE_PATH);
        public static Texture2D StarFull => ProjectDatabase.FindAssetWithPath<Texture2D>("star_full.png", RELATIVE_PATH);
        public static Texture2D ScriptableAd => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_ad.png", RELATIVE_PATH);
        public static Texture2D ScriptableIap => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_iap.png", RELATIVE_PATH);
        public static Texture2D ScriptableFirebase => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_firebase.png", RELATIVE_PATH);
        public static Texture2D ScriptableAdjust => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_adjust.png", RELATIVE_PATH);
        public static Texture2D ScriptableNotification => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_notification.png", RELATIVE_PATH);
        public static Texture2D ScriptableSetting => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_setting.png", RELATIVE_PATH);
        public static Texture2D ScriptableEditorSetting => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_editor_setting.png", RELATIVE_PATH);
        public static Texture2D ScriptableEditorSpine => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_editor_spine.png", RELATIVE_PATH);
        public static Texture2D ScriptableInterface => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_interface.png", RELATIVE_PATH);
        public static Texture2D ScriptableFactory => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_factory.png", RELATIVE_PATH);
        public static Texture2D ScriptableMesh => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_mesh.png", RELATIVE_PATH);
        public static Texture2D ScriptableLeaf => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_leaf.png", RELATIVE_PATH);
        public static Texture2D ScriptableUnity => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_unity.png", RELATIVE_PATH);
        public static Texture2D IconEyeOpen => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_eye_open.png", RELATIVE_PATH);
        public static Texture2D IconEyeClose => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_eye_close.png", RELATIVE_PATH);
        public static Texture2D TreeMapCurrent => ProjectDatabase.FindAssetWithPath<Texture2D>("tree_map_current.png", RELATIVE_PATH);
        public static Texture2D TreeMapLast => ProjectDatabase.FindAssetWithPath<Texture2D>("tree_map_last.png", RELATIVE_PATH);
        public static Texture2D TreeMapLevel => ProjectDatabase.FindAssetWithPath<Texture2D>("tree_map_level.png", RELATIVE_PATH);
        public static Texture2D TreeMapLevel4 => ProjectDatabase.FindAssetWithPath<Texture2D>("tree_map_level4.png", RELATIVE_PATH);
        public static Texture2D TreeMapLine => ProjectDatabase.FindAssetWithPath<Texture2D>("tree_map_line.png", RELATIVE_PATH);
        public static Texture2D IconTrim => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_trim.png", RELATIVE_PATH);
        public static Texture2D IconWarning => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_warning.png", RELATIVE_PATH);
        public static Texture2D IconCategoryLayout => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_category_layout.png", RELATIVE_PATH);
        public static Texture2D IconDelete => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_delete.png", RELATIVE_PATH);
        public static Texture2D IconDuplicate => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_duplicate.png", RELATIVE_PATH);
        public static Texture2D IconEdit => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_edit.png", RELATIVE_PATH);
        public static Texture2D IconPing => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_ping.png", RELATIVE_PATH);
        public static Texture2D IconCancel => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_cancel.png", RELATIVE_PATH);
        public static Texture2D IconCopyComponent => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_copy_component.png", RELATIVE_COMPONENT_HEADER_PATH);
        public static Texture2D IconCopyComponentDark => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_copy_component_dark.png", RELATIVE_COMPONENT_HEADER_PATH);
        public static Texture2D IconMoveDown => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_move_down.png", RELATIVE_COMPONENT_HEADER_PATH);
        public static Texture2D IconMoveDownDark => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_move_down_dark.png", RELATIVE_COMPONENT_HEADER_PATH);
        public static Texture2D IconMoveUp => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_move_up.png", RELATIVE_COMPONENT_HEADER_PATH);
        public static Texture2D IconMoveUpDark => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_move_up_dark.png", RELATIVE_COMPONENT_HEADER_PATH);

        public static Texture2D IconPasteComponentValues =>
            ProjectDatabase.FindAssetWithPath<Texture2D>("icon_paste_component_values.png", RELATIVE_COMPONENT_HEADER_PATH);

        public static Texture2D IconPasteComponentValuesDark =>
            ProjectDatabase.FindAssetWithPath<Texture2D>("icon_paste_component_values_dark.png", RELATIVE_COMPONENT_HEADER_PATH);

        public static Texture2D IconRemoveComponent => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_remove_component.png", RELATIVE_COMPONENT_HEADER_PATH);
        public static Texture2D IconRemoveComponentDark => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_remove_component_dark.png", RELATIVE_COMPONENT_HEADER_PATH);

        public static TextAsset ScriptableEventListenerTemplate =>
            ProjectDatabase.FindAssetWithPath<TextAsset>("ScriptableEventListenerTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);

        public static TextAsset ScreenViewTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("ScreenViewTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset ScreenPresenterTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("ScreenPresenterTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);

        public static TextAsset ScriptableEventTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("ScriptableEventTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset ScriptableListTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("ScriptableListTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset ScriptableVariableTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("ScriptableVariableTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset MonoBehaviourTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("MonoBehaviourTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset ClassTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("ClassTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset AndroidManifestTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("AndroidManifest.xml.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset MainGradleTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("mainTemplate.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset GradlePropertiesTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("gradleTemplate.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset GradleSettingsTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("settingsTemplate.txt", RELATIVE_TEMPLATE_PATH);
    }
}