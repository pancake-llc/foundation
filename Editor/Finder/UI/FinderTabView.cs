using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public class DrawCallback
    {
        public Action beforeDraw;
        public Action afterDraw;
    }


    public class FinderTabView
    {
        public int current;
        public GUIContent[] labels;
        public IWindow window;
        public Action onTabChange;
        public DrawCallback callback;
        public bool canDeselectAll; // can there be no active tabs

        public FinderTabView(IWindow w, bool canDeselectAll)
        {
            window = w;
            this.canDeselectAll = canDeselectAll;
        }

        public bool DrawLayout()
        {
            bool result = false;

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (callback != null && callback.beforeDraw != null) callback.beforeDraw();

                for (var i = 0; i < labels.Length; i++)
                {
                    var isActive = (i == current);

                    var lb = labels[i];
                    var clicked = (lb.image != null)
                        ? GUI2.ToolbarToggle(ref isActive, lb.image, Vector2.zero, lb.tooltip)
                        : GUI2.Toggle(ref isActive, lb, EditorStyles.toolbarButton);

                    if (!clicked) continue;

                    current = (!isActive && canDeselectAll) ? -1 : i;
                    result = true;

                    if (onTabChange != null) onTabChange();
                    if (window == null) continue;
                    window.OnSelectionChange(); // force refresh tabs
                    window.WillRepaint = true;
                }

                if (callback != null && callback.afterDraw != null) callback.afterDraw();
            }
            GUILayout.EndHorizontal();

            return result;
        }

        public static FinderTabView FromEnum(Type enumType, IWindow w, bool canDeselectAll = false)
        {
            var values = Enum.GetValues(enumType);
            var labels = new List<GUIContent>();

            foreach (var item in values)
            {
                labels.Add(new GUIContent(item.ToString()));
            }

            return new FinderTabView(w, canDeselectAll) {current = 0, labels = labels.ToArray()};
        }

        public static GUIContent GetGUIContent(object tex)
        {
            if (tex is GUIContent) return (GUIContent) tex;
            if (tex is Texture) return new GUIContent((Texture) tex);
            if (tex is string) return new GUIContent((string) tex);
            return GUIContent.none;
        }

        public static FinderTabView Create(IWindow w, bool canDeselectAll = false, params object[] titles)
        {
            var labels = new List<GUIContent>();
            foreach (var item in titles)
            {
                labels.Add(GetGUIContent(item));
            }

            return new FinderTabView(w, canDeselectAll) {current = 0, labels = labels.ToArray()};
        }
    }
}