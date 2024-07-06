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
        private class ClipData
        {
            public ITransport transport;
            public bool isSnapToFullVolume;
        }

        private class EntityData
        {
            public Tab selectedTab;
            public bool isLoop;
            public bool isPreviewing;
            public bool IsPlaying => isPreviewing || (Clips != null && Clips.IsPlaying);

            public ReorderableClips Clips { get; private set; }

            public EntityData(ReorderableClips clips) { Clips = clips; }

            public void Dispose()
            {
                Clips?.Dispose();
                Clips = null;
            }
        }

        public enum Tab
        {
            Clips,
            Overall
        }

        public static event Action OnEntityNameChanged;
        public static event Action OnRemoveEntity;

        private const float CLIP_PREVIEW_HEIGHT = 100f;
        private const float DEFAULT_FIELD_RATIO = 0.9f;
        private const float PREVIEW_PRETTINESS_OFFSET_Y = 7f; // for prettiness
        private const float FOLDOUT_ARROW_WIDTH = 15f;
        private const float MAX_TEXT_FIELD_WIDTH = 300f;
        private const float PERCENTAGE = 100f;
        private const float RANDOM_TOOL_BAR_WIDTH = 40f;

        private readonly float[] _headerRatios = new[] {0.55f, 0.2f, 0.25f};
        private readonly GUIContent _volumeLabel = new(nameof(SoundClip.Volume), "The playback volume of this clip");
        private readonly IUniqueIdGenerator _idGenerator = new IdGenerator();

        private readonly TabViewData[] _tabViewDatas =
        {
            new(0.475f, new GUIContent(nameof(Tab.Clips)), EditorPlayAudioClip.In.StopAllClips, null),
            new(0.475f, new GUIContent(nameof(Tab.Overall)), EditorPlayAudioClip.In.StopAllClips, null),
            new(0.05f, EditorGUIUtility.IconContent("pane options"), null, OnClickChangeDrawedProperties),
        };

        private Rect[] _headerRects = null;

        private readonly DrawClipPropertiesHelper _clipPropHelper = new();
        private readonly Dictionary<string, ClipData> _clipDataDict = new();
        private readonly Dictionary<string, EntityData> _entityDataDict = new();
        private readonly GUIContent _masterVolLabel = new("Master Volume", "Represent the master volume of all clips");
        private readonly GUIContent _loopingLabel = new("Looping");
        private readonly GUIContent _seamlessLabel = new("Seamless Setting");
        private readonly GUIContent _pitchLabel = new(nameof(AudioEntity.Pitch));
        private readonly GUIContent _spatialLabel = new("Spatial (3D Sound)");
        private readonly float[] _seamlessSettingRectRatio = {0.25f, 0.25f, 0.2f, 0.15f, 0.15f};

        private GenericMenu _changeAudioTypeOption;
        private SerializedProperty _entityThatIsModifyingAudioType;
        private AudioEntity _currentPreviewingEntity = null;
        private Rect[] _loopingRects;
        private Rect[] _seamlessRects;
        private readonly SerializedProperty[] _loopingToggles = new SerializedProperty[2];

        public override float SingleLineSpace => EditorGUIUtility.singleLineHeight + 3f;
        public float ClipPreviewPadding => ReorderableList.Defaults.padding;
        public float SnapVolumePadding => ReorderableList.Defaults.padding;

        private float TabLabelHeight => SingleLineSpace * 1.3f;
        private float TabLabelCompensation => SingleLineSpace * 2 - TabLabelHeight;

        protected override void OnEnable()
        {
            base.OnEnable();

            onCloseWindow += OnDisable;
            onSelectAsset += OnDisable;
            onLostFocus += OnLostFocus;

            _changeAudioTypeOption = CreateAudioTypeGenericMenu("Change current AudioType to ...", OnChangeEntityAudioType);
        }

        private void OnDisable()
        {
            foreach (var data in _entityDataDict.Values)
            {
                data.Dispose();
            }

            _entityDataDict.Clear();
            _clipDataDict.Clear();

            onCloseWindow -= OnDisable;
            onSelectAsset -= OnDisable;
            onLostFocus -= OnLostFocus;

            ResetPreview();

            OnEntityNameChanged = null;
            OnRemoveEntity = null;
            IsEnable = false;
        }

        private void OnLostFocus() { ResetPreview(); }

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
                var idProp = _entityThatIsModifyingAudioType.FindPropertyRelative(AudioEntity.ForEditor.Id);
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
            _headerRects ??= new Rect[_headerRatios.Length];

            Uniform.SplitRectHorizontal(foldoutRect, 5f, _headerRects, _headerRatios);
            var nameRect = _headerRects[0];
            var previewButtonRect = _headerRects[1];
            var audioTypeRect = _headerRects[2];
            audioTypeRect.x += 25f;

            property.isExpanded = EditorGUI.Foldout(nameRect, property.isExpanded, property.isExpanded ? string.Empty : nameProp.stringValue);
            DrawAudioTypeButton(audioTypeRect, property, AudioExtension.GetAudioType(idProp.intValue));
            if (!property.isExpanded || !TryGetAudioTypeSetting(property, out var setting)) return;

            GetOrCreateEntityDataDict(property, out var data);
            DrawEntityNameField(nameRect, nameProp, idProp.intValue);
            DrawEntityPreviewButton(previewButtonRect, property, data);

            Rect tabViewRect = GetRectAndIterateLine(position).SetHeight(GetTabWindowHeight());
            data.selectedTab = (Tab) DrawButtonTabsMixedView(tabViewRect,
                property,
                (int) data.selectedTab,
                TabLabelHeight,
                _tabViewDatas);

            DrawEmptyLine(1);

            position.x += AudioConstant.INDENT_IN_PIXEL;
            position.width -= AudioConstant.INDENT_IN_PIXEL * 2f;

            switch (data.selectedTab)
            {
                case Tab.Clips:
                    DrawReorderableClipsList(position, data.Clips, OnClipChanged);
                    var currSelectClip = data.Clips.CurrentSelectedClip;
                    if (currSelectClip.TryGetPropertyObject(ForEditor.AudioClip, out AudioClip audioClip))
                    {
                        DrawClipProperties(position,
                            currSelectClip,
                            audioClip,
                            setting,
                            out var transport,
                            out float volume);
                        if (setting.CanDraw(EDrawedProperty.ClipPreview) && audioClip != null && Event.current.type != EventType.Layout)
                        {
                            DrawEmptyLine(1);
                            var previewRect = GetNextLineRect(position);
                            previewRect.y -= PREVIEW_PRETTINESS_OFFSET_Y;
                            previewRect.height = CLIP_PREVIEW_HEIGHT;
                            _clipPropHelper.DrawClipPreview(previewRect,
                                transport,
                                audioClip,
                                volume,
                                currSelectClip.propertyPath,
                                data.Clips.SetPlayingClip);
                            data.Clips.SetPreviewRect(previewRect);
                            Offset += CLIP_PREVIEW_HEIGHT + ClipPreviewPadding;
                        }
                    }

                    break;
                case Tab.Overall:
                    DrawAdditionalBaseProperties(position, property, setting);
                    break;
            }

            float GetTabWindowHeight()
            {
                float height = TabLabelHeight;
                switch (data.selectedTab)
                {
                    case Tab.Clips:
                        height += GetClipListHeight(property, setting);
                        break;
                    case Tab.Overall:
                        height += GetAdditionalBasePropertiesHeight(property, setting) * SingleLineSpace + GetAdditionalBasePropertiesOffest(setting);
                        break;
                }

                height += TabLabelCompensation;
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

        private void GetOrCreateEntityDataDict(SerializedProperty property, out EntityData data)
        {
            if (!_entityDataDict.TryGetValue(property.propertyPath, out data))
            {
                var reorderableClips = new ReorderableClips(property);
                data = new EntityData(reorderableClips);
                _entityDataDict[property.propertyPath] = data;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = SingleLineSpace; // Header

            if (property.isExpanded && TryGetAudioTypeSetting(property, out var setting))
            {
                height += ReorderableList.Defaults.padding; // reorderableList element padding;
                height += TabLabelHeight + TabLabelCompensation;
#if !UNITY_2019_3_OR_NEWER
                height += SingleLineSpace;
#endif
                GetOrCreateEntityDataDict(property, out var data);
                switch (data.selectedTab)
                {
                    case Tab.Clips:
                        height += GetClipListHeight(property, setting);
                        break;
                    case Tab.Overall:
                        height += GetAdditionalBasePropertiesHeight(property, setting);
                        break;
                }
            }

            return height;
        }

        private float GetClipListHeight(SerializedProperty property, AudioEditorSetting.AudioTypeSetting setting)
        {
            var height = 0f;
            if (_entityDataDict.TryGetValue(property.propertyPath, out var data))
            {
                bool isShowClipProp = data.Clips.HasValidClipSelected;

                height += data.Clips.Height;
                height += isShowClipProp ? GetAdditionalClipPropertiesHeight(property, setting) : 0f;
                height += isShowClipProp && setting.CanDraw(EDrawedProperty.ClipPreview) ? CLIP_PREVIEW_HEIGHT + ClipPreviewPadding : 0f;
            }

            return height;
        }

        #endregion

        private void DrawEntityNameField(Rect position, SerializedProperty nameProp, int id)
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

        private void DrawEntityPreviewButton(Rect rect, SerializedProperty property, EntityData data)
        {
            if (!data.Clips.HasValidClipSelected) return;

            rect = rect.SetHeight(h => h * 1.1f);
            Uniform.SplitRectHorizontal(rect,
                0.5f,
                5f,
                out var playButtonRect,
                out var loopToggleRect);
            data.isLoop = EditorGUILayout.Toggle(new GUIContent(EditorGUIUtility.IconContent("d_preAudioLoopOff")), data.isLoop);
            if (GUI.Button(playButtonRect, GetPlaybackButtonIcon(data.IsPlaying)) && TryGetEntityInstance(property, out var entity))
            {
                if (data.IsPlaying)
                {
                    EditorPlayAudioClip.In.StopAllClips();
                    AudioExtension.ClearPreviewAudioData();
                    entity.Clips.ResetIsUse();
                }
                else
                {
                    StartPreview(data, entity);
                }
            }

            if (data.IsPlaying && data.selectedTab != Tab.Clips)
            {
                EditorPlayAudioClip.In.PlaybackIndicator.End();
            }
        }

        private void DrawReorderableClipsList(Rect position, ReorderableClips reorderableClips, Action<string> onClipChanged)
        {
            var rect = GetNextLineRect(position);
            reorderableClips.DrawReorderableList(rect);
            Offset += reorderableClips.Height;
            reorderableClips.onAudioClipChanged = onClipChanged;
        }

        private void DrawClipProperties(
            Rect position,
            SerializedProperty clipProp,
            AudioClip audioClip,
            AudioEditorSetting.AudioTypeSetting setting,
            out ITransport transport,
            out float volume)
        {
            
            var volumeProp = clipProp.FindPropertyRelative(ForEditor.Volume);

            if (!_clipDataDict.TryGetValue(clipProp.propertyPath, out var clipData))
            {
                clipData = new ClipData {transport = new SerializedTransport(clipProp, audioClip.length)};
                _clipDataDict[clipProp.propertyPath] = clipData;
            }

            transport = clipData.transport;

            if (setting.CanDraw(EDrawedProperty.Volume))
            {
                var volRect = GetRectAndIterateLine(position).SetWidth(w => w * DEFAULT_FIELD_RATIO);
                volumeProp.floatValue = DrawVolumeSlider(volRect,
                    _volumeLabel,
                    volumeProp.floatValue,
                    clipData.isSnapToFullVolume,
                    () => { clipData.isSnapToFullVolume = !clipData.isSnapToFullVolume; });
            }

            volume = volumeProp.floatValue;

            if (setting.CanDraw(EDrawedProperty.PlaybackPosition))
            {
                var playbackRect = GetRectAndIterateLine(position).SetWidth(w => w * DEFAULT_FIELD_RATIO);
                _clipPropHelper.DrawPlaybackPositionField(playbackRect, transport);
            }

            if (setting.CanDraw(EDrawedProperty.Fade))
            {
                var fadingRect = GetRectAndIterateLine(position).SetWidth(w => w * DEFAULT_FIELD_RATIO);
                _clipPropHelper.DrawFadingField(fadingRect, transport);
            }
        }

        private bool TryGetAudioTypeSetting(SerializedProperty property, out AudioEditorSetting.AudioTypeSetting setting)
        {
            int id = property.FindPropertyRelative(AudioEntity.ForEditor.Id).intValue;
            return AudioEditorSetting.Instance.TryGetAudioTypeSetting(AudioExtension.GetAudioType(id), out setting);
        }

        private void OnClipChanged(string clipPropPath) { _clipDataDict.Remove(clipPropPath); }

        private int GetAdditionalBasePropertiesHeight(SerializedProperty property, AudioEditorSetting.AudioTypeSetting setting)
        {
            var drawFlags = setting.drawedProperty;
            ConvertUnityEverythingFlagsToAll(ref drawFlags);
            int intBits = 32;
            int lineCount = GetDrawingLineCount(property,
                drawFlags,
                10,
                intBits,
                IsDefaultValue);
            var seamlessProp = property.FindPropertyRelative(AudioEntity.ForEditor.SeamlessLoop);

            int filterRange = FlagsExtension.GetFlagsRange(0, 10 - 1, FlagsExtension.FlagsRangeType.Excluded);
            int count = FlagsExtension.GetFlagsOnCount((int) setting.drawedProperty & filterRange);

            if (seamlessProp.boolValue) count++;
            float offset = 0f;
            offset += IsDefaultValueAndCanNotDraw(property, drawFlags, EDrawedProperty.Priority) ? 0f : 7;
            offset += IsDefaultValueAndCanNotDraw(property, drawFlags, EDrawedProperty.MasterVolume) ? 0f : SnapVolumePadding;

            return (int) (lineCount * SingleLineSpace + offset);
        }

        private float GetAdditionalClipPropertiesHeight(SerializedProperty property, AudioEditorSetting.AudioTypeSetting setting)
        {
            var drawFlags = setting.drawedProperty;
            ConvertUnityEverythingFlagsToAll(ref drawFlags);
            int lineCount = GetDrawingLineCount(property,
                drawFlags,
                0,
                10 - 1,
                IsDefaultValue);
            return lineCount * SingleLineSpace;
        }

        private int GetDrawingLineCount(
            SerializedProperty property,
            EDrawedProperty flags,
            int startIndex,
            int lastIndex,
            Func<SerializedProperty, EDrawedProperty, bool> onGetIsDefaultValue)
        {
            int count = 0;
            for (int i = startIndex; i <= lastIndex; i++)
            {
                int drawFlag = (1 << i);
                if (drawFlag > (int) EDrawedProperty.All) break;

                if (!EDrawedProperty.All.HasFlagUnsafe((EDrawedProperty) drawFlag)) continue;

                bool canDraw = ((int) flags & drawFlag) != 0;
                if (canDraw || !onGetIsDefaultValue.Invoke(property, (EDrawedProperty) drawFlag)) count++;
            }

            return count;
        }

        private float GetAdditionalBasePropertiesOffest(AudioEditorSetting.AudioTypeSetting setting)
        {
            var offset = 0f;
            offset += setting.drawedProperty.HasFlagUnsafe(EDrawedProperty.Priority) ? AudioConstant.TWO_SIDES_LABEL_OFFSET_Y : 0f;
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
            var drawFlags = setting.drawedProperty;
            ConvertUnityEverythingFlagsToAll(ref drawFlags);
            DrawMasterVolume();
            DrawPitchProperty();
            DrawPriorityProperty();
            DrawLoopProperty();
            DrawSpatialSetting();

            void DrawMasterVolume()
            {
                if (IsDefaultValueAndCanNotDraw(property,
                        drawFlags,
                        EDrawedProperty.MasterVolume,
                        out var masterVolProp,
                        out var volRandProp)) return;

                Offset += SnapVolumePadding;
                var masterVolRect = GetRectAndIterateLine(position);
                masterVolRect.width *= DEFAULT_FIELD_RATIO;
                var snapVolProp = property.FindPropertyRelative(AudioEntity.ForEditor.SnapToFullVolume);
                var randButtonRect = new Rect(masterVolRect.xMax + 5f, masterVolRect.y, RANDOM_TOOL_BAR_WIDTH, masterVolRect.height);
                if (DrawRandomButton(randButtonRect, ERandomFlag.Volume, property))
                {
                    float vol = masterVolProp.floatValue;
                    float volRange = volRandProp.floatValue;

                    Action<Rect> onDrawVu = null;
#if !UNITY_WEBGL
                    if (AudioEditorSetting.ShowVuColorOnVolumeSlider) onDrawVu = DrawVuMeter;
#endif
                    GetMixerMinMaxVolume(out float minVol, out float maxVol);
                    DrawRandomRangeSlider(masterVolRect,
                        _masterVolLabel,
                        ref vol,
                        ref volRange,
                        minVol,
                        maxVol,
                        RandomRangeSliderType.Volume,
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
                if (IsDefaultValueAndCanNotDraw(property,
                        drawFlags,
                        EDrawedProperty.Loop,
                        out var loopProp,
                        out var seamlessProp)) return;

                _loopingToggles[0] = loopProp;
                _loopingToggles[1] = seamlessProp;

                var loopRect = GetRectAndIterateLine(position);
                _loopingRects = _loopingRects ?? new Rect[_loopingToggles.Length];
                _loopingRects[0] = new Rect(loopRect) {width = 100f, x = loopRect.x + EditorGUIUtility.labelWidth};
                _loopingRects[1] = new Rect(loopRect) {width = 200f, x = _loopingRects[0].xMax};
                DrawToggleGroup(loopRect, _loopingLabel, _loopingToggles, _loopingRects);

                if (seamlessProp.boolValue) DrawSeamlessSetting(position, property);
            }

            void DrawPitchProperty()
            {
                if (IsDefaultValueAndCanNotDraw(property,
                        drawFlags,
                        EDrawedProperty.Pitch,
                        out var pitchProp,
                        out var pitchRandProp)) return;

                var pitchRect = GetRectAndIterateLine(position);
                pitchRect.width *= DEFAULT_FIELD_RATIO;

                bool isWebGL = EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL;
                var pitchSetting = isWebGL ? EPitchShiftingSetting.AudioSource : AudioSettings.PitchSetting;
                float minPitch = AudioConstant.MIN_PLAYABLE_PITCH;
                float maxPitch = pitchSetting == EPitchShiftingSetting.AudioMixer ? AudioConstant.MAX_MIXER_PITCH : AudioConstant.MAX_AUDIO_SOURCE_PITCH;
                _pitchLabel.tooltip = $"According to the current preference setting, the Pitch will be set on [{pitchSetting}] ";

                var randButtonRect = new Rect(pitchRect.xMax + 5f, pitchRect.y, RANDOM_TOOL_BAR_WIDTH, pitchRect.height);
                bool hasRandom = DrawRandomButton(randButtonRect, ERandomFlag.Pitch, property);

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
                                RandomRangeSliderType.Default);
                            var minFieldRect = new Rect(pitchRect) {x = pitchRect.x + EditorGUIUtility.labelWidth + 5f, width = AudioConstant.MIN_MAX_SLIDER_FIELD_WIDTH};
                            var maxFieldRect = new Rect(minFieldRect) {x = pitchRect.xMax - AudioConstant.MIN_MAX_SLIDER_FIELD_WIDTH};
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
                                RandomRangeSliderType.Default);
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
                if (IsDefaultValueAndCanNotDraw(property,
                        drawFlags,
                        EDrawedProperty.Priority,
                        out var priorityProp,
                        out _)) return;


                var priorityRect = GetRectAndIterateLine(position);
                priorityRect.width *= DEFAULT_FIELD_RATIO;

                var multiLabels = new MultiLabel() {main = nameof(AudioEntity.Priority), left = "High", right = "Low"};
                priorityProp.intValue = (int) Draw2SidesLabelSlider(priorityRect,
                    multiLabels,
                    priorityProp.intValue,
                    AudioConstant.HIGHEST_PRIORITY,
                    AudioConstant.LOWEST_PRIORITY);
                Offset += AudioConstant.TWO_SIDES_LABEL_OFFSET_Y;
            }

            void DrawSpatialSetting()
            {
                if (IsDefaultValueAndCanNotDraw(property,
                        drawFlags,
                        EDrawedProperty.SpatialSettings,
                        out var spatialProp,
                        out _)) return;

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

        private bool IsDefaultValue(SerializedProperty property, EDrawedProperty drawedProperty) { return IsDefaultValue(property, drawedProperty, out _, out _); }

        private bool IsDefaultValue(SerializedProperty property, EDrawedProperty drawedProperty, out SerializedProperty mainProp, out SerializedProperty secondaryProp)
        {
            mainProp = null;
            secondaryProp = null;
            switch (drawedProperty)
            {
                case EDrawedProperty.MasterVolume:
                    mainProp = property.FindPropertyRelative(AudioEntity.ForEditor.MasterVolume);
                    secondaryProp = property.FindPropertyRelative(AudioEntity.ForEditor.VolumeRandomRange);
                    return Mathf.Approximately(mainProp.floatValue, AudioConstant.FULL_VOLUME) && secondaryProp.floatValue == 0f;
                case EDrawedProperty.Loop:
                    mainProp = property.FindPropertyRelative(AudioEntity.ForEditor.Loop);
                    secondaryProp = property.FindPropertyRelative(AudioEntity.ForEditor.SeamlessLoop);
                    return !mainProp.boolValue && !secondaryProp.boolValue;
                case EDrawedProperty.Priority:
                    mainProp = property.FindPropertyRelative(AudioEntity.ForEditor.Priority);
                    return mainProp.intValue == AudioConstant.DEFAULT_PRIORITY;
                case EDrawedProperty.SpatialSettings:
                    mainProp = property.FindPropertyRelative(AudioEntity.ForEditor.SpatialSetting);
                    return mainProp.objectReferenceValue == null;
                case EDrawedProperty.Pitch:
                    mainProp = property.FindPropertyRelative(AudioEntity.ForEditor.Pitch);
                    secondaryProp = property.FindPropertyRelative(AudioEntity.ForEditor.PitchRandomRange);
                    return Mathf.Approximately(mainProp.floatValue, AudioConstant.DEFAULT_PITCH) && secondaryProp.floatValue == 0f;
            }

            return true;
        }

        private bool IsDefaultValueAndCanNotDraw(SerializedProperty checkedProp, EDrawedProperty drawFlags, EDrawedProperty drawTarget)
        {
            return IsDefaultValueAndCanNotDraw(checkedProp,
                drawFlags,
                drawTarget,
                out _,
                out _);
        }

        private bool IsDefaultValueAndCanNotDraw(
            SerializedProperty checkedProp,
            EDrawedProperty drawFlags,
            EDrawedProperty drawTarget,
            out SerializedProperty mainProp,
            out SerializedProperty secondaryProp)
        {
            return IsDefaultValue(checkedProp, drawTarget, out mainProp, out secondaryProp) && !drawFlags.HasFlagUnsafe(drawTarget);
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

            var seamlessTypeProp = property.FindPropertyRelative(AudioEntity.ForEditor.SeamlessTransitionType);
            var currentType = (AudioEntity.SeamlessType) seamlessTypeProp.enumValueIndex;
            currentType = (AudioEntity.SeamlessType) EditorGUI.EnumPopup(_seamlessRects[drawIndex], currentType);
            seamlessTypeProp.enumValueIndex = (int) currentType;
            drawIndex++;

            var transitionTimeProp = property.FindPropertyRelative(AudioEntity.ForEditor.TransitionTime);
            switch (currentType)
            {
                case AudioEntity.SeamlessType.Time:
                    transitionTimeProp.floatValue = Mathf.Abs(EditorGUI.FloatField(_seamlessRects[drawIndex], transitionTimeProp.floatValue));
                    break;
                case AudioEntity.SeamlessType.Tempo:
                    var tempoProp = property.FindPropertyRelative(AudioEntity.ForEditor.TransitionTempo);
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

        private bool DrawRandomButton(Rect rect, ERandomFlag targetFlag, SerializedProperty property)
        {
            var randFlagsProp = property.FindPropertyRelative(AudioEntity.ForEditor.RandomFlags);
            var randomFlags = (ERandomFlag) randFlagsProp.intValue;
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

        private void StartPreview(EntityData data, AudioEntity entity)
        {
            if (entity == null) return;

            _currentPreviewingEntity = entity;
            var clip = entity.PickNewClip(out int index);
            data.Clips.SelectAndSetPlayingElement(index);

            float volume = clip.Volume * entity.GetMasterVolume();
            float pitch = entity.GetPitch();
            Action onReplay = null;
            if (data.isLoop)
            {
                onReplay = ReplayPreview;
            }

            var clipData = new EditorPlayAudioClip.Data(clip) {Volume = volume};
            EditorPlayAudioClip.In.PlayClipByAudioSource(clipData, data.isLoop, onReplay, pitch);
            EditorPlayAudioClip.In.PlaybackIndicator.SetClipInfo(data.Clips.PreviewRect, new PreviewClip(clip), entity.GetPitch());
            data.isPreviewing = true;
            EditorPlayAudioClip.In.OnFinished = OnPreviewFinished;

            void ReplayPreview() { StartPreview(data, entity); }

            void OnPreviewFinished()
            {
                data.isPreviewing = false;
                data.Clips.SetPlayingClip(null);
            }
        }

        private void ResetPreview()
        {
            AudioExtension.ClearPreviewAudioData();
            _currentPreviewingEntity?.Clips?.ResetIsUse();
        }

        private bool TryGetEntityInstance(SerializedProperty property, out AudioEntity entity)
        {
            entity = null;
            object obj = fieldInfo.GetValue(property.serializedObject.targetObject);
            if (obj is AudioEntity[] entities)
            {
                const string baseName = "entities" + ".Array.data[";
                string num = property.propertyPath.Remove(property.propertyPath.Length - 1).Remove(0, baseName.Length);
                if (int.TryParse(num, out int index) && index < entities.Length) entity = entities[index];
            }

            return entity != null;
        }

        private static void OnClickChangeDrawedProperties(Rect rect, SerializedProperty property)
        {
            var idProp = property.FindPropertyRelative(nameof(AudioEntity.Id).ToLower());
            var nameProp = property.FindPropertyRelative(nameof(AudioEntity.Name).ToLower());

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent($"Remove [{nameProp.stringValue}]"), false, () => OnRemoveEntity?.Invoke());

            var audioType = AudioExtension.GetAudioType(idProp.intValue);
            if (!AudioEditorSetting.Instance.TryGetAudioTypeSetting(audioType, out var typeSetting)) return;

            menu.AddSeparator(string.Empty);
            menu.AddDisabledItem(new GUIContent($"Displayed properties of AudioType.{audioType}"));
            ForeachConcreteDrawedProperty(OnAddMenuItem);
            menu.DropDown(rect);

            void OnAddMenuItem(EDrawedProperty target) { menu.AddItem(new GUIContent(target.ToString()), typeSetting.CanDraw(target), OnChangeFlags, target); }

            void OnChangeFlags(object userData)
            {
                if (userData is EDrawedProperty target)
                {
                    bool hasFlag = typeSetting.CanDraw(target);
                    if (hasFlag) typeSetting.drawedProperty &= ~target;
                    else typeSetting.drawedProperty |= target;
                    AudioEditorSetting.Instance.WriteAudioTypeSetting(typeSetting.audioType, typeSetting);
                }
            }
        }
    }
}