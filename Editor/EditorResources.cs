using UnityEngine;

namespace PancakeEditor
{
    internal static class EditorResources
    {
        private const string RELATIVE_PATH = "Editor/Skins/Icons";

        internal static Texture2D ContentBackground => Editor.FindAssetWithPath<Texture2D>("content_bg.png", RELATIVE_PATH);
        internal static Texture2D ContentBackgroundDark => Editor.FindAssetWithPath<Texture2D>("content_bg_dark.png", RELATIVE_PATH);
        internal static Texture2D BoxBackground => Editor.FindAssetWithPath<Texture2D>("box_bg.png", RELATIVE_PATH);
        internal static Texture2D BoxBackgroundDark => Editor.FindAssetWithPath<Texture2D>("box_bg_dark.png", RELATIVE_PATH);
        internal static Texture2D EvenBackground => Editor.FindAssetWithPath<Texture2D>("even_bg.png", RELATIVE_PATH);
        internal static Texture2D EvenBackgroundBlue => Editor.FindAssetWithPath<Texture2D>("even_bg_select.png", RELATIVE_PATH);
        internal static Texture2D EvenBackgroundDark => Editor.FindAssetWithPath<Texture2D>("even_bg_dark.png", RELATIVE_PATH);

    }
}