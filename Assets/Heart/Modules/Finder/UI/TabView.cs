using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Finder
{
    public class DrawCallback
    {
        public Action afterDraw;
        public Action beforeDraw;
    }


    public class TabView
    {
        public DrawCallback callback;
        public bool canDeselectAll; // can there be no active tabs
        public int current;
        public GUIContent[] labels;
        public Action onTabChange;
        public IWindow window;

        public TabView(IWindow w, bool canDeselectAll)
        {
            window = w;
            this.canDeselectAll = canDeselectAll;
        }

        public bool DrawLayout()
        {
            var result = false;

            GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(20f));
            {
                if (callback != null && callback.beforeDraw != null) callback.beforeDraw();

                for (var i = 0; i < labels.Length; i++)
                {
                    bool isActive = i == current;

                    var lb = labels[i];
                    bool clicked = lb.image != null
                        ? GUI2.ToolbarToggle(ref isActive, lb.image, Vector2.zero, lb.tooltip)
                        : GUI2.Toggle(ref isActive, lb, EditorStyles.toolbarButton);

                    if (!clicked) continue;

                    current = !isActive && canDeselectAll ? -1 : i;
                    result = true;

                    onTabChange?.Invoke();
                    if (window == null) continue;
                    window.OnSelectionChange(); // force refresh tabs
                    window.WillRepaint = true;
                }

                if (callback != null && callback.afterDraw != null) callback.afterDraw();
            }
            GUILayout.EndHorizontal();

            return result;
        }

        public static TabView FromEnum(Type enumType, IWindow w, bool canDeselectAll = false)
        {
            var values = Enum.GetValues(enumType);
            var labels = new List<GUIContent>();

            foreach (object item in values)
            {
                labels.Add(MyGUIContent.FromString(item.ToString()));
            }

            return new TabView(w, canDeselectAll) {current = 0, labels = labels.ToArray()};
        }

        public static GUIContent GetGUIContent(object tex)
        {
            if (tex is GUIContent content) return content;
            if (tex is Texture texture) return MyGUIContent.FromTexture(texture);
            if (tex is string s) return MyGUIContent.FromString(s);
            return GUIContent.none;
        }

        public static TabView Create(IWindow w, bool canDeselectAll = false, params object[] titles)
        {
            var labels = new List<GUIContent>();
            foreach (object item in titles)
            {
                labels.Add(GetGUIContent(item));
            }

            return new TabView(w, canDeselectAll) {current = 0, labels = labels.ToArray()};
        }
    }
}