using UnityEngine;

namespace PancakeEditor
{
    internal static class EditorResources
    {
        private const string RELATIVE_PATH = "Editor/Misc/Icons";

        internal static Texture2D ContentBackground => Editor.FindAssetWithPath<Texture2D>("content_bg.png", RELATIVE_PATH);
        internal static Texture2D ContentBackgroundDark => Editor.FindAssetWithPath<Texture2D>("content_bg_dark.png", RELATIVE_PATH);
        internal static Texture2D BoxBackground => Editor.FindAssetWithPath<Texture2D>("box_bg.png", RELATIVE_PATH);
        internal static Texture2D BoxBackgroundDark => Editor.FindAssetWithPath<Texture2D>("box_bg_dark.png", RELATIVE_PATH);
        internal static Texture2D EvenBackground => Editor.FindAssetWithPath<Texture2D>("even_bg.png", RELATIVE_PATH);
        internal static Texture2D EvenBackgroundBlue => Editor.FindAssetWithPath<Texture2D>("even_bg_select.png", RELATIVE_PATH);
        internal static Texture2D EvenBackgroundDark => Editor.FindAssetWithPath<Texture2D>("even_bg_dark.png", RELATIVE_PATH);
        internal static Texture2D ScriptableEvent => Editor.FindAssetWithPath<Texture2D>("scriptable_event.png", RELATIVE_PATH);
        internal static Texture2D ScriptableEventListener => Editor.FindAssetWithPath<Texture2D>("scriptable_event_listener.png", RELATIVE_PATH);
        internal static Texture2D ScriptableList => Editor.FindAssetWithPath<Texture2D>("scriptable_list.png", RELATIVE_PATH);
        internal static Texture2D ScriptablePlaymodeResetter => Editor.FindAssetWithPath<Texture2D>("scriptable_playmode_resetter.png", RELATIVE_PATH);
        internal static Texture2D ScriptableVariable => Editor.FindAssetWithPath<Texture2D>("scriptable_variable.png", RELATIVE_PATH);
        internal static Texture2D Dreamblale => Editor.FindAssetWithPath<Texture2D>("dreamblade.png", RELATIVE_PATH);
    }
}