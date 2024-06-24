using System.Collections.Generic;
using UnityEngine;

namespace PancakeEditor.Common
{
    public static class EditorResources
    {
        private static readonly Dictionary<string, Texture2D> TextureCached = new();

        private const string RELATIVE_PATH = "Editor/Textures";
        private const string RELATIVE_TEMPLATE_PATH = "Editor/Templates";

        public static Texture2D IconEvent => ProjectDatabase.FindAssetWithPath<Texture2D>("so_blue_event.png", RELATIVE_PATH);
        public static Texture2D IconEventListener => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_event_listener.png", RELATIVE_PATH);
        public static Texture2D IconList => ProjectDatabase.FindAssetWithPath<Texture2D>("so_blue_list.png", RELATIVE_PATH);
        public static Texture2D IconVariable => ProjectDatabase.FindAssetWithPath<Texture2D>("so_blue_variable.png", RELATIVE_PATH);
        public static Texture2D StarEmpty => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_star_empty.png", RELATIVE_PATH);
        public static Texture2D StarFull => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_star_full.png", RELATIVE_PATH);
        public static Texture2D IconAds => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_ads.png", RELATIVE_PATH);
        public static Texture2D IconIAP => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_iap.png", RELATIVE_PATH);
        public static Texture2D IconFirebase => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_firebase.png", RELATIVE_PATH);
        public static Texture2D IconAdjust => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_adjust.png", RELATIVE_PATH);
        public static Texture2D IconSetting => ProjectDatabase.FindAssetWithPath<Texture2D>("so_blue_setting.png", RELATIVE_PATH);
        public static Texture2D IconEditorSetting => ProjectDatabase.FindAssetWithPath<Texture2D>("so_dark_setting.png", RELATIVE_PATH);
        public static Texture2D IconSpine => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_spine.png", RELATIVE_PATH);
        public static Texture2D IconGameService => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_gameservice.png", RELATIVE_PATH);
        public static Texture2D IconLocalization => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_localization.png", RELATIVE_PATH);
        public static Texture2D IconGameObject => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_gameobject.png", RELATIVE_PATH);
        public static Texture2D IconUnity => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_unity.png", RELATIVE_PATH);
        public static Texture2D IconLevelSytem => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_levelsystem.png", RELATIVE_PATH);
        public static Texture2D IconPackage => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_package.png", RELATIVE_PATH);
        public static Texture2D IconPopup => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_popup.png", RELATIVE_PATH);
        public static Texture2D IconYellowAudioSource => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_yellow_audiosource.png", RELATIVE_PATH);

        public static Texture2D BoxContentDark
        {
            get
            {
                TextureCached.TryGetValue(nameof(BoxContentDark), out var tex);

                if (tex != null) return tex;
                tex = Editor.ConvertToTexture(
                    "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQBAMAAADt3eJSAAAAD1BMVEVEREQlJSUmJiYtLS04ODhTxJsAAAAAJklEQVQI12NQYIAAGIMZxmBiEIAwGKnOQFhhDKaBlLOiIBAImQAAdGYChuP2NCcAAAAASUVORK5CYII=");
                TextureCached[nameof(BoxContentDark)] = tex;
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
                TextureCached[nameof(BoxBackgroundDark)] = tex;
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
                TextureCached[nameof(EvenBackground)] = tex;
                return tex;
            }
        }

        public static Texture2D TreeMapCurrent
        {
            get
            {
                TextureCached.TryGetValue(nameof(TreeMapCurrent), out var tex);

                if (tex != null) return tex;
                tex = ProjectDatabase.FindAssetWithPath<Texture2D>("tree_map_current.png", RELATIVE_PATH);
                TextureCached[nameof(TreeMapCurrent)] = tex;
                return tex;
            }
        }

        public static Texture2D TreeMapLast
        {
            get
            {
                TextureCached.TryGetValue(nameof(TreeMapLast), out var tex);

                if (tex != null) return tex;
                tex = ProjectDatabase.FindAssetWithPath<Texture2D>("tree_map_last.png", RELATIVE_PATH);
                TextureCached[nameof(TreeMapLast)] = tex;
                return tex;
            }
        }

        public static Texture2D TreeMapLevel
        {
            get
            {
                TextureCached.TryGetValue(nameof(TreeMapLevel), out var tex);

                if (tex != null) return tex;
                tex = ProjectDatabase.FindAssetWithPath<Texture2D>("tree_map_level.png", RELATIVE_PATH);
                TextureCached[nameof(TreeMapLevel)] = tex;
                return tex;
            }
        }

        public static Texture2D TreeMapLevel4
        {
            get
            {
                TextureCached.TryGetValue(nameof(TreeMapLevel4), out var tex);

                if (tex != null) return tex;
                tex = ProjectDatabase.FindAssetWithPath<Texture2D>("tree_map_level4.png", RELATIVE_PATH);
                TextureCached[nameof(TreeMapLevel4)] = tex;
                return tex;
            }
        }

        public static Texture2D TreeMapLine
        {
            get
            {
                TextureCached.TryGetValue(nameof(TreeMapLine), out var tex);

                if (tex != null) return tex;
                tex = ProjectDatabase.FindAssetWithPath<Texture2D>("tree_map_line.png", RELATIVE_PATH);
                TextureCached[nameof(TreeMapLine)] = tex;
                return tex;
            }
        }

        public static Texture2D IconTrim => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_trim.png", RELATIVE_PATH);
        public static Texture2D IconCategoryLayout => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_category_layout.png", RELATIVE_PATH);
        public static Texture2D IconDelete => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_delete.png", RELATIVE_PATH);
        public static Texture2D IconDuplicate => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_duplicate.png", RELATIVE_PATH);
        public static Texture2D IconEdit => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_edit.png", RELATIVE_PATH);
        public static Texture2D IconPing => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_ping.png", RELATIVE_PATH);
        public static Texture2D IconCancel => ProjectDatabase.FindAssetWithPath<Texture2D>("icon_cancel.png", RELATIVE_PATH);

        public static Texture2D IconCopyComponent(string theme)
        {
            TextureCached.TryGetValue($"{nameof(IconCopyComponent)}{theme}", out var tex);

            if (tex != null) return tex;
            tex = ProjectDatabase.FindAssetWithPath<Texture2D>(theme.Equals("DarkTheme") ? "icon_copy.png" : "icon_copy_dark.png", RELATIVE_PATH);
            TextureCached[$"{nameof(IconCopyComponent)}{theme}"] = tex;
            return tex;
        }

        public static Texture2D IconMoveDown(string theme)
        {
            TextureCached.TryGetValue($"{nameof(IconMoveDown)}{theme}", out var tex);

            if (tex != null) return tex;
            tex = ProjectDatabase.FindAssetWithPath<Texture2D>(theme.Equals("DarkTheme") ? "icon_arrow_down.png" : "icon_arrow_down_dark.png", RELATIVE_PATH);
            TextureCached[$"{nameof(IconMoveDown)}{theme}"] = tex;
            return tex;
        }

        public static Texture2D IconMoveUp(string theme)
        {
            TextureCached.TryGetValue($"{nameof(IconMoveUp)}{theme}", out var tex);

            if (tex != null) return tex;
            tex = ProjectDatabase.FindAssetWithPath<Texture2D>(theme.Equals("DarkTheme") ? "icon_arrow_up.png" : "icon_arrow_up_dark.png", RELATIVE_PATH);
            TextureCached[$"{nameof(IconMoveUp)}{theme}"] = tex;
            return tex;
        }

        public static Texture2D IconPasteComponentValues(string theme)
        {
            TextureCached.TryGetValue($"{nameof(IconPasteComponentValues)}{theme}", out var tex);

            if (tex != null) return tex;
            tex = ProjectDatabase.FindAssetWithPath<Texture2D>(theme.Equals("DarkTheme") ? "icon_paste.png" : "icon_paste_dark.png", RELATIVE_PATH);
            TextureCached[$"{nameof(IconPasteComponentValues)}{theme}"] = tex;
            return tex;
        }

        public static Texture2D IconRemoveComponent(string theme)
        {
            TextureCached.TryGetValue($"{nameof(IconRemoveComponent)}{theme}", out var tex);

            if (tex != null) return tex;
            tex = ProjectDatabase.FindAssetWithPath<Texture2D>(theme.Equals("DarkTheme") ? "icon_remove.png" : "icon_remove_dark.png", RELATIVE_PATH);
            TextureCached[$"{nameof(IconRemoveComponent)}{theme}"] = tex;
            return tex;
        }

        public static Texture2D IconReloadComponent(string theme)
        {
            TextureCached.TryGetValue($"{nameof(IconReloadComponent)}{theme}", out var tex);

            if (tex != null) return tex;
            tex = ProjectDatabase.FindAssetWithPath<Texture2D>(theme.Equals("DarkTheme") ? "icon_reload_csharp.png" : "icon_reload_csharp_dark.png", RELATIVE_PATH);
            TextureCached[$"{nameof(IconReloadComponent)}{theme}"] = tex;
            return tex;
        }

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