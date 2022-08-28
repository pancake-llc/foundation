using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public static class Uniform
    {
        private static readonly Dictionary<string, GUIStyle> CustomStyles = new Dictionary<string, GUIStyle>();
        private static GUISkin skin;
        private const string SKIN_PATH = "Assets/_Root/Editor/GUISkins/";
        private const string UPM_SKIN_PATH = "Packages/com.pancake.common/Editor/GUISkins/";
        private static GUIStyle uppercaseSectionHeaderExpand;
        private static GUIStyle uppercaseSectionHeaderCollapse;
        private static GUIStyle toggleButtonToolbar;
        private static GUIStyle boxArea;
        private static Texture2D chevronUp;
        private static Texture2D chevronDown;
        private static Texture2D eraserIcon;
        private static Texture2D pinIcon;
        private static Texture2D extrudeIcon;
        private static Texture2D prefabIcon;
        private static Texture2D alignLeft;
        private static Texture2D alignCenter;
        private static Texture2D alignRight;
        private static Texture2D alignBottom;
        private static Texture2D alignMiddle;
        private static Texture2D alignTop;
        private static Texture2D distributeHorizontal;
        private static Texture2D distributeVertical;
        private static Texture2D snapAllPic;
        private static Texture2D snapVerticalPic;
        private static Texture2D snapHorizontalPic;
        private static Texture2D freeParentModeOnPic;
        private static Texture2D freeParentModeOffPic;
        private static Texture2D allBorderPic;
        private static Texture2D pointPic;
        private static Texture2D verticalPointPic;
        private static Texture2D horizontalPointPic;
        private static Texture2D verticalBorderPic;
        private static Texture2D horizontalBorderPic;

        private static Texture2D aboutIcon;
        private static Texture2D helpOutlineIcon;
        private static Texture2D arrowLeftIcon;
        private static Texture2D arrowRightIcon;
        private static Texture2D autoFixIcon;
        private static Texture2D cleanIcon;
        private static Texture2D clearIcon;
        private static Texture2D collapseIcon;
        private static Texture2D copyIcon;
        private static Texture2D deleteIcon;
        private static Texture2D doubleArrowLeftIcon;
        private static Texture2D doubleArrowRightIcon;
        private static Texture2D expandIcon;
        private static Texture2D exportIcon;
        private static Texture2D findIcon;
        private static Texture2D filterIcon;
        private static Texture2D gearIcon;
        private static Texture2D hideIcon;
        private static Texture2D homeIcon;
        private static Texture2D issueIcon;
        private static Texture2D logIcon;
        private static Texture2D minusIcon;
        private static Texture2D plusIcon;
        private static Texture2D moreIcon;
        private static Texture2D restoreIcon;
        private static Texture2D revealIcon;
        private static Texture2D revealBigIcon;
        private static Texture2D selectAllIcon;
        private static Texture2D selectNoneIcon;
        private static Texture2D showIcon;
        private static Texture2D starIcon;
        private static Texture2D supportIcon;
        private static Texture2D repeatIcon;

        private const int CHEVRON_ICON_WIDTH = 10;
        private const int CHEVRON_ICON_RIGHT_MARGIN = 5;
        private const float SPACE_HALF_LINE = 2f;
        private const float SPACE_ONE_LINE = 4f;
        private const float SPACE_TWO_LINE = 8f;
        private const float SPACE_THREE_LINE = 12f;
        
        public static readonly Color Green = new Color(0.18f, 1f, 0.45f, 0.66f);
        public static readonly Color Orange = new Color(1f, 0.46f, 0f, 0.66f);
        public static readonly Color Blue = new Color(0f, 0.75f, 1f, 0.27f);
        public static readonly Color Purple = new Color(1f, 0.56f, 1f, 0.39f);
        public static readonly Color Red = new Color(1f, 0.1f, 0.13f, 0.66f);
        public static readonly Color InspectorLock = new Color(.6f, .6f, .6f, 1);
        public static readonly Color InspectorNullError = new Color(1f, .5f, .5f, 1);
        public class Property
        {
            public SerializedProperty property;
            public GUIContent content;

            public Property(SerializedProperty property, GUIContent content)
            {
                this.property = property;
                this.content = content;
            }

            public Property(GUIContent content) { this.content = content; }
        }

        public static InEditor.ProjectSetting<UniformFoldoutState> FoldoutSettings { get; set; } = new InEditor.ProjectSetting<UniformFoldoutState>();

        public static GUIStyle UppercaseSectionHeaderExpand { get
        {
            if (uppercaseSectionHeaderExpand == null) uppercaseSectionHeaderExpand = GetCustomStyle("Uppercase Section Header");
            return uppercaseSectionHeaderExpand;
        } }

        public static GUIStyle UppercaseSectionHeaderCollapse
        {
            get
            {
                if (uppercaseSectionHeaderCollapse == null) uppercaseSectionHeaderCollapse = new GUIStyle(GetCustomStyle("Uppercase Section Header")) {normal = new GUIStyleState()};
                return uppercaseSectionHeaderCollapse;
            }
        }

        public static GUIStyle ToggleButtonToolbar { get
        {
            if (toggleButtonToolbar == null) toggleButtonToolbar = new GUIStyle(GetCustomStyle("ToggleButton"));
            return toggleButtonToolbar;
        } }

        public static GUIStyle BoxArea { get
        {
            if (boxArea == null) boxArea = new GUIStyle(GetCustomStyle("BoxArea"));
            return boxArea;
        } }

        public static GUIStyle GetCustomStyle(string styleName)
        {
            if (CustomStyles.ContainsKey(styleName)) return CustomStyles[styleName];

            if (Skin != null)
            {
                var style = Skin.FindStyle(styleName);

                if (style == null) Debug.LogError("Couldn't find style " + styleName);
                else CustomStyles.Add(styleName, style);

                return style;
            }

            return null;
        }

        private static void G<T>(ref T t, string sourcePath) where T : class
        {
            if (t != null) return;
            string upmPath = UPM_SKIN_PATH + sourcePath;
            string path = !File.Exists(Path.GetFullPath(upmPath)) ? SKIN_PATH + sourcePath : upmPath;
            t = AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T;
        }

        public static GUISkin Skin
        {
            get
            {
                if (skin == null) G(ref skin, "Dark.guiskin");
                if (skin == null) Debug.LogError("Couldn't load the Dark.guiskin at GUISkins");

                return skin;
            }
        }

        public static Texture2D ChevronDown
        {
            get
            {
                if (chevronDown == null) G(ref chevronDown, "Icons/icon-chevron-down-dark.psd");
                return chevronDown;
            }
        }

        public static Texture2D ChevronUp
        {
            get
            {
                if (chevronUp == null) G(ref chevronUp, "Icons/icon-chevron-up-dark.psd");
                return chevronUp;
            }
        }

        public static Texture2D PinIcon
        {
            get
            {
                if (pinIcon == null) G(ref pinIcon, "Icons/pin.png");
                return pinIcon;
            }
        }

        public static Texture2D ExtrudeIcon
        {
            get
            {
                if (extrudeIcon == null) G(ref extrudeIcon, "Icons/extrude.png");
                return extrudeIcon;
            }
        }

        public static Texture2D EraserIcon
        {
            get
            {
                if (eraserIcon == null) G(ref eraserIcon, "Icons/eraser.png");
                return eraserIcon;
            }
        }

        public static Texture2D PrefabIcon
        {
            get
            {
                if (prefabIcon == null) G(ref prefabIcon, "Icons/prefab-default.png");
                return prefabIcon;
            }
        }

        public static Texture2D AlignLeft
        {
            get
            {
                if (alignLeft == null) G(ref alignLeft, "Icons/Tools/allign_left.png");
                return alignLeft;
            }
        }

        public static Texture2D AlignCenter
        {
            get
            {
                if (alignCenter == null) G(ref alignCenter, "Icons/Tools/allign_center.png");
                return alignCenter;
            }
        }

        public static Texture2D AlignRight
        {
            get
            {
                if (alignRight == null) G(ref alignRight, "Icons/Tools/allign_right.png");
                return alignRight;
            }
        }

        public static Texture2D AlignBottom
        {
            get
            {
                if (alignBottom == null) G(ref alignBottom, "Icons/Tools/allign_bottom.png");
                return alignBottom;
            }
        }

        public static Texture2D AlignMiddle
        {
            get
            {
                if (alignMiddle == null) G(ref alignMiddle, "Icons/Tools/allign_middle.png");
                return alignMiddle;
            }
        }

        public static Texture2D AlignTop
        {
            get
            {
                if (alignTop == null) G(ref alignTop, "Icons/Tools/allign_top.png");
                return alignTop;
            }
        }

        public static Texture2D DistributeHorizontal
        {
            get
            {
                if (distributeHorizontal == null) G(ref distributeHorizontal, "Icons/Tools/distribute_horizontally.png");
                return distributeHorizontal;
            }
        }

        public static Texture2D DistributeVertical
        {
            get
            {
                if (distributeVertical == null) G(ref distributeVertical, "Icons/Tools/distribute_vertically.png");
                return distributeVertical;
            }
        }

        public static Texture2D SnapAllPic
        {
            get
            {
                if (snapAllPic == null) G(ref snapAllPic, "Icons/Tools/snap_to_childs_all.png");
                return snapAllPic;
            }
        }

        public static Texture2D SnapHorizontalPic
        {
            get
            {
                if (snapHorizontalPic == null) G(ref snapHorizontalPic, "Icons/Tools/snap_to_childs_h.png");
                return snapHorizontalPic;
            }
        }

        public static Texture2D SnapVerticalPic
        {
            get
            {
                if (snapVerticalPic == null) G(ref snapVerticalPic, "Icons/Tools/snap_to_childs_v.png");
                return snapVerticalPic;
            }
        }

        public static Texture2D FreeParentModeOnPic
        {
            get
            {
                if (freeParentModeOnPic == null) G(ref freeParentModeOnPic, "Icons/Tools/free_parent_mode_on.png");
                return freeParentModeOnPic;
            }
        }

        public static Texture2D FreeParentModeOffPic
        {
            get
            {
                if (freeParentModeOffPic == null) G(ref freeParentModeOffPic, "Icons/Tools/free_parent_mode_off.png");
                return freeParentModeOffPic;
            }
        }

        public static Texture2D AllBorderPic
        {
            get
            {
                if (allBorderPic == null) G(ref allBorderPic, "Icons/Tools/snap_all_edges.png");
                return allBorderPic;
            }
        }

        public static Texture2D PointPic
        {
            get
            {
                if (pointPic == null) G(ref pointPic, "Icons/Tools/snap_all_direction_point.png");
                return pointPic;
            }
        }

        public static Texture2D HorizontalPointPic
        {
            get
            {
                if (horizontalPointPic == null) G(ref horizontalPointPic, "Icons/Tools/snap_horizontal_point.png");
                return horizontalPointPic;
            }
        }

        public static Texture2D VerticalPointPic
        {
            get
            {
                if (verticalPointPic == null) G(ref verticalPointPic, "Icons/Tools/snap_vertical_point.png");
                return verticalPointPic;
            }
        }

        public static Texture2D HorizontalBorderPic
        {
            get
            {
                if (horizontalBorderPic == null) G(ref horizontalBorderPic, "Icons/Tools/snap_horizontal_edges.png");
                return horizontalBorderPic;
            }
        }

        public static Texture2D VerticalBorderPic
        {
            get
            {
                if (verticalBorderPic == null) G(ref verticalBorderPic, "Icons/Tools/snap_vertical_edges.png");
                return verticalBorderPic;
            }
        }

        public static Texture2D AboutIcon
        {
            get
            {
                if (aboutIcon == null) G(ref aboutIcon, "Icons/Finder/about.png");
                return aboutIcon;
            }
        }

        public static Texture2D HelpOutlineIcon
        {
            get
            {
                if (helpOutlineIcon == null) G(ref helpOutlineIcon, "Icons/Finder/help_outline.png");
                return helpOutlineIcon;
            }
        }

        public static Texture2D ArrowLeftIcon
        {
            get
            {
                if (arrowLeftIcon == null) G(ref arrowLeftIcon, "Icons/Finder/arrow_left.png");
                return arrowLeftIcon;
            }
        }

        public static Texture2D ArrowRightIcon
        {
            get
            {
                if (arrowRightIcon == null) G(ref arrowRightIcon, "Icons/Finder/arrow_right.png");
                return arrowRightIcon;
            }
        }

        public static Texture2D AutoFixIcon
        {
            get
            {
                if (autoFixIcon == null) G(ref autoFixIcon, "Icons/Finder/auto_fix.png");
                return autoFixIcon;
            }
        }

        public static Texture2D CleanIcon
        {
            get
            {
                if (cleanIcon == null) G(ref cleanIcon, "Icons/Finder/clean.png");
                return cleanIcon;
            }
        }

        public static Texture2D ClearIcon
        {
            get
            {
                if (clearIcon == null) G(ref clearIcon, "Icons/Finder/clear.png");
                return clearIcon;
            }
        }

        public static Texture2D CollapseIcon
        {
            get
            {
                if (collapseIcon == null) G(ref collapseIcon, "Icons/Finder/collapse.png");
                return collapseIcon;
            }
        }

        public static Texture2D CopyIcon
        {
            get
            {
                if (copyIcon == null) G(ref copyIcon, "Icons/Finder/copy.png");
                return copyIcon;
            }
        }

        public static Texture2D DeleteIcon
        {
            get
            {
                if (deleteIcon == null) G(ref deleteIcon, "Icons/Finder/delete.png");
                return deleteIcon;
            }
        }

        public static Texture2D DoubleArrowLeftIcon
        {
            get
            {
                if (doubleArrowLeftIcon == null) G(ref doubleArrowLeftIcon, "Icons/Finder/double_arrow_left.png");
                return doubleArrowLeftIcon;
            }
        }

        public static Texture2D DoubleArrowRightIcon
        {
            get
            {
                if (doubleArrowRightIcon == null) G(ref doubleArrowRightIcon, "Icons/Finder/double_arrow_right.png");
                return doubleArrowRightIcon;
            }
        }

        public static Texture2D ExpandIcon
        {
            get
            {
                if (expandIcon == null) G(ref expandIcon, "Icons/Finder/expand.png");
                return expandIcon;
            }
        }

        public static Texture2D ExportIcon
        {
            get
            {
                if (exportIcon == null) G(ref exportIcon, "Icons/Finder/export.png");
                return exportIcon;
            }
        }

        public static Texture2D FindIcon
        {
            get
            {
                if (findIcon == null) G(ref findIcon, "Icons/Finder/find.png");
                return findIcon;
            }
        }

        public static Texture2D FilterIcon
        {
            get
            {
                if (filterIcon == null) G(ref filterIcon, "Icons/Finder/filter.png");
                return filterIcon;
            }
        }

        public static Texture2D GearIcon
        {
            get
            {
                if (gearIcon == null) G(ref gearIcon, "Icons/Finder/gear.png");
                return gearIcon;
            }
        }

        public static Texture2D HideIcon
        {
            get
            {
                if (hideIcon == null) G(ref hideIcon, "Icons/Finder/hide.png");
                return hideIcon;
            }
        }

        public static Texture2D HomeIcon
        {
            get
            {
                if (homeIcon == null) G(ref homeIcon, "Icons/Finder/home.png");
                return homeIcon;
            }
        }

        public static Texture2D IssueIcon
        {
            get
            {
                if (issueIcon == null) G(ref issueIcon, "Icons/Finder/issue.png");
                return issueIcon;
            }
        }

        public static Texture2D LogIcon
        {
            get
            {
                if (logIcon == null) G(ref logIcon, "Icons/Finder/log.png");
                return logIcon;
            }
        }

        public static Texture2D MinusIcon
        {
            get
            {
                if (minusIcon == null) G(ref minusIcon, "Icons/Finder/minus.png");
                return minusIcon;
            }
        }

        public static Texture2D PlusIcon
        {
            get
            {
                if (plusIcon == null) G(ref plusIcon, "Icons/Finder/plus.png");
                return plusIcon;
            }
        }

        public static Texture2D MoreIcon
        {
            get
            {
                if (moreIcon == null) G(ref moreIcon, "Icons/Finder/more.png");
                return moreIcon;
            }
        }

        public static Texture2D RestoreIcon
        {
            get
            {
                if (restoreIcon == null) G(ref restoreIcon, "Icons/Finder/restore.png");
                return restoreIcon;
            }
        }

        public static Texture2D RevealIcon
        {
            get
            {
                if (revealIcon == null) G(ref revealIcon, "Icons/Finder/reveal.png");
                return revealIcon;
            }
        }

        public static Texture2D RevealBigIcon
        {
            get
            {
                if (revealBigIcon == null) G(ref revealBigIcon, "Icons/Finder/reveal_big.png");
                return revealBigIcon;
            }
        }

        public static Texture2D SelectAllIcon
        {
            get
            {
                if (selectAllIcon == null) G(ref selectAllIcon, "Icons/Finder/select_all.png");
                return selectAllIcon;
            }
        }

        public static Texture2D SelectNoneIcon
        {
            get
            {
                if (selectNoneIcon == null) G(ref selectNoneIcon, "Icons/Finder/select_none.png");
                return selectNoneIcon;
            }
        }

        public static Texture2D ShowIcon
        {
            get
            {
                if (showIcon == null) G(ref showIcon, "Icons/Finder/show.png");
                return showIcon;
            }
        }

        public static Texture2D StarIcon
        {
            get
            {
                if (starIcon == null) G(ref starIcon, "Icons/Finder/star.png");
                return starIcon;
            }
        }

        public static Texture2D SupportIcon
        {
            get
            {
                if (supportIcon == null) G(ref supportIcon, "Icons/Finder/support.png");
                return supportIcon;
            }
        }

        public static Texture2D RepeatIcon
        {
            get
            {
                if (repeatIcon == null) G(ref repeatIcon, "Icons/Finder/repeat.png");
                return repeatIcon;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="foldout"></param>
        /// <returns></returns>
        public static Texture2D GetChevronIcon(bool foldout) { return foldout ? ChevronUp : ChevronDown; }

        /// <summary>
        /// Draw group selection with header
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sectionName"></param>
        /// <param name="drawer"></param>
        /// <param name="defaultFoldout"></param>
        public static void DrawUppercaseSection(string key, string sectionName, Action drawer, bool defaultFoldout = true)
        {
            if (!FoldoutSettings.Settings.ContainsKey(key)) FoldoutSettings.Settings.Add(key, defaultFoldout);

            bool foldout = FoldoutSettings.Settings[key];

            EditorGUILayout.BeginVertical(GetCustomStyle("Uppercase Section Box"), GUILayout.MinHeight(foldout ? 30 : 0));
            EditorGUILayout.BeginHorizontal(foldout ? UppercaseSectionHeaderExpand : UppercaseSectionHeaderCollapse);

            // Header label (and button).
            if (GUILayout.Button(sectionName, GetCustomStyle("Uppercase Section Header Label")))
                FoldoutSettings.Settings[key] = !FoldoutSettings.Settings[key];

            // The expand/collapse icon.
            var buttonRect = GUILayoutUtility.GetLastRect();
            var iconRect = new Rect(buttonRect.x + buttonRect.width - CHEVRON_ICON_WIDTH - CHEVRON_ICON_RIGHT_MARGIN,
                buttonRect.y,
                CHEVRON_ICON_WIDTH,
                buttonRect.height);
            GUI.Label(iconRect, GetChevronIcon(foldout), GetCustomStyle("Uppercase Section Header Chevron"));

            EditorGUILayout.EndHorizontal();

            // Draw the section content.
            if (foldout) GUILayout.Space(5);
            if (foldout && drawer != null) drawer();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw group selection with header
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sectionName"></param>
        /// <param name="drawer"></param>
        /// <param name="actionRightClick"></param>
        /// <param name="defaultFoldout"></param>
        public static void DrawUppercaseSectionWithRightClick(string key, string sectionName, Action drawer, Action actionRightClick, bool defaultFoldout = true)
        {
            if (!FoldoutSettings.Settings.ContainsKey(key)) FoldoutSettings.Settings.Add(key, defaultFoldout);

            bool foldout = FoldoutSettings.Settings[key];

            EditorGUILayout.BeginVertical(GetCustomStyle("Uppercase Section Box"), GUILayout.MinHeight(foldout ? 30 : 0));
            EditorGUILayout.BeginHorizontal(foldout ? UppercaseSectionHeaderExpand : UppercaseSectionHeaderCollapse);

            // Header label (and button).
            if (GUILayout.Button(sectionName, GetCustomStyle("Uppercase Section Header Label")))
            {
                if (Event.current.button == 1)
                {
                    actionRightClick?.Invoke();
                    return;
                }

                FoldoutSettings.Settings[key] = !FoldoutSettings.Settings[key];
            }

            // The expand/collapse icon.
            var buttonRect = GUILayoutUtility.GetLastRect();
            var iconRect = new Rect(buttonRect.x + buttonRect.width - CHEVRON_ICON_WIDTH - CHEVRON_ICON_RIGHT_MARGIN,
                buttonRect.y,
                CHEVRON_ICON_WIDTH,
                buttonRect.height);
            GUI.Label(iconRect, GetChevronIcon(foldout), GetCustomStyle("Uppercase Section Header Chevron"));

            EditorGUILayout.EndHorizontal();

            // Draw the section content.
            if (foldout) GUILayout.Space(5);
            if (foldout && drawer != null) drawer();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Icon content
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tooltip"></param>
        /// <returns></returns>
        public static GUIContent IconContent(string name, string tooltip)
        {
            var builtinIcon = EditorGUIUtility.IconContent(name);
            return new GUIContent(builtinIcon.image, tooltip);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldTitle"></param>
        /// <param name="text"></param>
        /// <param name="labelWidth"></param>
        /// <param name="textFieldWidthOption"></param>
        /// <returns></returns>
        public static string DrawTextField(string fieldTitle, string text, GUILayoutOption labelWidth, GUILayoutOption textFieldWidthOption = null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(4);
            EditorGUILayout.LabelField(new GUIContent(fieldTitle), labelWidth);
            GUILayout.Space(4);
            text = textFieldWidthOption == null ? GUILayout.TextField(text) : GUILayout.TextField(text, textFieldWidthOption);
            GUILayout.Space(4);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            return text;
        }

        /// <summary>
        /// Draw vertical group
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="options"></param>
        public static void Vertical(Action callback, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(options);
            callback();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw vertical group
        /// </summary>
        /// <param name="style"></param>
        /// <param name="callback"></param>
        public static void Vertical(GUIStyle style, Action callback)
        {
            EditorGUILayout.BeginVertical(style);
            callback();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw vertical group
        /// </summary>
        /// <param name="style"></param>
        /// <param name="callback"></param>
        /// <param name="options"></param>
        public static void Vertical(GUIStyle style, Action callback, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(style, options);
            callback();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw vertical group
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="options"></param>
        public static void VerticalScope(Action callback, params GUILayoutOption[] options)
        {
            using (new EditorGUILayout.VerticalScope(options))
            {
                callback();
            }
        }

        /// <summary>
        /// Draw vertical group
        /// </summary>
        /// <param name="style"></param>
        /// <param name="callback"></param>
        public static void VerticalScope(GUIStyle style, Action callback)
        {
            using (new EditorGUILayout.VerticalScope(style))
            {
                callback();
            }
        }

        /// <summary>
        /// Draw vertical group
        /// </summary>
        /// <param name="style"></param>
        /// <param name="callback"></param>
        /// <param name="options"></param>
        public static void VerticalScope(GUIStyle style, Action callback, params GUILayoutOption[] options)
        {
            using (new EditorGUILayout.VerticalScope(style, options))
            {
                callback();
            }
        }

        /// <summary>
        /// Draw horizontal group
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="options"></param>
        public static void Horizontal(Action callback, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(options);
            callback();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw horizontal group
        /// </summary>
        /// <param name="style"></param>
        /// <param name="callback"></param>
        public static void Horizontal(GUIStyle style, Action callback)
        {
            EditorGUILayout.BeginHorizontal(style);
            callback();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw horizontal group
        /// </summary>
        /// <param name="style"></param>
        /// <param name="callback"></param>
        /// <param name="options"></param>
        public static void Horizontal(GUIStyle style, Action callback, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(style, options);
            callback();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw horizontal scope
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="options"></param>
        public static void HorizontalScope(Action callback, params GUILayoutOption[] options)
        {
            using (new EditorGUILayout.HorizontalScope(options))
            {
                callback();
            }
        }

        /// <summary>
        /// Draw horizontal scope
        /// </summary>
        /// <param name="style"></param>
        /// <param name="callback"></param>
        public static void HorizontalScope(GUIStyle style, Action callback)
        {
            using (new EditorGUILayout.HorizontalScope(style))
            {
                callback();
            }
        }

        /// <summary>
        /// Draw horizontal scope
        /// </summary>
        /// <param name="style"></param>
        /// <param name="callback"></param>
        /// <param name="options"></param>
        public static void HorizontalScope(GUIStyle style, Action callback, params GUILayoutOption[] options)
        {
            using (new EditorGUILayout.HorizontalScope(style, options))
            {
                callback();
            }
        }

        /// <summary>
        /// Create button in editor gui
        /// </summary>
        /// <param name="label"></param>
        /// <param name="callback"></param>
        /// <param name="color"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool Button(string label, Action callback = null, Color? color = null, params GUILayoutOption[] options)
        {
            return Button(new GUIContent(label), callback, color, options);
        }

        /// <summary>
        /// Create button in editor gui
        /// </summary>
        /// <param name="content"></param>
        /// <param name="callback"></param>
        /// <param name="color"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool Button(GUIContent content, Action callback = null, Color? color = null, params GUILayoutOption[] options)
        {
            var c = GUI.color;
            GUI.color = color ?? c;
            bool b = GUILayout.Button(content, options);
            if (b) callback?.Invoke();
            GUI.color = c;
            return b;
        }

        /// <summary>
        /// create mini button
        /// </summary>
        /// <param name="label"></param>
        /// <param name="callback"></param>
        /// <param name="color"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static bool MiniButton(string label, Action callback = null, Color? color = null, params GUILayoutOption[] options)
        {
            var c = GUI.color;
            GUI.color = color ?? c;
            bool b = GUILayout.Button(new GUIContent(label), new GUIStyle(EditorStyles.miniButton) {fontSize = 11, font = EditorStyles.label.font}, options);
            if (b) callback?.Invoke();
            GUI.color = c;
            return b;
        }

        /// <summary>
        /// Show panel to pickup folder
        /// </summary>
        /// <param name="pathResult"></param>
        /// <param name="defaultPath"></param>
        /// <param name="keySave"></param>
        /// <param name="options"></param>
        public static void PickFolderPath(ref string pathResult, string defaultPath = "", string keySave = "", params GUILayoutOption[] options)
        {
            GUI.backgroundColor = Color.gray;
            if (GUILayout.Button(IconContent("d_Project", "Pick folder"), options))
            {
                string path = EditorUtility.OpenFolderPanel("Select folder", string.IsNullOrEmpty(pathResult) ? defaultPath : pathResult, "");
                if (!string.IsNullOrEmpty(path))
                {
                    pathResult = path;
                    if (!string.IsNullOrEmpty(keySave))
                    {
                        EditorPrefs.SetString(keySave, pathResult);
                    }
                }

                GUI.FocusControl(null);
            }

            GUI.backgroundColor = Color.white;
        }

        /// <summary>
        /// Show panel to pickup file
        /// </summary>
        /// <param name="pathResult"></param>
        /// <param name="defaultPath"></param>
        /// <param name="extension">extension type of file</param>
        /// <param name="keySave"></param>
        /// <param name="options"></param>
        public static void PickFilePath(ref string pathResult, string defaultPath = "", string extension = "db", string keySave = "", params GUILayoutOption[] options)
        {
            GUI.backgroundColor = Color.gray;
            if (GUILayout.Button(IconContent("d_editicon.sml", "Pick file"), options))
            {
                var path = EditorUtility.OpenFilePanel("Select file", string.IsNullOrEmpty(pathResult) ? defaultPath : pathResult, extension);
                if (!string.IsNullOrEmpty(path))
                {
                    pathResult = path;
                    if (!string.IsNullOrEmpty(keySave)) EditorPrefs.SetString(keySave, pathResult);
                }

                GUI.FocusControl(null);
            }

            GUI.backgroundColor = Color.white;
        }

        /// <summary>
        /// Disable groups
        /// </summary>
        public static void DisabledSection(Action onGUI = null, Func<bool> isDisabled = null)
        {
            EditorGUI.BeginDisabledGroup(isDisabled?.Invoke() ?? true);
            onGUI?.Invoke();
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public static void HelpBox(string message, MessageType type = MessageType.None) { EditorGUILayout.HelpBox(message, type); }

        public static void SpaceHalfLine() => GUILayout.Space(SPACE_HALF_LINE);
        public static void SpaceOneLine() => GUILayout.Space(SPACE_ONE_LINE);
        public static void SpaceTwoLine() => GUILayout.Space(SPACE_TWO_LINE);
        public static void SpaceThreeLine() => GUILayout.Space(SPACE_THREE_LINE);
    }
}