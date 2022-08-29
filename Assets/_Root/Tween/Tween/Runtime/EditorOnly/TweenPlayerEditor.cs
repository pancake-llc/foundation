#if UNITY_EDITOR

#if UNITY_2019_100_OR_NEWER
#define SERIALIZE_REFERENCE_SERIALIZATION_FIXED
#endif

#if UNITY_2019_3_OR_NEWER && !(UNITY_2019_3_0 || UNITY_2019_3_1 || UNITY_2019_3_2 || UNITY_2019_3_3 || UNITY_2019_3_4 || UNITY_2019_3_5 || UNITY_2019_3_6)
#define SERIALIZE_REFERENCE_UNDO_FIXED
#endif

using UnityEngine;
using UnityEditor;
using Pancake.Editor;

namespace Pancake.Tween
{
    public partial class TweenPlayer
    {
        [SerializeField] private bool foldoutControl = true;
        [SerializeField] private bool foldoutEvents = false;

        private bool _preview;
        private bool _dragging;

        private bool _enabledRecord;
        private float _normalizedTimeRecord;
        private PlayDirection _directionRecord;

#if !SERIALIZE_REFERENCE_SERIALIZATION_FIXED
        private System.Collections.Generic.List<TweenAnimation> _recordedAnimations;
#endif

        protected override void OnValidate()
        {
            base.OnValidate();

            if (animations != null)
                foreach (var anim in animations)
                    anim.OnValidate(this);
        }


        private void RecordAll()
        {
            _enabledRecord = enabled;
            _normalizedTimeRecord = _normalizedTime;
            _directionRecord = direction;

            if (animations != null)
                foreach (var anim in animations)
                    anim.RecordState();

#if !SERIALIZE_REFERENCE_SERIALIZATION_FIXED
            if (animations != null) _recordedAnimations = new System.Collections.Generic.List<TweenAnimation>(animations);
            else _recordedAnimations = null;
#endif
        }


        private void RestoreAll()
        {
            enabled = _enabledRecord;
            _normalizedTime = _normalizedTimeRecord;
            direction = _directionRecord;

#if SERIALIZE_REFERENCE_SERIALIZATION_FIXED
            if (_animations != null)
                foreach (var anim in _animations)
                    anim.RestoreState();

#else

            if (_recordedAnimations != null)
                foreach (var anim in _recordedAnimations)
                    anim.RestoreState();

            _recordedAnimations = null;

#endif
        }


        internal bool Playing
        {
            get => Application.isPlaying ? enabled : _preview;
            set
            {
                if (Application.isPlaying) enabled = value;
                else
                {
                    if (_preview != value)
                    {
                        _preview = value;

                        if (value)
                        {
                            EditorApplication.update += OnUpdate;
                            EditorApplication.quitting += StopPreview;
                            EditorApplication.playModeStateChanged += StopPreview2;

                            RecordAll();

                            enabled = true;
                        }
                        else
                        {
                            EditorApplication.update -= OnUpdate;
                            EditorApplication.quitting -= StopPreview;
                            EditorApplication.playModeStateChanged -= StopPreview2;

                            if (this) RestoreAll();
                        }
                    }
                }

                void StopPreview() { Playing = false; }

                void StopPreview2(PlayModeStateChange msg)
                {
                    if (msg == PlayModeStateChange.ExitingEditMode) Playing = false;
                }
            }
        }


        private bool Dragging
        {
            get => _dragging;
            set
            {
                if (_dragging != value)
                {
                    _dragging = value;

                    if (value)
                    {
                        if (!Playing) RecordAll();
                    }
                    else
                    {
                        if (!Playing) RestoreAll();
                    }
                }
            }
        }


        internal void MoveUpAnimationWithUndo(int index)
        {
            if (index > 0)
            {
#if SERIALIZE_REFERENCE_UNDO_FIXED
                Undo.RecordObject(this, "Move Up Animation");
#else
                Undo.RegisterCompleteObjectUndo(this, "Move Up Animation");
#endif
                animations.Swap(index, index - 1);
            }
        }


        internal void MoveDownAnimationWithUndo(int index)
        {
            if (index < animations.Count - 1)
            {
#if SERIALIZE_REFERENCE_UNDO_FIXED
                Undo.RecordObject(this, "Move Down Animation");
#else
                Undo.RegisterCompleteObjectUndo(this, "Move Down Animation");
#endif
                animations.Swap(index, index + 1);
            }
        }


        [ContextMenu("Swap 'From' with 'To'")]
        private void SwapFromWithTo()
        {
            if (animations != null)
            {
                Undo.RecordObject(this, "Swap 'From' with 'To'");

                foreach (var a in animations)
                {
                    if (a is ITweenFromTo i) i.SwapFromWithTo();
                }
            }
        }


        [ContextMenu("Let 'From' Equal 'Current'")]
        private void LetFromEqualCurrent()
        {
            if (animations != null)
            {
                Undo.RecordObject(this, "Let 'From' Equal 'Current'");

                foreach (var a in animations)
                {
                    if (a is ITweenUnmanaged i) i.LetFromEqualCurrent();
                }
            }
        }


        [ContextMenu("Let 'To' Equal 'Current'")]
        private void LetToEqualCurrent()
        {
            if (animations != null)
            {
                Undo.RecordObject(this, "Let 'To' Equal 'Current'");

                foreach (var a in animations)
                {
                    if (a is ITweenUnmanaged i) i.LetToEqualCurrent();
                }
            }
        }


        [ContextMenu("Let 'Current' Equal 'From'")]
        private void LetCurrentEqualFrom()
        {
            if (animations != null)
            {
                foreach (var a in animations)
                {
                    if (a is ITweenFromToWithTarget i && i.target)
                    {
                        Undo.RecordObject(i.target, "Let 'Current' Equal 'From'");
                        i.LetCurrentEqualFrom();
                    }
                }
            }
        }


        [ContextMenu("Let 'Current' Equal 'To'")]
        private void LetCurrentEqualTo()
        {
            if (animations != null)
            {
                foreach (var a in animations)
                {
                    if (a is ITweenFromToWithTarget i && i.target)
                    {
                        Undo.RecordObject(i.target, "Let 'Current' Equal 'To'");
                        i.LetCurrentEqualTo();
                    }
                }
            }
        }


        [CustomEditor(typeof(TweenPlayer))]
        [CanEditMultipleObjects]
        internal class Editor : BaseEditor<TweenPlayer>
        {
            private static GUIStyle _imageButtonStyle;

            private static GUIStyle imageButtonStyle
            {
                get
                {
                    if (_imageButtonStyle == null)
                    {
                        _imageButtonStyle = new GUIStyle(EditorStyles.miniButton);
                        _imageButtonStyle.padding = new RectOffset(0, 0, 0, 0);
                    }

                    return _imageButtonStyle;
                }
            }

            internal static Color progressBackgroundInvalid
            {
                get { return EditorGUIUtility.isProSkin ? new Color(0.05f, 0.05f, 0.05f, 0.25f) : new Color(0.25f, 0.25f, 0.25f, 0.25f); }
            }

            internal static Color progressBackgroundValid { get { return EditorGUIUtility.isProSkin ? new Color(0.05f, 0.05f, 0.05f) : new Color(0.25f, 0.25f, 0.25f); } }

            internal static Color progressForegroundInvalid { get { return new Color(0f, 0.6f, 0f, 0.25f); } }

            internal static Color progressForegroundValid { get { return new Color(0.1f, 0.8f, 0.1f); } }

            internal static Color separatorLineColor
            {
                get
                {
                    var lineColor = EditorGUIUtilities.labelNormalColor;
                    lineColor.a *= 0.25f;
                    return lineColor;
                }
            }

            private SerializedProperty _durationProp;
            private SerializedProperty _updateModeProp;
            private SerializedProperty _timeModeProp;
            private SerializedProperty _wrapModeProp;
            private SerializedProperty _arrivedActionProp;
            private SerializedProperty _syncOnAwakeProp;
            private SerializedProperty _onForwardArrivedProp;
            private SerializedProperty _onBackArrivedProp;
            private SerializedProperty _animationsProp;

            private GenericMenu _addMenu;


            private void ShowAddMenu(Rect rect)
            {
                if (_addMenu == null)
                {
                    _addMenu = EditorGUIUtilities.CreateMenu(TweenAnimation.allTypes.Keys,
                        t => new GUIContent(TweenAnimation.allTypes[t].menu),
                        t => MenuItemState.Normal,
                        t =>
                        {
#if SERIALIZE_REFERENCE_UNDO_FIXED
                            Undo.RecordObject(target, "Add Animation");
#else
                            Undo.RegisterCompleteObjectUndo(target, "Add Animation");
#endif
                            var anim = target.AddAnimation(t);
                            anim.Reset(target);
                        });
                }

                _addMenu.DropDown(rect);
            }


            private void OnEnable()
            {
                _durationProp = serializedObject.FindProperty(nameof(duration));
                _updateModeProp = serializedObject.FindProperty("updateMode");
                _timeModeProp = serializedObject.FindProperty(nameof(timeMode));
                _wrapModeProp = serializedObject.FindProperty(nameof(wrapMode));
                _arrivedActionProp = serializedObject.FindProperty(nameof(arrivedAction));
                _syncOnAwakeProp = serializedObject.FindProperty(nameof(sampleOnAwake));

                _onForwardArrivedProp = serializedObject.FindProperty(nameof(onForwardArrived));
                _onBackArrivedProp = serializedObject.FindProperty(nameof(onBackArrived));

                _animationsProp = serializedObject.FindProperty(nameof(animations));
            }


            private void OnDisable()
            {
                if (!Application.isPlaying && target)
                    target.Playing = false;
            }


            public override bool RequiresConstantRepaint()
            {
                if (Application.isPlaying) return target.isActiveAndEnabled;
                else return target._preview;
            }


            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                var rect = EditorGUILayout.GetControlRect();
                var rect2 = rect;

                // control foldout
                using (var scope = ChangeCheckScope.New(target))
                {
                    //rect2.width = rect2.height;
                    var result = GUI.Toggle(rect2, target.foldoutControl, GUIContent.none, EditorStyles.foldout);
                    if (scope.changed) target.foldoutControl = result;
                }

                // control label
                rect.xMin += rect2.height;
                EditorGUI.LabelField(rect, "Control", EditorStyles.boldLabel);

                // control fields
                if (target.foldoutControl)
                {
                    EditorGUILayout.PropertyField(_durationProp);
                    EditorGUILayout.PropertyField(_updateModeProp);
                    EditorGUILayout.PropertyField(_timeModeProp);
                    EditorGUILayout.PropertyField(_wrapModeProp);
                    EditorGUILayout.PropertyField(_arrivedActionProp);
                    EditorGUILayout.PropertyField(_syncOnAwakeProp);
                    GUILayout.Space(4);
                }

                if (!serializedObject.isEditingMultipleObjects)
                {
                    EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), separatorLineColor);
                    GUILayout.Space(4);

                    rect = EditorGUILayout.GetControlRect();
                    rect2 = rect;

                    // play button
                    rect2.width = EditorGUIUtility.singleLineHeight * 2 - 4;
                    using (GUIContentColorScope.New(target.Playing ? progressForegroundValid : EditorGUIUtilities.labelNormalColor))
                    {
                        target.Playing = GUI.Toggle(rect2, target.Playing, EditorGUIUtilities.TempContent(image: TweenEditorSetting.instance.play), imageButtonStyle);
                    }

                    // direction button
                    rect2.x = rect.xMax - rect2.width;
                    using (DisabledScope.New(!target.Playing))
                    {
                        using (GUIContentColorScope.New(EditorGUIUtilities.labelNormalColor))
                        {
                            if (GUI.Button(rect2,
                                    EditorGUIUtilities.TempContent(image: target.direction == PlayDirection.Forward
                                        ? TweenEditorSetting.instance.rightArrow
                                        : TweenEditorSetting.instance.leftArrow),
                                    imageButtonStyle))
                            {
                                target.ReverseDirection();
                            }
                        }
                    }

                    rect.xMin += EditorGUIUtility.singleLineHeight * 2;
                    rect.xMax -= EditorGUIUtility.singleLineHeight * 2;

                    // Mouse start drag
                    if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                    {
                        target.Dragging = true;
                    }

                    // Mouse end drag
                    if (Event.current.rawType == EventType.MouseUp && target.Dragging)
                    {
                        target.Dragging = false;
                        Repaint();
                    }

                    // progress bar
                    using (var scope = ChangeCheckScope.New())
                    {
                        float progress = EditorGUIUtilities.DragProgress(rect, target._normalizedTime, progressBackgroundValid, progressForegroundValid);
                        if (scope.changed && target.Dragging) target.NormalizedTime = progress;
                    }

                    GUILayout.Space(4);
                }

                EditorGUILayout.EndVertical();

                if (serializedObject.isEditingMultipleObjects)
                {
                    serializedObject.ApplyModifiedProperties();
                    return;
                }

                GUILayout.Space(4);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                rect = EditorGUILayout.GetControlRect();
                rect2 = rect;

                // events foldout
                using (var scope = ChangeCheckScope.New(target))
                {
                    //rect2.width = rect2.height;
                    var result = GUI.Toggle(rect2, target.foldoutEvents, GUIContent.none, EditorStyles.foldout);
                    if (scope.changed) target.foldoutEvents = result;
                }

                // events label
                rect.xMin += rect2.height;
                EditorGUI.LabelField(rect, "Events", EditorStyles.boldLabel);

                // events
                if (target.foldoutEvents)
                {
                    EditorGUILayout.PropertyField(_onForwardArrivedProp);
                    GUILayout.Space(2);
                    EditorGUILayout.PropertyField(_onBackArrivedProp);
                    GUILayout.Space(2);
                }


                EditorGUILayout.EndVertical();
                GUILayout.Space(4);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Animation List
                if (target.animations != null)
                {
                    for (int i = 0; i < target.animations.Count; i++)
                    {
                        target.animations[i].OnInspectorGUI(i, target, _animationsProp.GetArrayElementAtIndex(i));
                    }
                }

                serializedObject.ApplyModifiedProperties();

                // add button
                GUILayout.Space(4);
                var buttonRect = EditorGUILayout.GetControlRect();
                using (DisabledScope.New(target.Playing))
                {
                    if (GUI.Button(buttonRect, "Add Animation", EditorStyles.miniButton))
                    {
                        ShowAddMenu(buttonRect);
                    }
                }

                GUILayout.Space(4);

                EditorGUILayout.EndVertical();

                if (!Application.isPlaying && target._preview)
                {
                    SceneView.RepaintAll();
                }
            }
        } // class Editor
    } // class TweenPlayer
} // UnityExtensions.Tween

#endif // UNITY_EDITOR