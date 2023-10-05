using UnityEngine;

namespace Pancake.ExLibEditor
{
    public static class EditorResources
    {
        private const string RELATIVE_PATH = "Modules/Apex/ExLib/Core/Editor/Misc/Icons";
        private const string RELATIVE_REORDERABLE_ARRAY_PATH = "Modules/Apex/ExLib/Core/Editor/Misc/Icons/ReorderableArray";
        private const string RELATIVE_TEMPLATE_PATH = "Modules/Apex/ExLib/Core/Editor/Misc/Templates";

        public static Texture2D BoxContentDark => ProjectDatabase.FindAssetWithPath<Texture2D>("box_content_dark.psd", RELATIVE_PATH);
        public static Texture2D BoxBackgroundDark => ProjectDatabase.FindAssetWithPath<Texture2D>("box_bg_dark.psd", RELATIVE_PATH);
        public static Texture2D EvenBackground => ProjectDatabase.FindAssetWithPath<Texture2D>("even_bg.png", RELATIVE_PATH);
        public static Texture2D EvenBackgroundBlue => ProjectDatabase.FindAssetWithPath<Texture2D>("even_bg_select.png", RELATIVE_PATH);
        public static Texture2D EvenBackgroundDark => ProjectDatabase.FindAssetWithPath<Texture2D>("even_bg_dark.png", RELATIVE_PATH);
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
        public static Texture2D ScriptableInterface => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_interface.png", RELATIVE_PATH);
        public static Texture2D ScriptableFactory => ProjectDatabase.FindAssetWithPath<Texture2D>("scriptable_factory.png", RELATIVE_PATH);

        public static Texture2D ReorderableArrayEntryActive(string theme) =>
            ProjectDatabase.FindAssetWithPath<Texture2D>("EntryActiveTexture.png", $"{RELATIVE_REORDERABLE_ARRAY_PATH}/{theme}");

        public static Texture2D ReorderableArrayEntryBackground(string theme) =>
            ProjectDatabase.FindAssetWithPath<Texture2D>("EntryBkgTexture.png", $"{RELATIVE_REORDERABLE_ARRAY_PATH}/{theme}");

        public static Texture2D ReorderableArrayEntryEven(string theme) =>
            ProjectDatabase.FindAssetWithPath<Texture2D>("EntryEvenTexture.png", $"{RELATIVE_REORDERABLE_ARRAY_PATH}/{theme}");

        public static Texture2D ReorderableArrayEntryFocus(string theme) =>
            ProjectDatabase.FindAssetWithPath<Texture2D>("EntryFocusedTexture.png", $"{RELATIVE_REORDERABLE_ARRAY_PATH}/{theme}");

        public static Texture2D ReorderableArrayEntryOdd(string theme) =>
            ProjectDatabase.FindAssetWithPath<Texture2D>("EntryOddTexture.png", $"{RELATIVE_REORDERABLE_ARRAY_PATH}/{theme}");

        public static Texture2D ReorderableArrayHover(string theme) =>
            ProjectDatabase.FindAssetWithPath<Texture2D>("HoverTexture.png", $"{RELATIVE_REORDERABLE_ARRAY_PATH}/{theme}");

        public static Texture2D ReorderableArrayNormal(string theme) =>
            ProjectDatabase.FindAssetWithPath<Texture2D>("NormalTexture.png", $"{RELATIVE_REORDERABLE_ARRAY_PATH}/{theme}");

        public static Texture2D ReorderableArrayPress(string theme) =>
            ProjectDatabase.FindAssetWithPath<Texture2D>("PressTexture.png", $"{RELATIVE_REORDERABLE_ARRAY_PATH}/{theme}");

        public static TextAsset ScriptableEventListenerTemplate =>
            ProjectDatabase.FindAssetWithPath<TextAsset>("ScriptableEventListenerTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);

        public static TextAsset ScreenViewTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("ScreenViewTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset ScreenPresenterTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("ScreenPresenterTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);

        public static TextAsset ScriptableEventTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("ScriptableEventTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset ScriptableListTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("ScriptableListTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
        public static TextAsset ScriptableVariableTemplate => ProjectDatabase.FindAssetWithPath<TextAsset>("ScriptableVariableTemplate.cs.txt", RELATIVE_TEMPLATE_PATH);
    }
}