using System.Collections.Generic;
using System.Linq;
using PancakeEditor.Common;
using PancakeEditor.Localization;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.U2D;

// ReSharper disable UnusedMember.Local
namespace PancakeEditor
{
    public class Wizard : WindowBase
    {
        internal const float BUTTON_HEIGHT = 30f;

        private enum WizardType
        {
            All,
            Money,
            Setting,
            Tools
        }

        private enum WizardAllType
        {
            None = -1,
            Adjust,
            Advertisement,
            Build,
            Firebase,
            GameService,
            HeartSetting,
            HierarchySetting,
            InAppPurchase,
            InAppReview,
            LevelSystem,
            Localization,
            OtherPackage,
            ScreenSetting,
            Scriptable,
            Spine,
            Texture,
            UIDesign
        }

        private enum MonetizationType
        {
            Advertisement = WizardAllType.Advertisement,
            InAppPurchase = WizardAllType.InAppPurchase,
            Firebase = WizardAllType.Firebase,
            Adjust = WizardAllType.Adjust,
        }

        private enum ToolsType
        {
            InAppReview = WizardAllType.InAppReview,
            Spine = WizardAllType.Spine,
            GameSerice = WizardAllType.GameService,
            Localization = WizardAllType.Localization,
            OtherPacakge = WizardAllType.OtherPackage,
            Build = WizardAllType.Build
        }

        private enum SettingType
        {
            HeartConfig = WizardAllType.HeartSetting,
            Scriptable = WizardAllType.Scriptable,
            LevelSystem = WizardAllType.LevelSystem,
            ScreenSetting = WizardAllType.ScreenSetting,
            Texture = WizardAllType.Texture,
            HierarchySetting = WizardAllType.HierarchySetting
        }

        public enum LocaleTabType
        {
            Setting,
            Explore
        }

        public enum LevelEditorTabType
        {
            Setting,
            PickupArea
        }

        private Vector2 _leftSideScrollPosition = Vector2.zero;
        private Vector2 _rightSideScrollPosition = Vector2.zero;
        private List<int> _items;
        private WizardType _currentType = WizardType.All;
        private WizardAllType _selectedItemType = WizardAllType.HeartSetting;

        #region locale

        private LocaleTabType _currentLocaleTabType = LocaleTabType.Setting;
        private TreeViewState _treeViewState;
        internal LocaleTreeView localeTreeView;
        private SearchField _localeSearchField;
        private Rect BodyViewRect => new(0f, 48f, position.width - TAB_WIDTH * 4f - 22f, position.height - 172f);
        private Rect ToolbarRect => new(0f, 28f, position.width - TAB_WIDTH * 4f - 22f, 20f);
        private Rect BottomToolbarRect => new(0f, position.height - 118f, position.width - TAB_WIDTH * 4f - 22f, 20);
        private bool _localeInitialized;
        [SerializeField] private MultiColumnHeaderState multiColumnHeaderState;

        #endregion

        #region level-editor

        private LevelEditorTabType _currentLevelTabType = LevelEditorTabType.PickupArea;

        #endregion

        #region texture

        private SpriteAtlas _spriteAtlas;

        #endregion

        #region build

        private AndroidBuildPipelineSettings _currentAndroidBuildPipeline;

        #endregion

        private readonly Color[] _colors = {Uniform.RaisinBlack, Uniform.GothicOlive, Uniform.Maroon, Uniform.ElegantNavy, Uniform.CrystalPurple};
        private const float TAB_WIDTH = 50f;

        [SerializeField] private int tabIndex = -1;
        [SerializeField] private bool isInitialized;

        internal static Wizard window;

        [MenuItem("Tools/Pancake/Wizard %W", priority = 9000)]
        public new static void Show()
        {
            window = GetWindow<Wizard>("Wizard");
            window.autoRepaintOnSceneChange = true;
            window.Show(true);
        }

        protected override void Init()
        {
            base.Init();
            LevelSystemWindow.OnEnabled();
            if (isInitialized)
            {
                SelectTab(tabIndex);
                return;
            }

            SelectTab((int) _currentType, true);
            isInitialized = true;
            SessionState.SetBool("spine_flag", false);
            SessionState.SetBool("build_verify", false);
        }

        private void OnDisable()
        {
#if PANCAKE_SPINE
            SpineWindow.Clear();
#endif
            LevelSystemWindow.OnDisabled();
        }

        protected override void OnGUI()
        {
            Uniform.DrawLine(2);
            DrawTabs();
            EditorGUILayout.BeginHorizontal();
            DrawLeftSide();
            DrawRightSide();
            EditorGUILayout.EndHorizontal();
            base.OnGUI();
        }

        private void DrawRightSide()
        {
            if (_selectedItemType == WizardAllType.None) return;

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
            DrawRightSideHeader();
            _rightSideScrollPosition = EditorGUILayout.BeginScrollView(_rightSideScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandHeight(true));
            DrawContentRightSide();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawLeftSide()
        {
            const float width = TAB_WIDTH * 4f;
            var color = GUI.backgroundColor;
            GUI.backgroundColor = _colors[(int) _currentType];

            EditorGUILayout.BeginVertical("box", GUILayout.Width(width), GUILayout.ExpandHeight(true));

            _leftSideScrollPosition = EditorGUILayout.BeginScrollView(_leftSideScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandHeight(true));
            GUI.backgroundColor = color;
            DrawContentLeftSide(_items);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawContentLeftSide(List<int> contentsIndex)
        {
            if (contentsIndex == null) return;

            foreach (int i in contentsIndex)
            {
                EditorGUILayout.BeginHorizontal();
                var icon = GetIcon((WizardAllType) i);
                var style = new GUIStyle(GUIStyle.none) {contentOffset = new Vector2(0, 5)};
                var styleToggle = new GUIStyle(GUI.skin.button) {alignment = TextAnchor.MiddleLeft};
                GUILayout.Box(icon, style, GUILayout.Width(18), GUILayout.Height(18));
                bool clicked = GUILayout.Toggle((int) _selectedItemType == i, ((WizardAllType) i).ToString(), styleToggle, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();

                if (clicked) _selectedItemType = (WizardAllType) i;
            }
        }

        private void DrawRightSideHeader()
        {
            //Draw name and icon
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            var icon = GetIcon(_selectedItemType);
            GUILayout.Box(icon, GUIStyle.none, GUILayout.Width(32), GUILayout.Height(32));
            GUILayout.Label(_selectedItemType.ToString(), Uniform.HeaderLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawContentRightSide()
        {
            switch (_selectedItemType)
            {
                case WizardAllType.Advertisement when _currentType is WizardType.Money or WizardType.All:
                    AdvertisingWindow.OnInspectorGUI();
                    break;
                case WizardAllType.InAppPurchase when _currentType is WizardType.Money or WizardType.All:
                    IAPWindow.OnInspectorGUI();
                    break;
                case WizardAllType.Firebase when _currentType is WizardType.Money or WizardType.All:
                    FirebaseWindow.OnInspectorGUI();
                    break;
                case WizardAllType.Adjust when _currentType is WizardType.Money or WizardType.All:
                    AdjustWindow.OnInspectorGUI();
                    break;
                case WizardAllType.InAppReview when _currentType is WizardType.Tools or WizardType.All:
                    InAppReviewWindow.OnInspectorGUI();
                    break;
                case WizardAllType.HeartSetting when _currentType is WizardType.Setting or WizardType.All:
                    HeartSettingWindow.OnInspectorGUI();
                    break;
                case WizardAllType.ScreenSetting when _currentType is WizardType.Setting or WizardType.All:
                    ScreenSettingWindow.OnInspectorGUI(position);
                    break;
                case WizardAllType.Scriptable when _currentType is WizardType.Setting or WizardType.All:
                    ScriptableWindow.OnInspectorGUI();
                    break;
                case WizardAllType.OtherPackage when _currentType is WizardType.Tools or WizardType.All:
                    OtherPackageWindow.OnInspectorGUI();
                    break;
                case WizardAllType.LevelSystem when _currentType is WizardType.Setting or WizardType.All:
                    LevelSystemWindow.OnInspectorGUI(ref _currentLevelTabType, position);
                    break;
                case WizardAllType.Spine when _currentType is WizardType.Tools or WizardType.All:
                    SpineWindow.OnInspectorGUI(Repaint, position);
                    break;
                case WizardAllType.GameService when _currentType is WizardType.Tools or WizardType.All:
                    GameServiceWindow.OnInspectorGUI();
                    break;
                case WizardAllType.Localization when _currentType is WizardType.Tools or WizardType.All:
                    LocalizationWindow.OnInspectorGUI(ref _treeViewState,
                        ref localeTreeView,
                        ref multiColumnHeaderState,
                        BodyViewRect,
                        ToolbarRect,
                        BottomToolbarRect,
                        ref _localeSearchField,
                        ref _localeInitialized,
                        ref _currentLocaleTabType);
                    break;
                case WizardAllType.Texture when _currentType is WizardType.Setting or WizardType.All:
                    SettingTextureWindow.OnInspectorGUI(ref _spriteAtlas, this);
                    break;
                case WizardAllType.HierarchySetting when _currentType is WizardType.Setting or WizardType.All:
                    SettingHierarchyWindow.OnInspectorGUI();
                    break;
                case WizardAllType.Build when _currentType is WizardType.Tools or WizardType.All:
                    BuildWindow.OnInspectorGUI(ref _currentAndroidBuildPipeline);
                    break;
            }
        }

        private void DrawTabs()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            const float width = TAB_WIDTH * 4;
            GUILayout.Space(5);
            var style = new GUIStyle(EditorStyles.toolbarButton);
            tabIndex = GUILayout.Toolbar(tabIndex, System.Enum.GetNames(typeof(WizardType)), style, GUILayout.MaxWidth(width));

            if (tabIndex != (int) _currentType) OnTabSelected((WizardType) tabIndex, true);

            EditorGUILayout.EndHorizontal();
        }

        private void OnTabSelected(WizardType type, bool deselectCurrent = false)
        {
            Refresh(type);
            _currentType = type;
            switch (type)
            {
                case WizardType.All:
                    if (deselectCurrent) _selectedItemType = WizardAllType.HeartSetting;
                    break;
                case WizardType.Money:
                    if (deselectCurrent) _selectedItemType = WizardAllType.Advertisement;
                    break;
                case WizardType.Setting:
                    if (deselectCurrent) _selectedItemType = WizardAllType.Scriptable;
                    break;
                case WizardType.Tools:
                    if (deselectCurrent) _selectedItemType = WizardAllType.OtherPackage;
                    break;
            }
        }

        private void Refresh(WizardType type)
        {
            _items = new List<int>();
            switch (type)
            {
                case WizardType.All:
                    _items.AddRange(System.Enum.GetValues(typeof(WizardAllType)).Cast<int>());
                    _items.Remove(-1); // remove None
                    break;
                case WizardType.Money:
                    _items.AddRange(System.Enum.GetValues(typeof(MonetizationType)).Cast<int>());
                    break;
                case WizardType.Setting:
                    _items.AddRange(System.Enum.GetValues(typeof(SettingType)).Cast<int>());
                    break;
                case WizardType.Tools:
                    _items.AddRange(System.Enum.GetValues(typeof(ToolsType)).Cast<int>());
                    break;
            }
        }

        private void SelectTab(int index, bool deselect = false)
        {
            tabIndex = index;
            OnTabSelected((WizardType) tabIndex, deselect);
        }

        private Texture2D GetIcon(WizardAllType type)
        {
            return type switch
            {
                WizardAllType.Advertisement => EditorResources.ScriptableAd,
                WizardAllType.InAppPurchase => EditorResources.ScriptableIap,
                WizardAllType.Firebase => EditorResources.ScriptableFirebase,
                WizardAllType.Adjust => EditorResources.ScriptableAdjust,
                WizardAllType.InAppReview or WizardAllType.GameService or WizardAllType.Localization => EditorResources.ScriptableInterface,
                WizardAllType.HeartSetting => EditorResources.ScriptableSetting,
                WizardAllType.ScreenSetting => EditorResources.ScriptableSetting,
                WizardAllType.Scriptable or WizardAllType.Texture or WizardAllType.HierarchySetting or WizardAllType.LevelSystem => EditorResources
                    .ScriptableEditorSetting,
                WizardAllType.Spine => EditorResources.ScriptableEditorSpine,
                WizardAllType.Build or WizardAllType.OtherPackage => EditorResources.ScriptableUnity,
                _ => null
            };
        }
    }
}