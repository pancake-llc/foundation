using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using Pancake.Common;
using Pancake.Sound;
using PancakeEditor.Common;
using static PancakeEditor.Sound.EditorAudioEx;
using static Pancake.Sound.SoundClip;
using AudioSettings = Pancake.Sound.AudioSettings;
using Math = System.Math;

namespace PancakeEditor.Sound
{
    [CustomPropertyDrawer(typeof(AudioEntity))]
    public class AudioEntityPropertyDrawer : AuPropertyDrawer
    {
        public class ClipData
        {
            public ITransport transport;
            public bool isSnapToFullVolume;
        }

        public enum Tab
        {
            Clips,
            Overall
        }

        public static event Action OnEntityNameChanged;

        private const float CLIP_PREVIEW_HEIGHT = 100f;
        private const float DEFAULT_FIELD_RATIO = 0.9f;
        private const float PREVIEW_PRETTINESS_OFFSET_Y = 7f; // for prettiness
        private const float FOLDOUT_ARROW_WIDTH = 15f;
        private const float MAX_TEXT_FIELD_WIDTH = 300f;
        private const float PERCENTAGE = 100f;
        private const float RANDOM_TOOL_BAR_WIDTH = 40f;
        private const float MIN_MAX_SLIDER_FIELD_WIDTH = 50f;
        private const int ROUNDED_DIGITS = 3;

        private readonly GUIContent[] _tabLabelGUIContents = {new(nameof(Tab.Clips)), new(nameof(Tab.Overall))};
        private readonly float[] _tabLabelRatios = {0.5f, 0.5f};
        private readonly float[] _identityLabelRatios = {0.65f, 0.15f, 0.2f};
        private readonly GUIContent _volumeLabel = new(nameof(SoundClip.Volume), "The playback volume of this clip");
        private readonly IUniqueIdGenerator _idGenerator = new IdGenerator();
        private Rect[] _tabPreAllocRects;
        private Rect[] _identityLabelRects;

        private readonly Dictionary<string, ReorderableClips> _reorderableClipsDict = new();
        private readonly DrawClipPropertiesHelper _clipPropHelper = new();
        private readonly Dictionary<string, ClipData> _clipDataDict = new();
        private readonly Dictionary<string, Tab> _currSelectedTabDict = new();
        private readonly GUIContent _masterVolLabel = new("Master Volume", "Represent the master volume of all clips");
        private readonly GUIContent _loopingLabel = new("Looping");
        private readonly GUIContent _seamlessLabel = new("Seamless Setting");
        private readonly GUIContent _pitchLabel = new(nameof(AudioEntity.Pitch));
        private readonly GUIContent _spatialLabel = new("Spatial (3D Sound)");
        private readonly float[] _seamlessSettingRectRatio = {0.2f, 0.25f, 0.2f, 0.2f, 0.15f};

        private GenericMenu _changeAudioTypeOption;
        private SerializedProperty _entityThatIsModifyingAudioType;
        private Rect[] _seamlessRects;
        private readonly SerializedProperty[] _loopingToggles = new SerializedProperty[2];

        public override float SingleLineSpace => EditorGUIUtility.singleLineHeight + 3f;
        public float ClipPreviewPadding => ReorderableList.Defaults.padding;
        public float SnapVolumePadding => ReorderableList.Defaults.padding;

        private float TabLabelHeight => SingleLineSpace * 1.5f;

        protected override void OnEnable()
        {
            base.OnEnable();

            onCloseWindow += OnDisable;
            onSelectAsset += OnDisable;

            _changeAudioTypeOption = CreateAudioTypeGenericMenu("Change current AudioType to ...", OnChangeEntityAudioType);
        }

        private void OnDisable()
        {
            foreach (var reorderableClips in _reorderableClipsDict.Values)
            {
                reorderableClips.Dispose();
            }

            _reorderableClipsDict.Clear();
            _clipDataDict.Clear();

            onCloseWindow -= OnDisable;
            onSelectAsset -= OnDisable;

            IsEnable = false;
        }

        private GenericMenu CreateAudioTypeGenericMenu(string str, GenericMenu.MenuFunction2 onClickOption)
        {
            var menu = new GenericMenu();
            var text = new GUIContent(str);
            menu.AddItem(text, false, null);
            menu.AddSeparator(string.Empty);

            AudioExtension.ForeachConcreteAudioType((audioType) =>
            {
                var optionName = new GUIContent(audioType.ToString());
                menu.AddItem(optionName, false, onClickOption, audioType);
            });

            return menu;
        }

        private void OnChangeEntityAudioType(object type)
        {
            if (type is EAudioType audioType && _entityThatIsModifyingAudioType != null)
            {
                var idProp = _entityThatIsModifyingAudioType.FindPropertyRelative(AudioEntity.EditorPropertyName.Id);
                if (AudioExtension.GetAudioType(idProp.intValue) != audioType)
                {
                    idProp.intValue = _idGenerator.GetSimpleUniqueId(audioType);
                    idProp.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        #region Unity Entry Overrider

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
            property.serializedObject.Update();

            var nameProp = property.FindPropertyRelative(nameof(IAudioIdentity.Name).ToCamelCase());
            var idProp = property.FindPropertyRelative(nameof(IAudioIdentity.Id).ToCamelCase());

            var foldoutRect = GetRectAndIterateLine(position);
            _identityLabelRects ??= new Rect[_identityLabelRatios.Length];

            Uniform.SplitRectHorizontal(foldoutRect, 5f, _identityLabelRects, _identityLabelRatios);
            var nameRect = _identityLabelRects[0];
            //var idRect = _identityLabelRects[1];
            var audioTypeRect = _identityLabelRects[2];

            property.isExpanded = EditorGUI.Foldout(nameRect, property.isExpanded, property.isExpanded ? string.Empty : nameProp.stringValue);
            DrawAudioTypeButton(audioTypeRect, property, AudioExtension.GetAudioType(idProp.intValue));
            if (!property.isExpanded || !TryGetAudioTypeSetting(property, out var setting)) return;

            DrawEntityNameField(nameRect, nameProp);

            GetOrAddTabDict(property.propertyPath, out var tab);
            var tabViewRect = GetRectAndIterateLine(position);
            tabViewRect.height = GetTabWindowHeight();
            _tabPreAllocRects ??= new Rect[_tabLabelRatios.Length];
            tab = (Tab) DrawTabsView(tabViewRect,
                (int) tab,
                TabLabelHeight,
                _tabLabelGUIContents,
                _tabLabelRatios,
                _tabPreAllocRects);
            _currSelectedTabDict[property.propertyPath] = tab;
            DrawEmptyLine(1);

            position.x += INDENT_IN_PIXEL;
            position.width -= INDENT_IN_PIXEL * 2f;

            switch (tab)
            {
                case Tab.Clips:
                    var currClipList = DrawReorderableClipsList(position, property, OnClipChanged);
                    var currSelectClip = currClipList.CurrentSelectedClip;
                    if (currSelectClip.TryGetPropertyObject(EditorPropertyName.AudioClip, out AudioClip audioClip))
                    {
                        DrawClipProperties(position,
                            currSelectClip,
                            audioClip,
                            setting,
                            out var transport,
                            out float volume);
                        if (setting.drawedProperty.HasFlagUnsafe(EDrawedProperty.ClipPreview) && audioClip != null && Event.current.type != EventType.Layout)
                        {
                            DrawEmptyLine(1);
                            var previewRect = GetNextLineRect(position);
                            previewRect.y -= PREVIEW_PRETTINESS_OFFSET_Y;
                            previewRect.height = CLIP_PREVIEW_HEIGHT;
                            _clipPropHelper.DrawClipPreview(previewRect,
                                transport,
                                audioClip,
                                volume,
                                currSelectClip.propertyPath);
                            currClipList.SetPreviewRect(previewRect);
                            Offset += CLIP_PREVIEW_HEIGHT + ClipPreviewPadding;
                        }
                    }

                    break;
                case Tab.Overall:
                    DrawAdditionalBaseProperties(position, property, setting);
                    break;
            }

            if (Event.current.type == EventType.MouseDown && position.Contains(Event.current.mousePosition))
            {
                EditorPlayAudioClip.StopAllClips();
            }

            float GetTabWindowHeight()
            {
                float height = TabLabelHeight;
                switch (tab)
                {
                    case Tab.Clips:
                        height += GetClipListHeight(property, setting);
                        break;
                    case Tab.Overall:
                        height += GetAdditionalBaseProtiesLineCount(property, setting) * SingleLineSpace + GetAdditionalBasePropertiesOffest(setting);
                        break;
                }

                height += SingleLineSpace * 0.5f; // compensation for tab label's half line height
                return height;
            }
        }

        private void DrawAudioTypeButton(Rect position, SerializedProperty property, EAudioType audioType)
        {
            if (GUI.Button(position, string.Empty))
            {
                _entityThatIsModifyingAudioType = property;
                _changeAudioTypeOption.DropDown(position);
            }

            string audioTypeName = audioType == EAudioType.None ? "Undefined Type" : audioType.ToString();
            EditorGUI.DrawRect(position.PolarCoordinates(-1f), AudioEditorSetting.Instance.GetAudioTypeColor(audioType));
            EditorGUI.LabelField(position, audioTypeName, Uniform.CenterRichLabel);
        }


        private void GetOrAddTabDict(string propertyPath, out Tab tab)
        {
            if (!_currSelectedTabDict.TryGetValue(propertyPath, out tab)) _currSelectedTabDict[propertyPath] = Tab.Clips;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = SingleLineSpace; // Header

            if (property.isExpanded && TryGetAudioTypeSetting(property, out var setting))
            {
                height += ReorderableList.Defaults.padding; // reorderableList element padding;
                height += TabLabelHeight + SingleLineSpace * 0.5f; // Tab View + compensation
#if !UNITY_2019_3_OR_NEWER
                height += SingleLineSpace;
#endif
                GetOrAddTabDict(property.propertyPath, out var tab);
                switch (tab)
                {
                    case Tab.Clips:
                        height += GetClipListHeight(property, setting);
                        break;
                    case Tab.Overall:
                        height += GetAdditionalBaseProtiesLineCount(property, setting) * SingleLineSpace + GetAdditionalBasePropertiesOffest(setting);
                        break;
                }
            }

            return height;
        }

        private float GetClipListHeight(SerializedProperty property, AudioEditorSetting.AudioTypeSetting setting)
        {
            var height = 0f;
            if (_reorderableClipsDict.TryGetValue(property.propertyPath, out var clipList))
            {
                bool isShowClipProp = clipList.CurrentSelectedClip != null &&
                                      clipList.CurrentSelectedClip.TryGetPropertyObject(EditorPropertyName.AudioClip, out AudioClip _);

                height += clipList.Height;
                height += isShowClipProp ? GetAdditionalClipPropertiesLineCount(property, setting) * SingleLineSpace : 0f;
                height += isShowClipProp ? CLIP_PREVIEW_HEIGHT + ClipPreviewPadding : 0f;
            }

            return height;
        }

        #endregion

        private void DrawEntityNameField(Rect position, SerializedProperty nameProp)
        {
            EditorGUI.BeginChangeCheck();
#if UNITY_2019_3_OR_NEWER
            var nameRect = new Rect(position) {height = EditorGUIUtility.singleLineHeight};
            nameRect.x += FOLDOUT_ARROW_WIDTH;
            nameRect.width = Mathf.Min(nameRect.width - FOLDOUT_ARROW_WIDTH, MAX_TEXT_FIELD_WIDTH);
            nameRect.y += 1f;
#else
			Rect nameRect = new Rect(GetRectAndIterateLine(position));
#endif
            nameProp.stringValue = EditorGUI.TextField(nameRect, nameProp.stringValue);
            if (EditorGUI.EndChangeCheck())
            {
                OnEntityNameChanged?.Invoke();
            }
        }

        private ReorderableClips DrawReorderableClipsList(Rect position, SerializedProperty property, Action<string> onClipChanged)
        {
            bool hasReorderableClips = _reorderableClipsDict.TryGetValue(property.propertyPath, out var reorderableClips);
            if (!hasReorderableClips)
            {
                reorderableClips = new ReorderableClips(property);
                _reorderableClipsDict.Add(property.propertyPath, reorderableClips);
            }

            var rect = GetNextLineRect(position);
            reorderableClips.DrawReorderableList(rect);
            Offset += reorderableClips.Height;
            reorderableClips.onAudioClipChanged = onClipChanged;
            return reorderableClips;
        }

        private void DrawClipProperties(
            Rect position,
            SerializedProperty clipProp,
            AudioClip audioClip,
            AudioEditorSetting.AudioTypeSetting setting,
            out ITransport transport,
            out float volume)
        {
            var volumeProp = clipProp.FindPropertyRelative(EditorPropertyName.Volume);
            var delayProp = clipProp.FindPropertyRelative(EditorPropertyName.Delay);
            var startPosProp = clipProp.FindPropertyRelative(EditorPropertyName.StartPosition);
            var endPosProp = clipProp.FindPropertyRelative(EditorPropertyName.EndPosition);
            var fadeInProp = clipProp.FindPropertyRelative(EditorPropertyName.FadeIn);
            var fadeOutProp = clipProp.FindPropertyRelative(EditorPropertyName.FadeOut);

            if (!_clipDataDict.TryGetValue(clipProp.propertyPath, out var clipData))
            {
                clipData = new ClipData
                {
                    transport = new SerializedTransport(startPosProp,
                        endPosProp,
                        fadeInProp,
                        fadeOutProp,
                        delayProp,
                        audioClip.length)
                };

                _clipDataDict[clipProp.propertyPath] = clipData;
            }

            transport = clipData.transport;

            if (CanDraw(EDrawedProperty.Volume))
            {
                var volRect = GetRectAndIterateLine(position);
                volRect.width *= DEFAULT_FIELD_RATIO;
                volumeProp.floatValue = DrawVolumeSlider(volRect,
                    _volumeLabel,
                    volumeProp.floatValue,
                    clipData.isSnapToFullVolume,
                    () => { clipData.isSnapToFullVolume = !clipData.isSnapToFullVolume; });
            }

            volume = volumeProp.floatValue;

            if (CanDraw(EDrawedProperty.PlaybackPosition))
            {
                var playbackRect = GetRectAndIterateLine(position);
                playbackRect.width *= DEFAULT_FIELD_RATIO;
                _clipPropHelper.DrawPlaybackPositionField(playbackRect, transport);
            }

            if (CanDraw(EDrawedProperty.Fade))
            {
                var fadingRect = GetRectAndIterateLine(position);
                fadingRect.width *= DEFAULT_FIELD_RATIO;
                _clipPropHelper.DrawFadingField(fadingRect, transport);
            }

            bool CanDraw(EDrawedProperty drawedProperty) { return setting.drawedProperty.HasFlagUnsafe(drawedProperty); }
        }

        private bool TryGetAudioTypeSetting(SerializedProperty property, out AudioEditorSetting.AudioTypeSetting setting)
        {
            int id = property.FindPropertyRelative(AudioEntity.EditorPropertyName.Id).intValue;
            return AudioEditorSetting.Instance.TryGetAudioTypeSetting(AudioExtension.GetAudioType(id), out setting);
        }

        private void OnClipChanged(string clipPropPath) { _clipDataDict.Remove(clipPropPath); }

        private int GetAdditionalBaseProtiesLineCount(SerializedProperty property, AudioEditorSetting.AudioTypeSetting setting)
        {
            int filterRange = FlagsExtension.GetFlagsRange(0, 10 - 1, FlagsExtension.FlagsRangeType.Excluded);
            ConvertUnityEverythingFlagsToAll(ref setting.drawedProperty);
            int count = FlagsExtension.GetFlagsOnCount((int) setting.drawedProperty & filterRange);

            var seamlessProp = property.FindPropertyRelative(AudioEntity.EditorPropertyName.SeamlessLoop);
            if (seamlessProp.boolValue) count++;

            return count;
        }

        private float GetAdditionalBasePropertiesOffest(AudioEditorSetting.AudioTypeSetting setting)
        {
            var offset = 0f;
            offset += setting.drawedProperty.HasFlagUnsafe(EDrawedProperty.Priority) ? TWO_SIDES_LABEL_OFFSET_Y : 0f;
            offset += setting.drawedProperty.HasFlagUnsafe(EDrawedProperty.MasterVolume) ? SnapVolumePadding : 0f;
            return offset;
        }

        private int GetAdditionalClipPropertiesLineCount(SerializedProperty property, AudioEditorSetting.AudioTypeSetting setting)
        {
            int filterRange = FlagsExtension.GetFlagsRange(0, 10 - 1, FlagsExtension.FlagsRangeType.Included);
            ConvertUnityEverythingFlagsToAll(ref setting.drawedProperty);
            return FlagsExtension.GetFlagsOnCount((int) setting.drawedProperty & filterRange);
        }

        private void DrawAdditionalBaseProperties(Rect position, SerializedProperty property, AudioEditorSetting.AudioTypeSetting setting)
        {
            DrawMasterVolume();
            DrawPitchProperty();
            DrawPriorityProperty();
            DrawLoopProperty();
            DrawSpatialSetting();

            void DrawMasterVolume()
            {
                if (!setting.drawedProperty.HasFlagUnsafe(EDrawedProperty.MasterVolume)) return;

                Offset += SnapVolumePadding;
                var masterVolRect = GetRectAndIterateLine(position);
                masterVolRect.width *= DEFAULT_FIELD_RATIO;
                var masterVolProp = property.FindPropertyRelative(AudioEntity.EditorPropertyName.MasterVolume);
                var snapVolProp = property.FindPropertyRelative(AudioEntity.EditorPropertyName.SnapToFullVolume);

                var randButtonRect = new Rect(masterVolRect.xMax + 5f, masterVolRect.y, RANDOM_TOOL_BAR_WIDTH, masterVolRect.height);
                if (DrawRandomButton(randButtonRect, ERandomFlags.Volume, property))
                {
                    var volRandProp = property.FindPropertyRelative(AudioEntity.EditorPropertyName.VolumeRandomRange);
                    float vol = masterVolProp.floatValue;
                    float volRange = volRandProp.floatValue;

                    Action<Rect> onDrawVu = null;
#if !UNITY_WEBGL
                    if (AudioEditorSetting.ShowVuColorOnVolumeSlider) onDrawVu = DrawVuMeter;
#endif
                    bool isWebGL = EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL;
                    DrawRandomRangeSlider(masterVolRect,
                        _masterVolLabel,
                        ref vol,
                        ref volRange,
                        AudioConstant.MIN_VOLUME,
                        isWebGL ? AudioConstant.FULL_VOLUME : AudioConstant.MAX_VOLUME,
                        EditorAudioEx.RandomRangeSliderType.Volume,
                        onDrawVu);
                    masterVolProp.floatValue = vol;
                    volRandProp.floatValue = volRange;
                }
                else
                {
                    masterVolProp.floatValue = DrawVolumeSlider(masterVolRect,
                        _masterVolLabel,
                        masterVolProp.floatValue,
                        snapVolProp.boolValue,
                        () => { snapVolProp.boolValue = !snapVolProp.boolValue; });
                }
            }

            void DrawLoopProperty()
            {
                if (setting.drawedProperty.HasFlagUnsafe(EDrawedProperty.Loop))
                {
                    _loopingToggles[0] = property.FindPropertyRelative(AudioEntity.EditorPropertyName.Loop);
                    _loopingToggles[1] = property.FindPropertyRelative(AudioEntity.EditorPropertyName.SeamlessLoop);

                    var loopRect = GetRectAndIterateLine(position);
                    DrawToggleGroup(loopRect, _loopingLabel, _loopingToggles);

                    if (_loopingToggles[1].boolValue) DrawSeamlessSetting(position, property);
                }
            }

            void DrawPitchProperty()
            {
                if (!setting.drawedProperty.HasFlagUnsafe(EDrawedProperty.Pitch)) return;

                var pitchRect = GetRectAndIterateLine(position);
                pitchRect.width *= DEFAULT_FIELD_RATIO;

                bool isWebGL = EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL;
                var pitchSetting = isWebGL ? EPitchShiftingSetting.AudioSource : AudioSettings.PitchSetting;
                float minPitch = pitchSetting == EPitchShiftingSetting.AudioMixer ? AudioConstant.MIN_MIXER_PITCH : AudioConstant.MIN_AUDIO_SOURCE_PITCH;
                float maxPitch = pitchSetting == EPitchShiftingSetting.AudioMixer ? AudioConstant.MAX_MIXER_PITCH : AudioConstant.MAX_AUDIO_SOURCE_PITCH;
                _pitchLabel.tooltip = $"According to the current preference setting, the Pitch will be set on [{pitchSetting}] ";

                var pitchProp = property.FindPropertyRelative(AudioEntity.EditorPropertyName.Pitch);
                var pitchRandProp = property.FindPropertyRelative(AudioEntity.EditorPropertyName.PitchRandomRange);

                var randButtonRect = new Rect(pitchRect.xMax + 5f, pitchRect.y, RANDOM_TOOL_BAR_WIDTH, pitchRect.height);
                bool hasRandom = DrawRandomButton(randButtonRect, ERandomFlags.Pitch, property);

                float pitch = Mathf.Clamp(pitchProp.floatValue, minPitch, maxPitch);
                float pitchRange = pitchRandProp.floatValue;

                switch (pitchSetting)
                {
                    case EPitchShiftingSetting.AudioMixer:
                        pitch = (float) Math.Round(pitch * PERCENTAGE, MidpointRounding.AwayFromZero);
                        pitchRange = (float) Math.Round(pitchRange * PERCENTAGE, MidpointRounding.AwayFromZero);
                        minPitch *= PERCENTAGE;
                        maxPitch *= PERCENTAGE;
                        if (hasRandom)
                        {
                            DrawRandomRangeSlider(pitchRect,
                                _pitchLabel,
                                ref pitch,
                                ref pitchRange,
                                minPitch,
                                maxPitch,
                                EditorAudioEx.RandomRangeSliderType.Default);
                            var minFieldRect = new Rect(pitchRect) {x = pitchRect.x + EditorGUIUtility.labelWidth + 5f, width = MIN_MAX_SLIDER_FIELD_WIDTH};
                            var maxFieldRect = new Rect(minFieldRect) {x = pitchRect.xMax - MIN_MAX_SLIDER_FIELD_WIDTH};
                            DrawPercentageLabel(minFieldRect);
                            DrawPercentageLabel(maxFieldRect);
                        }
                        else
                        {
                            pitch = EditorGUI.Slider(pitchRect,
                                _pitchLabel,
                                pitch,
                                minPitch,
                                maxPitch);
                            DrawPercentageLabel(pitchRect);
                        }

                        pitch /= PERCENTAGE;
                        pitchRange /= PERCENTAGE;
                        break;

                    case EPitchShiftingSetting.AudioSource:
                        if (hasRandom)
                        {
                            DrawRandomRangeSlider(pitchRect,
                                _pitchLabel,
                                ref pitch,
                                ref pitchRange,
                                minPitch,
                                maxPitch,
                                EditorAudioEx.RandomRangeSliderType.Default);
                        }
                        else
                        {
                            pitch = EditorGUI.Slider(pitchRect,
                                _pitchLabel,
                                pitch,
                                minPitch,
                                maxPitch);
                        }

                        break;
                }

                pitchProp.floatValue = pitch;
                pitchRandProp.floatValue = pitchRange;
            }

            void DrawPriorityProperty()
            {
                if (setting.drawedProperty.HasFlagUnsafe(EDrawedProperty.Priority))
                {
                    var priorityRect = GetRectAndIterateLine(position);
                    priorityRect.width *= DEFAULT_FIELD_RATIO;
                    var priorityProp = property.FindPropertyRelative(AudioEntity.EditorPropertyName.Priority);

                    var multiLabels = new EditorAudioEx.MultiLabel() {main = nameof(AudioEntity.Priority), left = "High", right = "Low"};
                    priorityProp.intValue = (int) Draw2SidesLabelSlider(priorityRect,
                        multiLabels,
                        priorityProp.intValue,
                        AudioConstant.HIGHEST_PRIORITY,
                        AudioConstant.LOWEST_PRIORITY);
                    Offset += TWO_SIDES_LABEL_OFFSET_Y;
                }
            }

            void DrawSpatialSetting()
            {
                if (setting.drawedProperty.HasFlagUnsafe(EDrawedProperty.SpatialSettings))
                {
                    var spatialProp = property.FindPropertyRelative(AudioEntity.EditorPropertyName.SpatialSetting);
                    var suffixRect = EditorGUI.PrefixLabel(GetRectAndIterateLine(position), _spatialLabel);
                    Uniform.SplitRectHorizontal(suffixRect,
                        0.5f,
                        5f,
                        out var objFieldRect,
                        out var buttonRect);
                    EditorGUI.ObjectField(objFieldRect, spatialProp, GUIContent.none);
                    bool hasSetting = spatialProp.objectReferenceValue != null;
                    string buttonLabel = hasSetting ? "Open Panel" : "Create And Open";
                    if (GUI.Button(buttonRect, buttonLabel))
                    {
                        if (!hasSetting)
                        {
                            string entityName = property.FindPropertyRelative(nameof(IAudioIdentity.Name).ToCamelCase()).stringValue;
                            string path = EditorUtility.SaveFilePanelInProject("Save Spatial Setting", entityName + "_Spatial", "asset", "Message");
                            if (!string.IsNullOrEmpty(path))
                            {
                                var newSetting = ScriptableObject.CreateInstance<SpatialSetting>();
                                AssetDatabase.CreateAsset(newSetting, path);
                                spatialProp.objectReferenceValue = newSetting;
                                spatialProp.serializedObject.ApplyModifiedProperties();

                                SpatialSettingsEditorWindow.ShowWindow(spatialProp);
                                GUIUtility.ExitGUI();
                                AssetDatabase.SaveAssets();
                            }
                        }
                        else
                        {
                            SpatialSettingsEditorWindow.ShowWindow(spatialProp);
                            GUIUtility.ExitGUI();
                        }
                    }
                }
            }
        }

        private void DrawPercentageLabel(Rect fieldRect)
        {
            var percentageRect = new Rect(fieldRect) {width = 15, x = fieldRect.xMax - 15};
            EditorGUI.LabelField(percentageRect, "%");
        }

        private void DrawSeamlessSetting(Rect totalPosition, SerializedProperty property)
        {
            var suffixRect = EditorGUI.PrefixLabel(GetRectAndIterateLine(totalPosition), _seamlessLabel);
            _seamlessRects ??= new Rect[_seamlessSettingRectRatio.Length];
            Uniform.SplitRectHorizontal(suffixRect, 10f, _seamlessRects, _seamlessSettingRectRatio);

            var drawIndex = 0;
            EditorGUI.LabelField(_seamlessRects[drawIndex], "Transition By");
            drawIndex++;

            var seamlessTypeProp = property.FindPropertyRelative(AudioEntity.EditorPropertyName.SeamlessTransitionType);
            var currentType = (AudioEntity.SeamlessType) seamlessTypeProp.enumValueIndex;
            currentType = (AudioEntity.SeamlessType) EditorGUI.EnumPopup(_seamlessRects[drawIndex], currentType);
            seamlessTypeProp.enumValueIndex = (int) currentType;
            drawIndex++;

            var transitionTimeProp = property.FindPropertyRelative(AudioEntity.EditorPropertyName.TransitionTime);
            switch (currentType)
            {
                case AudioEntity.SeamlessType.Time:
                    transitionTimeProp.floatValue = Mathf.Abs(EditorGUI.FloatField(_seamlessRects[drawIndex], transitionTimeProp.floatValue));
                    break;
                case AudioEntity.SeamlessType.Tempo:
                    var tempoProp = property.FindPropertyRelative(AudioEntity.EditorPropertyName.TransitionTempo);
                    var bpmProp = tempoProp.FindPropertyRelative(nameof(AudioEntity.TempoTransition.bpm));
                    var beatsProp = tempoProp.FindPropertyRelative(nameof(AudioEntity.TempoTransition.beats));

                    Uniform.SplitRectHorizontal(_seamlessRects[drawIndex],
                        0.5f,
                        2f,
                        out var tempoValue,
                        out var tempoLabel);
                    bpmProp.floatValue = Mathf.Abs(EditorGUI.FloatField(tempoValue, bpmProp.floatValue));
                    EditorGUI.LabelField(tempoLabel, "BPM");
                    drawIndex++;

                    Uniform.SplitRectHorizontal(_seamlessRects[drawIndex],
                        0.5f,
                        2f,
                        out var beatsValue,
                        out var beatsLabel);
                    beatsProp.intValue = EditorGUI.IntField(beatsValue, beatsProp.intValue);
                    EditorGUI.LabelField(beatsLabel, "Beats");

                    transitionTimeProp.floatValue = Mathf.Abs(AudioExtension.TempoToTime(bpmProp.floatValue, beatsProp.intValue));
                    break;
                case AudioEntity.SeamlessType.ClipSetting:
                    transitionTimeProp.floatValue = AudioPlayer.USE_ENTITY_SETTING;
                    break;
            }
        }

        private void DrawRandomRangeSlider(
            Rect rect,
            GUIContent label,
            ref float value,
            ref float valueRange,
            float minLimit,
            float maxLimit,
            EditorAudioEx.RandomRangeSliderType sliderType,
            Action<Rect> onGetSliderRect = null)
        {
            float minRand = value - valueRange * 0.5f;
            float maxRand = value + valueRange * 0.5f;
            minRand = (float) Math.Round(Mathf.Clamp(minRand, minLimit, maxLimit), ROUNDED_DIGITS, MidpointRounding.AwayFromZero);
            maxRand = (float) Math.Round(Mathf.Clamp(maxRand, minLimit, maxLimit), ROUNDED_DIGITS, MidpointRounding.AwayFromZero);
            switch (sliderType)
            {
                case EditorAudioEx.RandomRangeSliderType.Default:
                    DrawMinMaxSlider(rect,
                        label,
                        ref minRand,
                        ref maxRand,
                        minLimit,
                        maxLimit,
                        MIN_MAX_SLIDER_FIELD_WIDTH,
                        onGetSliderRect);
                    break;
                case EditorAudioEx.RandomRangeSliderType.Logarithmic:
                    DrawLogarithmicMinMaxSlider(rect,
                        label,
                        ref minRand,
                        ref maxRand,
                        minLimit,
                        maxLimit,
                        MIN_MAX_SLIDER_FIELD_WIDTH,
                        onGetSliderRect);
                    break;
                case EditorAudioEx.RandomRangeSliderType.Volume:
                    DrawRandomRangeVolumeSlider(rect,
                        label,
                        ref minRand,
                        ref maxRand,
                        minLimit,
                        maxLimit,
                        MIN_MAX_SLIDER_FIELD_WIDTH,
                        onGetSliderRect);
                    break;
            }

            valueRange = maxRand - minRand;
            value = minRand + valueRange * 0.5f;
        }

        private bool DrawRandomButton(Rect rect, ERandomFlags targetFlag, SerializedProperty property)
        {
            var randFlagsProp = property.FindPropertyRelative(AudioEntity.EditorPropertyName.RandomFlags);
            var randomFlags = (ERandomFlags) randFlagsProp.intValue;
            bool hasRandom = randomFlags.HasFlagUnsafe(targetFlag);
            hasRandom = GUI.Toggle(rect, hasRandom, "RND", EditorStyles.miniButton);
            randomFlags = hasRandom ? randomFlags | targetFlag : randomFlags & ~targetFlag;
            randFlagsProp.intValue = (int) randomFlags;
            return hasRandom;
        }

        private void ConvertUnityEverythingFlagsToAll(ref EDrawedProperty drawedProperty)
        {
            if ((int) drawedProperty == -1) drawedProperty = EDrawedProperty.All;
        }
    }
}