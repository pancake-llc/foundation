using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public class Wizard : EditorWindow
    {
        private enum WizardType
        {
            All,
            Monetize,
            Tracking,
            Utilities
        }

        private enum WizardAllType
        {
            None = -1,
            Advertisement,
            InAppPurchase,
            Firebase,
            Adjust,
            Tween
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
        }

        private enum WizardUtilitiesType
        {
            Tween = WizardAllType.Tween,
        }

        private Vector2 _leftSideScrollPosition = Vector2.zero;
        private Vector2 _rightSideScrollPosition = Vector2.zero;
        private List<int> _items;
        private WizardType _currentType = WizardType.All;
        private WizardAllType _selectedItemType = WizardAllType.None;

        private readonly Color[] _colors = {Color.gray, Color.blue, Color.green, Color.yellow};
        private const float TAB_WIDTH = 65f;
        private const float BUTTON_HEIGHT = 40f;

        [SerializeField] private int tabIndex = -1;
        [SerializeField] private bool isInitialized;


        [MenuItem("Tools/Pancake/Wizard #W")]
        public new static void Show()
        {
            var window = GetWindow<Wizard>("Wizard");
            window.autoRepaintOnSceneChange = true;
            window.Show(true);
        }

        private void OnEnable()
        {
            if (isInitialized)
            {
                SelectTab(tabIndex);
                return;
            }

            SelectTab((int) _currentType, true);
            isInitialized = true;
        }

        private void OnGUI()
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

            var index = 0;
            foreach (int i in contentsIndex)
            {
                EditorGUILayout.BeginHorizontal();
                var icon = GetIcon((WizardAllType) i);
                var style = new GUIStyle(GUIStyle.none) {contentOffset = new Vector2(0, 5)};
                GUILayout.Box(icon, style, GUILayout.Width(18), GUILayout.Height(18));
                bool clicked = GUILayout.Toggle((int) _selectedItemType == index, ((WizardAllType) i).ToString(), GUI.skin.button, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();

                if (clicked)
                {
                    _selectedItemType = (WizardAllType) index;
                }

                index++;
            }
        }

        private void DrawRightSideHeader()
        {
            //Draw name and icon
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            var icon = GetIcon(_selectedItemType);
            GUILayout.Box(icon, GUIStyle.none, GUILayout.Width(32), GUILayout.Height(32));
            GUILayout.Label(((WizardAllType) _selectedItemType).ToString(), EditorStyles.label);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawContentRightSide()
        {
            switch (_selectedItemType)
            {
                case WizardAllType.Advertisement when _currentType is WizardType.Monetize or WizardType.All:
                    MonetizeAdvertisingDrawer.OnInspectorGUI();
                    break;
                case WizardAllType.InAppPurchase when _currentType is WizardType.Monetize or WizardType.All:
                    MonetizeIAPDrawer.OnInspectorGUI();
                    break;
                case WizardAllType.Firebase when _currentType is WizardType.Monetize or WizardType.All:
                    break;
                case WizardAllType.Adjust when _currentType is WizardType.Monetize or WizardType.All:
                    break;
                case WizardAllType.Tween when _currentType is WizardType.Utilities or WizardType.All:
                    UtilitiesTweenDrawer.OnInspectorGUI();
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
            if (deselectCurrent)
            {
                _selectedItemType = WizardAllType.None;
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
                case WizardType.Monetize:
                    _items.AddRange(System.Enum.GetValues(typeof(WizardMonetizeType)).Cast<int>());
                    break;
                case WizardType.Tracking:
                    _items.AddRange(System.Enum.GetValues(typeof(WizardTrackingType)).Cast<int>());
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
                case WizardAllType.Tween: return EditorResources.ScriptableTween;
                default:
                    return null;
            }
        }
    }
}