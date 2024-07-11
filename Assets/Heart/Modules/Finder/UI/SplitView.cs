using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Finder
{
    public class SplitView
    {
        public bool isHorz;
        public List<Info> splits = new();
        private const float SPLIT_SIZE = 2f;
        private Rect _rect;
        private int _visibleCount;
        private bool _dirty;
        private int _resizeIndex = -1;

        private readonly IWindow _window;

        public SplitView(IWindow w) { _window = w; }

        public bool IsVisible => _visibleCount > 0;

        public void CalculateWeight()
        {
            _visibleCount = 0;
            var totalWeight = 0f;

            for (var i = 0; i < splits.Count; i++)
            {
                var info = splits[i];
                if (!info.visible) continue;

                info.stIndex = _visibleCount;
                totalWeight += info.weight;

                _visibleCount++;
            }

            if (_visibleCount == 0 || totalWeight == 0) return;

            for (var i = 0; i < splits.Count; i++)
            {
                var info = splits[i];
                if (!info.visible) continue;
                info.normWeight = info.weight / totalWeight;
            }
        }

        public void Draw(Rect rect)
        {
            if (rect.width > 0 || rect.height > 0) _rect = rect;

            if (_dirty)
            {
                _dirty = false;
                CalculateWeight();
            }

            float sz = (_visibleCount - 1) * SPLIT_SIZE;
            float dx = _rect.x;
            float dy = _rect.y;

            for (var i = 0; i < splits.Count; i++)
            {
                var info = splits[i];
                if (!info.visible) continue;

                var rr = new Rect(dx, dy, isHorz ? (_rect.width - sz) * info.normWeight : _rect.width, isHorz ? _rect.height : (_rect.height - sz) * info.normWeight);

                if (rr.width > 0 && rr.height > 0) info.rect = rr;

                if (info.draw != null) info.DoDraw();

                if (info.stIndex < _visibleCount - 1) DrawSpliter(i, isHorz ? info.rect.xMax : info.rect.yMax);

                if (isHorz) dx += info.rect.width + SPLIT_SIZE;
                else dy += info.rect.height + SPLIT_SIZE;
            }
        }

        public void DrawLayout()
        {
            var rect = StartLayout(isHorz);
            {
                Draw(rect);
            }
            EndLayout(isHorz);
        }


        private void RefreshSpliterPos(int index, float px)
        {
            var sp1 = splits[index];
            var sp2 = splits[index + 1];

            var r1 = sp1.rect;
            var r2 = sp2.rect;

            float w1 = sp1.weight;
            float w2 = sp2.weight;
            float tt = w1 + w2;

            float dd = isHorz ? r2.xMax - r1.xMin : r2.yMax - r1.yMin - SPLIT_SIZE;
            float m = isHorz ? Event.current.mousePosition.x - r1.x : Event.current.mousePosition.y - r1.y;
            float pct = Mathf.Min(0.9f, Mathf.Max(0.1f, m / dd));

            sp1.weight = tt * pct;
            sp2.weight = tt * (1 - pct);

            _dirty = true;
            if (_window != null) _window.WillRepaint = true;
        }

        private void DrawSpliter(int index, float px)
        {
            var dRect = _rect;

            if (isHorz)
            {
                dRect.x = px + SPLIT_SIZE;
                dRect.width = SPLIT_SIZE;
            }
            else
            {
                dRect.y = px;
                dRect.height = SPLIT_SIZE;
            }

            if (Event.current.type == EventType.Repaint || Event.current.type == EventType.MouseMove) GUI2.Rect(dRect, Color.black, 0.4f);

            var dRect2 = GUI2.Padding(dRect, -2f, -2f);

            EditorGUIUtility.AddCursorRect(dRect2, isHorz ? MouseCursor.ResizeHorizontal : MouseCursor.ResizeVertical);
            if (Event.current.type == EventType.MouseDown && dRect2.Contains(Event.current.mousePosition))
            {
                _resizeIndex = index;
                RefreshSpliterPos(index, px);
            }

            if (_resizeIndex == index) RefreshSpliterPos(index, px);

            if (Event.current.type == EventType.MouseUp) _resizeIndex = -1;
        }

        private readonly GUILayoutOption[] _expandWh = {GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)};

        private Rect StartLayout(bool horz)
        {
            return horz
                ? EditorGUILayout.BeginHorizontal(_expandWh)
                : EditorGUILayout.BeginVertical(_expandWh);
        }

        private void EndLayout(bool horz)
        {
            if (horz) EditorGUILayout.EndHorizontal();
            else EditorGUILayout.EndVertical();
        }

        [Serializable]
        public class Info
        {
            public GUIContent title;

            public Rect rect;
            public float normWeight;
            public int stIndex;

            public bool visible = true;
            public float weight = 1f;
            public Action<Rect> draw;

            public void DoDraw()
            {
                var drawRect = rect;
                if (title != null)
                {
                    var titleRect = new Rect(rect.x, rect.y, rect.width, 16f);
                    GUI2.Rect(titleRect, Color.black, 0.2f);

                    titleRect.xMin += 4f;
                    GUI.Label(titleRect, title, EditorStyles.boldLabel);
                    drawRect.yMin += 16f;
                }

                draw(drawRect);
            }
        }
    }
}