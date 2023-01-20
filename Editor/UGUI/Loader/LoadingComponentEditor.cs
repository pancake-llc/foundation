#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Pancake.Linq;
using Pancake.Loader;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Editor
{
    [CustomEditor(typeof(LoadingComponent))]
    [Serializable]
    public class LoadingComponentEditor : UnityEditor.Editor
    {
        private Image _pakCountdownFilled;
        private Image _pakCountdownBg;
        private SpinnerItem _selectedSpinnerItem;
        private List<Transform> _spinnerList = new List<Transform>();
        private LoadingComponent _loading;

        private void Init()
        {
            _loading = target as LoadingComponent;
            // Property variables
            LoadingSceneProperties.singleBackgroundSprite.property = serializedObject.FindProperty("singleBackgroundSprite");

            // Layout variables
            LoadingSceneProperties.hintsColor.property = serializedObject.FindProperty("hintsColor");
            LoadingSceneProperties.hintsFontSize.property = serializedObject.FindProperty("hintsFontSize");
            LoadingSceneProperties.hintsFont.property = serializedObject.FindProperty("hintsFont");
            LoadingSceneProperties.statusColor.property = serializedObject.FindProperty("statusColor");
            LoadingSceneProperties.statusSize.property = serializedObject.FindProperty("statusSize");
            LoadingSceneProperties.statusFont.property = serializedObject.FindProperty("statusFont");
            LoadingSceneProperties.spinnerColor.property = serializedObject.FindProperty("spinnerColor");
            LoadingSceneProperties.spinnerIndex.property = serializedObject.FindProperty("spinnerIndex");
            LoadingSceneProperties.statusSchema.property = serializedObject.FindProperty("statusSchema");

            LoadingSceneProperties.isHints.property = serializedObject.FindProperty("isHints");
            LoadingSceneProperties.hintsCollection.property = serializedObject.FindProperty("hintsCollection");
            LoadingSceneProperties.isChangeHintsWithTimer.property = serializedObject.FindProperty("isChangeHintsWithTimer");
            LoadingSceneProperties.hintsLifeTime.property = serializedObject.FindProperty("hintsLifeTime");

            LoadingSceneProperties.enableRandomBackground.property = serializedObject.FindProperty("enableRandomBackground");
            LoadingSceneProperties.backgroundSprites.property = serializedObject.FindProperty("backgroundCollection");
            LoadingSceneProperties.autoChangeBackground.property = serializedObject.FindProperty("isAutoChangeBg");
            LoadingSceneProperties.timeAutoChangeBg.property = serializedObject.FindProperty("timeAutoChangeBg");
            LoadingSceneProperties.backgroundFadingSpeed.property = serializedObject.FindProperty("backgroundFadingSpeed");

            LoadingSceneProperties.canvasGroup.property = serializedObject.FindProperty("canvasGroup");
            LoadingSceneProperties.txtStatus.property = serializedObject.FindProperty("txtStatus");
            LoadingSceneProperties.progressBar.property = serializedObject.FindProperty("progressBar");
            LoadingSceneProperties.txtHints.property = serializedObject.FindProperty("txtHints");
            LoadingSceneProperties.background.property = serializedObject.FindProperty("background");
            LoadingSceneProperties.backgroundAnimator.property = serializedObject.FindProperty("backgroundAnimator");
            LoadingSceneProperties.mainLoadingAnimator.property = serializedObject.FindProperty("mainLoadingAnimator");
            LoadingSceneProperties.spinnerParent.property = serializedObject.FindProperty("spinnerParent");
            LoadingSceneProperties.enableStatusLabel.property = serializedObject.FindProperty("enableStatusLabel");
            LoadingSceneProperties.virtualLoadTime.property = serializedObject.FindProperty("virtualLoadTime");
            LoadingSceneProperties.fadingAnimationSpeed.property = serializedObject.FindProperty("fadingAnimationSpeed");
            LoadingSceneProperties.timeDelayDestroy.property = serializedObject.FindProperty("timeDelayDestroy");
            LoadingSceneProperties.onBeginEvents.property = serializedObject.FindProperty("onBeginEvents");
            LoadingSceneProperties.onFinishEvents.property = serializedObject.FindProperty("onFinishEvents");
        }

        private void OnEnable()
        {
            Init();
            _spinnerList.Clear();
            foreach (Transform child in (Transform) LoadingSceneProperties.spinnerParent.property.objectReferenceValue)
            {
                _spinnerList.Add(child);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Uniform.DrawGroupFoldout("LOADING_SPINNER", "SPINNER", DrawLayout, false);
            Uniform.SpaceOneLine();
            Uniform.DrawGroupFoldout("LOADING_HINT", "HINT", DrawHint, false);
            Uniform.SpaceOneLine();
            Uniform.DrawGroupFoldout("LOADING_BACKGROUND", "BG", DrawBackground, false);
            Uniform.SpaceOneLine();
            Uniform.DrawGroupFoldout("LOADING_STATUS", "STATUS", DrawStatus, false);
            Uniform.SpaceOneLine();
            Uniform.DrawGroupFoldout("LOADING_SETTING", "SETTING", DrawSetting, false);

            void DrawLayout()
            {
                EditorGUILayout.PropertyField(LoadingSceneProperties.spinnerParent.property, LoadingSceneProperties.spinnerParent.content);
                if (LoadingSceneProperties.spinnerIndex.property.intValue == 1)
                {
                    EditorGUILayout.PropertyField(LoadingSceneProperties.progressBar.property, LoadingSceneProperties.progressBar.content);
                }

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Spinner"), GUILayout.Width(120));
                LoadingSceneProperties.spinnerIndex.property.intValue = EditorGUILayout.Popup(LoadingSceneProperties.spinnerIndex.property.intValue,
                    _spinnerList.Map(_ => _.gameObject.name).ToArray());
                _selectedSpinnerItem = _spinnerList[LoadingSceneProperties.spinnerIndex.property.intValue].GetComponent<SpinnerItem>();
                _selectedSpinnerItem.UpdateColor(LoadingSceneProperties.spinnerColor.property.colorValue);
                EditorUtility.SetDirty(target);

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(LoadingSceneProperties.spinnerColor.content, GUILayout.Width(120));
                LoadingSceneProperties.spinnerColor.property.colorValue = EditorGUILayout.ColorField(LoadingSceneProperties.spinnerColor.property.colorValue);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Restore Color"))
                {
                    if (LoadingSceneProperties.isHints.property.boolValue)
                    {
                        _loading.hintsFontSize = _loading.txtHints.fontSize;
                        _loading.hintsFont = _loading.txtHints.font;
                        _loading.hintsColor = _loading.txtHints.color;
                    }

                    if (LoadingSceneProperties.enableStatusLabel.property.boolValue)
                    {
                        _loading.statusSize = _loading.txtStatus.fontSize;
                        _loading.statusFont = _loading.txtStatus.font;
                        _loading.statusColor = _loading.txtStatus.color;
                    }
                }

                if (GUILayout.Button("Update Color"))
                {
                    if (LoadingSceneProperties.isHints.property.boolValue)
                    {
                        _loading.txtHints.fontSize = _loading.hintsFontSize;
                        _loading.txtHints.font = _loading.hintsFont;
                        _loading.txtHints.color = _loading.hintsColor;
                    }

                    if (LoadingSceneProperties.enableStatusLabel.property.boolValue)
                    {
                        _loading.txtStatus.fontSize = _loading.statusSize;
                        _loading.txtStatus.font = _loading.statusFont;
                        _loading.txtStatus.color = _loading.statusColor;
                    }

                    try
                    {
                        _selectedSpinnerItem = _spinnerList[_loading.spinnerIndex].GetComponent<SpinnerItem>();
                        _selectedSpinnerItem.UpdateColor(_loading.spinnerColor);
                        EditorUtility.SetDirty(_loading.gameObject);
                    }
                    catch
                    {
                        Debug.Log("Loading Screen - Cannot initialize selected Spinner Item.", this);
                    }
                }

                for (int i = 0; i < _loading.spinnerParent.childCount; i++)
                {
                    var child = _loading.spinnerParent.GetChild(i);
                    if (child.name != _spinnerList[_loading.spinnerIndex].ToString().Replace(" (UnityEngine.RectTransform)", "").Trim())
                        child.gameObject.SetActive(false);
                    else child.gameObject.SetActive(true);
                }

                GUILayout.EndHorizontal();
            }

            void DrawHint()
            {
                Uniform.Toggle(LoadingSceneProperties.isHints.property, LoadingSceneProperties.isHints.content);
                if (LoadingSceneProperties.isHints.property.boolValue)
                {
                    EditorGUILayout.PropertyField(LoadingSceneProperties.txtHints.property, LoadingSceneProperties.txtHints.content);
                    EditorGUILayout.PropertyField(LoadingSceneProperties.hintsFontSize.property, LoadingSceneProperties.hintsFontSize.content);
                    EditorGUILayout.PropertyField(LoadingSceneProperties.hintsFont.property, LoadingSceneProperties.hintsFont.content);
                    EditorGUILayout.PropertyField(LoadingSceneProperties.hintsColor.property, LoadingSceneProperties.hintsColor.content);

                    if (LoadingSceneProperties.txtHints.property.objectReferenceValue != null)
                    {
                        _loading.txtHints.gameObject.SetActive(true);
                        _loading.txtHints.fontSize = _loading.hintsFontSize;
                        _loading.txtHints.font = _loading.hintsFont;
                        _loading.txtHints.color = _loading.hintsColor;
                    }

                    Uniform.Toggle(LoadingSceneProperties.isChangeHintsWithTimer.property, LoadingSceneProperties.isChangeHintsWithTimer.content);

                    if (LoadingSceneProperties.isChangeHintsWithTimer.property.boolValue)
                    {
                        EditorGUILayout.PropertyField(LoadingSceneProperties.hintsLifeTime.property, LoadingSceneProperties.hintsLifeTime.content);
                    }

                    Uniform.SpaceOneLine();
                    EditorGUI.indentLevel = 1;
                    EditorGUILayout.PropertyField(LoadingSceneProperties.hintsCollection.property, LoadingSceneProperties.hintsCollection.content);
                    LoadingSceneProperties.hintsCollection.property.isExpanded = true;
                    EditorGUI.indentLevel = 0;

                    if (_loading.txtHints == null)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.HelpBox("'Hint Text Object' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
                        GUILayout.EndHorizontal();
                    }
                }
                else if (LoadingSceneProperties.isHints.property.boolValue == false && LoadingSceneProperties.txtHints.property.objectReferenceValue != null)
                    _loading.txtHints.gameObject.SetActive(false);
            }

            void DrawBackground()
            {
                Uniform.Toggle(LoadingSceneProperties.enableRandomBackground.property, LoadingSceneProperties.enableRandomBackground.content);
                EditorGUILayout.PropertyField(LoadingSceneProperties.background.property, LoadingSceneProperties.background.content);

                if (LoadingSceneProperties.enableRandomBackground.property.boolValue)
                {
                    EditorGUILayout.PropertyField(LoadingSceneProperties.backgroundAnimator.property, LoadingSceneProperties.backgroundAnimator.content);
                    EditorGUI.indentLevel = 1;
                    EditorGUILayout.PropertyField(LoadingSceneProperties.backgroundSprites.property, LoadingSceneProperties.backgroundSprites.content, true);
                    LoadingSceneProperties.backgroundSprites.property.isExpanded = true;
                    EditorGUI.indentLevel = 0;

                    Uniform.Toggle(LoadingSceneProperties.autoChangeBackground.property, LoadingSceneProperties.autoChangeBackground.content);
                    if (LoadingSceneProperties.autoChangeBackground.property.boolValue)
                    {
                        EditorGUILayout.PropertyField(LoadingSceneProperties.timeAutoChangeBg.property, LoadingSceneProperties.timeAutoChangeBg.content);
                        EditorGUILayout.PropertyField(LoadingSceneProperties.backgroundFadingSpeed.property, LoadingSceneProperties.backgroundFadingSpeed.content);
                    }
                }
                else
                {
                    EditorGUILayout.PropertyField(LoadingSceneProperties.singleBackgroundSprite.property, LoadingSceneProperties.singleBackgroundSprite.content);

                    if (_loading.background != null) _loading.background.sprite = _loading.singleBackgroundSprite;
                }

                if (_loading.background == null)
                {
                    GUILayout.Space(4);
                    EditorGUILayout.HelpBox("'Image Object' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
                }
            }

            void DrawStatus()
            {
                Uniform.Toggle(LoadingSceneProperties.enableStatusLabel.property, LoadingSceneProperties.enableStatusLabel.content);
                if (LoadingSceneProperties.enableStatusLabel.property.boolValue)
                {
                    EditorGUILayout.PropertyField(LoadingSceneProperties.txtStatus.property, LoadingSceneProperties.txtStatus.content);
                    EditorGUILayout.PropertyField(LoadingSceneProperties.statusSchema.property, LoadingSceneProperties.statusSchema.content);
                    EditorGUILayout.PropertyField(LoadingSceneProperties.statusSize.property, LoadingSceneProperties.statusSize.content);
                    EditorGUILayout.PropertyField(LoadingSceneProperties.statusFont.property, LoadingSceneProperties.statusFont.content);
                    EditorGUILayout.PropertyField(LoadingSceneProperties.statusColor.property, LoadingSceneProperties.statusColor.content);

                    if (LoadingSceneProperties.txtStatus.property == null || LoadingSceneProperties.txtStatus.property.objectReferenceValue == null)
                    {
                        GUILayout.Space(2);
                        EditorGUILayout.HelpBox("'Status Label' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
                    }
                    else
                    {
                        _loading.txtStatus.fontSize = _loading.statusSize;
                        _loading.txtStatus.font = _loading.statusFont;
                        _loading.txtStatus.color = _loading.statusColor;
                    }

                    GUILayout.Space(2);
                }

                if (LoadingSceneProperties.txtStatus.property != null && LoadingSceneProperties.txtStatus.property.objectReferenceValue != null)
                    _loading.txtStatus.gameObject.SetActive(LoadingSceneProperties.enableStatusLabel.property.boolValue);
            }

            void DrawSetting()
            {
                EditorGUILayout.PropertyField(LoadingSceneProperties.canvasGroup.property, LoadingSceneProperties.canvasGroup.content);
                EditorGUILayout.PropertyField(LoadingSceneProperties.mainLoadingAnimator.property, LoadingSceneProperties.mainLoadingAnimator.content);

                Uniform.SpaceTwoLine();
                EditorGUILayout.PropertyField(LoadingSceneProperties.fadingAnimationSpeed.property, LoadingSceneProperties.fadingAnimationSpeed.content);
                EditorGUILayout.PropertyField(LoadingSceneProperties.timeDelayDestroy.property, LoadingSceneProperties.timeDelayDestroy.content);
                Uniform.SpaceOneLine();
               
                EditorGUILayout.PropertyField(LoadingSceneProperties.virtualLoadTime.property, LoadingSceneProperties.virtualLoadTime.content);
                Uniform.SpaceOneLine();
                EditorGUILayout.PropertyField(LoadingSceneProperties.onBeginEvents.property);
                EditorGUILayout.PropertyField(LoadingSceneProperties.onFinishEvents.property);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif