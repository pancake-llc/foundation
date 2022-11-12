#if UNITY_EDITOR
using UnityEngine;

namespace Pancake.Editor
{
    public static class EditorResources
    {
        private const string RELATIVE_PATH = "Editor/GUISkin/Icons";

        public static Texture2D CircleCheckmark => InEditor.FindAssetWithPath<Texture2D>("circle_checkmark.png", RELATIVE_PATH);
        public static Texture2D ChevronUpDark => InEditor.FindAssetWithPath<Texture2D>("icon-chevron-up-dark.psd", RELATIVE_PATH);
        public static Texture2D ChevronDownDark => InEditor.FindAssetWithPath<Texture2D>("icon-chevron-down-dark.psd", RELATIVE_PATH);
        public static Texture2D ChevronUpLight => InEditor.FindAssetWithPath<Texture2D>("icon-chevron-up-light.psd", RELATIVE_PATH);
        public static Texture2D ChevronDownLight => InEditor.FindAssetWithPath<Texture2D>("icon-chevron-down-light.psd", RELATIVE_PATH);
        public static Texture2D PinIcon => InEditor.FindAssetWithPath<Texture2D>("pin.png", RELATIVE_PATH);
        public static Texture2D PrefabIcon => InEditor.FindAssetWithPath<Texture2D>("prefab-icon.png", RELATIVE_PATH);
        public static Texture2D AlignLeft => InEditor.FindAssetWithPath<Texture2D>("allign_left.png", RELATIVE_PATH + "/UI");
        public static Texture2D AlignCenter => InEditor.FindAssetWithPath<Texture2D>("allign_center.png", RELATIVE_PATH + "/UI");
        public static Texture2D AlignRight => InEditor.FindAssetWithPath<Texture2D>("allign_right.png", RELATIVE_PATH + "/UI");
        public static Texture2D AlignBottom => InEditor.FindAssetWithPath<Texture2D>("allign_bottom.png", RELATIVE_PATH + "/UI");
        public static Texture2D AlignMiddle => InEditor.FindAssetWithPath<Texture2D>("allign_middle.png", RELATIVE_PATH + "/UI");
        public static Texture2D AlignTop => InEditor.FindAssetWithPath<Texture2D>("allign_top.png", RELATIVE_PATH + "/UI");
        public static Texture2D DistributeHorizontal => InEditor.FindAssetWithPath<Texture2D>("distribute_horizontally.png", RELATIVE_PATH + "/UI");
        public static Texture2D DistributeVertical => InEditor.FindAssetWithPath<Texture2D>("distribute_vertically.png", RELATIVE_PATH + "/UI");
        public static Texture2D SnapAllPic => InEditor.FindAssetWithPath<Texture2D>("snap_to_childs_all.png", RELATIVE_PATH + "/UI");
        public static Texture2D SnapVertical => InEditor.FindAssetWithPath<Texture2D>("snap_to_childs_v.png", RELATIVE_PATH + "/UI");
        public static Texture2D SnapHorizontal => InEditor.FindAssetWithPath<Texture2D>("snap_to_childs_h.png", RELATIVE_PATH + "/UI");
        public static Texture2D FreeParentModeOn => InEditor.FindAssetWithPath<Texture2D>("free_parent_mode_on.png", RELATIVE_PATH + "/UI");
        public static Texture2D FreeParentModeOff => InEditor.FindAssetWithPath<Texture2D>("free_parent_mode_off.png", RELATIVE_PATH + "/UI");
        public static Texture2D SnapAllBorder => InEditor.FindAssetWithPath<Texture2D>("snap_all_edges.png", RELATIVE_PATH + "/UI");
        public static Texture2D SnapAllPoint => InEditor.FindAssetWithPath<Texture2D>("snap_all_direction_point.png", RELATIVE_PATH + "/UI");
        public static Texture2D SnapVerticalPoint => InEditor.FindAssetWithPath<Texture2D>("snap_vertical_point.png", RELATIVE_PATH + "/UI");
        public static Texture2D SnapHorizontalPoint => InEditor.FindAssetWithPath<Texture2D>("snap_horizontal_point.png", RELATIVE_PATH + "/UI");
        public static Texture2D SnapVerticalBorder => InEditor.FindAssetWithPath<Texture2D>("snap_vertical_edges.png", RELATIVE_PATH + "/UI");
        public static Texture2D SnapHorizontalBorder => InEditor.FindAssetWithPath<Texture2D>("snap_horizontal_edges.png", RELATIVE_PATH + "/UI");
        public static Texture2D HelpOutlineIcon => InEditor.FindAssetWithPath<Texture2D>("help_outline.png", RELATIVE_PATH);
        public static Texture2D AutoFixIcon => InEditor.FindAssetWithPath<Texture2D>("auto_fix.png", RELATIVE_PATH);
        public static Texture2D CleanIcon => InEditor.FindAssetWithPath<Texture2D>("clean.png", RELATIVE_PATH);
        public static Texture2D CopyIcon => InEditor.FindAssetWithPath<Texture2D>("copy.png", RELATIVE_PATH);
        public static Texture2D DeleteIcon => InEditor.FindAssetWithPath<Texture2D>("delete.png", RELATIVE_PATH);
        public static Texture2D DoubleArrowLeftIcon => InEditor.FindAssetWithPath<Texture2D>("double_arrow_left.png", RELATIVE_PATH);
        public static Texture2D DoubleArrowRightIcon => InEditor.FindAssetWithPath<Texture2D>("double_arrow_right.png", RELATIVE_PATH);
        public static Texture2D ExportIcon => InEditor.FindAssetWithPath<Texture2D>("export.png", RELATIVE_PATH);
        public static Texture2D FindIcon => InEditor.FindAssetWithPath<Texture2D>("find.png", RELATIVE_PATH);
        public static Texture2D FilterIcon => InEditor.FindAssetWithPath<Texture2D>("filter.png", RELATIVE_PATH);
        public static Texture2D SettingIcon => InEditor.FindAssetWithPath<Texture2D>("setting.png", RELATIVE_PATH);
        public static Texture2D HideIcon => InEditor.FindAssetWithPath<Texture2D>("hide.png", RELATIVE_PATH);
        public static Texture2D HomeIcon => InEditor.FindAssetWithPath<Texture2D>("home.png", RELATIVE_PATH);
        public static Texture2D IssueIcon => InEditor.FindAssetWithPath<Texture2D>("issue.png", RELATIVE_PATH);
        public static Texture2D LOGIcon => InEditor.FindAssetWithPath<Texture2D>("log.png", RELATIVE_PATH);
        public static Texture2D MinusIcon => InEditor.FindAssetWithPath<Texture2D>("minus.png", RELATIVE_PATH);
        public static Texture2D PlusIcon => InEditor.FindAssetWithPath<Texture2D>("plus.png", RELATIVE_PATH);
        public static Texture2D MoreIcon => InEditor.FindAssetWithPath<Texture2D>("more.png", RELATIVE_PATH);
        public static Texture2D RestoreIcon => InEditor.FindAssetWithPath<Texture2D>("restore.png", RELATIVE_PATH);
        public static Texture2D SelectAllIcon => InEditor.FindAssetWithPath<Texture2D>("select_all.png", RELATIVE_PATH);
        public static Texture2D SelectNoneIcon => InEditor.FindAssetWithPath<Texture2D>("select_none.png", RELATIVE_PATH);
        public static Texture2D ShowIcon => InEditor.FindAssetWithPath<Texture2D>("show.png", RELATIVE_PATH);
        public static Texture2D StarIcon => InEditor.FindAssetWithPath<Texture2D>("star.png", RELATIVE_PATH);
        public static Texture2D InboxIcon => InEditor.FindAssetWithPath<Texture2D>("inbox.png", RELATIVE_PATH);
        public static Texture2D RepeatIcon => InEditor.FindAssetWithPath<Texture2D>("repeat.png", RELATIVE_PATH);

        public static Texture2D LinkBlack => InEditor.FindAssetWithPath<Texture2D>("link_black.png", RELATIVE_PATH);
        public static Texture2D LinkBlue => InEditor.FindAssetWithPath<Texture2D>("link_blue.png", RELATIVE_PATH);
        public static Texture2D LinkWhite => InEditor.FindAssetWithPath<Texture2D>("link_white.png", RELATIVE_PATH);
        public static Texture2D ButtonNormalTexture => InEditor.FindAssetWithPath<Texture2D>("btn_normal_texture.png", RELATIVE_PATH);
        public static Texture2D ButtonHoverTexture => InEditor.FindAssetWithPath<Texture2D>("btn_hover_texture.png", RELATIVE_PATH);
        public static Texture2D ButtonPressTexture => InEditor.FindAssetWithPath<Texture2D>("btn_press_texture.png", RELATIVE_PATH);
        public static Texture2D BoxBackground => InEditor.FindAssetWithPath<Texture2D>("box_bg.png", RELATIVE_PATH);
        public static Texture2D BoxBackgroundDark => InEditor.FindAssetWithPath<Texture2D>("box_bg_dark.png", RELATIVE_PATH);
        public static Texture2D ContentBackground => InEditor.FindAssetWithPath<Texture2D>("content_bg.png", RELATIVE_PATH);
        public static Texture2D ContentBackgroundDark => InEditor.FindAssetWithPath<Texture2D>("content_bg_dark.png", RELATIVE_PATH);
        public static Texture2D ToggleNormalBackground => InEditor.FindAssetWithPath<Texture2D>("toggle-normal-background.png", RELATIVE_PATH);
        public static Texture2D ToggleOnNormalBackground => InEditor.FindAssetWithPath<Texture2D>("toggle-on-normal-background.png", RELATIVE_PATH);
        public static Texture2D ToolboxAreaNormalBackground => InEditor.FindAssetWithPath<Texture2D>("toolbox-area-normal-background.png", RELATIVE_PATH);
    }
}
#endif