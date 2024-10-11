using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Common
{
    public struct Uniform
    {
        #region field

        private static GUIStyle contentList;
        private static GUIStyle contentListBlue;
        private static GUIStyle contentListDark;
        private static GUIStyle contentBox;
        private static GUIStyle box;
        private static GUIStyle foldoutButton;
        private static GUIStyle foldoutIcon;
        private static GUIStyle installedIcon;
        private static GUIStyle headerLabel;
        private static GUIStyle richLabel;
        private static GUIStyle richLabelHelpBox;
        private static GUIStyle boldRichLabel;
        private static GUIStyle centerRichLabel;
        private static bool prevGuiEnabled = true;

        private static readonly Dictionary<string, GUIContent> CachedIconContent = new();
        private static readonly UniformFoldoutState FoldoutSettings = new();
        public static string Theme => EditorGUIUtility.isProSkin ? "DarkTheme" : "LightTheme";

        #endregion


        #region prop

        public static GUIStyle FoldoutButton
        {
            get
            {
                if (foldoutButton != null) return foldoutButton;

                foldoutButton = new GUIStyle
                {
                    normal = new GUIStyleState {textColor = Color.white},
                    padding = new RectOffset(0, 0, 3, 3),
                    stretchWidth = true,
                    richText = true,
                    fontSize = 11,
                    fontStyle = FontStyle.Bold
                };

                return foldoutButton;
            }
        }

        public static GUIStyle FoldoutIcon
        {
            get
            {
                if (foldoutIcon != null) return foldoutIcon;

                foldoutIcon = new GUIStyle {padding = new RectOffset(0, 0, 5, 0)};

                return foldoutIcon;
            }
        }

        public static GUIStyle InstalledIcon
        {
            get
            {
                if (installedIcon != null) return installedIcon;

                installedIcon = new GUIStyle {padding = new RectOffset(0, 0, 3, 0), fixedWidth = 30, fixedHeight = 30};

                return installedIcon;
            }
        }

        public static GUIStyle HeaderLabel
        {
            get
            {
                if (headerLabel != null) return headerLabel;
                headerLabel = new GUIStyle(EditorStyles.label) {fontSize = 13, fontStyle = FontStyle.Bold, padding = new RectOffset(0, 0, 6, 0)};
                return headerLabel;
            }
        }

        public static GUIStyle RichLabel
        {
            get
            {
                if (richLabel != null) return richLabel;
                richLabel = new GUIStyle(EditorStyles.label) {richText = true};
                return richLabel;
            }
        }

        public static GUIStyle RichLabelHelpBox
        {
            get
            {
                if (richLabelHelpBox != null) return richLabelHelpBox;
                richLabelHelpBox = new GUIStyle(EditorStyles.helpBox) {richText = true};
                return richLabelHelpBox;
            }
        }

        public static GUIStyle BoldRichLabel
        {
            get
            {
                if (boldRichLabel != null) return boldRichLabel;
                boldRichLabel = new GUIStyle(EditorStyles.label) {fontStyle = FontStyle.Bold, richText = true};
                return boldRichLabel;
            }
        }

        public static GUIStyle CenterRichLabel
        {
            get
            {
                if (centerRichLabel != null) return centerRichLabel;
                centerRichLabel = new GUIStyle(EditorStyles.label) {richText = true, alignment = TextAnchor.MiddleCenter};
                return centerRichLabel;
            }
        }

        #endregion


        #region color

        // ReSharper disable InconsistentNaming
        public static Color Slate_50 = FromHtml("#f8fafc");
        public static Color Slate_100 = FromHtml("#f1f5f9");
        public static Color Slate_200 = FromHtml("#e2e8f0");
        public static Color Slate_300 = FromHtml("#cbd5e1");
        public static Color Slate_400 = FromHtml("#94a3b8");
        public static Color Slate_500 = FromHtml("#64748b");
        public static Color Slate_600 = FromHtml("#475569");
        public static Color Slate_700 = FromHtml("#334155");
        public static Color Slate_800 = FromHtml("#1e293b");
        public static Color Slate_900 = FromHtml("#0f172a");
        public static Color Slate_950 = FromHtml("#020617");
        
        public static Color Gray_50 = FromHtml("#f9fafb");
        public static Color Gray_100 = FromHtml("#f3f4f6");
        public static Color Gray_200 = FromHtml("#e5e7eb");
        public static Color Gray_300 = FromHtml("#d1d5db");
        public static Color Gray_400 = FromHtml("#9ca3af");
        public static Color Gray_500 = FromHtml("#6b7280");
        public static Color Gray_600 = FromHtml("#4b5563");
        public static Color Gray_700 = FromHtml("#374151");
        public static Color Gray_800 = FromHtml("#1f2937");
        public static Color Gray_900 = FromHtml("#111827");
        public static Color Gray_950 = FromHtml("#030712");

        public static Color Zinc_50 = FromHtml("#fafafa");
        public static Color Zinc_100 = FromHtml("#f4f4f5");
        public static Color Zinc_200 = FromHtml("#e4e4e7");
        public static Color Zinc_300 = FromHtml("#d4d4d8");
        public static Color Zinc_400 = FromHtml("#a1a1aa");
        public static Color Zinc_500 = FromHtml("#71717a");
        public static Color Zinc_600 = FromHtml("#52525b");
        public static Color Zinc_700 = FromHtml("#3f3f46");
        public static Color Zinc_800 = FromHtml("#27272a");
        public static Color Zinc_900 = FromHtml("#18181b");
        public static Color Zinc_950 = FromHtml("#09090b");

        public static Color Neutral_50 = FromHtml("#fafafa");
        public static Color Neutral_100 = FromHtml("#f5f5f5");
        public static Color Neutral_200 = FromHtml("#e5e5e5");
        public static Color Neutral_300 = FromHtml("#d4d4d4");
        public static Color Neutral_400 = FromHtml("#a3a3a3");
        public static Color Neutral_500 = FromHtml("#737373");
        public static Color Neutral_600 = FromHtml("#525252");
        public static Color Neutral_700 = FromHtml("#404040");
        public static Color Neutral_800 = FromHtml("#262626");
        public static Color Neutral_900 = FromHtml("#171717");
        public static Color Neutral_950 = FromHtml("#0a0a0a");

        public static Color Stone_50 = FromHtml("#fafaf9");
        public static Color Stone_100 = FromHtml("#f5f5f4");
        public static Color Stone_200 = FromHtml("#e7e5e4");
        public static Color Stone_300 = FromHtml("#d6d3d1");
        public static Color Stone_400 = FromHtml("#a8a29e");
        public static Color Stone_500 = FromHtml("#78716c");
        public static Color Stone_600 = FromHtml("#57534e");
        public static Color Stone_700 = FromHtml("#44403c");
        public static Color Stone_800 = FromHtml("#292524");
        public static Color Stone_900 = FromHtml("#1c1917");
        public static Color Stone_950 = FromHtml("#0c0a09");

        public static Color Red_50 = FromHtml("#fef2f2");
        public static Color Red_100 = FromHtml("#fee2e2");
        public static Color Red_200 = FromHtml("#fecaca");
        public static Color Red_300 = FromHtml("#fca5a5");
        public static Color Red_400 = FromHtml("#f87171");
        public static Color Red_500 = FromHtml("#ef4444");
        public static Color Red_600 = FromHtml("#dc2626");
        public static Color Red_700 = FromHtml("#b91c1c");
        public static Color Red_800 = FromHtml("#991b1b");
        public static Color Red_900 = FromHtml("#7f1d1d");
        public static Color Red_950 = FromHtml("#450a0a");

        public static Color Orange_50 = FromHtml("#fff7ed");
        public static Color Orange_100 = FromHtml("#ffedd5");
        public static Color Orange_200 = FromHtml("#fed7aa");
        public static Color Orange_300 = FromHtml("#fdba74");
        public static Color Orange_400 = FromHtml("#fb923c");
        public static Color Orange_500 = FromHtml("#f97316");
        public static Color Orange_600 = FromHtml("#ea580c");
        public static Color Orange_700 = FromHtml("#c2410c");
        public static Color Orange_800 = FromHtml("#9a3412");
        public static Color Orange_900 = FromHtml("#7c2d12");
        public static Color Orange_950 = FromHtml("#431407");

        public static Color Amber_50 = FromHtml("#fffbeb");
        public static Color Amber_100 = FromHtml("#fef3c7");
        public static Color Amber_200 = FromHtml("#fde68a");
        public static Color Amber_300 = FromHtml("#fcd34d");
        public static Color Amber_400 = FromHtml("#fbbf24");
        public static Color Amber_500 = FromHtml("#f59e0b");
        public static Color Amber_600 = FromHtml("#d97706");
        public static Color Amber_700 = FromHtml("#b45309");
        public static Color Amber_800 = FromHtml("#92400e");
        public static Color Amber_900 = FromHtml("#78350f");
        public static Color Amber_950 = FromHtml("#451a03");

        public static Color Yellow_50 = FromHtml("#fefce8");
        public static Color Yellow_100 = FromHtml("#fef9c3");
        public static Color Yellow_200 = FromHtml("#fef08a");
        public static Color Yellow_300 = FromHtml("#fde047");
        public static Color Yellow_400 = FromHtml("#facc15");
        public static Color Yellow_500 = FromHtml("#eab308");
        public static Color Yellow_600 = FromHtml("#ca8a04");
        public static Color Yellow_700 = FromHtml("#a16207");
        public static Color Yellow_800 = FromHtml("#854d0e");
        public static Color Yellow_900 = FromHtml("#713f12");
        public static Color Yellow_950 = FromHtml("#422006");

        public static Color Lime_50 = FromHtml("#f7fee7");
        public static Color Lime_100 = FromHtml("#ecfccb");
        public static Color Lime_200 = FromHtml("#d9f99d");
        public static Color Lime_300 = FromHtml("#bef264");
        public static Color Lime_400 = FromHtml("#a3e635");
        public static Color Lime_500 = FromHtml("#84cc16");
        public static Color Lime_600 = FromHtml("#65a30d");
        public static Color Lime_700 = FromHtml("#4d7c0f");
        public static Color Lime_800 = FromHtml("#3f6212");
        public static Color Lime_900 = FromHtml("#365314");
        public static Color Lime_950 = FromHtml("#1a2e05");

        public static Color Green_50 = FromHtml("#f0fdf4");
        public static Color Green_100 = FromHtml("#dcfce7");
        public static Color Green_200 = FromHtml("#bbf7d0");
        public static Color Green_300 = FromHtml("#86efac");
        public static Color Green_400 = FromHtml("#4ade80");
        public static Color Green_500 = FromHtml("#22c55e");
        public static Color Green_600 = FromHtml("#16a34a");
        public static Color Green_700 = FromHtml("#15803d");
        public static Color Green_800 = FromHtml("#166534");
        public static Color Green_900 = FromHtml("#14532d");
        public static Color Green_950 = FromHtml("#052e16");

        public static Color Emerald_50 = FromHtml("#ecfdf5");
        public static Color Emerald_100 = FromHtml("#d1fae5");
        public static Color Emerald_200 = FromHtml("#a7f3d0");
        public static Color Emerald_300 = FromHtml("#6ee7b7");
        public static Color Emerald_400 = FromHtml("#34d399");
        public static Color Emerald_500 = FromHtml("#10b981");
        public static Color Emerald_600 = FromHtml("#059669");
        public static Color Emerald_700 = FromHtml("#047857");
        public static Color Emerald_800 = FromHtml("#065f46");
        public static Color Emerald_900 = FromHtml("#064e3b");
        public static Color Emerald_950 = FromHtml("#022c22");

        public static Color Teal_50 = FromHtml("#f0fdfa");
        public static Color Teal_100 = FromHtml("#ccfbf1");
        public static Color Teal_200 = FromHtml("#99f6e4");
        public static Color Teal_300 = FromHtml("#5eead4");
        public static Color Teal_400 = FromHtml("#2dd4bf");
        public static Color Teal_500 = FromHtml("#14b8a6");
        public static Color Teal_600 = FromHtml("#0d9488");
        public static Color Teal_700 = FromHtml("#0f766e");
        public static Color Teal_800 = FromHtml("#115e59");
        public static Color Teal_900 = FromHtml("#134e4a");
        public static Color Teal_950 = FromHtml("#042f2e");

        public static Color Cyan_50 = FromHtml("#ecfeff");
        public static Color Cyan_100 = FromHtml("#cffafe");
        public static Color Cyan_200 = FromHtml("#a5f3fc");
        public static Color Cyan_300 = FromHtml("#67e8f9");
        public static Color Cyan_400 = FromHtml("#22d3ee");
        public static Color Cyan_500 = FromHtml("#06b6d4");
        public static Color Cyan_600 = FromHtml("#0891b2");
        public static Color Cyan_700 = FromHtml("#0e7490");
        public static Color Cyan_800 = FromHtml("#155e75");
        public static Color Cyan_900 = FromHtml("#164e63");
        public static Color Cyan_950 = FromHtml("#083344");

        public static Color Sky_50 = FromHtml("#f0f9ff");
        public static Color Sky_100 = FromHtml("#e0f2fe");
        public static Color Sky_200 = FromHtml("#bae6fd");
        public static Color Sky_300 = FromHtml("#7dd3fc");
        public static Color Sky_400 = FromHtml("#38bdf8");
        public static Color Sky_500 = FromHtml("#0ea5e9");
        public static Color Sky_600 = FromHtml("#0284c7");
        public static Color Sky_700 = FromHtml("#0369a1");
        public static Color Sky_800 = FromHtml("#075985");
        public static Color Sky_900 = FromHtml("#0c4a6e");
        public static Color Sky_950 = FromHtml("#082f49");

        public static Color Blue_50 = FromHtml("#eff6ff");
        public static Color Blue_100 = FromHtml("#dbeafe");
        public static Color Blue_200 = FromHtml("#bfdbfe");
        public static Color Blue_300 = FromHtml("#93c5fd");
        public static Color Blue_400 = FromHtml("#60a5fa");
        public static Color Blue_500 = FromHtml("#3b82f6");
        public static Color Blue_600 = FromHtml("#2563eb");
        public static Color Blue_700 = FromHtml("#1d4ed8");
        public static Color Blue_800 = FromHtml("#1e40af");
        public static Color Blue_900 = FromHtml("#1e3a8a");
        public static Color Blue_950 = FromHtml("#172554");

        public static Color Indigo_50 = FromHtml("#eef2ff");
        public static Color Indigo_100 = FromHtml("#e0e7ff");
        public static Color Indigo_200 = FromHtml("#c7d2fe");
        public static Color Indigo_300 = FromHtml("#a5b4fc");
        public static Color Indigo_400 = FromHtml("#818cf8");
        public static Color Indigo_500 = FromHtml("#6366f1");
        public static Color Indigo_600 = FromHtml("#4f46e5");
        public static Color Indigo_700 = FromHtml("#4338ca");
        public static Color Indigo_800 = FromHtml("#3730a3");
        public static Color Indigo_900 = FromHtml("#312e81");
        public static Color Indigo_950 = FromHtml("#1e1b4b");

        public static Color Violet_50 = FromHtml("#f5f3ff");
        public static Color Violet_100 = FromHtml("#ede9fe");
        public static Color Violet_200 = FromHtml("#ddd6fe");
        public static Color Violet_300 = FromHtml("#c4b5fd");
        public static Color Violet_400 = FromHtml("#a78bfa");
        public static Color Violet_500 = FromHtml("#8b5cf6");
        public static Color Violet_600 = FromHtml("#7c3aed");
        public static Color Violet_700 = FromHtml("#6d28d9");
        public static Color Violet_800 = FromHtml("#5b21b6");
        public static Color Violet_900 = FromHtml("#4c1d95");
        public static Color Violet_950 = FromHtml("#2e1065");

        public static Color Purple_50 = FromHtml("#faf5ff");
        public static Color Purple_100 = FromHtml("#f3e8ff");
        public static Color Purple_200 = FromHtml("#e9d5ff");
        public static Color Purple_300 = FromHtml("#d8b4fe");
        public static Color Purple_400 = FromHtml("#c084fc");
        public static Color Purple_500 = FromHtml("#a855f7");
        public static Color Purple_600 = FromHtml("#9333ea");
        public static Color Purple_700 = FromHtml("#7e22ce");
        public static Color Purple_800 = FromHtml("#6b21a8");
        public static Color Purple_900 = FromHtml("#581c87");
        public static Color Purple_950 = FromHtml("#3b0764");

        public static Color Fuchsia_50 = FromHtml("#fdf4ff");
        public static Color Fuchsia_100 = FromHtml("#fae8ff");
        public static Color Fuchsia_200 = FromHtml("#f5d0fe");
        public static Color Fuchsia_300 = FromHtml("#f0abfc");
        public static Color Fuchsia_400 = FromHtml("#e879f9");
        public static Color Fuchsia_500 = FromHtml("#d946ef");
        public static Color Fuchsia_600 = FromHtml("#c026d3");
        public static Color Fuchsia_700 = FromHtml("#a21caf");
        public static Color Fuchsia_800 = FromHtml("#86198f");
        public static Color Fuchsia_900 = FromHtml("#701a75");
        public static Color Fuchsia_950 = FromHtml("#4a044e");

        public static Color Pink_50 = FromHtml("#fdf2f8");
        public static Color Pink_100 = FromHtml("#fce7f3");
        public static Color Pink_200 = FromHtml("#fbcfe8");
        public static Color Pink_300 = FromHtml("#f9a8d4");
        public static Color Pink_400 = FromHtml("#f472b6");
        public static Color Pink_500 = FromHtml("#ec4899");
        public static Color Pink_600 = FromHtml("#db2777");
        public static Color Pink_700 = FromHtml("#be185d");
        public static Color Pink_800 = FromHtml("#9d174d");
        public static Color Pink_900 = FromHtml("#831843");
        public static Color Pink_950 = FromHtml("#500724");

        public static Color Rose_50 = FromHtml("#fff1f2");
        public static Color Rose_100 = FromHtml("#ffe4e6");
        public static Color Rose_200 = FromHtml("#fecdd3");
        public static Color Rose_300 = FromHtml("#fda4af");
        public static Color Rose_400 = FromHtml("#fb7185");
        public static Color Rose_500 = FromHtml("#f43f5e");
        public static Color Rose_600 = FromHtml("#e11d48");
        public static Color Rose_700 = FromHtml("#be123c");
        public static Color Rose_800 = FromHtml("#9f1239");
        public static Color Rose_900 = FromHtml("#881337");
        public static Color Rose_950 = FromHtml("#4c0519");
// ReSharper restore InconsistentNaming

        private static Color FromHtml(string hex) =>
            new(Convert.ToByte(hex[1..3], 16) / 255.0f, Convert.ToByte(hex[3..5], 16) / 255.0f, Convert.ToByte(hex[5..7], 16) / 255.0f);

        public static Color Success => Green_500;
        public static Color Error => Red_500;
        public static Color Warning => Orange_500;
        public static Color Notice => Cyan_500;
        public static Color HighlightBackground => EditorGUIUtility.isProSkin ? new Color(0.17f, 0.36f, 0.53f) : new Color(0.23f, 0.45f, 0.69f);
        public static Color HighlightBackgroundInactive => EditorGUIUtility.isProSkin ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.68f, 0.68f, 0.68f);
        public static Color WindowBackground => EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.78f, 0.78f, 0.78f);

        #endregion


        #region draw

        public static void DrawBox(Rect position, GUIStyle style, bool isHover = false, bool isActive = false, bool on = false, bool hasKeyboardFocus = false)
        {
            if (Event.current.type == EventType.Repaint)
            {
                style.Draw(position,
                    GUIContent.none,
                    isHover,
                    isActive,
                    on,
                    hasKeyboardFocus);
            }
        }

        /// <summary>
        /// Icon content
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tooltip"></param>
        /// <returns></returns>
        public static GUIContent IconContent(string name, string tooltip = "")
        {
            if (CachedIconContent.TryGetValue(name, out var result)) return result ?? GUIContent.none;
            var builtinIcon = EditorGUIUtility.IconContent(name) ?? new GUIContent(Texture2D.whiteTexture);
            result = new GUIContent(builtinIcon.image, tooltip);
            CachedIconContent.Add(name, result);
            return result;
        }

        /// <summary>
        /// Draw header with title
        /// </summary>
        /// <param name="title"></param>
        public static void DrawFooter(string title)
        {
            var bgStyle = new GUIStyle(GUIStyle.none) {normal = {background = EditorCreator.CreateTexture(Zinc_800)}};
            GUILayout.BeginVertical(bgStyle, GUILayout.Height(20));

            var titleStyle = new GUIStyle(GUIStyle.none)
            {
                margin = new RectOffset(4, 4, 4, 4),
                padding = new RectOffset(0, 8, 0, 0),
                fontSize = 9,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleRight,
                wordWrap = false,
                richText = true,
                imagePosition = ImagePosition.TextOnly,
                stretchHeight = true,
                stretchWidth = true,
                normal = {textColor = Color.white}
            };

            EditorGUILayout.LabelField(title, titleStyle);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draw only the property specified.
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="fieldName"></param>
        /// <param name="isReadOnly"></param>
        public static void DrawOnlyField(SerializedObject serializedObject, string fieldName, bool isReadOnly)
        {
            serializedObject.Update();
            var prop = serializedObject.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    if (prop.name != fieldName) continue;

                    GUI.enabled = !isReadOnly;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(prop.name), true);
                    GUI.enabled = true;
                } while (prop.NextVisible(false));
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draw property field with validate wrong value
        /// </summary>
        /// <param name="property"></param>
        /// <param name="error"></param>
        /// <param name="warning"></param>
        /// <param name="onError"></param>
        /// <param name="onWarning"></param>
        public static void DrawPropertyFieldValidate(SerializedProperty property, bool error, bool warning = false, Action onError = null, Action onWarning = null)
        {
            if (error) GUI.color = Error;
            else if (warning) GUI.color = Warning;
            EditorGUILayout.PropertyField(property, true);
            GUI.color = Color.white;
            if (error) onError?.Invoke();
            else if (warning) onWarning?.Invoke();
        }

        /// <summary>
        /// Draws a line in the inspector.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="height"></param>
        public static void DrawLine(Color color, float height = 1f)
        {
            var rect = EditorGUILayout.GetControlRect(false, height);
            rect.height = height;
            EditorGUI.DrawRect(rect, color);
        }

        /// <summary>
        /// Draws a line in the inspector.
        /// </summary>
        /// <param name="height"></param>
        public static void DrawLine(float height = 1f) { DrawLine(new Color(0.5f, 0.5f, 0.5f, 1f), height); }

        /// <summary>
        /// Draws all properties like base.OnInspectorGUI() but excludes a field by name.
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="fieldToSkip">The name of the field that should be excluded. Example: "m_Script" will skip the default Script field.</param>
        public static void DrawInspectorExcept(SerializedObject serializedObject, string fieldToSkip) { DrawInspectorExcept(serializedObject, new[] {fieldToSkip}); }

        /// <summary>
        /// Draws all properties like base.OnInspectorGUI() but excludes the specified fields by name.
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="fieldsToSkip">
        /// An array of names that should be excluded.
        /// Example: new string[] { "m_Script" , "myInt" } will skip the default Script field and the Integer field myInt.
        /// </param>
        public static void DrawInspectorExcept(SerializedObject serializedObject, string[] fieldsToSkip)
        {
            serializedObject.Update();
            var prop = serializedObject.GetIterator();
            if (prop.NextVisible(true))
            {
                do
                {
                    if (fieldsToSkip.Any(prop.name.Contains)) continue;

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(prop.name), true);
                } while (prop.NextVisible(false));
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draw group selection with header
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sectionName"></param>
        /// <param name="drawer"></param>
        /// <param name="defaultFoldout"></param>
        public static void DrawGroupFoldout(string key, string sectionName, Action drawer, bool defaultFoldout = true)
        {
            bool foldout = GetFoldoutState(key, defaultFoldout);

            var rect = EditorGUILayout.BeginVertical(GUILayout.MinHeight(foldout ? 20 : 0));
            {
                GUI.Box(rect, GUIContent.none);
                EditorGUILayout.BeginHorizontal();

                var area = EditorGUILayout.BeginVertical();
                {
                    GUI.Box(new Rect(area) {xMin = 18}, GUIContent.none);
                    if (GUILayout.Button($"    {sectionName}", FoldoutButton)) SetFoldoutState(key, !foldout);
                }
                EditorGUILayout.EndVertical();

                var buttonRect = GUILayoutUtility.GetLastRect();
                var iconRect = new Rect(buttonRect.x, buttonRect.y, 10, buttonRect.height);
                GUI.Label(iconRect, foldout ? IconContent("d_IN_foldout_act_on") : IconContent("d_IN_foldout"), FoldoutIcon);

                EditorGUILayout.EndHorizontal();

                if (foldout) GUILayout.Space(4);
                if (foldout && drawer != null) drawer();
            }
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
        public static void DrawGroupFoldoutWithRightClick(string key, string sectionName, Action drawer, Action actionRightClick, bool defaultFoldout = true)
        {
            bool foldout = GetFoldoutState(key, defaultFoldout);

            var rect = EditorGUILayout.BeginVertical(GUILayout.MinHeight(foldout ? 20 : 0));
            {
                GUI.Box(rect, GUIContent.none);
                EditorGUILayout.BeginHorizontal();

                var area = EditorGUILayout.BeginVertical();
                {
                    GUI.Box(new Rect(area) {xMin = 18}, GUIContent.none);
                    if (GUILayout.Button($"    {sectionName}", FoldoutButton))
                    {
                        if (Event.current.button == 1) actionRightClick?.Invoke();

                        SetFoldoutState(key, !foldout);
                    }
                }
                EditorGUILayout.EndVertical();

                var buttonRect = GUILayoutUtility.GetLastRect();
                var iconRect = new Rect(buttonRect.x, buttonRect.y, 10, buttonRect.height);
                GUI.Label(iconRect, foldout ? IconContent("d_IN_foldout_act_on") : IconContent("d_IN_foldout"), FoldoutIcon);

                EditorGUILayout.EndHorizontal();

                if (foldout) GUILayout.Space(4);
                if (foldout && drawer != null) drawer();
            }

            EditorGUILayout.EndVertical();
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
        /// Centers a rect inside another window.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static Rect CenterInWindow(Rect window, Rect origin)
        {
            var pos = window;
            float w = (origin.width - pos.width) * 0.5f;
            float h = (origin.height - pos.height) * 0.5f;
            pos.x = origin.x + w;
            pos.y = origin.y + h;
            return pos;
        }

        /// <summary>
        /// Draw label installed and icon
        /// </summary>
        /// <param name="version"></param>
        /// <param name="labelMargin"></param>
        public static void DrawInstalled(string version, RectOffset labelMargin = null)
        {
            EditorGUILayout.BeginHorizontal();
            labelMargin ??= new RectOffset(0, 0, 0, 0);
            GUILayout.Label(version, new GUIStyle(EditorStyles.linkLabel) {alignment = TextAnchor.MiddleLeft, margin = labelMargin});
            var lastRect = GUILayoutUtility.GetLastRect();
            var iconRect = new Rect(lastRect.x + EditorStyles.label.CalcSize(new GUIContent(version)).x + 2f, lastRect.y + 1.3f, 10, lastRect.height);
            GUI.Label(iconRect, IconContent("CollabNew"), InstalledIcon);
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawTitleField(string title, Rect rect = default)
        {
            var area = EditorGUILayout.BeginVertical();
            {
                GUI.Box(rect == default ? new Rect(area) {xMin = 18} : rect, GUIContent.none);
                var targetStyle = new GUIStyle {fontSize = 11, normal = {textColor = Color.white}, alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold};
                if (rect == default) EditorGUILayout.LabelField(title, targetStyle);
                else EditorGUI.LabelField(rect, title, targetStyle);
            }
            EditorGUILayout.EndVertical();
        }

        public static GUIStyle GetTabStyle(int i, int length)
        {
            if (length == 1) return "Tab onlyOne";
            if (i == 0) return "Tab first";
            if (i == length - 1) return "Tab last";
            return "Tab middle";
        }

        #endregion


        #region foldout state

        private static bool GetFoldoutState(string key, bool defaultFoldout = true)
        {
            if (!FoldoutSettings.ContainsKey(key)) FoldoutSettings.Add(key, defaultFoldout);
            return FoldoutSettings[key];
        }

        private static void SetFoldoutState(string key, bool state)
        {
            if (!FoldoutSettings.ContainsKey(key))
            {
                FoldoutSettings.Add(key, state);
            }
            else
            {
                FoldoutSettings[key] = state;
            }
        }

        [Serializable]
        public class FoldoutState
        {
            public string key;
            public bool state;
        }

        [Serializable]
        public class UniformFoldoutState
        {
            public List<FoldoutState> uniformFoldouts;

            public UniformFoldoutState() { uniformFoldouts = new List<FoldoutState>(); }

            public bool ContainsKey(string key) { return uniformFoldouts.Any(foldoutState => foldoutState.key.Equals(key)); }

            public void Add(string key, bool value) { uniformFoldouts.Add(new FoldoutState {key = key, state = value}); }

            public bool this[string key]
            {
                get
                {
                    foreach (var foldoutState in uniformFoldouts)
                    {
                        if (foldoutState.key.Equals(key))
                        {
                            return foldoutState.state;
                        }
                    }

                    return false;
                }
                set
                {
                    foreach (var foldoutState in uniformFoldouts)
                    {
                        if (foldoutState.key.Equals(key))
                        {
                            foldoutState.state = value;
                            break;
                        }
                    }
                }
            }
        }

        #endregion


        #region gui

        public static void ResetGUIEnabled() => GUI.enabled = prevGuiEnabled;

        public static void SetGUIEnabled(bool enabled)
        {
            prevGuiEnabled = GUI.enabled;
            GUI.enabled = enabled;
        }

        public static void Space(float value = 4) => GUILayout.Space(value);

        #endregion
    }
}