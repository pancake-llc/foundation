using System;
using System.Collections.Generic;
using System.Linq;
using Pancake.ExLibEditor;
using Pancake.ScriptableEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public class Wizard : ScriptableWindowBase
    {
        private enum WizardType
        {
            All,
            Money,
            Track,
            Setting,
            Utilities
        }

        private enum WizardAllType
        {
            None = -1,
            Advertisement,
            InAppPurchase,
            Firebase,
            Adjust,
            HeartSetting,
            Scriptable,
            LevelSystem,
            Notification,
            InAppReview,
            NeedleConsole,
            SelectiveProfiling,
            IOS14AdvertisingSupport,
            ParticleEffectForUGUI,
            UIEffect,
            ScreenSetting,
            Spine,
        }

        private enum WizardMonetizeType
        {
            Advertisement = WizardAllType.Advertisement,
            InAppPurchase = WizardAllType.InAppPurchase,
        }

        private enum WizardTrackingType
        {
            Firebase = WizardAllType.Firebase,
            Adjust = WizardAllType.Adjust,
            IOS14AdvertisingSupport = WizardAllType.IOS14AdvertisingSupport
        }

        private enum WizardUtilitiesType
        {
            Notification = WizardAllType.Notification,
            InAppReview = WizardAllType.InAppReview,
            NeedleConsole = WizardAllType.NeedleConsole,
            SelectiveProfiling = WizardAllType.SelectiveProfiling,
            ParticleEffectForUGUI = WizardAllType.ParticleEffectForUGUI,
            UIEffect = WizardAllType.UIEffect,
            Spine = WizardAllType.Spine
        }

        private enum WizardSettingType
        {
            HeartConfig = WizardAllType.HeartSetting,
            Scriptable = WizardAllType.Scriptable,
            LevelSystem = WizardAllType.LevelSystem,
            ScreenSetting = WizardAllType.ScreenSetting
        }

        private Vector2 _leftSideScrollPosition = Vector2.zero;
        private Vector2 _rightSideScrollPosition = Vector2.zero;
        private List<int> _items;
        private WizardType _currentType = WizardType.All;
        private WizardAllType _selectedItemType = WizardAllType.None;

        private readonly Color[] _colors = {Uniform.DeepCarminePink, Color.yellow, Uniform.RichBlack, Uniform.FluorescentBlue, Uniform.FieryRose};
        private const float TAB_WIDTH = 65f;

        [SerializeField] private int tabIndex = -1;
        [SerializeField] private bool isInitialized;

        [MenuItem("Tools/Pancake/Wizard #W")]
        public new static void Show()
        {
            var window = GetWindow<Wizard>("Wizard");
            window.autoRepaintOnSceneChange = true;
            window.Show(true);
            SessionState.SetBool("spine_flag", false);
        }

        protected override void OnEnable()
        {
            if (isInitialized)
            {
                SelectTab(tabIndex);
                return;
            }

            SelectTab((int) _currentType, true);
            isInitialized = true;
            SessionState.SetBool("advertising_flag", false);
        }

        private void OnDisable()
        {
#if PANCAKE_SPINE
            UtilitiesSpineDrawer.Clear();
#endif
        }

        protected override void OnGUI()
        {
            Uniform.DrawHeader("Wizard");
            Uniform.DrawLine(2);
            DrawTabs();
            EditorGUILayout.BeginHorizontal();
            DrawLeftSide();
            DrawRightSide();
            EditorGUILayout.EndHorizontal();
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
                GUILayout.Box(icon, style, GUILayout.Width(18), GUILayout.Height(18));
                bool clicked = GUILayout.Toggle((int) _selectedItemType == i, ((WizardAllType) i).ToString(), GUI.skin.button, GUILayout.ExpandWidth(true));
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
            GUILayout.Label(((WizardAllType) _selectedItemType).ToString(), Uniform.HeaderLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawContentRightSide()
        {
            switch (_selectedItemType)
            {
                case WizardAllType.Advertisement when _currentType is WizardType.Money or WizardType.All:
                    MonetizeAdvertisingDrawer.OnInspectorGUI();
                    break;
                case WizardAllType.InAppPurchase when _currentType is WizardType.Money or WizardType.All:
                    MonetizeIAPDrawer.OnInspectorGUI();
                    break;
                case WizardAllType.Firebase when _currentType is WizardType.Track or WizardType.All:
                    TrackingFirebaseDrawer.OnInspectorGUI();
                    break;
                case WizardAllType.Adjust when _currentType is WizardType.Track or WizardType.All:
                    TrackingAdjustDrawer.OnInspectorGUI();
                    break;
                case WizardAllType.Notification when _currentType is WizardType.Utilities or WizardType.All:
                    UtilitiesNotificationDrawer.OnInspectorGUI();
                    break;
                case WizardAllType.InAppReview when _currentType is WizardType.Utilities or WizardType.All:
                    UtilitiesInAppReviewDrawer.OnInspectorGUI();
                    break;
                case WizardAllType.NeedleConsole when _currentType is WizardType.Utilities or WizardType.All:
                    UtilitiesNeedleConsoleDrawer.OnInspectorGUI();
                    break;
                case WizardAllType.SelectiveProfiling when _currentType is WizardType.Utilities or WizardType.All:
                    UtilitiesSelectiveProfilingDrawer.OnInspectorGUI();
                    break;
                case WizardAllType.HeartSetting when _currentType is WizardType.Setting or WizardType.All:
                    UtilitiesHeartSettingDrawer.OnInspectorGUI();
                    break;
                case WizardAllType.ScreenSetting when _currentType is WizardType.Setting or WizardType.All:
                    UtilitiesScreenSettingDrawer.OnInspectorGUI(position);
                    break;
                case WizardAllType.IOS14AdvertisingSupport when _currentType is WizardType.Track or WizardType.All:
                    TrackingIOS14AdvertisingSupportDrawer.OnInspectorGUI();
                    break;
                case WizardAllType.Scriptable when _currentType is WizardType.Setting or WizardType.All:
                    UtilitiesScriptableDrawer.OnInspectorGUI();
                    break;
                case WizardAllType.ParticleEffectForUGUI when _currentType is WizardType.Utilities or WizardType.All:
                    UtilitiesParticleEffectForUGUIDrawer.OnInspectorGUI();
                    break;
                case WizardAllType.UIEffect when _currentType is WizardType.Utilities or WizardType.All:
                    UtilitiesUIEffectDrawer.OnInspectorGUI();
                    break;
                case WizardAllType.LevelSystem when _currentType is WizardType.Setting or WizardType.All:
                    UtilitiesLevelSystemDrawer.OnInspectorGUI();
                    break;
                case WizardAllType.Spine when _currentType is WizardType.Utilities or WizardType.All:
                    UtilitiesSpineDrawer.OnInspectorGUI(Repaint, position);
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
            if (deselectCurrent) _selectedItemType = WizardAllType.None;
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
                    _items.AddRange(System.Enum.GetValues(typeof(WizardMonetizeType)).Cast<int>());
                    break;
                case WizardType.Track:
                    _items.AddRange(System.Enum.GetValues(typeof(WizardTrackingType)).Cast<int>());
                    break;
                case WizardType.Setting:
                    _items.AddRange(System.Enum.GetValues(typeof(WizardSettingType)).Cast<int>());
                    break;
                case WizardType.Utilities:
                    _items.AddRange(System.Enum.GetValues(typeof(WizardUtilitiesType)).Cast<int>());
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
            switch (type)
            {
                case WizardAllType.Advertisement: return EditorResources.ScriptableAd;
                case WizardAllType.InAppPurchase: return EditorResources.ScriptableIap;
                case WizardAllType.Firebase: return EditorResources.ScriptableFirebase;
                case WizardAllType.Adjust: return EditorResources.ScriptableAdjust;
                case WizardAllType.Notification: return EditorResources.ScriptableNotification;
                case WizardAllType.InAppReview:
                case WizardAllType.NeedleConsole: 
                case WizardAllType.SelectiveProfiling: 
                case WizardAllType.IOS14AdvertisingSupport: return EditorResources.ScriptableInterface;
                case WizardAllType.HeartSetting: return EditorResources.ScriptableSetting;
                case WizardAllType.ScreenSetting: return EditorResources.ScriptableSetting;
                case WizardAllType.Scriptable: return EditorResources.ScriptableEditorSetting;
                case WizardAllType.ParticleEffectForUGUI: return EditorResources.ScriptableSetting;
                case WizardAllType.UIEffect: return EditorResources.ScriptableSetting;
                case WizardAllType.LevelSystem: return EditorResources.ScriptableEditorSetting;
                case WizardAllType.Spine: return EditorResources.ScriptableEditorSetting;
                default:
                    return null;
            }
        }
    }
}