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

        public static Texture2D IconTreeMapCurrent
        {
            get
            {
                TextureCached.TryGetValue(nameof(IconTreeMapCurrent), out var tex);

                if (tex != null) return tex;
                tex = ProjectDatabase.FindAssetWithPath<Texture2D>("icon_tree_map_current.png", RELATIVE_PATH);
                TextureCached[nameof(IconTreeMapCurrent)] = tex;
                return tex;
            }
        }

        public static Texture2D IconTreeMapLast
        {
            get
            {
                TextureCached.TryGetValue(nameof(IconTreeMapLast), out var tex);

                if (tex != null) return tex;
                tex = ProjectDatabase.FindAssetWithPath<Texture2D>("icon_tree_map_last.png", RELATIVE_PATH);
                TextureCached[nameof(IconTreeMapLast)] = tex;
                return tex;
            }
        }

        public static Texture2D IconTreeMapLevel
        {
            get
            {
                TextureCached.TryGetValue(nameof(IconTreeMapLevel), out var tex);

                if (tex != null) return tex;
                tex = ProjectDatabase.FindAssetWithPath<Texture2D>("icon_tree_map_level.png", RELATIVE_PATH);
                TextureCached[nameof(IconTreeMapLevel)] = tex;
                return tex;
            }
        }

        public static Texture2D IconTreeMapLine
        {
            get
            {
                TextureCached.TryGetValue(nameof(IconTreeMapLine), out var tex);

                if (tex != null) return tex;
                tex = ProjectDatabase.FindAssetWithPath<Texture2D>("icon_tree_map_line.png", RELATIVE_PATH);
                TextureCached[nameof(IconTreeMapLine)] = tex;
                return tex;
            }
        }

        public static Texture2D IconCopyComponent(string theme)
        {
            TextureCached.TryGetValue($"{nameof(IconCopyComponent)}{theme}", out var tex);

            if (tex != null) return tex;
            tex = ProjectDatabase.FindAssetWithPath<Texture2D>(theme.Equals("DarkTheme") ? "icon_copy.png" : "icon_copy_dark.png", RELATIVE_PATH);
            TextureCached[$"{nameof(IconCopyComponent)}{theme}"] = tex;
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

        public static Texture2D IconNullGuardPassed(string theme)
        {
            TextureCached.TryGetValue($"{nameof(IconNullGuardPassed)}{theme}", out var tex);

            if (tex != null) return tex;
            tex = ProjectDatabase.FindAssetWithPath<Texture2D>(theme.Equals("DarkTheme") ? "icon_nullguard_passed.png" : "icon_nullguard_passed_dark.png", RELATIVE_PATH);
            TextureCached[$"{nameof(IconNullGuardPassed)}{theme}"] = tex;
            return tex;
        }

        public static TextAsset ScreenViewTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("ScreenViewTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset ScreenPresenterTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("ScreenPresenterTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset AndroidManifestTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("AndroidManifest.xml.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset MainGradleTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("mainTemplate.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset GradlePropertiesTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("gradleTemplate.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset GradleSettingsTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("settingsTemplate.txt", RELATIVE_TEMPLATE_PATH);
    }
}