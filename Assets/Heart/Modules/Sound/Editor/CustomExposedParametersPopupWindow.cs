using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Audio;
using UnityEditorInternal;
using System;
using Pancake.Sound;

namespace PancakeEditor.Sound
{
    public class CustomExposedParametersPopupWindow : PopupWindowContent
    {
        private class EffectExposedParameter
        {
            public string name;
            public readonly int originalIndex;

            public EffectExposedParameter(string name, int index)
            {
                this.name = name;
                originalIndex = index;
            }
        }

        public const int MAX_NAME_LENGTH = 64;

        private ReorderableList _reorderableList;
        private SerializedProperty _exposedParams;
        private Vector2 _scrollPos;
        private bool _isRename;
        private int _currentSelectedIndex;
        private GenericMenu _rightClickMenu;
        private int _currentRightClickIndex;
        private int _currentRenameIndex;

        public void CreateReorderableList(AudioMixer mixer)
        {
            var serializedMixer = new SerializedObject(mixer);
            _exposedParams = serializedMixer.FindProperty("m_ExposedParameters");
            var filteredParams = GetFilteredExposedParameters(_exposedParams);

            _reorderableList = new ReorderableList(filteredParams,
                typeof(string),
                false,
                false,
                false,
                false)
            {
                drawElementCallback = DrawElement,
                onSelectCallback = OnSelect,
                elementHeight = 18,
                headerHeight = 0,
                footerHeight = 0,
                showDefaultBackground = false
            };

            void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (Event.current.isMouse && Event.current.button == 1 && rect.Contains(Event.current.mousePosition)) // Right click
                {
                    _rightClickMenu ??= CreateRightClickMenu();
                    _currentRightClickIndex = index;
                    _rightClickMenu.DropDown(rect);
                }

                if (Event.current.isMouse && Event.current.clickCount >= 2) // Double click
                {
                    _currentRenameIndex = index;
                    _isRename = true;
                }

                if (_isRename && index == _currentRenameIndex)
                {
                    EditorGUI.BeginChangeCheck();
                    string newName = EditorGUI.TextField(rect, filteredParams[index].name);
                    if (EditorGUI.EndChangeCheck() && IsValidName(newName))
                    {
                        filteredParams[index].name = newName;
                        ChangeExposedParameterName(filteredParams[index]);
                    }
                }
                else
                {
                    EditorGUI.LabelField(rect, filteredParams[index].name);
                }
            }

            void OnSelect(ReorderableList list)
            {
                if (_currentSelectedIndex != list.index) _isRename = false;

                _currentSelectedIndex = list.index;
            }
        }

        private void EnableRenameByRightClick()
        {
            _currentRenameIndex = _currentRightClickIndex;
            _isRename = true;
        }

        private GenericMenu CreateRightClickMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Unexpose"), false, DeleteExposedParameter);
            menu.AddItem(new GUIContent("Rename"), false, EnableRenameByRightClick);
            return menu;
        }

        private void DeleteExposedParameter()
        {
            var selectedParameter = _reorderableList.list[_currentRightClickIndex] as EffectExposedParameter;
            if (selectedParameter != null)
            {
                _exposedParams.DeleteArrayElementAtIndex(selectedParameter.originalIndex);
                _reorderableList.list.RemoveAt(_currentRightClickIndex);
                _exposedParams.serializedObject.ApplyModifiedProperties();
            }
        }

        private void ChangeExposedParameterName(EffectExposedParameter parameter)
        {
            var exposedParaProp = _exposedParams.GetArrayElementAtIndex(parameter.originalIndex);
            var exposedParaNameProp = exposedParaProp.FindPropertyRelative("name");
            exposedParaNameProp.stringValue = parameter.name;
            exposedParaProp.serializedObject.ApplyModifiedProperties();
        }

        private List<EffectExposedParameter> GetFilteredExposedParameters(SerializedProperty paramsProp)
        {
            var result = new List<EffectExposedParameter>();
            for (var i = 0; i < paramsProp.arraySize; i++)
            {
                var exposedParaProp = paramsProp.GetArrayElementAtIndex(i);
                var exposedParaNameProp = exposedParaProp.FindPropertyRelative("name");
                if (!IsCoreParameter(exposedParaNameProp.stringValue))
                {
                    result.Add(new EffectExposedParameter(exposedParaNameProp.stringValue, i));
                }
            }

            return result;

            bool IsCoreParameter(string paraName)
            {
                bool endWithNumber = char.IsNumber(paraName[paraName.Length - 1]);
                bool mightBeGenericTrack = paraName.StartsWith(AudioConstant.GENERIC_TRACK_NAME, StringComparison.Ordinal);
                return IsGenericTrack() || IsGenericTrackEffect() || IsDominatorTrack() || IsMainTrack() || IsMasterTrack();

                bool IsGenericTrack() => endWithNumber && mightBeGenericTrack;

                bool IsGenericTrackEffect() =>
                    !endWithNumber && mightBeGenericTrack && paraName.EndsWith(AudioConstant.EFFECT_PARA_NAME_SUFFIX, StringComparison.Ordinal);

                bool IsDominatorTrack() => endWithNumber && paraName.StartsWith(AudioConstant.DOMINATOR_TRACK_NAME, StringComparison.Ordinal);
                bool IsMainTrack() => paraName.StartsWith(AudioConstant.MAIN_TRACK_NAME, StringComparison.Ordinal);
                bool IsMasterTrack() => !endWithNumber && paraName.Equals(AudioConstant.MASTER_TRACK_NAME);
            }
        }

        private bool IsValidName(string newName)
        {
            if (newName.Length > MAX_NAME_LENGTH)
            {
                Debug.LogWarning(AudioConstant.LOG_HEADER + $"Maximum name length of an exposed parameter is {MAX_NAME_LENGTH}");
                return false;
            }

            return true;
        }

        public override void OnGUI(Rect rect)
        {
            if (_reorderableList != null)
            {
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                _reorderableList.DoLayoutList();
                EditorGUILayout.EndScrollView();
            }
        }
    }
}