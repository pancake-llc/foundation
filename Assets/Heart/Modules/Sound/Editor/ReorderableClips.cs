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

        private readonly ReorderableList _reorderableList;
        private readonly SerializedProperty _playModeProp;
        private int _currSelectedClipIndex = -1;
        private SerializedProperty _currSelectedClip;
        private Rect _previewRect;

        public bool IsMulticlips => _reorderableList.count > 1;
        public float Height => _reorderableList.GetHeight();
        private Vector2 PlayButtonSize => new(30f, 20f);

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
            _playModeProp = entityProperty.FindPropertyRelative(AudioEntity.EditorPropertyName.AudioPlayMode);
            _reorderableList = CreateReorderabeList(entityProperty);
            UpdatePlayMode();

            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        public void Dispose() { Undo.undoRedoPerformed -= OnUndoRedoPerformed; }

        public void SetPreviewRect(Rect rect) { _previewRect = rect; }

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
            var clipsProp = entityProperty.FindPropertyRelative(AudioEntity.EditorPropertyName.Clips);
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

        private EAudioPlayMode UpdatePlayMode()
        {
            if (!IsMulticlips) _playModeProp.enumValueIndex = 0;
            else if (IsMulticlips && _playModeProp.enumValueIndex == 0) _playModeProp.enumValueIndex = (int) DEFAULT_MULTICLIPS_MODE;

            return (EAudioPlayMode) _playModeProp.enumValueIndex;
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
                        var audioClipProp = clipProp.FindPropertyRelative(SoundClip.EditorPropertyName.AudioClip);
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
            var multiclipOptionRect = new Rect(rect) {width = (rect.width - labelRect.width - valueRect.width) * 0.5f, x = labelRect.xMax};

            EditorGUI.LabelField(labelRect, "Clips");
            if (IsMulticlips)
            {
                var popupStyle = new GUIStyle(EditorStyles.popup) {alignment = TextAnchor.MiddleLeft};
                var currentPlayMode = (EAudioPlayMode) _playModeProp.enumValueIndex;
                _playModeProp.enumValueIndex = (int) (EAudioPlayMode) EditorGUI.EnumPopup(multiclipOptionRect, currentPlayMode, popupStyle);
                currentPlayMode = (EAudioPlayMode) _playModeProp.enumValueIndex;
                switch (currentPlayMode)
                {
                    case EAudioPlayMode.Sequence:
                        EditorGUI.LabelField(valueRect, "Index", Uniform.CenterRichLabel);
                        break;
                    case EAudioPlayMode.Random:
                        EditorGUI.LabelField(valueRect, "Weight", Uniform.CenterRichLabel);
                        break;
                }

                EditorGUI.LabelField(multiclipOptionRect.DissolveHorizontal(0.5f), "(PlayMode)".SetColor(Color.gray), Uniform.CenterRichLabel);
            }
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var clipProp = _reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            var audioClipProp = clipProp.FindPropertyRelative(SoundClip.EditorPropertyName.AudioClip);
            var volProp = clipProp.FindPropertyRelative(SoundClip.EditorPropertyName.Volume);

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
                if (audioClipProp.objectReferenceValue is AudioClip audioClip)
                {
                    bool isPlaying = EditorPlayAudioClip.CurrentPlayingClip == audioClip;
                    string icon = isPlaying ? "PreMatQuad" : "PlayButton";
                    var buttonGUIContent = new GUIContent(EditorGUIUtility.IconContent(icon).image, EditorPlayAudioClip.PLAY_WITH_VOLUME_SETTING);
                    if (GUI.Button(buttonRect, buttonGUIContent))
                    {
                        if (isPlaying) EditorPlayAudioClip.StopAllClips();
                        else
                        {
                            float startPos = clipProp.FindPropertyRelative(SoundClip.EditorPropertyName.StartPosition).floatValue;
                            float endPos = clipProp.FindPropertyRelative(SoundClip.EditorPropertyName.EndPosition).floatValue;
                            if (Event.current.button == 0) EditorPlayAudioClip.PlayClip(audioClip, startPos, endPos);
                            else EditorPlayAudioClip.PlayClipByAudioSource(audioClip, volProp.floatValue, startPos, endPos);

                            if (EditorPlayAudioClip.PlaybackIndicator.IsPlaying && EditorPlayAudioClip.CurrentPlayingClip == audioClip)
                            {
                                var clip = new PreviewClip() {StartPosition = startPos, EndPosition = endPos, Length = audioClip.length,};

                                EditorPlayAudioClip.PlaybackIndicator.SetClipInfo(_previewRect, clip);
                            }
                        }
                    }
                }
            }

            void DrawVolumeSlider()
            {
                var labelRect = new Rect(sliderRect) {width = SLIDER_LABEL_WIDTH};
                sliderRect.width -= SLIDER_LABEL_WIDTH;
                sliderRect.x = labelRect.xMax;
                EditorGUI.LabelField(labelRect, EditorGUIUtility.IconContent("SceneViewAudio On"));
                float newVol = EditorAudioEx.DrawVolumeSlider(sliderRect, volProp.floatValue, out bool hasChanged, out float newSliderValue);
                if (hasChanged)
                {
                    volProp.floatValue = newVol;
                    EditorAudioEx.DrawDecibelValuePeeking(volProp.floatValue, 3f, sliderRect, newSliderValue);
                }
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
                        var weightProp = clipProp.FindPropertyRelative(SoundClip.EditorPropertyName.Weight);
                        var intFieldStyle = new GUIStyle(EditorStyles.numberField) {alignment = TextAnchor.MiddleCenter};
                        weightProp.intValue = EditorGUI.IntField(valueRect, weightProp.intValue, intFieldStyle);
                        break;
                }
            }
        }


        private void OnDrawFooter(Rect rect)
        {
            ReorderableList.defaultBehaviours.DrawFooter(rect, _reorderableList);
            if (CurrentSelectedClip.TryGetPropertyObject(SoundClip.EditorPropertyName.AudioClip, out AudioClip audioClip))
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

        private void OnSelect(ReorderableList list) { EditorPlayAudioClip.StopAllClips(); }

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