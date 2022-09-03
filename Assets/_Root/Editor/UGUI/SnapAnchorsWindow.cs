using System;
using System.Collections.Generic;
using System.Linq;
using Pancake.Editor;
using UnityEditor;
using UnityEngine;

namespace Pancake.UI.Editor
{
    public class SnapAnchorsWindow : EditorWindow
    {
        private const string HIGHLIGHT_COLOR = "#0ef05d";

        public enum AnchorMode
        {
            Border,
            Point,
        }


        private List<RectTransform> _objects;

        private AnchorMode _mode = AnchorMode.Border;
        private bool _parentPosition;
        private Vector2 _point = new Vector2(0.5f, 0.5f);

        private GUIStyle _setPivotStyle, _selectPointStyle;

        [MenuItem("Tools/Pancake/UI/Snap Anchors", false, 30)]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(SnapAnchorsWindow), false, "Snap Anchors");
            window.minSize = window.maxSize = new Vector2(200, 325);
        }

        private void OnEnable() { Selection.selectionChanged += Repaint; }

        private void OnGUI()
        {
            #region init styles

            if (_selectPointStyle == null)
            {
                _selectPointStyle = new GUIStyle(EditorStyles.helpBox) {margin = new RectOffset(0, 0, 0, 0), richText = true, alignment = TextAnchor.MiddleCenter};
            }

            if (_setPivotStyle == null)
            {
                _setPivotStyle = new GUIStyle(EditorStyles.miniButton) {richText = true, alignment = TextAnchor.MiddleCenter};
            }

            #endregion

            _objects = Selection.gameObjects.Where((o) => o.transform is RectTransform).Select((o) => o.transform as RectTransform).ToList();


            EditorGUILayout.Space();
            DrawModeSelection();
            EditorGUILayout.Space();

            bool active = _objects.Count > 0;
            if (!(active))
            {
                EditorGUI.BeginDisabledGroup(true);
            }

            if (_objects.Count > 0)
            {
                string txt = (_objects.Count == 1) ? _objects[0].name : string.Format("{0} UI Elements", _objects.Count);
                EditorGUILayout.LabelField(txt, EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                GUIStyle warn = GUI.skin.GetStyle("WarningOverlay");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);
                GUILayout.TextArea("No UI Element selected.", warn);
                GUILayout.Space(5);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();


            switch (_mode)
            {
                case AnchorMode.Border:
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button(new GUIContent(EditorResources.Instance.allBorderPic, "Snap to all borders"), GUILayout.Width(120), GUILayout.Height(120)))
                        SnapBorder(left: true, right: true, top: true, bottom: true);


                    // TOP DOWN
                    if (GUILayout.Button(new GUIContent(EditorResources.Instance.verticalBorderPic, "Snap to top and bottom border"), GUILayout.Width(60), GUILayout.Height(120)))
                        SnapBorder(left: false, right: false, top: true, bottom: true);

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    // LEFT RIGHT
                    if (GUILayout.Button(new GUIContent(EditorResources.Instance.horizontalBorderPic, "Snap to left and right border"), GUILayout.Width(120), GUILayout.Height(60)))
                        SnapBorder(left: true, right: true, top: false, bottom: false);

                    EditorGUILayout.LabelField("", GUILayout.Width(60));

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();


                    if (!(active))
                        EditorGUI.EndDisabledGroup();
                }
                    break;

                case AnchorMode.Point:

                    DrawPointButtons();

                    if (!(active))
                        EditorGUI.EndDisabledGroup();

                    GUILayout.Space(-12); // move upwards a bit since there is empty space.
                    _parentPosition = EditorGUILayout.ToggleLeft("Use Parent Space", _parentPosition);

                    _point = EditorGUILayout.Vector2Field("Snap Point", _point);

                    string btnText = string.Format("Set Pivot to <color={0}>({1:f}, {2:f})</color>", HIGHLIGHT_COLOR, _point.x, _point.y);
                    if (GUILayout.Button(btnText, _setPivotStyle))
                    {
                        Undo.RecordObjects(_objects.Select(o => o as UnityEngine.Object).ToArray(), "set pivots");
                        foreach (var obj in _objects)
                        {
                            obj.pivot = _point;
                        }
                    }

                    break;

                default:
                    break;
            }


            EditorGUILayout.Space();
        }

        private void SetPoint(float x, float y) { _point = new Vector2(x, y); }

        private Vector2 GetPivotOffset(RectTransform obj)
        {
            if (_mode == AnchorMode.Border)
                return Vector2.zero;

            Vector2 result;

            if (_parentPosition && _mode == AnchorMode.Point)
            {
                RectTransform parentTransform = obj.parent as RectTransform;
                Rect parent = (parentTransform != null) ? parentTransform.ToScreenRect(true) : new Rect(0, 0, Screen.width, Screen.height);

                Rect rect = obj.ToScreenRect(true);
                Vector2 p = _point;

                result = new Vector2(p.x * parent.width, p.y * parent.height);
                result += parent.position;
                result -= rect.position;
                result = new Vector2(result.x / rect.width, result.y / rect.height) - obj.pivot;
            }
            else
            {
                result = _point - obj.pivot;
            }

            return result;
        }

        private void DrawPointButtons()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent(EditorResources.Instance.pointPic, "Snap all directions to position"), GUILayout.Width(120), GUILayout.Height(100)))
                SnapPoint(horizontal: true, vertical: true);

            // TOP DOWN
            if (GUILayout.Button(new GUIContent(EditorResources.Instance.verticalPointPic, "Snap vertically to position"), GUILayout.Width(60), GUILayout.Height(100)))
                SnapPoint(horizontal: false, vertical: true);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // LEFT RIGHT
            if (GUILayout.Button(new GUIContent(EditorResources.Instance.horizontalPointPic, "Snap horizontally to position"), GUILayout.Width(120), GUILayout.Height(60)))
                SnapPoint(horizontal: true, vertical: false);


            EditorGUILayout.BeginVertical();
            // const string style = "Label";
            var style = _selectPointStyle;
            EditorGUILayout.BeginHorizontal();
            DrawSelectionPoint("┌", style, 0f, 1f);
            DrawSelectionPoint("┬", style, 0.5f, 1f);
            DrawSelectionPoint("┐", style, 1f, 1f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            DrawSelectionPoint("├", style, 0f, 0.5f);
            DrawSelectionPoint("┼", style, 0.5f, 0.5f);
            DrawSelectionPoint("┤", style, 1f, 0.5f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            DrawSelectionPoint("└", style, 0f, 0f);
            DrawSelectionPoint("┴", style, 0.5f, 0f);
            DrawSelectionPoint("┘", style, 1f, 0f);
            EditorGUILayout.EndHorizontal();

            if (_objects.Count == 1)
            {
                var p = _objects[0].pivot;
                string content = "[ Pivot ]";
                content = HighlightTextIfMatchCoordinate("[ Pivot ]", p.x, p.y);
                if (GUILayout.Button(content, style, GUILayout.Width(60), GUILayout.Height(16)))
                {
                    SetPoint(p.x, p.y);
                }
            }
            else
            {
                GUILayout.Label("");
            }

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSelectionPoint(string content, GUIStyle style, float x, float y)
        {
            const float size = 20;
            content = HighlightTextIfMatchCoordinate(content, x, y);

            if (GUILayout.Button(content, style, GUILayout.Width(size), GUILayout.Height(size)))
            {
                SetPoint(x, y);
            }
        }

        private string HighlightTextIfMatchCoordinate(string content, float x, float y)
        {
            if (_point.x == x && _point.y == y)
            {
                content = string.Format("<color={0}>{1}</color>", HIGHLIGHT_COLOR, content);
            }

            return content;
        }

        private void DrawModeSelection()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Toggle((_mode == AnchorMode.Border), "Border", EditorStyles.miniButtonLeft) && (_mode != AnchorMode.Border))
            {
                _mode = AnchorMode.Border;
            }

            if (GUILayout.Toggle((_mode == AnchorMode.Point), "Point", EditorStyles.miniButtonRight) && (_mode != AnchorMode.Point))
            {
                _mode = AnchorMode.Point;
            }

            EditorGUILayout.EndHorizontal();
        }


        private void SnapBorder(bool left, bool right, bool top, bool bottom)
        {
            Undo.SetCurrentGroupName("Border" + DateTime.Now.ToFileTime());
            int group = Undo.GetCurrentGroup();

            foreach (var obj in _objects)
            {
                SnapBorder(obj,
                    left,
                    right,
                    top,
                    bottom);
            }

            Undo.CollapseUndoOperations(group);
        }

        internal static void SnapBorder(RectTransform obj, bool left, bool right, bool top, bool bottom)
        {
            Undo.RecordObject(obj.transform, "Snap Anchors Border");

            RectTransform parentTransform = obj.parent as RectTransform;
            Rect parent = (parentTransform != null) ? parentTransform.ToScreenRect() : new Rect(0, 0, Screen.width, Screen.height);

            Quaternion tmpRot = obj.rotation;
            Vector3 tmpScale = obj.localScale;

            obj.rotation = Quaternion.identity;
            obj.localScale = Vector3.one;

            Rect rect = obj.ToScreenRect();

            float sx = CalculateSize(obj.sizeDelta.x, left, right);
            float sy = CalculateSize(obj.sizeDelta.y, top, bottom);
            float x = CalculateAncherPos(obj.pivot.x,
                sx,
                left,
                right,
                obj.anchoredPosition.x); // (obj.sizeDelta.x * obj.pivot.x) - (sx * obj.sizeDelta.x);
            float y = CalculateAncherPos(obj.pivot.y,
                sy,
                top,
                bottom,
                obj.anchoredPosition.y); // (obj.sizeDelta.y * obj.pivot.y) - (sy * obj.sizeDelta.y);

            if (left || bottom)
            {
                float xMin = CalculateMinAnchor(left,
                    rect.xMin,
                    parent.xMin,
                    parent.size.x,
                    obj.anchorMin.x); //(left) ? (rect.xMin - parentRect.xMin) / parentRect.size.x : obj.anchorMin.x;
                float yMin = 1 - CalculateMaxAnchor(top,
                    rect.yMax,
                    parent.yMax,
                    parent.size.y,
                    1 - obj.anchorMin.y); // (top) ? rect.yMax / parent.size.y : obj.anchorMax.y;
                obj.anchorMin = new Vector2(xMin, yMin);
            }

            if (right || top)
            {
                float xMax = CalculateMaxAnchor(right,
                    rect.xMax,
                    parent.xMax,
                    parent.size.x,
                    obj.anchorMax.x); //(right) ? rect.xMax / parent.size.x : obj.anchorMax.x;
                float yMax = 1 - CalculateMinAnchor(bottom,
                    rect.yMin,
                    parent.yMin,
                    parent.size.y,
                    1 - obj.anchorMax.y); //(bottom) ? rect.yMin / parent.size.y : obj.anchorMin.y;
                obj.anchorMax = new Vector2(xMax, yMax);
            }

            obj.anchoredPosition = new Vector2(x, y);
            obj.sizeDelta = new Vector3(sx, sy);

            obj.rotation = tmpRot;
            obj.localScale = tmpScale;
        }

        private void SnapPoint(bool horizontal, bool vertical)
        {
            Undo.SetCurrentGroupName("Border" + DateTime.Now.ToFileTime());
            int group = Undo.GetCurrentGroup();

            foreach (var obj in _objects)
            {
                Vector2 pivotOffset = GetPivotOffset(obj);
                SnapPoint(obj, pivotOffset, horizontal, vertical);
            }

            Undo.CollapseUndoOperations(group);
        }

        private void SnapPoint(RectTransform obj, Vector2 pivotOffset, bool horizontal, bool vertical)
        {
            Undo.RecordObject(obj.transform, "Snap Anchors Point");

            Vector2 pivot = obj.pivot + pivotOffset;

            RectTransform parentTransform = obj.parent as RectTransform;
            Rect parent = (parentTransform != null) ? parentTransform.ToScreenRect(true) : new Rect(0, 0, Screen.width, Screen.height);

            Rect rect = obj.ToScreenRect(true);


            Vector2 pos = new Vector2(pivot.x * rect.width, pivot.y * rect.height);
            pos += rect.position;
            pos -= parent.position;
            pos.x /= parent.width;
            pos.y /= parent.height;

            Vector2 diff = obj.anchoredPosition + new Vector2(pivotOffset.x * rect.width, pivotOffset.y * rect.height);

            if (horizontal && vertical)
            {
                obj.anchorMin = pos;
                obj.anchorMax = pos;
                obj.sizeDelta = rect.size;
                obj.anchoredPosition -= diff;
            }
            else if (horizontal)
            {
                obj.anchorMin = new Vector2(pos.x, obj.anchorMin.y);
                obj.anchorMax = new Vector2(pos.x, obj.anchorMax.y);
                obj.sizeDelta = new Vector2(rect.size.x, obj.sizeDelta.y);
                obj.anchoredPosition -= new Vector2(diff.x, 0);
            }
            else if (vertical)
            {
                obj.anchorMin = new Vector2(obj.anchorMin.x, pos.y);
                obj.anchorMax = new Vector2(obj.anchorMax.x, pos.y);
                obj.sizeDelta = new Vector2(obj.sizeDelta.x, rect.size.y);
                obj.anchoredPosition -= new Vector2(0, diff.y);
            }
        }

        private static float CalculateMinAnchor(bool calculate, float innerPos, float outerPos, float outerSize, float fallback)
        {
            return (calculate) ? (innerPos - outerPos) / outerSize : fallback;
        }

        private static float CalculateMaxAnchor(bool calculate, float innerPos, float outerPos, float outerSize, float fallback)
        {
            return (calculate) ? 1 - ((outerPos - innerPos) / outerSize) : fallback;
        }

        private static float CalculateSize(float size, bool front, bool back)
        {
            if (front && back)
            {
                return 0;
            }
            else if (front || back)
            {
                return 0.5f * size;
            }
            else
            {
                return size;
            }
        }

        private static float CalculateAncherPos(float pivot, float size, bool front, bool back, float fallback)
        {
            if (!(front) && !(back))
                return fallback;

            if (size == 0)
                return 0;

            return 0.5f * size - pivot * size;
        }
    }
}