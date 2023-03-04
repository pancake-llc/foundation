using System;
using System.Collections.Generic;
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

        private Vector2 _leftSideScrollPosition = Vector2.zero;
        private Vector2 _rightSideScrollPosition = Vector2.zero;
        private List<IFeatureItem> _items;
        private WizardType _currentType = WizardType.All;

        private readonly Color[] _colors = {Color.gray, Color.blue, Color.green, Color.yellow};
        private const float TAB_WIDTH = 65f;
        private const float BUTTON_HEIGHT = 40f;

        [SerializeField] private int tabIndex = -1;
        [SerializeField] private bool isInitialized;


        [MenuItem("Tools/Pancake/Wizard")]
        public new static void Show() => GetWindow<Wizard>("Wizard");

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

        private void DrawRightSide() { }

        private void DrawLeftSide()
        {
            const float width = TAB_WIDTH * 4f;
            var color = GUI.backgroundColor;
            GUI.backgroundColor = _colors[(int) _currentType];
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(width), GUILayout.ExpandHeight(true));

            _leftSideScrollPosition = EditorGUILayout.BeginScrollView(_leftSideScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandHeight(true));
            GUI.backgroundColor = color;
            DrawContentLeftSide();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawContentLeftSide() { }
        private void DrawContentRightSide() { }

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
                //
            }
        }

        private void Refresh(WizardType type)
        {
            switch (type)
            {
                case WizardType.All:
                    break;
                case WizardType.Monetize:
                    break;
                case WizardType.Tracking:
                    break;
                case WizardType.Utilities:
                    break;
            }
        }

        private void SelectTab(int index, bool deselect = false)
        {
            tabIndex = index;
            OnTabSelected((WizardType) tabIndex, deselect);
        }
    }
}