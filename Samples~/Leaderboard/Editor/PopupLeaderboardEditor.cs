#if UNITY_EDITOR
using Pancake.GameService;
using Pancake.UI;
using TMPro;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using CountryCode = Pancake.GameService.CountryCode;

namespace Pancake.Editor
{
    [CustomEditor(typeof(PopupLeaderboard))]
    public class PopupLeaderboardEditor : UnityEditor.Editor
    {
        private SerializedProperty _countryCode;
        private SerializedProperty _btnNextPage;
        private SerializedProperty _btnBackPage;
        private SerializedProperty _btnWorld;
        private SerializedProperty _btnCountry;
        private SerializedProperty _btnFriend;
        private SerializedProperty _txtName;
        private SerializedProperty _txtRank;
        private SerializedProperty _txtCurrentPage;
        private SerializedProperty _rankSlots;

        private SerializedProperty _colorRank1Bg;
        private SerializedProperty _colorRank1Overlay;
        private SerializedProperty _colorRank1Boder;
        private SerializedProperty _colorRank1Header;
        private SerializedProperty _colorRank1Text;
        private SerializedProperty _colorRank2Bg;
        private SerializedProperty _colorRank2Overlay;
        private SerializedProperty _colorRank2Boder;
        private SerializedProperty _colorRank2Header;
        private SerializedProperty _colorRank2Text;
        private SerializedProperty _colorRank3Bg;
        private SerializedProperty _colorRank3Overlay;
        private SerializedProperty _colorRank3Boder;
        private SerializedProperty _colorRank3Header;
        private SerializedProperty _colorRank3Text;
        private SerializedProperty _colorRankYouBg;
        private SerializedProperty _colorRankYouOverlay;
        private SerializedProperty _colorRankYouBoder;
        private SerializedProperty _colorRankYouHeader;
        private SerializedProperty _colorRankYouText;
        private SerializedProperty _colorOutRankBg;
        private SerializedProperty _colorOutRankOverlay;
        private SerializedProperty _colorOutRankBoder;
        private SerializedProperty _colorOutRankHeader;
        private SerializedProperty _colorOutRankText;

        private SerializedProperty _txtWarning;
        private SerializedProperty _block;
        private SerializedProperty _content;
        private SerializedProperty _spriteTabNormal;
        private SerializedProperty _spriteTabSelected;
        private SerializedProperty _colorTabTextNormal;
        private SerializedProperty _colorTabTextSelected;
        private SerializedProperty _nameTableLeaderboard;
        private SerializedProperty _displayRankCurve;
        private ReorderableList _rankSlotList;

        private const int DEFAULT_LABEL_WIDTH = 110;

        protected virtual void OnEnable()
        {
            _btnNextPage = serializedObject.FindProperty("btnNextPage");
            _btnBackPage = serializedObject.FindProperty("btnBackPage");
            _countryCode = serializedObject.FindProperty("countryCode");
            _btnWorld = serializedObject.FindProperty("btnWorld");
            _btnCountry = serializedObject.FindProperty("btnCountry");
            _btnFriend = serializedObject.FindProperty("btnFriend");
            _txtName = serializedObject.FindProperty("txtName");
            _txtRank = serializedObject.FindProperty("txtRank");
            _txtCurrentPage = serializedObject.FindProperty("txtCurrentPage");
            _rankSlots = serializedObject.FindProperty("rankSlots");
            var colorRank1 = serializedObject.FindProperty("colorRank1");
            _colorRank1Bg = colorRank1.FindPropertyRelative("colorBackground");
            _colorRank1Overlay = colorRank1.FindPropertyRelative("colorOverlay");
            _colorRank1Boder = colorRank1.FindPropertyRelative("colorBoder");
            _colorRank1Header = colorRank1.FindPropertyRelative("colorHeader");
            _colorRank1Text = colorRank1.FindPropertyRelative("colorText");
            var colorRank2 = serializedObject.FindProperty("colorRank2");
            _colorRank2Bg = colorRank2.FindPropertyRelative("colorBackground");
            _colorRank2Overlay = colorRank2.FindPropertyRelative("colorOverlay");
            _colorRank2Boder = colorRank2.FindPropertyRelative("colorBoder");
            _colorRank2Header = colorRank2.FindPropertyRelative("colorHeader");
            _colorRank2Text = colorRank2.FindPropertyRelative("colorText");
            var colorRank3 = serializedObject.FindProperty("colorRank3");
            _colorRank3Bg = colorRank3.FindPropertyRelative("colorBackground");
            _colorRank3Overlay = colorRank3.FindPropertyRelative("colorOverlay");
            _colorRank3Boder = colorRank3.FindPropertyRelative("colorBoder");
            _colorRank3Header = colorRank3.FindPropertyRelative("colorHeader");
            _colorRank3Text = colorRank3.FindPropertyRelative("colorText");
            var colorOutRank = serializedObject.FindProperty("colorOutRank");
            _colorOutRankBg = colorOutRank.FindPropertyRelative("colorBackground");
            _colorOutRankOverlay = colorOutRank.FindPropertyRelative("colorOverlay");
            _colorOutRankBoder = colorOutRank.FindPropertyRelative("colorBoder");
            _colorOutRankHeader = colorOutRank.FindPropertyRelative("colorHeader");
            _colorOutRankText = colorOutRank.FindPropertyRelative("colorText");
            var colorRankYou = serializedObject.FindProperty("colorRankYou");
            _colorRankYouBg = colorRankYou.FindPropertyRelative("colorBackground");
            _colorRankYouOverlay = colorRankYou.FindPropertyRelative("colorOverlay");
            _colorRankYouBoder = colorRankYou.FindPropertyRelative("colorBoder");
            _colorRankYouHeader = colorRankYou.FindPropertyRelative("colorHeader");
            _colorRankYouText = colorRankYou.FindPropertyRelative("colorText");
            _txtWarning = serializedObject.FindProperty("txtWarning");
            _block = serializedObject.FindProperty("block");
            _content = serializedObject.FindProperty("content");
            _nameTableLeaderboard = serializedObject.FindProperty("nameTableLeaderboard");
            _displayRankCurve = serializedObject.FindProperty("displayRankCurve");
            _spriteTabNormal = serializedObject.FindProperty("spriteTabNormal");
            _spriteTabSelected = serializedObject.FindProperty("spriteTabSelected");
            _colorTabTextNormal = serializedObject.FindProperty("colorTabTextNormal");
            _colorTabTextSelected = serializedObject.FindProperty("colorTabTextSelected");

            _rankSlotList = new ReorderableList(serializedObject,
                _rankSlots,
                true,
                true,
                true,
                true);
            _rankSlotList.drawElementCallback = DrawListRankItem;
            _rankSlotList.drawHeaderCallback = DrawRankHeader;
        }

        private void DrawRankHeader(Rect rect) { EditorGUI.LabelField(rect, "Rank Slots"); }

        private void DrawListRankItem(Rect rect, int index, bool isactive, bool isfocused)
        {
            SerializedProperty element = _rankSlotList.serializedProperty.GetArrayElementAtIndex(index); //The element in the list
            EditorGUI.PropertyField(rect, element, new GUIContent(element.displayName), element.isExpanded);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            OnDrawExtraSetting();
            Repaint();
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        protected virtual void OnDrawExtraSetting()
        {
            Uniform.SpaceOneLine();
            Uniform.DrawGroupFoldout("UIPOPUP_LEADERBOARD", "LEADERBOARD SETTING", DrawSetting);
        }

        private void DrawSetting()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Country Code", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _countryCode.objectReferenceValue = EditorGUILayout.ObjectField(_countryCode.objectReferenceValue, typeof(CountryCode), allowSceneObjects: false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Next Page", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _btnNextPage.objectReferenceValue = EditorGUILayout.ObjectField(_btnNextPage.objectReferenceValue, typeof(UIButton), allowSceneObjects: true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Back Page", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _btnBackPage.objectReferenceValue = EditorGUILayout.ObjectField(_btnBackPage.objectReferenceValue, typeof(UIButton), allowSceneObjects: true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Button World", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _btnWorld.objectReferenceValue = EditorGUILayout.ObjectField(_btnWorld.objectReferenceValue, typeof(UIButton), allowSceneObjects: true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Button Country", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _btnCountry.objectReferenceValue = EditorGUILayout.ObjectField(_btnCountry.objectReferenceValue, typeof(UIButton), allowSceneObjects: true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Button Friend", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _btnFriend.objectReferenceValue = EditorGUILayout.ObjectField(_btnFriend.objectReferenceValue, typeof(UIButton), allowSceneObjects: true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Name Text", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _txtName.objectReferenceValue = EditorGUILayout.ObjectField(_txtName.objectReferenceValue, typeof(TextMeshProUGUI), allowSceneObjects: true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Rank Text", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _txtRank.objectReferenceValue = EditorGUILayout.ObjectField(_txtRank.objectReferenceValue, typeof(TextMeshProUGUI), allowSceneObjects: true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Page Text", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _txtCurrentPage.objectReferenceValue = EditorGUILayout.ObjectField(_txtCurrentPage.objectReferenceValue, typeof(TextMeshProUGUI), allowSceneObjects: true);
            EditorGUILayout.EndHorizontal();

            _rankSlotList.DoLayoutList();

            void DrawColor(SerializedProperty bg, SerializedProperty overlay, SerializedProperty boder, SerializedProperty header, SerializedProperty text)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Background", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                bg.colorValue = EditorGUILayout.ColorField(bg.colorValue);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Overlay", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                overlay.colorValue = EditorGUILayout.ColorField(overlay.colorValue);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Boder", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                boder.colorValue = EditorGUILayout.ColorField(boder.colorValue);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Header", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                header.colorValue = EditorGUILayout.ColorField(header.colorValue);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Text", GUILayout.Width(DEFAULT_LABEL_WIDTH));
                text.colorValue = EditorGUILayout.ColorField(text.colorValue);
                EditorGUILayout.EndHorizontal();
            }

            Uniform.DrawGroupFoldout("LB_COLOR_RANK1",
                "COLOR RANK 1",
                () =>
                {
                    DrawColor(_colorRank1Bg,
                        _colorRank1Overlay,
                        _colorRank1Boder,
                        _colorRank1Header,
                        _colorRank1Text);
                },
                false);


            Uniform.DrawGroupFoldout("LB_COLOR_RANK2",
                "COLOR RANK 2",
                () =>
                {
                    DrawColor(_colorRank2Bg,
                        _colorRank2Overlay,
                        _colorRank2Boder,
                        _colorRank2Header,
                        _colorRank2Text);
                },
                false);


            Uniform.DrawGroupFoldout("LB_COLOR_RANK3",
                "COLOR RANK 3",
                () =>
                {
                    DrawColor(_colorRank3Bg,
                        _colorRank3Overlay,
                        _colorRank3Boder,
                        _colorRank3Header,
                        _colorRank3Text);
                },
                false);

            Uniform.DrawGroupFoldout("LB_COLOR_RANKYOU",
                "COLOR RANK YOU",
                () =>
                {
                    DrawColor(_colorRankYouBg,
                        _colorRankYouOverlay,
                        _colorRankYouBoder,
                        _colorRankYouHeader,
                        _colorRankYouText);
                },
                false);

            Uniform.DrawGroupFoldout("LB_COLOR_OUTRANK",
                "COLOR OUT RANK",
                () =>
                {
                    DrawColor(_colorOutRankBg,
                        _colorOutRankOverlay,
                        _colorOutRankBoder,
                        _colorOutRankHeader,
                        _colorOutRankText);
                },
                false);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Warning Text", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _txtWarning.objectReferenceValue = EditorGUILayout.ObjectField(_txtWarning.objectReferenceValue, typeof(TextMeshProUGUI), allowSceneObjects: true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Block", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _block.objectReferenceValue = EditorGUILayout.ObjectField(_block.objectReferenceValue, typeof(GameObject), allowSceneObjects: true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Content", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _content.objectReferenceValue = EditorGUILayout.ObjectField(_content.objectReferenceValue, typeof(GameObject), allowSceneObjects: true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Table Name", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _nameTableLeaderboard.stringValue = EditorGUILayout.TextField(_nameTableLeaderboard.stringValue);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Curve", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _displayRankCurve.animationCurveValue = EditorGUILayout.CurveField(_displayRankCurve.animationCurveValue);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Tab Normal", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _spriteTabNormal.objectReferenceValue = EditorGUILayout.ObjectField(_spriteTabNormal.objectReferenceValue, typeof(Sprite), allowSceneObjects: false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Text Color", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _colorTabTextNormal.colorValue = EditorGUILayout.ColorField(_colorTabTextNormal.colorValue);
            EditorGUILayout.EndHorizontal();

            Uniform.SpaceOneLine();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Tab Selected", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _spriteTabSelected.objectReferenceValue = EditorGUILayout.ObjectField(_spriteTabSelected.objectReferenceValue, typeof(Sprite), allowSceneObjects: false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Text Color", GUILayout.Width(DEFAULT_LABEL_WIDTH));
            _colorTabTextSelected.colorValue = EditorGUILayout.ColorField(_colorTabTextSelected.colorValue);
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif