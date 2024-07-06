using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LitMotion;
using Pancake.Common;
using Pancake.Linq;
using Pancake.Sound;
using PancakeEditor.Sound.Reflection;
using PancakeEditor.Common;
using PancakeEditor.Sound;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Audio;
using AudioSettings = Pancake.Sound.AudioSettings;

namespace PancakeEditor
{
    public static class AudioWindow
    {
        private static bool isVolumeSnapped;
        private static AudioMixer mixer;
        private static AudioMixerGroup duplicateTrackSource;
        private static readonly IUniqueIdGenerator IdGenerator = new IdGenerator();
        private static ReorderableList assetReorderableList;
        private static int currentSelectAssetIndex;
        private static readonly Dictionary<string, AudioAssetEditor> AssetEditorDict = new();
        private static bool hasOutputAssetPath;
        private static bool isInEntitiesEditMode;
        private static bool hasAssetListReordered;
        private static Vector2 assetListScrollPos = Vector2.zero;
        private static Vector2 entitiesScrollPos = Vector2.zero;
        private static int pickerId = -1;
        private static int currProjectSettingVoiceCount = -1;
        private static int currentMixerTracksCount = -1;

        private static AudioMixer AudioMixer
        {
            get
            {
                if (!mixer)
                {
                    var result = ProjectDatabase.FindAll<AudioMixer>().Filter(a => a.name.Equals(AudioConstant.MIXER_NAME)).FirstOrDefault();
                    if (result != null) mixer = result;
                }

                return mixer;
            }
        }

        private enum EMultiClipsImportOption
        {
            MultipleForEach,
            Cancel,
            OneForAll,
        }

        public static void OnInspectorGUI(
            Rect position,
            ref Vector2 scrollPosition,
            ref Wizard.AudioTabType currentTab,
            ref UnityEditor.Editor effectTrackEditor,
            ref Wizard.AudioSetingTabType currentSettingTab)
        {
            var audioSetting = Resources.Load<AudioSettings>(nameof(AudioSettings));
            var audioEditorSetting = Resources.Load<AudioEditorSetting>(nameof(AudioEditorSetting));
            var audioDatas = ProjectDatabase.FindAll<AudioData>();
            var audioData = !audioDatas.IsNullOrEmpty() ? audioDatas[0] : null;
            if (audioSetting == null || audioEditorSetting == null || audioData == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create Missing Audio Setting", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
                {
                    if (audioSetting == null)
                    {
                        var setting = ScriptableObject.CreateInstance<AudioSettings>();
                        if (!Directory.Exists(Common.Editor.DEFAULT_RESOURCE_PATH)) Directory.CreateDirectory(Common.Editor.DEFAULT_RESOURCE_PATH);
                        AssetDatabase.CreateAsset(setting, $"{Common.Editor.DEFAULT_RESOURCE_PATH}/{nameof(AudioSettings)}.asset");
                        Debug.Log($"{nameof(AudioSettings).SetColor("f75369")} was created ad {Common.Editor.DEFAULT_RESOURCE_PATH}/{nameof(AudioSettings)}.asset");
                    }

                    if (audioEditorSetting == null)
                    {
                        var editorSetting = ScriptableObject.CreateInstance<AudioEditorSetting>();
                        editorSetting.ResetToFactorySettings();
                        if (!Directory.Exists(Common.Editor.DEFAULT_EDITOR_RESOURCE_PATH)) Directory.CreateDirectory(Common.Editor.DEFAULT_EDITOR_RESOURCE_PATH);
                        AssetDatabase.CreateAsset(editorSetting, $"{Common.Editor.DEFAULT_EDITOR_RESOURCE_PATH}/{nameof(AudioEditorSetting)}.asset");
                        Debug.Log(
                            $"{nameof(AudioEditorSetting).SetColor("f75369")} was created ad {Common.Editor.DEFAULT_EDITOR_RESOURCE_PATH}/{nameof(AudioEditorSetting)}.asset");
                    }

                    if (audioData == null)
                    {
                        var editorAudioData = ScriptableObject.CreateInstance<AudioData>();
                        const string path = "Assets/_Root/Audio";
                        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                        AssetDatabase.CreateAsset(editorAudioData, $"{path}/{nameof(AudioData)}.asset");
                        Debug.Log($"{nameof(AudioData).SetColor("f75369")} was created ad {path}/{nameof(AudioData)}.asset");
                    }

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                DrawTab(ref currentTab);
                switch (currentTab)
                {
                    case Wizard.AudioTabType.Library:
                        DrawTabLibrary(position);
                        break;
                    case Wizard.AudioTabType.Setting:
                        DrawTabSetting(position, ref currentSettingTab);
                        break;
                    case Wizard.AudioTabType.EffectEditor:
                        DrawTabEffectEditor(ref scrollPosition, ref mixer, ref effectTrackEditor);
                        break;
                }
            }
        }

        public static void OnLostFocus()
        {
            EditorPlayAudioClip.In.StopAllClips();
            EditorPlayAudioClip.In.RemovePlaybackIndicatorListener(Wizard.CallRepaint);
        }

        public static void OnFocus() { EditorPlayAudioClip.In.AddPlaybackIndicatorListener(Wizard.CallRepaint); }

        public static void OnEnable()
        {
            hasOutputAssetPath = Directory.Exists(LibraryDataContainer.Data.Settings.assetOutputPath);
            ResetSetting();
            EditorPlayAudioClip.In.AddPlaybackIndicatorListener(Wizard.CallRepaint);
            InitEditorDictionary();
            InitReorderableList();
            Undo.undoRedoPerformed += Wizard.CallRepaint;
        }

        public static void OnDisable()
        {
            EditorAudioEx.onCloseWindow?.Invoke();
            ResetTracksAndAudioVoices();
            EditorPlayAudioClip.In.RemovePlaybackIndicatorListener(Wizard.CallRepaint);
            foreach (var editor in AssetEditorDict.Values)
            {
                UnityEngine.Object.DestroyImmediate(editor);
            }

            Undo.undoRedoPerformed -= Wizard.CallRepaint;
            if (hasAssetListReordered) LibraryDataContainer.Data.SaveSetting();
        }

        public static void OnPostprocessAllAssets() { }

        private static void ResetSetting() { EditorAudioEx.onSelectAsset = null; }

        private static void DrawTabEffectEditor(ref Vector2 scrollPosition, ref AudioMixer mixer, ref UnityEditor.Editor effectTrackEditor)
        {
            mixer = EditorGUILayout.ObjectField(new GUIContent("AudioMixer"), mixer, typeof(AudioMixer), allowSceneObjects: false) as AudioMixer;
            if (mixer == null)
            {
                var result = ProjectDatabase.FindAll<AudioMixer>().Filter(a => a.name.Equals(AudioConstant.MIXER_NAME)).FirstOrDefault();
                if (result != null) mixer = result;
            }

            if (mixer == null)
            {
                GUILayout.Label($"{AudioConstant.MIXER_NAME} not found. Please select an AudioMixer to edit".SetColor(Uniform.Orange), Uniform.CenterRichLabel);
            }
            else
            {
                var effectTrack = mixer.FindMatchingGroups(AudioConstant.EFFECT_TRACK_NAME).FirstOrDefault();
                if (effectTrack)
                {
                    UnityEditor.Editor.CreateCachedEditor(effectTrack, null, ref effectTrackEditor);

                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    {
                        GUILayout.FlexibleSpace();

                        var buttonContent = new GUIContent("Effect Exposed Parameters");
                        if (EditorGUILayout.DropdownButton(buttonContent, FocusType.Passive))
                        {
                            var exposedParaPopup = new CustomExposedParametersPopupWindow();
                            exposedParaPopup.CreateReorderableList(mixer);
                            var rect = GUILayoutUtility.GetRect(buttonContent, EditorStyles.toolbarDropDown);
                            PopupWindow.Show(rect, exposedParaPopup);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();

                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                    effectTrackEditor.OnInspectorGUI();
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        private static void DrawTabSetting(Rect position, ref Wizard.AudioSetingTabType settingTabType)
        {
            Uniform.DrawLine(Uniform.BabyBlueEyes);
            EditorGUILayout.BeginHorizontal();
            DrawButtonSettingAudio(ref settingTabType);
            DrawButtonSettingGui(ref settingTabType);
            DrawButtonSettingMiscellanous(ref settingTabType);
            EditorGUILayout.EndHorizontal();

            switch (settingTabType)
            {
                case Wizard.AudioSetingTabType.Audio:
                    DrawTabAudio();
                    break;
                case Wizard.AudioSetingTabType.GUI:
                    DrawTabGui();
                    break;
                case Wizard.AudioSetingTabType.Miscellaneous:
                    DrawTabMiscellaneous(position);
                    break;
            }
        }

        private static void DrawTabLibrary(Rect position)
        {
            if (!hasOutputAssetPath)
            {
                DrawAssetOutputPath(position);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            {
                if (isInEntitiesEditMode && TryGetCurrentAssetEditor(out var editor))
                {
                    DrawEntitiesList(position, editor);
                }
                else
                {
                    Uniform.SplitRectHorizontal(position,
                        0.65f,
                        10,
                        out var entitiesFactoryRect,
                        out var assetListRect);

                    DrawEntityFactory(position, entitiesFactoryRect);

                    DrawAssetList(assetListRect);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawTab(ref Wizard.AudioTabType currentTab)
        {
            EditorGUILayout.BeginHorizontal();

            DrawButtonLibrary(ref currentTab);
            DrawButtonSetting(ref currentTab);
            DrawButtonEffectEditor(ref currentTab);
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawTabAudio()
        {
            GUILayout.Space(2);
            DrawAudioFilterSlope();
            GUILayout.Space(4);
            DrawBGMSetting();
            GUILayout.Space(2);
            DrawCombFilteringSetting();
            GUILayout.Space(4);
            DrawDefaultEasing();
            DrawSeamlessLoopEasing();
            GUILayout.Space(4);
            DrawAudioPlayerSetting();
            GUILayout.Space(4);
            DrawAudioProjectSettings();

            void DrawBGMSetting()
            {
                GUILayout.Label("BGM".ToWhiteBold(), Uniform.RichLabel);
                using (new EditorGUI.IndentLevelScope())
                {
                    AudioSettings.AlwaysPlayMusicAsBGM = EditorGUILayout.Toggle("Always Play Music As BGM", AudioSettings.AlwaysPlayMusicAsBGM);

                    if (AudioSettings.AlwaysPlayMusicAsBGM)
                    {
                        AudioSettings.DefaultBGMTransition = (EAudioTransition) EditorGUILayout.EnumPopup("Default Transition", AudioSettings.DefaultBGMTransition);
                        AudioSettings.DefaultBGMTransitionTime = EditorGUILayout.FloatField("Default Transition Time", AudioSettings.DefaultBGMTransitionTime);
                    }
                }
            }

            void DrawCombFilteringSetting()
            {
                EditorGUILayout.LabelField("Comb Filtering".ToWhiteBold(), Uniform.RichLabel);
                using (new EditorGUI.IndentLevelScope())
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Time To Prevent Comb Filtering");

                    AudioSettings.CombFilteringPreventionInSeconds = EditorGUILayout.FloatField(AudioSettings.CombFilteringPreventionInSeconds);

                    EditorGUI.BeginDisabledGroup(Mathf.Approximately(AudioSettings.CombFilteringPreventionInSeconds, 0.04f));
                    if (GUILayout.Button("Default", GUILayout.Width(80))) AudioSettings.CombFilteringPreventionInSeconds = 0.04f;

                    EditorGUI.EndDisabledGroup();
                    GUILayout.EndHorizontal();

                    AudioSettings.LogCombFilteringWarning = EditorGUILayout.Toggle("Log Warning If Occurs", AudioSettings.LogCombFilteringWarning);
                }
            }

            void DrawDefaultEasing()
            {
                EditorGUILayout.LabelField("Default Easing".ToWhiteBold(), Uniform.RichLabel);
                using (new EditorGUI.IndentLevelScope())
                {
                    AudioSettings.DefaultFadeInEase = (Ease) EditorGUILayout.EnumPopup("Fade In", AudioSettings.DefaultFadeInEase);
                    AudioSettings.DefaultFadeOutEase = (Ease) EditorGUILayout.EnumPopup("Fade Out", AudioSettings.DefaultFadeOutEase);
                }
            }

            void DrawSeamlessLoopEasing()
            {
                EditorGUILayout.LabelField("Seamless Loop Easing".ToWhiteBold(), Uniform.RichLabel);
                using (new EditorGUI.IndentLevelScope())
                {
                    AudioSettings.SeamlessFadeInEase = (Ease) EditorGUILayout.EnumPopup("Fade In", AudioSettings.SeamlessFadeInEase);
                    AudioSettings.SeamlessFadeOutEase = (Ease) EditorGUILayout.EnumPopup("Fade Out", AudioSettings.SeamlessFadeOutEase);
                }
            }

            void DrawAudioFilterSlope()
            {
                AudioSettings.AudioFilterSlope = (EFilterSlope) EditorGUILayout.EnumPopup("Audio Filter Slope", AudioSettings.AudioFilterSlope);
            }

            void DrawAudioPlayerSetting()
            {
                EditorGUILayout.LabelField("Audio Player".ToWhiteBold(), Uniform.RichLabel);

                using (new EditorGUI.IndentLevelScope())
                {
                    AudioSettings.LogAccessRecycledPlayerWarning = EditorGUILayout.ToggleLeft(new GUIContent("Log Access Recycled Player Warning",
                            "Whether to log a warning when trying to access an AudioPlayer that has finished playing and has been recycled into the Object Pool."),
                        AudioSettings.LogAccessRecycledPlayerWarning);

                    AudioSettings.DefaultAudioPlayerPoolSize = EditorGUILayout.IntField("Audio Player Object Pool Size", AudioSettings.DefaultAudioPlayerPoolSize);
                }
            }


            void DrawAudioProjectSettings()
            {
                EditorGUILayout.LabelField("Project Settings".ToWhiteBold(), Uniform.RichLabel);

                using (new EditorGUI.IndentLevelScope())
                {
                    if (HasValidProjectSettingVoiceCount())
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        {
                            EditorGUILayout.IntField("Max Real Voices", currProjectSettingVoiceCount);
                            EditorGUILayout.IntField("Virtual Tracks", AudioConstant.VIRTUAL_TRACK_COUNT);
                        }
                        EditorGUI.EndDisabledGroup();
                    }

                    if (HasValidMixerTracksCount() && currentMixerTracksCount < currProjectSettingVoiceCount + AudioConstant.VIRTUAL_TRACK_COUNT)
                    {
                        string text = string.Format(
                            "The {0} track count should be greater than the sum of Virtual and the Max Real Voices! Click the button below to match them, or set it back to 32 (Unity's default setting) in {1}",
                            AudioConstant.MIXER_NAME.ToWhiteBold(),
                            "Edit/Project Settings".SetColor(Uniform.BabyBlueEyes));
                        EditorGUILayout.LabelField(new GUIContent(text, EditorGUIUtility.IconContent("console.warnicon").image), Uniform.RichLabelHelpBox);
                        if (GUILayout.Button(GUIContent.none, GUIStyle.none))
                        {
                            SettingsService.OpenProjectSettings("Project/Audio");
                        }

                        if (GUILayout.Button("Auto-adding tracks to match audio voices.") && EditorUtility.DisplayDialog("Confirmation",
                                $"This action cannot be undone! Please check the following instructions before you proceed.\n\n1. Backup the {AudioConstant.MIXER_NAME}.asset file if you've modified it for your own needs\n2. The process will duplicate the last generic track of the MainAudioMixer. Make sure it's at its original setting.",
                                "OK",
                                "Cancel"))
                        {
                            AutoMatchAudioVoices();
                        }
                    }
                }
            }
        }

        private static void DrawTabGui()
        {
            GUILayout.Space(2);
            AudioEditorSetting.ShowVuColorOnVolumeSlider = EditorGUILayout.ToggleLeft("Show VU color on volume slider", AudioEditorSetting.ShowVuColorOnVolumeSlider);
            AudioEditorSetting.ShowAudioTypeOnSoundId = EditorGUILayout.ToggleLeft("Show audioType on SoundId", AudioEditorSetting.ShowAudioTypeOnSoundId);

            if (AudioEditorSetting.ShowAudioTypeOnSoundId)
            {
                EditorGUILayout.LabelField("Audio Type Color".ToWhiteBold(), Uniform.RichLabel);
                using (new EditorGUI.IndentLevelScope())
                {
                    AudioExtension.ForeachConcreteAudioType(SetAudioTypeLabelColor);
                }
            }

            EditorGUILayout.LabelField("Displayed Properties".ToWhiteBold(), Uniform.RichLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                AudioExtension.ForeachConcreteAudioType(SetAudioTypeDrawedProperties);
            }

            void SetAudioTypeLabelColor(EAudioType audioType)
            {
                if (AudioEditorSetting.Instance.TryGetAudioTypeSetting(audioType, out var setting))
                {
                    setting.color = EditorGUILayout.ColorField(audioType.ToString(), setting.color, GUILayout.MaxWidth(300));
                    AudioEditorSetting.Instance.WriteAudioTypeSetting(audioType, setting);
                }
            }

            void SetAudioTypeDrawedProperties(EAudioType audioType)
            {
                if (AudioEditorSetting.Instance.TryGetAudioTypeSetting(audioType, out var setting))
                {
                    setting.drawedProperty = (EDrawedProperty) EditorGUILayout.EnumFlagsField(audioType.ToString(), setting.drawedProperty, GUILayout.MaxWidth(300));
                    AudioEditorSetting.Instance.WriteAudioTypeSetting(audioType, setting);
                }
            }
        }

        private static void DrawTabMiscellaneous(Rect position)
        {
            GUILayout.Space(4);
            position.y += EditorGUIUtility.singleLineHeight * 2;
            EditorGUILayout.LabelField("Asset Output Path".ToBold(), Uniform.CenterRichLabel);
            var halfLineSize = new Vector2(position.width * 0.5f, EditorGUIUtility.singleLineHeight);
            var assetOutputRect = Uniform.GetHorizontalCenterRect(GUILayoutUtility.GetRect(halfLineSize.x, halfLineSize.y), halfLineSize.x, halfLineSize.y);
            EditorAudioEx.DrawAssetOutputPath(assetOutputRect, () => hasOutputAssetPath = true);
            GUILayout.Space(4);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Dominator Track", GUILayout.MaxHeight(30), GUILayout.MaxWidth(200))) AddDominatorTrack();
            GUILayout.Space(20);
            if (GUILayout.Button("Reset To Factory Settings", GUILayout.MaxHeight(30), GUILayout.MaxWidth(200)))
            {
                AudioSettings.ResetToFactorySettings();
                AudioEditorSetting.Instance.ResetToFactorySettings();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        #region draw button

        private static void DrawButtonEffectEditor(ref Wizard.AudioTabType currentTab)
        {
            bool clicked = GUILayout.Toggle(currentTab == Wizard.AudioTabType.EffectEditor,
                "Effect Editor",
                Uniform.GetTabStyle(3, 4),
                GUILayout.ExpandWidth(true),
                GUILayout.Height(22));
            if (clicked && currentTab != Wizard.AudioTabType.EffectEditor) currentTab = Wizard.AudioTabType.EffectEditor;
        }

        private static void DrawButtonSetting(ref Wizard.AudioTabType currentTab)
        {
            bool clicked = GUILayout.Toggle(currentTab == Wizard.AudioTabType.Setting,
                "Settings",
                Uniform.GetTabStyle(1, 4),
                GUILayout.ExpandWidth(true),
                GUILayout.Height(22));
            if (clicked && currentTab != Wizard.AudioTabType.Setting) currentTab = Wizard.AudioTabType.Setting;
        }

        private static void DrawButtonLibrary(ref Wizard.AudioTabType currentTab)
        {
            bool clicked = GUILayout.Toggle(currentTab == Wizard.AudioTabType.Library,
                "Library",
                Uniform.GetTabStyle(0, 4),
                GUILayout.ExpandWidth(true),
                GUILayout.Height(22));
            if (clicked && currentTab != Wizard.AudioTabType.Library) currentTab = Wizard.AudioTabType.Library;
        }

        private static void DrawButtonSettingAudio(ref Wizard.AudioSetingTabType setingTabType)
        {
            bool clicked = GUILayout.Toggle(setingTabType == Wizard.AudioSetingTabType.Audio,
                "Audio",
                Uniform.GetTabStyle(0, 2),
                GUILayout.ExpandWidth(true),
                GUILayout.Height(22));
            if (clicked && setingTabType != Wizard.AudioSetingTabType.Audio) setingTabType = Wizard.AudioSetingTabType.Audio;
        }

        private static void DrawButtonSettingGui(ref Wizard.AudioSetingTabType setingTabType)
        {
            bool clicked = GUILayout.Toggle(setingTabType == Wizard.AudioSetingTabType.GUI,
                "GUI",
                Uniform.GetTabStyle(0, 2),
                GUILayout.ExpandWidth(true),
                GUILayout.Height(22));
            if (clicked && setingTabType != Wizard.AudioSetingTabType.GUI) setingTabType = Wizard.AudioSetingTabType.GUI;
        }

        private static void DrawButtonSettingMiscellanous(ref Wizard.AudioSetingTabType setingTabType)
        {
            bool clicked = GUILayout.Toggle(setingTabType == Wizard.AudioSetingTabType.Miscellaneous,
                "Miscellaneous",
                Uniform.GetTabStyle(0, 2),
                GUILayout.ExpandWidth(true),
                GUILayout.Height(22));
            if (clicked && setingTabType != Wizard.AudioSetingTabType.Miscellaneous) setingTabType = Wizard.AudioSetingTabType.Miscellaneous;
        }

        #endregion

        #region method

        public static void RemoveAssetEditor(string guid)
        {
            if (AssetEditorDict.TryGetValue(guid, out var editor)) UnityEngine.Object.DestroyImmediate(editor);

            AssetEditorDict.Remove(guid);
            LibraryDataContainer.Data.Settings.guids.Remove(guid);
        }

        private static void InitEditorDictionary()
        {
            AssetEditorDict.Clear();
            foreach (string guid in LibraryDataContainer.Data.Settings.guids)
            {
                if (!string.IsNullOrEmpty(guid) && !AssetEditorDict.ContainsKey(guid))
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

                    var editor = UnityEditor.Editor.CreateEditor(asset, typeof(AudioAssetEditor)) as AudioAssetEditor;
                    if (editor != null)
                    {
                        editor.Init(IdGenerator);
                        AssetEditorDict.Add(guid, editor);
                    }
                }
            }
        }

        private static void InitReorderableList()
        {
            assetReorderableList = new ReorderableList(LibraryDataContainer.Data.Settings.guids, typeof(string));

            assetReorderableList.drawHeaderCallback = OnDrawHeader;
            assetReorderableList.onAddCallback = OnAdd;
            assetReorderableList.onRemoveCallback = OnRemove;
            assetReorderableList.drawElementCallback = OnDrawElement;
            assetReorderableList.onSelectCallback = OnSelect;
            assetReorderableList.onReorderCallback = OnReordered;

            void OnDrawHeader(Rect rect) { EditorGUI.LabelField(rect, "Asset List"); }

            void OnAdd(ReorderableList list)
            {
                ShowCreateAssetAskName();
                GUIUtility.ExitGUI();
            }

            void OnRemove(ReorderableList list)
            {
                string path = AssetDatabase.GUIDToAssetPath(LibraryDataContainer.Data.Settings.guids[list.index]);
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.Refresh();
                // AssetPostprocessorEditor will do the rest
            }

            void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                if (AssetEditorDict.TryGetValue(LibraryDataContainer.Data.Settings.guids[index], out var editor))
                {
                    if (editor.Asset == null) return;

                    EditorGUI.LabelField(rect, editor.Asset.AssetName, Uniform.RichLabel);
                }

                if (index == currentSelectAssetIndex && Event.current.isMouse && Event.current.clickCount >= 2)
                {
                    isInEntitiesEditMode = true;
                }
            }

            void OnReordered(ReorderableList list) { hasAssetListReordered = true; }
        }

        private static void ShowCreateAssetAskName()
        {
            // In the following case. List has better performance than IEnumerable , even with a ToList() method.
            var assetNames = AssetEditorDict.Values.Select(x => x.Asset.AssetName).ToList();
            AssetNameWindow.Show(assetNames, assetName => CreateAsset(assetName));
        }

        private static AudioAssetEditor CreateAsset(string entityName)
        {
            if (!TryGetNewPath(entityName, out string path, out string fileName)) return null;

            var newAsset = ScriptableObject.CreateInstance(typeof(AudioAsset));
            AssetDatabase.CreateAsset(newAsset, path);
            EditorAudioEx.AddNewAssetToCoreData(newAsset);
            AssetDatabase.SaveAssets();

            var editor = UnityEditor.Editor.CreateEditor(newAsset, typeof(AudioAssetEditor)) as AudioAssetEditor;
            string guid = AssetDatabase.AssetPathToGUID(path);
            // ReSharper disable once PossibleNullReferenceException
            editor.Init(IdGenerator);
            editor.SetData(guid, fileName);

            AssetEditorDict.Add(guid, editor);
            LibraryDataContainer.Data.Settings.guids.Add(guid);
            LibraryDataContainer.Data.SaveSetting();
            assetReorderableList.index = assetReorderableList.count - 1;
            return editor;
        }

        private static bool TryGetNewPath(string entityName, out string path, out string result)
        {
            path = string.Empty;
            result = entityName;

            var index = 0;
            path = GetNewAssetPath(entityName);
            while (File.Exists(path))
            {
                index++;
                result = entityName + index;
                path = GetNewAssetPath(result);
            }

            return true;

            string GetNewAssetPath(string fileName) { return $"{EditorAudioEx.AssetOutputPath}/{fileName}.asset"; }
        }

        private static void OnSelect(ReorderableList list)
        {
            if (list.index != currentSelectAssetIndex)
            {
                EditorAudioEx.onSelectAsset?.Invoke();
                currentSelectAssetIndex = list.index;
                EditorPlayAudioClip.In.StopAllClips();
                foreach (var pair in AssetEditorDict)
                {
                    string guid = pair.Key;
                    var editor = pair.Value;
                    if (guid == LibraryDataContainer.Data.Settings.guids[list.index])
                    {
                        editor.RemoveEntitiesListener();
                        editor.AddEntitiesListener();
                        editor.Verify();
                    }
                    else editor.RemoveEntitiesListener();
                }
            }
        }

        private static void DrawAssetOutputPath(Rect position)
        {
            EditorGUILayout.LabelField("Asset Output Path".ToBold(), Uniform.CenterRichLabel);
            EditorGUILayout.LabelField("The current audio asset output path is missing. Please select a new location.".ToItalic().SetColor(Uniform.Orange),
                Uniform.CenterRichLabel);
            var halfLineSize = new Vector2(position.width * 0.5f, EditorGUIUtility.singleLineHeight);
            var assetOutputRect = Uniform.GetHorizontalCenterRect(GUILayoutUtility.GetRect(halfLineSize.x, halfLineSize.y), halfLineSize.x, halfLineSize.y);
            EditorAudioEx.DrawAssetOutputPath(assetOutputRect, () => hasOutputAssetPath = true);
        }

        private static bool TryGetCurrentAssetEditor(out AudioAssetEditor editor)
        {
            editor = null;
            if (LibraryDataContainer.Data.Settings.guids == null || assetReorderableList == null) return false;

            if (LibraryDataContainer.Data.Settings.guids.Count > 0 && assetReorderableList.index >= 0)
            {
                int index = Mathf.Clamp(assetReorderableList.index, 0, LibraryDataContainer.Data.Settings.guids.Count - 1);
                if (AssetEditorDict.TryGetValue(LibraryDataContainer.Data.Settings.guids[index], out editor)) return true;
            }
            else if (AssetEditorDict.TryGetValue(AudioConstant.TEMP_ASSET_NAME, out editor)) return true;

            return false;
        }

        private static void DrawEntitiesList(Rect position, AudioAssetEditor editor)
        {
            var rect = new Rect(position);
            rect.width -= Wizard.TAB_WIDTH * 4 + 45;
            rect.height -= GUI.skin.box.padding.top * 2;
            float offsetX = 0 + GUI.skin.box.padding.top;
            float offsetY = ReorderableList.Defaults.padding + GUI.skin.box.padding.top + EditorGUIUtility.singleLineHeight + 1.5f;

            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(rect.width), GUILayout.Height(rect.height));
            {
                entitiesScrollPos = EditorGUILayout.BeginScrollView(entitiesScrollPos);
                {
                    DrawEntitiesHeader(editor.Asset, editor.SetAssetName);
                    editor.DrawEntitiesList();
                }
                EditorGUILayout.EndScrollView();
                DrawClipPropertiesHelper.DrawPlaybackIndicator(rect.Scoping(position, new Vector2(offsetX, offsetY)), -entitiesScrollPos);
            }
            EditorGUILayout.EndVertical();
        }

        // The ReorderableList default header background GUIStyle has set fixedHeight to non-0 and stretchHeight to false, which is unreasonable...
        // Use another style or Draw it manually could solve the problem and accept more customization.
        private static void DrawEntitiesHeader(IAudioAsset asset, Action<string> onAssetNameChanged)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_tab_prev@2x"), GUILayout.Width(28), GUILayout.Height(28)))
                {
                    isInEntitiesEditMode = false;
                    assetReorderableList.index = -1;
                }

                GUILayout.Space(10f);

                var headerRect = GUILayoutUtility.GetRect(200, EditorGUIUtility.singleLineHeight * 2);
                if (Event.current.type == EventType.Repaint)
                {
                    GUI.skin.window.Draw(headerRect,
                        false,
                        false,
                        false,
                        false);
                    EditorStyles.textField.Draw(headerRect.PolarCoordinates(-1f),
                        headerRect.Contains(Event.current.mousePosition),
                        false,
                        false,
                        false);
                    EditorGUI.DrawRect(headerRect.PolarCoordinates(-2f), new Color(1f, 1f, 1f, 0.1f));
                }

                DrawAssetNameField(headerRect, asset, onAssetNameChanged);

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawAssetNameField(Rect headerRect, IAudioAsset asset, Action<string> onAssetNameChanged)
        {
            const string namingHint = "Click to name";
            string displayName = string.IsNullOrWhiteSpace(asset.AssetName) || EditorAudioEx.IsTempReservedName(asset.AssetName) ? namingHint : asset.AssetName;
            var wordWrapStyle = new GUIStyle(Uniform.CenterRichLabel) {wordWrap = true, fontSize = 16};

            EditorGUI.BeginChangeCheck();
            string newName = EditorGUI.DelayedTextField(headerRect, displayName, wordWrapStyle);
            if (EditorGUI.EndChangeCheck() && !newName.Equals(asset.AssetName) && !newName.Equals(displayName) && IsValidAssetName(newName))
                onAssetNameChanged?.Invoke(newName);
        }

        private static bool IsValidAssetName(string newName)
        {
            if (Common.Editor.IsInvalidName(newName, out var code))
            {
                switch (code)
                {
                    case EValidationErrorCode.StartWithNumber:
                        Debug.LogWarning("[Audio Asset Name] Name starts with number is not recommended");
                        break;
                    case EValidationErrorCode.ContainsInvalidWord:
                        Debug.LogWarning("[Audio Asset Name] Contains invalid words!");
                        break;
                    case EValidationErrorCode.ContainsWhiteSpace:
                        Debug.LogWarning("[Audio Asset Name] Name with whitespace is not recommended");
                        break;
                    case EValidationErrorCode.IsDuplicate:
                        Debug.LogWarning("[Audio Asset Name] Name already exists!");
                        break;
                }

                return false;
            }

            const string tempName = "Temp";
            bool isTempReservedName = (newName.Length == tempName.Length || newName.Length > tempName.Length && char.IsNumber(newName[tempName.Length])) &&
                                      newName.StartsWith(tempName, StringComparison.Ordinal);
            if (isTempReservedName)
            {
                Debug.LogWarning($"[{newName}] has been reserved for temp asset");
                return false;
            }

            return true;
        }

        private static void DrawEntityFactory(Rect position, Rect factoryRect)
        {
            EditorGUILayout.BeginVertical();
            {
                HandleDragAndDrop(position, factoryRect);

                GUILayout.Space(factoryRect.height * 0.3f);
                EditorGUILayout.LabelField("Drag & Drop".SetSize(30), Uniform.CenterRichLabel, GUILayout.MaxHeight(40));
                GUILayout.Space(15f);
                EditorGUILayout.LabelField("or", Uniform.CenterRichLabel);

                var centerLineRect = GUILayoutUtility.GetLastRect();
                using (new Handles.DrawingScope(Color.grey))
                {
                    float middleX = centerLineRect.xMin + centerLineRect.width * 0.5f;
                    float middleY = centerLineRect.yMin + centerLineRect.height * 0.5f;
                    Handles.DrawAAPolyLine(2f, new Vector3(middleX - 160, middleY), new Vector3(middleX - 30, middleY));
                    Handles.DrawAAPolyLine(2f, new Vector3(middleX + 160, middleY), new Vector3(middleX + 30, middleY));
                }

                GUILayout.Space(15f);
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Browse", GUILayout.Width(80), GUILayout.Height(30)))
                    {
                        // ReSharper disable once AccessToStaticMemberViaDerivedType
                        pickerId = EditorGUIUtility.GetControlID(FocusType.Passive);
                        EditorGUIUtility.ShowObjectPicker<AudioClip>(null, false, string.Empty, pickerId);
                    }

                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();
                HandleObjectPicker();
            }
            EditorGUILayout.EndVertical();
        }

        private static void HandleObjectPicker()
        {
            if (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "ObjectSelectorClosed" &&
                EditorGUIUtility.GetObjectPickerControlID() == pickerId)
            {
                var audioClip = EditorGUIUtility.GetObjectPickerObject() as AudioClip;
                if (audioClip)
                {
                    var tempEditor = CreateAsset(AudioConstant.TEMP_ASSET_NAME);
                    CreateNewEntity(tempEditor, audioClip);
                }

                pickerId = -1;
            }
        }

        private static void HandleDragAndDrop(Rect position, Rect entitiesRect)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            if (Event.current.type == EventType.DragPerform && entitiesRect.Scoping(position).Contains(Event.current.mousePosition))
            {
                var objs = DragAndDrop.objectReferences;

                var clips = new List<AudioClip>();
                foreach (var obj in objs)
                {
                    if (obj is AudioClip audioClip) clips.Add(audioClip);
                }

                if (clips.Count == 0 && objs.Length > 0)
                {
                    Debug.LogWarning(AudioConstant.LOG_HEADER + "The file isn't an Audio Clip");
                    return;
                }

                AudioAssetEditor tempEditor = null;

                if (clips.Count > 1)
                {
                    var option = (EMultiClipsImportOption) EditorUtility.DisplayDialogComplex("Multiple audio clips import confirmation", // Title
                        "You have drop more than one audio clips.\nDo you want to create multiple AudioEntities for each clip, or create one AudioEntity to contain them in its clip list?", //Message
                        EMultiClipsImportOption.MultipleForEach.ToString(), // OK
                        EMultiClipsImportOption.Cancel.ToString(), // Cancel
                        EMultiClipsImportOption.OneForAll.ToString()); // Alt

                    switch (option)
                    {
                        case EMultiClipsImportOption.MultipleForEach:
                            foreach (var c in clips)
                            {
                                tempEditor = CreateAsset(AudioConstant.TEMP_ASSET_NAME);
                                CreateNewEntity(tempEditor, c);
                            }

                            break;
                        case EMultiClipsImportOption.Cancel:
                            // Do Nothing
                            break;
                        case EMultiClipsImportOption.OneForAll:
                            tempEditor = CreateAsset(AudioConstant.TEMP_ASSET_NAME);
                            CreateNewEntity(tempEditor, clips);
                            break;
                    }
                }
                else if (clips.Count == 1)
                {
                    tempEditor = CreateAsset(AudioConstant.TEMP_ASSET_NAME);
                    CreateNewEntity(tempEditor, clips[0]);
                }

                if (tempEditor != null)
                {
                    tempEditor.Verify();
                    tempEditor.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private static void CreateNewEntity(AudioAssetEditor editor, AudioClip clip)
        {
            var entity = editor.CreateNewEntity();
            entity.FindPropertyRelative(AudioEntity.ForEditor.Name).stringValue = clip.name;
            var clipListProp = entity.FindPropertyRelative(AudioEntity.ForEditor.Clips);

            editor.SetClipList(clipListProp, 0, clip);
            isInEntitiesEditMode = true;
        }

        private static void CreateNewEntity(AudioAssetEditor editor, List<AudioClip> clips)
        {
            var entity = editor.CreateNewEntity();
            var clipListProp = entity.FindPropertyRelative(AudioEntity.ForEditor.Clips);

            for (var i = 0; i < clips.Count; i++)
            {
                editor.SetClipList(clipListProp, i, clips[i]);
            }

            isInEntitiesEditMode = true;
        }

        private static void DrawAssetList(Rect assetListRect)
        {
            assetListRect.width -= 100 - 10;
            assetListRect.height -= GUI.skin.box.padding.top * 2 + 111;
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(assetListRect.width), GUILayout.Height(assetListRect.height));
            {
                assetListScrollPos = EditorGUILayout.BeginScrollView(assetListScrollPos);
                {
                    assetReorderableList.DoLayoutList();
                }
                EditorGUILayout.EndScrollView();

                GUILayout.FlexibleSpace();
                if (TryGetCurrentAssetEditor(out var editor))
                {
                    DrawIssueMessage(editor);
                }
                else if (assetReorderableList.count > 0)
                {
                    EditorGUILayout.HelpBox("Double-click on an asset to edit its content.", MessageType.Info);
                }
            }
            EditorGUILayout.EndVertical();

            void DrawIssueMessage(AudioAssetEditor assetEditor)
            {
                string text = assetEditor.CurrMessage;

                if (string.IsNullOrEmpty(text))
                {
                    EditorGUILayout.HelpBox("Double-click on an asset to edit its content.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox(text, MessageType.Error);
                }
            }
        }

        private static bool HasValidProjectSettingVoiceCount()
        {
            if (currProjectSettingVoiceCount < 0)
            {
                currProjectSettingVoiceCount = 0; // if it's still 0 after the following search. then just skip for the rest of the time.
                currProjectSettingVoiceCount = EditorAudioEx.GetProjectSettingRealAudioVoices();
            }

            return currProjectSettingVoiceCount > 0;
        }

        private static bool HasValidMixerTracksCount()
        {
            if (currentMixerTracksCount < 0)
            {
                currentMixerTracksCount = 0; // if it's still 0 after the following search. then just skip for the rest of the time.
                if (AudioMixer)
                {
                    var tracks = AudioMixer.FindMatchingGroups(AudioConstant.GENERIC_TRACK_NAME);
                    currentMixerTracksCount = tracks?.Length ?? 0;
                    duplicateTrackSource = tracks?.Last();
                }
            }

            return currentMixerTracksCount > 0;
        }

        private static void AutoMatchAudioVoices()
        {
            AudioMixerGroup mainTrack = AudioMixer.FindMatchingGroups(AudioConstant.MAIN_TRACK_NAME)
                ?.Where(x => x.name.Length == AudioConstant.MAIN_TRACK_NAME.Length)
                .FirstOrDefault();
            if (mainTrack == default || currentMixerTracksCount == default)
            {
                Debug.LogError(AudioConstant.LOG_HEADER + "Can't get the Main track or other BroAudio track");
                return;
            }

            if (duplicateTrackSource)
            {
                for (int i = currentMixerTracksCount + 1; i <= currProjectSettingVoiceCount + AudioConstant.VIRTUAL_TRACK_COUNT; i++)
                {
                    string trackName = $"{AudioConstant.GENERIC_TRACK_NAME}{i}";
                    AudioReflection.DuplicateAudioTrack(AudioMixer, mainTrack, duplicateTrackSource, trackName);
                }

                // reset it to restart the checking
                ResetTracksAndAudioVoices();
            }
            else
            {
                Debug.LogError(AudioConstant.LOG_HEADER + "No valid track for duplicating");
            }
        }

        private static void ResetTracksAndAudioVoices()
        {
            currProjectSettingVoiceCount = -1;
            currentMixerTracksCount = -1;
        }

        private static void AddDominatorTrack()
        {
            AudioMixerGroup masterTrack = AudioMixer.FindMatchingGroups(AudioConstant.MASTER_TRACK_NAME).FirstOrDefault();
            AudioMixerGroup[] dominatorTracks = AudioMixer.FindMatchingGroups(AudioConstant.DOMINATOR_TRACK_NAME);
            if (masterTrack != null && dominatorTracks != null && dominatorTracks.Length > 0)
            {
                string trackName = $"{AudioConstant.DOMINATOR_TRACK_NAME}{dominatorTracks.Length + 1}";
                AudioReflection.DuplicateAudioTrack(AudioMixer,
                    masterTrack,
                    dominatorTracks[dominatorTracks.Length - 1],
                    trackName,
                    EExposedParameterType.Volume);
            }
        }

        #endregion
    }
}