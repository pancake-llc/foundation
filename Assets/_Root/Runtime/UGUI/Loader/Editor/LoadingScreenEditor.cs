#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Pancake.Loader;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.LoaderEditor
{
    [CustomEditor(typeof(LoadingScreen))]
    [Serializable]
    public class LoadingScreenEditor : UnityEditor.Editor
    {
        private LoadingScreen _loadingScreen;
        private int _currentTab;
        private Image _pakCountdownFilled;
        private Image _pakCountdownBg;
        private int _selectedSpinnerIndex;
        private SpinnerItem _selectedSpinnerItem;
        private List<Transform> _spinnerList = new List<Transform>();
        private List<string> _spinnerTitles = new List<string>();

        private void OnEnable()
        {
            _loadingScreen = (LoadingScreen) target;

            _spinnerList.Clear();
            foreach (Transform child in _loadingScreen.spinnerParent)
            {
                _spinnerList.Add(child);
            }

            foreach (var transform in _spinnerList)
            {
                _spinnerTitles.Add(transform.name);
            }

            _selectedSpinnerIndex = _loadingScreen.spinnerIndex;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var customSkin = (GUISkin) Resources.Load("loader-dark-skin");

            var toolbarTabs = new GUIContent[5];
            toolbarTabs[0] = new GUIContent("Layout");
            toolbarTabs[1] = new GUIContent("Hints");
            toolbarTabs[2] = new GUIContent("Images");
            toolbarTabs[3] = new GUIContent("Resources");
            toolbarTabs[4] = new GUIContent("Settings");

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent("Layout", "Layout"), customSkin.FindStyle("Toolbar Layout"))) _currentTab = 0;
            if (GUILayout.Button(new GUIContent("Hints", "Hints"), customSkin.FindStyle("Toolbar Hints"))) _currentTab = 1;
            if (GUILayout.Button(new GUIContent("Background", "Background"), customSkin.FindStyle("Toolbar Background"))) _currentTab = 2;
            if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Toolbar Resources"))) _currentTab = 3;
            if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Toolbar Settings"))) _currentTab = 4;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            _currentTab = GUILayout.Toolbar(_currentTab, toolbarTabs, customSkin.FindStyle("Toolbar Indicators"));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Property variables
            var singleBackgroundSprite = serializedObject.FindProperty("singleBackgroundSprite");
            var updateHelper = serializedObject.FindProperty("updateHelper");

            // Layout variables
            var hintsColor = serializedObject.FindProperty("hintsColor");
            var hintsFontSize = serializedObject.FindProperty("hintsFontSize");
            var hintsFont = serializedObject.FindProperty("hintsFont");
            var statusColor = serializedObject.FindProperty("statusColor");
            var statusSize = serializedObject.FindProperty("statusSize");
            var statusFont = serializedObject.FindProperty("statusFont");
            var pakColor = serializedObject.FindProperty("pakColor");
            var pakSize = serializedObject.FindProperty("pakSize");
            var pakFont = serializedObject.FindProperty("pakFont");
            var spinnerColor = serializedObject.FindProperty("spinnerColor");
            var statusSchema = serializedObject.FindProperty("statusSchema");

            // Hint variables
            var isHints = serializedObject.FindProperty("isHints");
            var hintsCollection = serializedObject.FindProperty("hintsCollection");
            var isChangeHintsWithTimer = serializedObject.FindProperty("isChangeHintsWithTimer");
            var hintsLifeTime = serializedObject.FindProperty("hintsLifeTime");

            // Image variables
            var enableRandomBackground = serializedObject.FindProperty("enableRandomBackground");
            var backgroundCollection = serializedObject.FindProperty("backgroundCollection");
            var isAutoChangeBg = serializedObject.FindProperty("isAutoChangeBg");
            var timeAutoChangeBg = serializedObject.FindProperty("timeAutoChangeBg");
            var backgroundFadingSpeed = serializedObject.FindProperty("backgroundFadingSpeed");

            // Resources
            var canvasGroup = serializedObject.FindProperty("canvasGroup");
            var txtStatus = serializedObject.FindProperty("txtStatus");
            var progressBar = serializedObject.FindProperty("progressBar");
            var txtHints = serializedObject.FindProperty("txtHints");
            var background = serializedObject.FindProperty("background");
            var backgroundAnimator = serializedObject.FindProperty("backgroundAnimator");
            var mainLoadingAnimator = serializedObject.FindProperty("mainLoadingAnimator");
            var txtPak = serializedObject.FindProperty("txtPak");
            var sliderCountdownPak = serializedObject.FindProperty("sliderCountdownPak");
            var txtCountdownPak = serializedObject.FindProperty("txtCountdownPak");
            var spinnerParent = serializedObject.FindProperty("spinnerParent");

            // Settings
            var enableStatusLabel = serializedObject.FindProperty("enableStatusLabel");
            var enablePressAnyKey = serializedObject.FindProperty("enablePressAnyKey");
            var useSpecificKey = serializedObject.FindProperty("useSpecificKey");
            var keyCode = serializedObject.FindProperty("keyCode");
            var pakText = serializedObject.FindProperty("pakText");
            var pakCountdownTimer = serializedObject.FindProperty("pakCountdownTimer");
            var enableVirtualLoading = serializedObject.FindProperty("enableVirtualLoading");
            var virtualLoadTime = serializedObject.FindProperty("virtualLoadTime");
            var fadingAnimationSpeed = serializedObject.FindProperty("fadingAnimationSpeed");
            var timeDelayDestroy = serializedObject.FindProperty("timeDelayDestroy");
            var onBeginEvents = serializedObject.FindProperty("onBeginEvents");
            var onFinishEvents = serializedObject.FindProperty("onFinishEvents");

            switch (_currentTab)
            {
                case 0:
                    GUILayout.Label("SPINNER", customSkin.FindStyle("Header"));
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Selected Spinner"), customSkin.FindStyle("Text"), GUILayout.Width(120));

                    _selectedSpinnerIndex = EditorGUILayout.Popup(_selectedSpinnerIndex, _spinnerTitles.ToArray());
                    _loadingScreen.spinnerIndex = _selectedSpinnerIndex;

                    if (GUILayout.Button("Update", customSkin.button))
                    {
                        if (isHints.boolValue)
                        {
                            _loadingScreen.txtHints.fontSize = _loadingScreen.hintsFontSize;
                            _loadingScreen.txtHints.font = _loadingScreen.hintsFont;
                            _loadingScreen.txtHints.color = _loadingScreen.hintsColor;
                        }

                        if (enableStatusLabel.boolValue)
                        {
                            _loadingScreen.txtStatus.fontSize = _loadingScreen.statusSize;
                            _loadingScreen.txtStatus.font = _loadingScreen.statusFont;
                            _loadingScreen.txtStatus.color = _loadingScreen.statusColor;
                        }

                        if (enablePressAnyKey.boolValue)
                        {
                            _loadingScreen.txtPak.fontSize = _loadingScreen.pakSize;
                            _loadingScreen.txtPak.font = _loadingScreen.pakFont;
                            _loadingScreen.txtPak.color = _loadingScreen.pakColor;
                            _loadingScreen.txtCountdownPak.color = _loadingScreen.pakColor;

                            try
                            {
                                _pakCountdownFilled = _loadingScreen.sliderCountdownPak.transform.Find("Filled").GetComponent<Image>();
                                _pakCountdownBg = _loadingScreen.sliderCountdownPak.transform.Find("Background").GetComponent<Image>();
                            }

                            catch
                            {
                                //
                            }

                            if (_pakCountdownFilled != null && _pakCountdownBg != null)
                            {
                                _pakCountdownFilled.color = _loadingScreen.spinnerColor;
                                _pakCountdownBg.color = new Color(_loadingScreen.spinnerColor.r, _loadingScreen.spinnerColor.g, _loadingScreen.spinnerColor.b, 0.08f);
                            }
                        }

                        try
                        {
                            _selectedSpinnerItem = _spinnerList[_selectedSpinnerIndex].GetComponent<SpinnerItem>();
                            _selectedSpinnerItem.UpdateColor(_loadingScreen.spinnerColor);
                            EditorSceneManager.MarkSceneDirty(PrefabStageUtility.GetCurrentPrefabStage().scene);
                        }

                        catch
                        {
                            Debug.Log("Loading Screen - Cannot initialize selected Spinner Item.", this);
                        }

                        updateHelper.boolValue = true;
                        updateHelper.boolValue = false;
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.Space(4);
                    EditorGUILayout.PropertyField(spinnerColor, new GUIContent(""));
                    GUILayout.Space(4);
                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("Restore Collor", customSkin.button))
                    {
                        if (isHints.boolValue)
                        {
                            _loadingScreen.hintsFontSize = _loadingScreen.txtHints.fontSize;
                            _loadingScreen.hintsFont = _loadingScreen.txtHints.font;
                            _loadingScreen.hintsColor = _loadingScreen.txtHints.color;
                        }

                        if (enableStatusLabel.boolValue)
                        {
                            _loadingScreen.statusSize = _loadingScreen.txtStatus.fontSize;
                            _loadingScreen.statusFont = _loadingScreen.txtStatus.font;
                            _loadingScreen.statusColor = _loadingScreen.txtStatus.color;
                        }

                        if (enablePressAnyKey.boolValue)
                        {
                            _loadingScreen.pakSize = _loadingScreen.txtPak.fontSize;
                            _loadingScreen.pakFont = _loadingScreen.txtPak.font;
                            _loadingScreen.pakColor = _loadingScreen.txtPak.color;
                        }
                    }

                    if (GUILayout.Button("Update Collor", customSkin.button))
                    {
                        if (isHints.boolValue)
                        {
                            _loadingScreen.txtHints.fontSize = _loadingScreen.hintsFontSize;
                            _loadingScreen.txtHints.font = _loadingScreen.hintsFont;
                            _loadingScreen.txtHints.color = _loadingScreen.hintsColor;
                        }

                        if (enableStatusLabel.boolValue)
                        {
                            _loadingScreen.txtStatus.fontSize = _loadingScreen.statusSize;
                            _loadingScreen.txtStatus.font = _loadingScreen.statusFont;
                            _loadingScreen.txtStatus.color = _loadingScreen.statusColor;
                        }

                        if (enablePressAnyKey.boolValue)
                        {
                            _loadingScreen.txtPak.fontSize = _loadingScreen.pakSize;
                            _loadingScreen.txtPak.font = _loadingScreen.pakFont;
                            _loadingScreen.txtPak.color = _loadingScreen.pakColor;
                            _loadingScreen.txtCountdownPak.color = _loadingScreen.pakColor;

                            try
                            {
                                _pakCountdownFilled = _loadingScreen.sliderCountdownPak.transform.Find("Filled").GetComponent<Image>();
                                _pakCountdownBg = _loadingScreen.sliderCountdownPak.transform.Find("Background").GetComponent<Image>();
                            }

                            catch
                            {
                                //
                            }

                            if (_pakCountdownFilled != null && _pakCountdownBg != null)
                            {
                                _pakCountdownFilled.color = _loadingScreen.spinnerColor;
                                _pakCountdownBg.color = new Color(_loadingScreen.spinnerColor.r, _loadingScreen.spinnerColor.g, _loadingScreen.spinnerColor.b, 0.08f);
                            }
                        }

                        try
                        {
                            _selectedSpinnerItem = _spinnerList[_selectedSpinnerIndex].GetComponent<SpinnerItem>();
                            _selectedSpinnerItem.UpdateColor(_loadingScreen.spinnerColor);
                            EditorSceneManager.MarkSceneDirty(PrefabStageUtility.GetCurrentPrefabStage().scene);
                        }

                        catch
                        {
                            Debug.Log("Loading Screen - Cannot initialize selected Spinner Item.", this);
                        }

                        updateHelper.boolValue = true;
                        updateHelper.boolValue = false;
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();


                    for (int i = 0; i < _loadingScreen.spinnerParent.childCount; i++)
                    {
                        var child = _loadingScreen.spinnerParent.GetChild(i);
                        if (child.name != _spinnerList[_selectedSpinnerIndex].ToString().Replace(" (UnityEngine.RectTransform)", "").Trim())
                            child.gameObject.SetActive(false);
                        else child.gameObject.SetActive(true);
                    }

                    GUILayout.Space(18);
                    GUILayout.Label("EDITOR DEBUG", customSkin.FindStyle("Header"));

                    if (_loadingScreen.canvasGroup != null)
                    {
                        if (_loadingScreen.canvasGroup.alpha == 0)
                        {
                            if (GUILayout.Button("Make It Visible", customSkin.button))
                                _loadingScreen.canvasGroup.alpha = 1;
                        }

                        else
                        {
                            if (GUILayout.Button("Make It Invisible", customSkin.button))
                                _loadingScreen.canvasGroup.alpha = 0;
                        }
                    }

                    GUILayout.Space(6);
                    break;

                case 1:
                    GUILayout.Label("HINTS", customSkin.FindStyle("Header"));
                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    isHints.boolValue = GUILayout.Toggle(isHints.boolValue, new GUIContent("Enable Random Hints"), customSkin.FindStyle("Toggle"));
                    if (isHints.boolValue)
                    {
                        EditorGUILayout.LabelField(new GUIContent("Hint"), GUILayout.Width(100));
                        GUILayout.BeginHorizontal();

                        EditorGUILayout.PropertyField(hintsFontSize, new GUIContent(""), GUILayout.Width(40));
                        EditorGUILayout.PropertyField(hintsFont, new GUIContent(""));
                        EditorGUILayout.PropertyField(hintsColor, new GUIContent(""));

                        GUILayout.EndHorizontal();

                        if (txtHints.objectReferenceValue != null) _loadingScreen.txtHints.gameObject.SetActive(true);
                        GUILayout.Space(4);
                        GUILayout.BeginHorizontal();

                        isChangeHintsWithTimer.boolValue =
                            GUILayout.Toggle(isChangeHintsWithTimer.boolValue, new GUIContent("Change With Timer"), customSkin.FindStyle("Toggle"));
                        if (isChangeHintsWithTimer.boolValue)
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(new GUIContent(""), customSkin.FindStyle("Text"), GUILayout.Width(130));
                            EditorGUILayout.PropertyField(hintsLifeTime, new GUIContent(""));
                            GUILayout.EndHorizontal();
                        }

                        GUILayout.EndHorizontal();
                        GUILayout.Space(4);

                        EditorGUI.indentLevel = 1;
                        EditorGUILayout.PropertyField(hintsCollection, new GUIContent("Hints Collection"), true);
                        EditorGUI.indentLevel = 0;
                        
                        GUILayout.Space(4);
                        if (GUILayout.Button("Add a new hint", customSkin.button)) _loadingScreen.hintsCollection.Add("Type your hint here.");

                        if (_loadingScreen.txtHints == null)
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.HelpBox("'Hint Text Object' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
                            GUILayout.EndHorizontal();
                        }
                    }
                    else if (isHints.boolValue == false && txtHints.objectReferenceValue != null) _loadingScreen.txtHints.gameObject.SetActive(false);

                    GUILayout.EndVertical();

                    GUILayout.Space(6);
                    break;

                case 2:
                    GUILayout.Label("BACKGROUND", customSkin.FindStyle("Header"));
                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    enableRandomBackground.boolValue = GUILayout.Toggle(enableRandomBackground.boolValue, new GUIContent("Enable Random Background"), customSkin.FindStyle("Toggle"));
                    if (enableRandomBackground.boolValue)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUI.indentLevel = 1;

                        EditorGUILayout.PropertyField(backgroundCollection, new GUIContent("Backgrounds"), true);
                        backgroundCollection.isExpanded = true;

                        EditorGUI.indentLevel = 0;
                        GUILayout.Space(4);
                        GUILayout.EndHorizontal();
                        
                        EditorGUI.indentLevel = 1;
                        isAutoChangeBg.boolValue = GUILayout.Toggle(isAutoChangeBg.boolValue, new GUIContent("Change With Timer"), customSkin.FindStyle("Toggle"));
                        if (isAutoChangeBg.boolValue)
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(new GUIContent("Time"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                            EditorGUILayout.PropertyField(timeAutoChangeBg, new GUIContent(""));
                            GUILayout.EndHorizontal();
                            
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(new GUIContent("Fading Speed"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                            EditorGUILayout.PropertyField(backgroundFadingSpeed, new GUIContent(""));
                            GUILayout.EndHorizontal();
                        }
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField(new GUIContent("Background"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                        EditorGUILayout.PropertyField(singleBackgroundSprite, new GUIContent(""));

                        GUILayout.EndHorizontal();

                        if (_loadingScreen.background != null) _loadingScreen.background.sprite = _loadingScreen.singleBackgroundSprite;

                        updateHelper.boolValue = true;
                        updateHelper.boolValue = false;
                    }
                    
                    EditorGUI.indentLevel = 0;
                    if (_loadingScreen.background == null)
                    {
                        GUILayout.Space(4);
                        EditorGUILayout.HelpBox("'Image Object' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
                    }

                    GUILayout.EndVertical();

                    GUILayout.Space(4);
                    break;

                case 3:
                    GUILayout.Label("RESOURCES", customSkin.FindStyle("Header"));
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.LabelField(new GUIContent("Canvas Group"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    EditorGUILayout.PropertyField(canvasGroup, new GUIContent(""));

                    GUILayout.EndHorizontal();

                    if (enableStatusLabel.boolValue)
                    {
                        GUILayout.BeginHorizontal(EditorStyles.helpBox);

                        EditorGUILayout.LabelField(new GUIContent("Status Text"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                        EditorGUILayout.PropertyField(txtStatus, new GUIContent(""));

                        GUILayout.EndHorizontal();
                    }

                    if (isHints.boolValue)
                    {
                        GUILayout.BeginHorizontal(EditorStyles.helpBox);

                        EditorGUILayout.LabelField(new GUIContent("Hint Text"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                        EditorGUILayout.PropertyField(txtHints, new GUIContent(""));

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.LabelField(new GUIContent("Progress Slider"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    EditorGUILayout.PropertyField(progressBar, new GUIContent(""));

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.LabelField(new GUIContent("Main Animator"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    EditorGUILayout.PropertyField(mainLoadingAnimator, new GUIContent(""));

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.LabelField(new GUIContent("Spinner Parent"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    EditorGUILayout.PropertyField(spinnerParent, new GUIContent(""));

                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.LabelField(new GUIContent("Background"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    EditorGUILayout.PropertyField(background, new GUIContent(""));

                    GUILayout.EndHorizontal();

                    if (enableRandomBackground.boolValue)
                    {
                        GUILayout.BeginHorizontal(EditorStyles.helpBox);

                        EditorGUILayout.LabelField(new GUIContent("BG Animator"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                        EditorGUILayout.PropertyField(backgroundAnimator, new GUIContent(""));

                        GUILayout.EndHorizontal();
                    }

                    if (enablePressAnyKey.boolValue)
                    {
                        GUILayout.BeginHorizontal(EditorStyles.helpBox);

                        EditorGUILayout.LabelField(new GUIContent("PAK Text"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                        EditorGUILayout.PropertyField(txtPak, new GUIContent(""));

                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal(EditorStyles.helpBox);

                        EditorGUILayout.LabelField(new GUIContent("PAK CD Slider"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                        EditorGUILayout.PropertyField(sliderCountdownPak, new GUIContent(""));

                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal(EditorStyles.helpBox);

                        EditorGUILayout.LabelField(new GUIContent("PAK CD Label"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                        EditorGUILayout.PropertyField(txtCountdownPak, new GUIContent(""));

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.Space(6);
                    break;

                case 4:
                    GUILayout.Label("SETTINGS", customSkin.FindStyle("Header"));

                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    enableStatusLabel.boolValue = GUILayout.Toggle(enableStatusLabel.boolValue, new GUIContent("Enable Status Label"), customSkin.FindStyle("Toggle"));
                    if (enableStatusLabel.boolValue)
                    {
                        EditorGUILayout.LabelField(new GUIContent("Schema"), customSkin.FindStyle("Text"), GUILayout.Width(100));
                        EditorGUILayout.PropertyField(statusSchema, new GUIContent(""), GUILayout.Height(25));

                        GUILayout.Space(2);
                        EditorGUILayout.LabelField(new GUIContent("Font"), customSkin.FindStyle("Text"), GUILayout.Width(100));
                        GUILayout.BeginHorizontal();

                        EditorGUILayout.PropertyField(statusSize, new GUIContent(""), GUILayout.Width(40));
                        EditorGUILayout.PropertyField(statusFont, new GUIContent(""));
                        EditorGUILayout.PropertyField(statusColor, new GUIContent(""));

                        GUILayout.EndHorizontal();

                        if (txtStatus == null || txtStatus.objectReferenceValue == null)
                        {
                            GUILayout.Space(2);
                            EditorGUILayout.HelpBox("'Status Label' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
                        }

                        GUILayout.Space(2);
                    }

                    if (txtStatus != null && txtStatus.objectReferenceValue != null) _loadingScreen.txtStatus.gameObject.SetActive(enableStatusLabel.boolValue);

                    GUILayout.EndVertical();

                    GUILayout.Space(2);

                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    enablePressAnyKey.boolValue = GUILayout.Toggle(enablePressAnyKey.boolValue, new GUIContent("Enable Press Any Key"), customSkin.FindStyle("Toggle"));
                    if (enablePressAnyKey.boolValue)
                    {
                        EditorGUILayout.LabelField(new GUIContent("Text"), GUILayout.Width(120));
                        GUILayout.Space(-15);
                        EditorGUILayout.PropertyField(pakText, new GUIContent(""), GUILayout.Height(50));

                        if (txtPak != null && txtPak.objectReferenceValue != null) _loadingScreen.txtPak.text = _loadingScreen.pakText;


                        EditorGUILayout.LabelField(new GUIContent("Font"), GUILayout.Width(100));

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(pakSize, new GUIContent(""), GUILayout.Width(40));
                        EditorGUILayout.PropertyField(pakFont, new GUIContent(""));
                        EditorGUILayout.PropertyField(pakColor, new GUIContent(""));
                        GUILayout.EndHorizontal();

                        GUILayout.Space(2);
                        EditorGUILayout.BeginHorizontal();
                        useSpecificKey.boolValue = GUILayout.Toggle(useSpecificKey.boolValue, new GUIContent("Use Specific PAK key"), customSkin.FindStyle("Toggle"));
                        if (useSpecificKey.boolValue)
                        {
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(new GUIContent(""), customSkin.FindStyle("Text"), GUILayout.Width(130));
                            EditorGUILayout.PropertyField(keyCode, new GUIContent(""));
                            GUILayout.EndHorizontal();
                        }

                        EditorGUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(new GUIContent("PAK Countdown"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                        EditorGUILayout.PropertyField(pakCountdownTimer, new GUIContent(""));
                        GUILayout.EndHorizontal();

                        if (_loadingScreen.mainLoadingAnimator == null && enablePressAnyKey.boolValue)
                        {
                            GUILayout.Space(2);
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.HelpBox("'Main Animator' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
                            GUILayout.EndHorizontal();
                        }

                        GUILayout.Space(2);
                    }

                    GUILayout.EndVertical();

                    GUILayout.Space(2);
                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    enableVirtualLoading.boolValue =
                        GUILayout.Toggle(enableVirtualLoading.boolValue, new GUIContent("Enable Virtual Loading"), customSkin.FindStyle("Toggle"));

                    if (enableVirtualLoading.boolValue) EditorGUILayout.PropertyField(virtualLoadTime, new GUIContent("Virtual Load Time"));
                    GUILayout.Space(2);
                    GUILayout.EndVertical();

                    GUILayout.Space(2);

                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(new GUIContent("Fading Speed"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    EditorGUILayout.PropertyField(fadingAnimationSpeed, new GUIContent(""));

                    GUILayout.EndHorizontal();

                    GUILayout.Space(2);
                    GUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(new GUIContent("Time Delay Destroy"), customSkin.FindStyle("Text"), GUILayout.Width(120));
                    EditorGUILayout.PropertyField(timeDelayDestroy, new GUIContent(""));

                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();

                    GUILayout.Space(18);
                    GUILayout.Label("EVENTS", customSkin.FindStyle("Header"));

                    EditorGUILayout.PropertyField(onBeginEvents);
                    EditorGUILayout.PropertyField(onFinishEvents);

                    GUILayout.Space(6);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif