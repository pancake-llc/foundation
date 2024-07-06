using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using Pancake.Common;
using Pancake.Sound;
using PancakeEditor.Common;

namespace PancakeEditor.Sound
{
    public class ReorderableClips
    {
        public Action<string> onAudioClipChanged;

        public const EAudioPlayMode DEFAULT_MULTICLIPS_MODE = EAudioPlayMode.Random;
        private const float GAP = 5f;
        private const float HEADER_LABEL_WIDTH = 50f;
        private const float MULTICLIPS_VALUE_LABEL_WIDTH = 60f;
        private const float MULTICLIPS_VALUE_FIELD_WIDTH = 40f;
        private const float SLIDER_LABEL_WIDTH = 25;
        private const float OBJECT_PICKER_RATIO = 0.6f;

        private ReorderableList _reorderableList;
        private SerializedProperty _entityProp;
        private SerializedProperty _playModeProp;
        private int _currSelectedClipIndex = -1;
        private SerializedProperty _currSelectedClip;
        private Rect _previewRect;
        private string _currentPlayingClipPath;
        private GUIContent _weightGUIContent = new("Weight", "Probability = Weight / Total Weight");


        private Vector2 PlayButtonSize => new(30f, 20f);
        public bool IsMulticlips => _reorderableList.count > 1;
        public float Height => _reorderableList.GetHeight();
        public Rect PreviewRect => _previewRect;
        public bool IsPlaying => _currentPlayingClipPath != null;
        public bool HasValidClipSelected => CurrentSelectedClip != null && CurrentSelectedClip.TryGetPropertyObject(nameof(SoundClip.AudioClip), out AudioClip _);

        public SerializedProperty CurrentSelectedClip
        {
            get
            {
                if (_reorderableList.count > 0)
                {
                    if (_reorderableList.index < 0) _reorderableList.index = 0;

                    if (_currSelectedClipIndex != _reorderableList.index)
                    {
                        _currSelectedClip = _reorderableList.serializedProperty.GetArrayElementAtIndex(_reorderableList.index);
                        _currSelectedClipIndex = _reorderableList.index;
                    }
                    else if (_currSelectedClip == null)
                    {
                        _currSelectedClip = _reorderableList.serializedProperty.GetArrayElementAtIndex(_reorderableList.index);
                    }
                }
                else _currSelectedClip = null;

                return _currSelectedClip;
            }
        }

        public ReorderableClips(SerializedProperty entityProperty)
        {
            _entityProp = entityProperty;
            _playModeProp = entityProperty.FindPropertyRelative(AudioEntity.ForEditor.AudioPlayMode);
            _reorderableList = CreateReorderabeList(entityProperty);
            UpdatePlayMode();

            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        public void Dispose()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            _currentPlayingClipPath = null;
        }

        public void SetPreviewRect(Rect rect) { _previewRect = rect; }

        public void SelectAndSetPlayingElement(int index)
        {
            if (index >= 0)
            {
                _reorderableList.index = index;
                SetPlayingClip(_reorderableList.serializedProperty.GetArrayElementAtIndex(index).propertyPath);
            }
        }

        public void SetPlayingClip(string clipPath) { _currentPlayingClipPath = clipPath; }

        private void OnUndoRedoPerformed()
        {
            _reorderableList.serializedProperty.serializedObject.Update();
            int count = _reorderableList.count;
            if (count == _reorderableList.index || count == _currSelectedClipIndex)
            {
                _currSelectedClipIndex = count - 1;
                _reorderableList.index = count - 1;
                _currSelectedClip = null;
            }
        }

        public void DrawReorderableList(Rect position) { _reorderableList.DoList(position); }

        private ReorderableList CreateReorderabeList(SerializedProperty entityProperty)
        {
            var clipsProp = entityProperty.FindPropertyRelative(AudioEntity.ForEditor.Clips);
            var list = new ReorderableList(clipsProp.serializedObject, clipsProp)
            {
                drawHeaderCallback = OnDrawHeader,
                drawElementCallback = OnDrawElement,
                drawFooterCallback = OnDrawFooter,
                onAddCallback = OnAdd,
                onRemoveCallback = OnRemove,
                onSelectCallback = OnSelect
            };
            return list;
        }

        private void UpdatePlayMode()
        {
            if (!IsMulticlips) _playModeProp.enumValueIndex = 0;
            else if (IsMulticlips && _playModeProp.enumValueIndex == 0) _playModeProp.enumValueIndex = (int) DEFAULT_MULTICLIPS_MODE;
        }

        private void HandleClipsDragAndDrop(Rect rect)
        {
            var currType = Event.current.type;
            if ((currType == EventType.DragUpdated || currType == EventType.DragPerform) && rect.Contains(Event.current.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (currType == EventType.DragPerform && DragAndDrop.objectReferences?.Length > 0)
                {
                    foreach (var clipObj in DragAndDrop.objectReferences)
                    {
                        var clipProp = AddClip(_reorderableList);
                        var audioClipProp = clipProp.FindPropertyRelative(SoundClip.ForEditor.AudioClip);
                        audioClipProp.objectReferenceValue = clipObj;
                    }

                    UpdatePlayMode();
                    _reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
                    DragAndDrop.AcceptDrag();
                }
            }
        }

        #region ReorderableList Callback

        private void OnDrawHeader(Rect rect)
        {
            HandleClipsDragAndDrop(rect);

            var labelRect = new Rect(rect) {width = HEADER_LABEL_WIDTH};
            var valueRect = new Rect(rect) {width = MULTICLIPS_VALUE_LABEL_WIDTH, x = rect.xMax - MULTICLIPS_VALUE_LABEL_WIDTH};
            Rect remainRect = new Rect(rect) {width = (rect.width - HEADER_LABEL_WIDTH - MULTICLIPS_VALUE_LABEL_WIDTH), x = labelRect.xMax};
            Uniform.SplitRectHorizontal(remainRect,
                0.5f,
                10f,
                out var multiclipOptionRect,
                out var masterVolRect);

            EditorGUI.LabelField(labelRect, "Clips");
            if (IsMulticlips)
            {
                var playMode = (EAudioPlayMode) _playModeProp.enumValueIndex;
                playMode = (EAudioPlayMode) EditorGUI.EnumPopup(multiclipOptionRect, playMode);
                _playModeProp.enumValueIndex = (int) playMode;

                DrawMasterVolume(masterVolRect);

                GUIContent guiContent = new GUIContent(string.Empty);
                switch (playMode)
                {
                    case EAudioPlayMode.Single:
                        guiContent.tooltip = "Always play the first clip";
                        break;
                    case EAudioPlayMode.Sequence:
                        EditorGUI.LabelField(valueRect, "Index", Uniform.CenterRichLabel);
                        guiContent.tooltip = "Plays the next clip each time";
                        break;
                    case EAudioPlayMode.Random:
                        EditorGUI.LabelField(valueRect, _weightGUIContent, Uniform.CenterRichLabel);
                        guiContent.tooltip = "Plays a clip randomly";
                        break;
                    case EAudioPlayMode.Shuffle:
                        guiContent.tooltip = "Plays a clip randomly without repeating the previous one.";
                        break;
                }

                EditorGUI.LabelField(multiclipOptionRect.DissolveHorizontal(0.5f), "(PlayMode)".SetColor(Color.gray), Uniform.CenterRichLabel);
                EditorGUI.LabelField(multiclipOptionRect, guiContent);
            }
        }

        private void DrawMasterVolume(Rect masterVolRect)
        {
            int id = _entityProp.FindPropertyRelative(AudioEntity.ForEditor.Id).intValue;
            if (!AudioEditorSetting.ShowMasterVolumeOnClipListHeader ||
                !AudioEditorSetting.Instance.TryGetAudioTypeSetting(AudioExtension.GetAudioType(id), out var typeSetting) ||
                !typeSetting.CanDraw(EDrawedProperty.MasterVolume))
            {
                return;
            }

            var masterProp = _entityProp.FindPropertyRelative(AudioEntity.ForEditor.MasterVolume);
            var masterRandProp = _entityProp.FindPropertyRelative(AudioEntity.ForEditor.VolumeRandomRange);
            float masterVol = masterProp.floatValue;
            float masterVolRand = masterRandProp.floatValue;
            var flags = (ERandomFlag) _entityProp.FindPropertyRelative(AudioEntity.ForEditor.RandomFlags).intValue;
            EditorAudioEx.GetMixerMinMaxVolume(out float minVol, out float maxVol);
            var masterVolLabelRect = new Rect(masterVolRect) {width = SLIDER_LABEL_WIDTH};
            var masterVolSldierRect = new Rect(masterVolRect) {width = masterVolRect.width - SLIDER_LABEL_WIDTH, x = masterVolLabelRect.xMax};

            EditorGUI.LabelField(masterVolLabelRect, EditorGUIUtility.IconContent("SceneViewAudio On"));
            if (flags.HasFlagUnsafe(ERandomFlag.Volume))
            {
                EditorAudioEx.DrawRandomRangeSlider(masterVolSldierRect,
                    GUIContent.none,
                    ref masterVol,
                    ref masterVolRand,
                    minVol,
                    maxVol,
                    EditorAudioEx.RandomRangeSliderType.Volume);
            }
            else
            {
                masterVol = EditorAudioEx.DrawVolumeSlider(masterVolSldierRect, masterVol, out _, out float newSliderInFullScale);
                EditorAudioEx.DrawDecibelValuePeeking(masterVol, 3f, masterVolRect, newSliderInFullScale);
            }

            masterProp.floatValue = masterVol;
            masterRandProp.floatValue = masterVolRand;
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var clipProp = _reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            var audioClipProp = clipProp.FindPropertyRelative(SoundClip.ForEditor.AudioClip);
            var volProp = clipProp.FindPropertyRelative(SoundClip.ForEditor.Volume);

            var buttonRect = new Rect(rect) {width = PlayButtonSize.x, height = PlayButtonSize.y};
            buttonRect.y += (_reorderableList.elementHeight - PlayButtonSize.y) * 0.5f;
            var valueRect = new Rect(rect) {width = MULTICLIPS_VALUE_LABEL_WIDTH, x = rect.xMax - MULTICLIPS_VALUE_LABEL_WIDTH};

            float remainWidth = rect.width - buttonRect.width - valueRect.width;
            var clipRect = new Rect(rect) {width = remainWidth * OBJECT_PICKER_RATIO - GAP, x = buttonRect.xMax + GAP};
            var sliderRect = new Rect(rect) {width = remainWidth * (1 - OBJECT_PICKER_RATIO) - GAP, x = clipRect.xMax + GAP};

            DrawPlayClipButton();
            DrawObjectPicker();
            DrawVolumeSlider();
            DrawMulticlipsValue();

            void DrawObjectPicker()
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(clipRect, audioClipProp, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorAudioEx.ResetAudioClipPlaybackSetting(clipProp);
                    onAudioClipChanged?.Invoke(clipProp.propertyPath);
                }
            }

            void DrawPlayClipButton()
            {
                AudioClip audioClip = audioClipProp.objectReferenceValue as AudioClip;
                if (audioClip == null) return;

                bool isPlaying = string.Equals(_currentPlayingClipPath, clipProp.propertyPath);
                var image = EditorAudioEx.GetPlaybackButtonIcon(isPlaying).image;
                var buttonGUIContent = new GUIContent(image, EditorPlayAudioClip.IGNORE_SETTING_TOOLTIP);
                if (GUI.Button(buttonRect, buttonGUIContent))
                {
                    if (isPlaying) EditorPlayAudioClip.In.StopAllClips();
                    else PreviewAudio(audioClip);
                }
            }

            void PreviewAudio(AudioClip audioClip)
            {
                PreviewClip previewClipGUI;
                if (Event.current.button == 0) // Left Click
                {
                    var transport = new SerializedTransport(clipProp, audioClip.length);
                    var clipData = new EditorPlayAudioClip.Data(audioClip, volProp.floatValue, transport);
                    EditorPlayAudioClip.In.PlayClipByAudioSource(clipData);
                    previewClipGUI = new PreviewClip(transport);
                }
                else
                {
                    EditorPlayAudioClip.In.PlayClip(audioClip, 0f, 0f);
                    previewClipGUI = new PreviewClip(audioClip.length);
                }

                _currentPlayingClipPath = clipProp.propertyPath;
                EditorPlayAudioClip.In.OnFinished = () => _currentPlayingClipPath = null;

                if (EditorPlayAudioClip.In.PlaybackIndicator.IsPlaying)
                {
                    EditorPlayAudioClip.In.PlaybackIndicator.SetClipInfo(_previewRect, previewClipGUI);
                }
            }

            void DrawVolumeSlider()
            {
                var labelRect = new Rect(sliderRect) {width = SLIDER_LABEL_WIDTH};
                sliderRect.width -= SLIDER_LABEL_WIDTH;
                sliderRect.x = labelRect.xMax;
                EditorGUI.LabelField(labelRect, EditorGUIUtility.IconContent("SceneViewAudio On"));
                float newVol = EditorAudioEx.DrawVolumeSlider(sliderRect, volProp.floatValue, out bool hasChanged, out float newSliderValue);
                if (hasChanged) volProp.floatValue = newVol;
                EditorAudioEx.DrawDecibelValuePeeking(volProp.floatValue, 3f, sliderRect, newSliderValue);
            }

            void DrawMulticlipsValue()
            {
                valueRect.width = MULTICLIPS_VALUE_FIELD_WIDTH;
                valueRect.x += (MULTICLIPS_VALUE_LABEL_WIDTH - MULTICLIPS_VALUE_FIELD_WIDTH) * 0.5f;
                var currentPlayMode = (EAudioPlayMode) _playModeProp.enumValueIndex;
                switch (currentPlayMode)
                {
                    case EAudioPlayMode.Single:
                        break;
                    case EAudioPlayMode.Sequence:
                        EditorGUI.LabelField(valueRect, index.ToString(), Uniform.CenterRichLabel);
                        break;
                    case EAudioPlayMode.Random:
                        var weightProp = clipProp.FindPropertyRelative(SoundClip.ForEditor.Weight);
                        var intFieldStyle = new GUIStyle(EditorStyles.numberField) {alignment = TextAnchor.MiddleCenter};
                        weightProp.intValue = EditorGUI.IntField(valueRect, weightProp.intValue, intFieldStyle);
                        break;
                }
            }
        }


        private void OnDrawFooter(Rect rect)
        {
            ReorderableList.defaultBehaviours.DrawFooter(rect, _reorderableList);
            if (CurrentSelectedClip.TryGetPropertyObject(SoundClip.ForEditor.AudioClip, out AudioClip audioClip))
            {
                EditorGUI.LabelField(rect, audioClip.name.SetColor(Uniform.Green).ToBold(), Uniform.RichLabel);
            }
        }

        private void OnRemove(ReorderableList list)
        {
            ReorderableList.defaultBehaviours.DoRemoveButton(list);
            UpdatePlayMode();
        }

        private void OnAdd(ReorderableList list)
        {
            AddClip(list);
            UpdatePlayMode();
        }

        private void OnSelect(ReorderableList list) { EditorPlayAudioClip.In.StopAllClips(); }

        #endregion

        private SerializedProperty AddClip(ReorderableList list)
        {
            ReorderableList.defaultBehaviours.DoAddButton(list);
            var clipProp = list.serializedProperty.GetArrayElementAtIndex(list.count - 1);
            EditorAudioEx.ResetAudioClipSerializedProperties(clipProp);
            return clipProp;
        }
    }
}