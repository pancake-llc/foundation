using System;
using Pancake.Editor;
using UnityEditor;
using UnityEngine;

namespace Pancake.UI.Editor
{
    public class SmartParentWindow : EditorWindow
    {
        private bool _isFreeMovementEnabled;
        private RectTransform _selection;
        private RectTransformData _previousTransform;
        private GUIContent _snapAllContent, _snapVerticalContent, _snapHorizontalContent, _freeParentModeOnContent, _freeParentModeOffContent;

        [MenuItem("Tools/Pancake/UI/Smart Parent", false, 30)]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(SmartParentWindow), false, "Smart Parent");
            window.minSize = window.maxSize = new Vector2(200, 255);
        }

        private void OnEnable()
        {
            _isFreeMovementEnabled = false;

            _snapAllContent = new GUIContent(EditorResources.SnapAllPoint, "Trims size to children horizontally and vertically. Also snap Anchors to borders.");
            _snapVerticalContent = new GUIContent(EditorResources.SnapVertical, "Trims size to children vertically. Also snap Anchors to borders vertically.");
            _snapHorizontalContent = new GUIContent(EditorResources.SnapHorizontal, "Trims size to children horizontally. Also snap Anchors to borders horizontally.");
            _freeParentModeOnContent = new GUIContent(EditorResources.FreeParentModeOn, "When this mode is enabled children are not moved along with the parent.");
            _freeParentModeOffContent = new GUIContent(EditorResources.FreeParentModeOff, "When this mode is enabled children are not moved along with the parent.");

            Selection.selectionChanged += SelectionChanged;
            EditorApplication.update += UpdateTransforms;

            SelectionChanged();
        }

        private void OnDisable()
        {
            _isFreeMovementEnabled = false;

            Selection.selectionChanged -= SelectionChanged;
            EditorApplication.update -= UpdateTransforms;
        }


        private void OnGUI()
        {
            EditorGUILayout.Space();

            var go = Selection.activeObject as GameObject;
            bool canSelectParent = Selection.objects.Length == 1
                && go != null
                && go.transform as RectTransform != null
                && go.transform.parent != null;

            if (canSelectParent)
            {
                if (GUILayout.Button("Select Parent", EditorStyles.miniButton))
                {
                    Selection.activeObject = go.transform.parent.gameObject;
                }
            }
            else
            {
                GUILayout.Label("");
            }

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(_selection == null);

            if (_selection != null)
            {
                EditorGUILayout.LabelField(_selection.name, EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                DrawEmphasisedLabel("No valid object selected.");
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            #region snap all
            if (GUILayout.Button(_snapAllContent, GUILayout.Width(120), GUILayout.Height(120)))
            {
                SnapToChildren(true, true);
            }
            #endregion

            #region snap vertically

            if (GUILayout.Button(_snapVerticalContent, GUILayout.Width(60), GUILayout.Height(120)))
            {
                SnapToChildren(false, true);
            }

            #endregion

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            #region snap horizontally
            if (GUILayout.Button(_snapHorizontalContent, GUILayout.Width(120), GUILayout.Height(60)))
            {
                SnapToChildren(true, false);
            }
            #endregion

            EditorGUI.EndDisabledGroup();

            #region free parent mode

            bool prev = _isFreeMovementEnabled;
            var content = (prev) ? _freeParentModeOnContent : _freeParentModeOffContent;
            _isFreeMovementEnabled = GUILayout.Toggle(_isFreeMovementEnabled, content, "Button", GUILayout.Width(60), GUILayout.Height(60));

            bool turnedOn = !prev && _isFreeMovementEnabled;
            if (turnedOn && _selection != null)
            {
                _previousTransform = new RectTransformData(_selection);
            }

            #endregion

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (_isFreeMovementEnabled && _selection != null)
            {
                DrawEmphasisedLabel("Children are detached.");
            }
        }

        private static void DrawEmphasisedLabel(string text)
        {
            GUIStyle warn = GUI.skin.GetStyle("WarningOverlay");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            GUILayout.Label(text, warn);
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
        }

        private void SelectionChanged()
        {
            var sel = Selection.GetFiltered(typeof(RectTransform), SelectionMode.TopLevel);

            if (sel.Length != 1)
            {
                _selection = null;
                Repaint();
                return;
            }

            var rt = sel[0] as RectTransform;
            if(rt.childCount == 0 || rt.parent == null)
            {
                _selection = null;
                Repaint();
                return;
            }

            if (rt  == _selection)
                return;

            _selection = rt;
            _previousTransform = new RectTransformData(_selection);

            Repaint();
        }

        private void UpdateTransforms()
        {
            if (!_isFreeMovementEnabled || _selection == null)
                return;

            RectTransformData currentTransform = new RectTransformData(_selection);
            UpdateTransforms(_selection, currentTransform, _previousTransform);
            _previousTransform = currentTransform;
        }

        private void SnapToChildren(bool snapHorizontally, bool snapVertically)
        {
            if (_selection == null)
                return;

            if (_selection.childCount == 0)
                return;

            float xMin = float.MaxValue;
            float yMin = float.MaxValue;
            float xMax = float.MinValue;
            float yMax = float.MinValue;

            foreach (var child in _selection)
            {
                var rt = child as RectTransform;
                if (rt == null)
                    continue;

                Rect rect = rt.ToScreenRect(startAtBottom: true);

                xMin = Mathf.Min(xMin, rect.xMin);
                yMin = Mathf.Min(yMin, rect.yMin);
                xMax = Mathf.Max(xMax, rect.xMax);
                yMax = Mathf.Max(yMax, rect.yMax);
            }

            Rect childBounds = Rect.MinMaxRect(xMin, yMin, xMax, yMax);

            var parent = _selection.parent as RectTransform;
            Rect parentRect = (parent != null)
                ? parent.ToScreenRect(startAtBottom: true)
                : new Rect(0, 0, Screen.width, Screen.height);

            RectTransformData prev = new RectTransformData(_selection);
            RectTransformData cur = new RectTransformData().PullFromData(prev);


            if (snapHorizontally)
            {
                Snap(cur, 0, childBounds.xMin, childBounds.xMax, parentRect.xMin, parentRect.width);
            }

            if (snapVertically)
            {
                Snap(cur, 1, childBounds.yMin, childBounds.yMax, parentRect.yMin, parentRect.height);
            }

            #region do actual operation with undo

            Undo.RecordObject(_selection, "Snap To Children " + DateTime.Now.ToFileTime());
            int group = Undo.GetCurrentGroup();

            // push!
            cur.PushToTransform(_selection);

            foreach(Transform child in _selection)
            {
                Undo.RecordObject(child, "transform child");
            }

            if (!_isFreeMovementEnabled)
            {
                // update child positions
                UpdateTransforms(_selection, cur, prev);
            }

            Undo.CollapseUndoOperations(group);

            #endregion
        }

        private static void Snap(RectTransformData data, int axis, float min, float max, float parentMin, float parentSize)
        {
            data.anchorMin[axis] = (min - parentMin) / parentSize;
            data.anchorMax[axis] = (max - parentMin) / parentSize;

            data.anchoredPosition[axis] = 0;
            data.sizeDelta[axis] = 0;
        }

        private static void UpdateTransforms(RectTransform selection, RectTransformData currentTransform, RectTransformData previousTransform)
        {
            if (currentTransform == previousTransform)
                return;

            RectTransform parent = selection.parent as RectTransform;
            Rect parentRect = parent.rect;

            Rect cur = currentTransform.ToRect(parentRect, relativeSpace: true);
            bool isCurZero = Mathf.Approximately(cur.width, 0) || Mathf.Approximately(cur.height, 0);

            Rect prev = previousTransform.ToRect(parentRect, relativeSpace: true);
            bool isPrevZero = Mathf.Approximately(prev.width, 0) || Mathf.Approximately(prev.height, 0);

            if (isCurZero || isPrevZero)
            {
                return;
            }

            float scaleH = 1 / cur.width;
            float scaleV = 1 / cur.height;

            foreach (var child in selection)
            {
                RectTransform rt = child as RectTransform;
                if (rt == null)
                    continue;


                // prev to parent-parent-relative-space

                float xMin = prev.x + prev.width * rt.anchorMin.x;
                float xMax = prev.x + prev.width * rt.anchorMax.x;

                float yMin = prev.y + prev.height * rt.anchorMin.y;
                float yMax = prev.y + prev.height * rt.anchorMax.y;


                // parent-parent-relative-space to cur

                xMin = xMin * scaleH - cur.x * scaleH;
                xMax = xMax * scaleH - cur.x * scaleH;

                yMin = yMin * scaleV - cur.y * scaleV;
                yMax = yMax * scaleV - cur.y * scaleV;


                // assign calculated values

                rt.anchorMin = new Vector2(xMin, yMin);
                rt.anchorMax = new Vector2(xMax, yMax);
            }
        }
    }
}
