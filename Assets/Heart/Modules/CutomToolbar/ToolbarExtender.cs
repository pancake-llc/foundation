using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityToolbarExtender
{
    [InitializeOnLoad]
    public static class ToolbarExtender
    {
        private static int mToolCount;
        private static GUIStyle mCommandStyle;

        public static readonly List<Action> LeftToolbarGUI = new();
        public static readonly List<Action> RightToolbarGUI = new();

        static ToolbarExtender()
        {
            EditorApplication.update += Init;
            EditorApplication.playModeStateChanged += OnChangePlayMode;
        }

        private static void OnChangePlayMode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
                InitElements();
        }

        private static void Init()
        {
            EditorApplication.update -= Init;
            InitElements();
        }

        public static void InitElements()
        {
            var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");

            var fieldName = "k_ToolCount";

            var toolIcons = toolbarType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            mToolCount = toolIcons != null ? ((int) toolIcons.GetValue(null)) : 8;

            ToolbarCallback.OnToolbarGUI -= OnGUI;
            ToolbarCallback.OnToolbarGUI += OnGUI;
            ToolbarCallback.OnToolbarGUILeft -= GUILeft;
            ToolbarCallback.OnToolbarGUILeft += GUILeft;
            ToolbarCallback.OnToolbarGUIRight -= GUIRight;
            ToolbarCallback.OnToolbarGUIRight += GUIRight;
        }

        public const float SPACE = 8;
        public const float LARGE_SPACE = 20;
        public const float BUTTON_WIDTH = 32;
        public const float DROPDOWN_WIDTH = 80;
        public const float PLAY_PAUSE_STOP_WIDTH = 140;

        public static void OnGUI()
        {
            // Create two containers, left and right
            // Screen is whole toolbar

            if (mCommandStyle == null)
            {
                mCommandStyle = new GUIStyle("CommandLeft");
            }

            var screenWidth = EditorGUIUtility.currentViewWidth;

            // Following calculations match code reflected from Toolbar.OldOnGUI()
            float playButtonsPosition = Mathf.RoundToInt((screenWidth - PLAY_PAUSE_STOP_WIDTH) / 2);

            var leftRect = new Rect(0, 0, screenWidth, Screen.height);
            leftRect.xMin += SPACE; // Spacing left
            leftRect.xMin += BUTTON_WIDTH * mToolCount; // Tool buttons
            leftRect.xMin += SPACE; // Spacing between tools and pivot

            leftRect.xMin += 64 * 2; // Pivot buttons
            leftRect.xMax = playButtonsPosition;

            var rightRect = new Rect(0, 0, screenWidth, Screen.height);
            rightRect.xMin = playButtonsPosition;
            rightRect.xMin += mCommandStyle.fixedWidth * 3; // Play buttons
            rightRect.xMax = screenWidth;
            rightRect.xMax -= SPACE; // Spacing right
            rightRect.xMax -= DROPDOWN_WIDTH; // Layout
            rightRect.xMax -= SPACE; // Spacing between layout and layers
            rightRect.xMax -= DROPDOWN_WIDTH; // Layers
            rightRect.xMax -= SPACE; // Spacing between layers and account

            rightRect.xMax -= DROPDOWN_WIDTH; // Account
            rightRect.xMax -= SPACE; // Spacing between account and cloud
            rightRect.xMax -= BUTTON_WIDTH; // Cloud
            rightRect.xMax -= SPACE; // Spacing between cloud and collab
            rightRect.xMax -= 78; // Colab

            // Add spacing around existing controls
            leftRect.xMin += SPACE;
            leftRect.xMax -= SPACE;
            rightRect.xMin += SPACE;
            rightRect.xMax -= SPACE;

            // Add top and bottom margins
            leftRect.y = 4;
            leftRect.height = 22;
            rightRect.y = 4;
            rightRect.height = 22;

            if (leftRect.width > 0)
            {
                GUILayout.BeginArea(leftRect);
                GUILayout.BeginHorizontal();
                foreach (var handler in LeftToolbarGUI)
                {
                    handler();
                }

                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }

            if (rightRect.width > 0)
            {
                GUILayout.BeginArea(rightRect);
                GUILayout.BeginHorizontal();
                foreach (var handler in RightToolbarGUI)
                {
                    handler();
                }

                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        }

        private static void GUILeft()
        {
            GUILayout.BeginHorizontal();
            foreach (var handler in LeftToolbarGUI)
            {
                handler();
            }

            GUILayout.EndHorizontal();
        }

        private static void GUIRight()
        {
            GUILayout.BeginHorizontal();
            foreach (var handler in RightToolbarGUI)
            {
                handler();
            }

            GUILayout.EndHorizontal();
        }
    }
}