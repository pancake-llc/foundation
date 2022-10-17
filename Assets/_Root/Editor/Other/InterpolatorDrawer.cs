#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Pancake.Editor;

namespace Pancake.Tween
{
    /// <summary>
    /// Interpolator Drawer
    /// </summary>
    [CustomPropertyDrawer(typeof(Interpolator))]
    internal class InterpolatorDrawer : PropertyDrawer
    {
        /// <summary>
        /// Utilities for editor GUI.
        /// </summary>
        private struct EditorGUIUtilities
        {
            private static GUIContent tempContent = new GUIContent();
            private static Texture2D paneOptionsIconDark;

            private static int dragState;
            private static Vector2 dragPos;

            public static Texture2D PaneOptionsIconDark
            {
                get
                {
                    if (paneOptionsIconDark == null)
                    {
                        paneOptionsIconDark = (Texture2D) EditorGUIUtility.Load("Builtin Skins/DarkSkin/Images/pane options.png");
                    }

                    return paneOptionsIconDark;
                }
            }


            /// <summary>
            /// Get a temporary GUIContent（use this to avoid GC).
            /// </summary>
            public static GUIContent TempContent(string text = null, Texture image = null, string tooltip = null)
            {
                tempContent.text = text;
                tempContent.image = image;
                tempContent.tooltip = tooltip;

                return tempContent;
            }

            /// <summary>
            /// Draw a rect wireframe.
            /// </summary>
            public static void DrawWireRect(Rect rect, Color color, float borderWidth = 1f)
            {
                Rect border = new Rect(rect.x, rect.y, rect.width, borderWidth);
                EditorGUI.DrawRect(border, color);
                border.y = rect.yMax - borderWidth;
                EditorGUI.DrawRect(border, color);
                border.yMax = border.yMin;
                border.yMin = rect.yMin + borderWidth;
                border.width = borderWidth;
                EditorGUI.DrawRect(border, color);
                border.x = rect.xMax - borderWidth;
                EditorGUI.DrawRect(border, color);
            }

            /// <summary>
            /// Draw a progress bar that can be dragged.
            /// </summary>
            public static float DragProgress(Rect rect, float value01, Color backgroundColor, Color foregroundColor, bool draggable = true)
            {
                var progressRect = rect;
                progressRect.width = Mathf.Round(progressRect.width * value01);

                EditorGUI.DrawRect(rect, backgroundColor);
                EditorGUI.DrawRect(progressRect, foregroundColor);

                int id = GUIUtility.GetControlID(FocusType.Passive);
                Event current = Event.current;

                switch (current.GetTypeForControl(id))
                {
                    case EventType.MouseDown:
                        if (rect.Contains(current.mousePosition) && current.button == 0)
                        {
                            EditorGUIUtility.editingTextField = false;
                            GUIUtility.hotControl = id;
                            dragState = 1;

                            if (draggable)
                            {
                                float offset = current.mousePosition.x - rect.x + 1f;
                                value01 = Mathf.Clamp01(offset / rect.width);
                            }

                            GUI.changed = true;

                            current.Use();
                        }

                        break;

                    case EventType.MouseUp:
                        if (GUIUtility.hotControl == id && dragState != 0)
                        {
                            GUIUtility.hotControl = 0;
                            dragState = 0;
                            current.Use();
                        }

                        break;

                    case EventType.MouseDrag:
                        if (GUIUtility.hotControl != id)
                        {
                            break;
                        }

                        if (dragState != 0)
                        {
                            if (draggable)
                            {
                                float offset = current.mousePosition.x - rect.x + 1f;
                                value01 = Mathf.Clamp01(offset / rect.width);
                            }

                            GUI.changed = true;

                            current.Use();
                        }

                        break;

                    case EventType.Repaint:
                        if (draggable) EditorGUIUtility.AddCursorRect(rect, MouseCursor.SlideArrow);
                        break;
                }

                return value01;
            }
        }

        internal struct HandlesColorScope : IDisposable
        {
            private Color _orginal;

            public static HandlesColorScope New(Color value)
            {
                var scope = new HandlesColorScope {_orginal = Handles.color};
                Handles.color = value;
                return scope;
            }

            void IDisposable.Dispose() { Handles.color = _orginal; }
        }

        internal struct HandlesMatrixScope : IDisposable
        {
            private Matrix4x4 _orginal;

            public static HandlesMatrixScope New(Matrix4x4 value)
            {
                var scope = new HandlesMatrixScope {_orginal = Handles.matrix};
                Handles.matrix = value;
                return scope;
            }

            void IDisposable.Dispose() { Handles.matrix = _orginal; }
        }

        internal struct ChangeCheckScope : IDisposable
        {
            private bool _end;
            private bool _changed;
            private UnityEngine.Object _undoRecordObject;

            public bool changed
            {
                get
                {
                    if (!_end)
                    {
                        _end = true;
                        _changed = EditorGUI.EndChangeCheck();
                        if (_changed && _undoRecordObject)
                        {
                            Undo.RecordObject(_undoRecordObject, _undoRecordObject.name);
                        }
                    }

                    return _changed;
                }
            }

            public static ChangeCheckScope New(UnityEngine.Object undoRecordObject = null)
            {
                EditorGUI.BeginChangeCheck();
                return new ChangeCheckScope {_end = false, _changed = false, _undoRecordObject = undoRecordObject};
            }

            void IDisposable.Dispose()
            {
                if (!_end)
                {
                    _end = true;
                    _changed = EditorGUI.EndChangeCheck();
                }
            }
        }


        private static Vector3[] segmentVertices = new Vector3[2];
        private static Vector3[] boundsVertices = new Vector3[10];

        // Data buffer at last sample
        private int _lastType;
        private float _lastStrength;

        private float _minValue, _maxValue;
        private List<Vector3> _samples = new List<Vector3>(64);

        private void Sample(int type, float strength, int maxSegments, float maxError)
        {
            if (_samples.Count == 0 || type != _lastType || strength != _lastStrength)
            {
                _lastType = type;
                _lastStrength = strength;
                _samples.Clear();

                var interpolator = new Interpolator((Ease) type, strength);

                // add first point

                Vector3 point = new Vector3(0, interpolator[0]);
                _samples.Add(point);

                // add other points

                Vector3 lastSample = point, lastEvaluate = point;
                _minValue = _maxValue = point.y;

                float minSlope = float.MinValue;
                float maxSlope = float.MaxValue;

                for (int i = 1; i <= maxSegments; i++)
                {
                    point.x = i / (float) maxSegments;
                    point.y = interpolator[point.x];

                    if (_minValue > point.y) _minValue = point.y;
                    if (_maxValue < point.y) _maxValue = point.y;

                    maxSlope = Mathf.Min((point.y - lastSample.y + maxError) / (point.x - lastSample.x), maxSlope);
                    minSlope = Mathf.Max((point.y - lastSample.y - maxError) / (point.x - lastSample.x), minSlope);

                    if (minSlope >= maxSlope)
                    {
                        _samples.Add(lastSample = lastEvaluate);
                        maxSlope = (point.y - lastSample.y + maxError) / (point.x - lastSample.x);
                        minSlope = (point.y - lastSample.y - maxError) / (point.x - lastSample.x);
                    }

                    lastEvaluate = point;
                }

                // add last point

                _samples.Add(point);
                if (_minValue > point.y) _minValue = point.y;
                if (_maxValue < point.y) _maxValue = point.y;

                // Calculate drawn boundary values

                if (_maxValue - _minValue < 1f)
                {
                    if (_minValue < 0f)
                    {
                        _maxValue = _minValue + 1f;
                    }
                    else if (_maxValue > 1f)
                    {
                        _minValue = _maxValue - 1f;
                    }
                    else
                    {
                        _minValue = 0f;
                        _maxValue = 1f;
                    }
                }
            }
        }


        private static void DrawAALine(Vector3 point1, Vector3 point2)
        {
            segmentVertices[0] = point1;
            segmentVertices[1] = point2;
            Handles.DrawAAPolyLine(segmentVertices);
        }

        // draw curve
        private void DrawCurve(Rect rect, bool drawStrength)
        {
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));

            Vector2 origin = new Vector2(rect.x + 1, rect.y + 1);
            Vector2 scale = new Vector2(rect.width - 2, (rect.height - 2) / (_maxValue - _minValue));

            if (_maxValue > 0f && _minValue < 1f)
            {
                float yMin = origin.y + (_maxValue - Mathf.Min(_maxValue, 1f)) * scale.y;
                float yMax = origin.y + (_maxValue - Mathf.Max(_minValue, 0f)) * scale.y;
                Rect rect01 = new Rect(rect.x, yMin, rect.width, yMax - yMin);
                EditorGUI.DrawRect(rect01, new Color(1f, 1f, 1f, 0.15f));
            }

            if (drawStrength)
            {
                EditorGUI.DrawRect(new Rect(rect.x + (rect.width - 1) * _lastStrength, rect.y, 1, rect.height), new Color(1, 0.33f, 0));
            }

            Vector3 last = _samples[0];
            last.x = origin.x + last.x * scale.x;
            last.y = origin.y + (_maxValue - last.y) * scale.y;

            using (HandlesColorScope.New(new Color(0f, 1f, 0f)))
            {
                Vector3 point;

                for (int i = 1; i < _samples.Count; i++)
                {
                    point = _samples[i];
                    point.x = origin.x + point.x * scale.x;
                    point.y = origin.y + (_maxValue - point.y) * scale.y;

                    DrawAALine(last, point);
                    last = point;
                }
            }

            EditorGUIUtilities.DrawWireRect(rect, new Color(0, 0, 0, 0.4f));
        }


        public override bool CanCacheInspectorGUI(SerializedProperty property) { return false; }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return EditorGUIUtility.singleLineHeight * 2 + 2; }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            var typeProp = property.FindPropertyRelative("ease");
            int type = typeProp.intValue;
            var strengthProp = property.FindPropertyRelative("strength");

            var buttonRect = new Rect(position.x, position.y, EditorGUIUtilities.PaneOptionsIconDark.width, EditorGUIUtilities.PaneOptionsIconDark.height);
            using (var scope = ChangeCheckScope.New())
            {
                EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Arrow);

                System.Enum newType = EditorGUI.EnumPopup(buttonRect, GUIContent.none, (Ease) type);

                if (scope.changed)
                {
                    typeProp.intValue = type = (int) (Ease) newType;
                    strengthProp.floatValue = 0.5f;
                }
            }

            if ((Ease) type == Ease.CustomCurve)
            {
                EditorGUIUtility.AddCursorRect(position, MouseCursor.Zoom);
                EditorGUI.PropertyField(position, property.FindPropertyRelative("customCurve"), GUIContent.none);
            }
            else
            {
                bool drawStrength;

                switch ((Ease) type)
                {
                    case Ease.Accelerate:
                    case Ease.Decelerate:
                    case Ease.AccelerateDecelerate:
                    case Ease.Anticipate:
                    case Ease.Overshoot:
                    case Ease.AnticipateOvershoot:
                    case Ease.Bounce:
                        strengthProp.floatValue = EditorGUIUtilities.DragProgress(position, strengthProp.floatValue, default, default);
                        drawStrength = true;
                        break;
                    default:
                        drawStrength = false;
                        break;
                }

                if (Event.current.type == EventType.Repaint)
                {
                    Sample(type, strengthProp.floatValue, Mathf.Min((int) position.width, 256), 0.002f);
                    DrawCurve(position, drawStrength);
                }
            }

            EditorGUI.LabelField(buttonRect, EditorGUIUtilities.TempContent(image: EditorGUIUtilities.PaneOptionsIconDark), GUIStyle.none);
        }
    }
}

#endif