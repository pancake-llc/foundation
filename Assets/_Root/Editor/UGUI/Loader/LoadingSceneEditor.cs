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
    [CustomEditor(typeof(LoadingScene))]
    [Serializable]
    public class LoadingSceneEditor : UnityEditor.Editor
    {
        private Image _pakCountdownFilled;
        private Image _pakCountdownBg;
        private SpinnerItem _selectedSpinnerItem;
        private List<Transform> _spinnerList = new List<Transform>();
        private LoadingScene _loading;

        private void Init()
        {
            _loading = target as LoadingScene;
            // Property variables
            LoadingSceneProperties.singleBackgroundSprite.property = serializedObject.FindProperty("singleBackgroundSprite");

            // Layout variables
            LoadingSceneProperties.hintsColor.property = serializedObject.FindProperty("hintsColor");
            LoadingSceneProperties.hintsFontSize.property = serializedObject.FindProperty("hintsFontSize");
            LoadingSceneProperties.hintsFont.property = serializedObject.FindProperty("hintsFont");
            LoadingSceneProperties.statusColor.property = serializedObject.FindProperty("statusColor");
            LoadingSceneProperties.statusSize.property = serializedObject.FindProperty("statusSize");
            LoadingSceneProperties.statusFont.property = serializedObject.FindProperty("statusFont");
            LoadingSceneProperties.pakColor.property = serializedObject.FindProperty("pakColor");
            LoadingSceneProperties.pakSize.property = serializedObject.FindProperty("pakSize");
            LoadingSceneProperties.pakFont.property = serializedObject.FindProperty("pakFont");
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
            LoadingSceneProperties.txtPak.property = serializedObject.FindProperty("txtPak");
            LoadingSceneProperties.sliderCountdownPak.property = serializedObject.FindProperty("sliderCountdownPak");
            LoadingSceneProperties.txtCountdownPak.property = serializedObject.FindProperty("txtCountdownPak");
            LoadingSceneProperties.spinnerParent.property = serializedObject.FindProperty("spinnerParent");
            LoadingSceneProperties.enableStatusLabel.property = serializedObject.FindProperty("enableStatusLabel");
            LoadingSceneProperties.enablePressAnyKey.property = serializedObject.FindProperty("enablePressAnyKey");
            LoadingSceneProperties.useSpecificKey.property = serializedObject.FindProperty("useSpecificKey");
            LoadingSceneProperties.keyCode.property = serializedObject.FindProperty("keyCode");
            LoadingSceneProperties.pakText.property = serializedObject.FindProperty("pakText");
            LoadingSceneProperties.pakCountdownTimer.property = serializedObject.FindProperty("pakCountdownTimer");
            LoadingSceneProperties.enableVirtualLoading.property = serializedObject.FindProperty("enableVirtualLoading");
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

            if (PrefabUtility.IsPartOfAnyPrefab(_loading.gameObject))
            {
                if (!_loading.gameObject.IsAddressableWithLabel(LoadingScene.LABEL))
                {
                    Uniform.HelpBox("Click the toogle below to mark the popup as can be loaded by addressable", MessageType.Warning);
                    if (GUILayout.Button("Mark Popup")) _loading.gameObject.MarkAddressableWithLabel(LoadingScene.LABEL);
                }
                else
                {
                    Uniform.HelpBox("Marked as popup", MessageType.Info);
                }
            }

            Uniform.DrawUppercaseSection("LOADING_SPINNER", "SPINNER", DrawLayout, false);
            Uniform.SpaceOneLine();
            Uniform.DrawUppercaseSection("LOADING_HINT", "HINT", DrawHint, false);
            Uniform.SpaceOneLine();
            Uniform.DrawUppercaseSection("LOADING_BACKGROUND", "BG", DrawBackground, false);
            Uniform.SpaceOneLine();
            Uniform.DrawUppercaseSection("LOADING_STATUS", "STATUS", DrawStatus, false);
            Uniform.SpaceOneLine();
            Uniform.DrawUppercaseSection("LOADING_PAK", "PRESS ANY KEY", DrawPak, false);
            Uniform.SpaceOneLine();
            Uniform.DrawUppercaseSection("LOADING_SETTING", "SETTING", DrawSetting, false);

            void DrawLayout()
            {
                EditorGUILayout.PropertyField(LoadingSceneProperties.spinnerParent.property, LoadingSceneProperties.spinnerParent.content);
                if (LoadingSceneProperties.spinnerIndex.property.intValue == 1)
                {
                    EditorGUILayout.PropertyField(LoadingSceneProperties.progressBar.property, LoadingSceneProperties.progressBar.content);
                }

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Spinner"), GUILayout.Width(120));
                LoadingSceneProperties.spinnerIndex.property.intValue = EditorGUILayout.Popup(LoadingSceneProperties.spinnerIndex.property.intValue, _spinnerList.Map(_=>_.gameObject.name).ToArray());
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

                    if (LoadingSceneProperties.enablePressAnyKey.property.boolValue)
                    {
                        _loading.pakSize = _loading.txtPak.fontSize;
                        _loading.pakFont = _loading.txtPak.font;
                        _loading.pakColor = _loading.txtPak.color;
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

                    if (LoadingSceneProperties.enablePressAnyKey.property.boolValue)
                    {
                        _loading.txtPak.fontSize = _loading.pakSize;
                        _loading.txtPak.font = _loading.pakFont;
                        _loading.txtPak.color = _loading.pakColor;
                        _loading.txtCountdownPak.color = _loading.pakColor;

                        try
                        {
                            _pakCountdownFilled = _loading.sliderCountdownPak.transform.Find("Filled").GetComponent<Image>();
                            _pakCountdownBg = _loading.sliderCountdownPak.transform.Find("Background").GetComponent<Image>();
                        }

                        catch
                        {
                            //
                        }

                        if (_pakCountdownFilled != null && _pakCountdownBg != null)
                        {
                            _pakCountdownFilled.color = _loading.spinnerColor;
                            _pakCountdownBg.color = new Color(_loading.spinnerColor.r, _loading.spinnerColor.g, _loading.spinnerColor.b, 0.08f);
                        }
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

            void DrawPak()
            {
                Uniform.Toggle(LoadingSceneProperties.enablePressAnyKey.property, LoadingSceneProperties.enablePressAnyKey.content);
                if (LoadingSceneProperties.enablePressAnyKey.property.boolValue)
                {
                    EditorGUILayout.PropertyField(LoadingSceneProperties.txtCountdownPak.property, LoadingSceneProperties.txtCountdownPak.content);
                    EditorGUILayout.PropertyField(LoadingSceneProperties.txtPak.property, LoadingSceneProperties.txtPak.content);
                    EditorGUILayout.PropertyField(LoadingSceneProperties.pakSize.property, LoadingSceneProperties.pakSize.content);
                    EditorGUILayout.PropertyField(LoadingSceneProperties.pakFont.property, LoadingSceneProperties.pakFont.content);
                    EditorGUILayout.PropertyField(LoadingSceneProperties.pakColor.property, LoadingSceneProperties.pakColor.content);
                    EditorGUILayout.PropertyField(LoadingSceneProperties.sliderCountdownPak.property, LoadingSceneProperties.sliderCountdownPak.content);
                    EditorGUILayout.PropertyField(LoadingSceneProperties.pakCountdownTimer.property, LoadingSceneProperties.pakCountdownTimer.content);
                    EditorGUILayout.PropertyField(LoadingSceneProperties.pakText.property, new GUIContent(""));

                    if (LoadingSceneProperties.txtPak.property != null && LoadingSceneProperties.txtPak.property.objectReferenceValue != null)
                    {
                        _loading.txtPak.text = _loading.pakText;

                        _loading.txtPak.fontSize = _loading.pakSize;
                        _loading.txtPak.font = _loading.pakFont;
                        _loading.txtPak.color = _loading.pakColor;
                        _loading.txtCountdownPak.color = _loading.pakColor;

                        try
                        {
                            _pakCountdownFilled = _loading.sliderCountdownPak.transform.Find("Filled").GetComponent<Image>();
                            _pakCountdownBg = _loading.sliderCountdownPak.transform.Find("Background").GetComponent<Image>();
                        }
                        catch
                        {
                            //
                        }

                        if (_pakCountdownFilled != null && _pakCountdownBg != null)
                        {
                            _pakCountdownFilled.color = _loading.spinnerColor;
                            _pakCountdownBg.color = new Color(_loading.spinnerColor.r, _loading.spinnerColor.g, _loading.spinnerColor.b, 0.08f);
                        }
                    }

                    Uniform.SpaceTwoLine();

                    Uniform.Toggle(LoadingSceneProperties.useSpecificKey.property, LoadingSceneProperties.useSpecificKey.content);
                    if (LoadingSceneProperties.useSpecificKey.property.boolValue)
                    {
                        EditorGUILayout.PropertyField(LoadingSceneProperties.keyCode.property, LoadingSceneProperties.keyCode.content);
                    }

                    if (_loading.mainLoadingAnimator == null && LoadingSceneProperties.enablePressAnyKey.property.boolValue)
                    {
                        GUILayout.Space(2);
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.HelpBox("'Main Animator' is not assigned. Please assign the correct variable.", MessageType.Error);
                        GUILayout.EndHorizontal();
                    }

                    GUILayout.Space(2);
                }
            }

            void DrawSetting()
            {
                EditorGUILayout.PropertyField(LoadingSceneProperties.canvasGroup.property, LoadingSceneProperties.canvasGroup.content);
                EditorGUILayout.PropertyField(LoadingSceneProperties.mainLoadingAnimator.property, LoadingSceneProperties.mainLoadingAnimator.content);

                Uniform.SpaceTwoLine();
                EditorGUILayout.PropertyField(LoadingSceneProperties.fadingAnimationSpeed.property, LoadingSceneProperties.fadingAnimationSpeed.content);
                EditorGUILayout.PropertyField(LoadingSceneProperties.timeDelayDestroy.property, LoadingSceneProperties.timeDelayDestroy.content);
                Uniform.SpaceOneLine();
                Uniform.Toggle(LoadingSceneProperties.enableVirtualLoading.property, LoadingSceneProperties.enableVirtualLoading.content);
                if (LoadingSceneProperties.enableVirtualLoading.property.boolValue)
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