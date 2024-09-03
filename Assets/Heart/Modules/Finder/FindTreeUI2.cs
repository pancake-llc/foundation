using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Finder
{
    public class FindTreeUI2
    {
        internal readonly Drawer drawer;
        public float itemPaddingRight = 0f;
        private Vector2 _position;
        private TreeItem _rootItem;
        internal Rect visibleRect;

        public FindTreeUI2(Drawer drawer) { this.drawer = drawer; }

        public void Reset(params string[] root)
        {
            _position = Vector2.zero;

            _rootItem = new TreeItem
            {
                tree = this,
                id = "$root",
                height = 0,
                depth = -1,
                isOpen = true,
                highlight = false,
                childCount = root.Length
            };

            _rootItem.RefreshChildren(root);
            _rootItem.DeepOpen();
        }

        public void Draw(Rect rect)
        {
            if (rect.width > 0) visibleRect = rect;

            var contentRect = new Rect(0f, 0f, 1f, _rootItem.childrenHeight);
            bool noScroll = contentRect.height < visibleRect.height;
            if (noScroll) _position = Vector2.zero;

            var minY = (int) _position.y;
            var maxY = (int) (_position.y + visibleRect.height);
            contentRect.x -= FinderWindowBase.TreeIndent;

            TreeItem.drawCall = 0;
            TreeItem.drawRender = 0;
            _position = GUI.BeginScrollView(visibleRect, _position, contentRect);
            {
                var r = new Rect(0, 0, rect.width - (noScroll ? 4f : 18f) - itemPaddingRight, 16f);
                var index = 0;
                _rootItem.Draw(ref index, ref r, minY, maxY);
            }

            GUI.EndScrollView();
        }

        public void DrawLayout()
        {
            var evtType = Event.current.type;
            var r = GUILayoutUtility.GetRect(1f, Screen.width, 16f, Screen.height);

            if (evtType != EventType.Layout) visibleRect = r;

            Draw(visibleRect);
        }

        public bool NoScroll() { return _rootItem.childrenHeight < visibleRect.height; }

        // ------------------------ DELEGATE --------------

        public class Drawer
        {
            public virtual int GetHeight(string id) { return 16; }

            public virtual int GetChildCount(string id) { return 0; }

            public virtual string[] GetChildren(string id) { return null; }

            public virtual void Draw(Rect r, TreeItem item) { GUI.Label(r, MyGUIContent.From(item.id)); }
        }

        public class GroupDrawer : Drawer
        {
            public readonly Action<Rect, string, int> drawGroup;
            public readonly Action<Rect, string> drawItem;
            private Dictionary<string, List<string>> _groupDict;

            public bool hideGroupIfPossible;
            internal FindTreeUI2 tree;

            public GroupDrawer(Action<Rect, string, int> drawGroup, Action<Rect, string> drawItem)
            {
                this.drawItem = drawItem;
                this.drawGroup = drawGroup;
            }

            public bool HasChildren => tree != null && tree._rootItem.childCount > 0;

            public bool HasValidTree => _groupDict != null && tree != null;

            // ----------------- TREE WRAPPER ------------------
            public bool TreeNoScroll() { return tree.NoScroll(); }


            public void Reset<T>(List<T> items, Func<T, string> idFunc, Func<T, string> groupFunc, Action<List<string>> customGroupSort = null)
            {
                _groupDict = new Dictionary<string, List<string>>();

                for (var i = 0; i < items.Count; i++)
                {
                    string groupName = groupFunc(items[i]);
                    if (groupName == null) continue; // do not exclude groupName string.Empty

                    string itemId = idFunc(items[i]);
                    if (string.IsNullOrEmpty(itemId)) continue; // ignore items without id

                    if (!_groupDict.TryGetValue(groupName, out var list))
                    {
                        list = new List<string>();
                        _groupDict.Add(groupName, list);
                    }

                    list.Add(itemId);
                }

                if (tree == null) tree = new FindTreeUI2(this);

                var groups = _groupDict.Keys.ToList();

                if (hideGroupIfPossible && groups.Count == 1) //single group : Flat list
                {
                    var v = _groupDict[groups[0]];
                    tree.Reset(v.ToArray());
                    _groupDict.Clear();
                }
                else
                {
                    //multiple groups
                    if (customGroupSort != null) customGroupSort(groups);
                    else groups.Sort();

                    tree.Reset(groups.ToArray());
                }
            }

            public void Draw(Rect r) { tree?.Draw(r); }

            public void DrawLayout() { tree?.DrawLayout(); }

            // ----------------- DRAWER WRAPPER ------------------

            public override int GetChildCount(string id)
            {
                if (string.IsNullOrEmpty(id)) return 0;

                return _groupDict.TryGetValue(id, out var group) ? group.Count : 0;
            }

            public override string[] GetChildren(string id) { return _groupDict.TryGetValue(id, out var group) ? group.ToArray() : null; }

            public override void Draw(Rect r, TreeItem item)
            {
                if (_groupDict.TryGetValue(item.id, out _))
                {
                    drawGroup(r, item.id, item.childCount);
                    return;
                }

                drawItem(r, item.id);
            }
        }

        // ------------------------ TreeItem2 --------------

        public class TreeItem
        {
            public static int drawCall;
            public static int drawRender;

            internal bool isOpen;

            public int childCount;
            public List<TreeItem> children;
            public int childrenHeight;
            public int depth; // item depth

            public int height;
            public bool highlight;

            public string id; // item id

            internal TreeItem parent;

            internal FindTreeUI2 tree;

            public bool IsOpen
            {
                get => isOpen;
                set
                {
                    if (isOpen == value || childCount == 0) return;

                    isOpen = value;

                    if (isOpen)
                    {
                        if (children == null) RefreshChildren(tree.drawer.GetChildren(id));

                        //Update height for all parents
                        var p = parent;
                        while (p != null)
                        {
                            p.childrenHeight += childrenHeight;
                            p = p.parent;
                        }
                    }
                    else
                    {
                        //Update height for all parents
                        var p = parent;
                        while (p != null)
                        {
                            p.childrenHeight -= childrenHeight;
                            p = p.parent;
                        }
                    }
                }
            }

            internal void DeepOpen()
            {
                IsOpen = true;
                if (children == null) return;

                for (var i = 0; i < children.Count; i++)
                {
                    children[i].DeepOpen();
                }
            }

            internal void Draw(ref int index, ref Rect rect, int minY, int maxY)
            {
                drawCall++;

                float min = rect.y;
                float max = rect.y + height;
                bool interMin = min >= minY && min <= maxY;
                bool interMax = max >= minY && max <= maxY;

                if (height > 0 && (interMin || interMax))
                {
                    drawRender++;
                    rect.height = height;

                    if (index % 2 == 1 && FinderWindowBase.AlternateRowColor)
                    {
                        var o = GUI.color;
                        GUI.color = FinderWindowBase.RowColor;

                        GUI.DrawTexture(new Rect(rect.x - FinderWindowBase.TreeIndent, rect.y, rect.width, rect.height), EditorGUIUtility.whiteTexture);
                        GUI.color = o;
                    }

                    float x = (depth + 1) * 16f;
                    tree.drawer.Draw(new Rect(x, rect.y, rect.width - x, rect.height), this);

                    if (childCount > 0) IsOpen = GUI.Toggle(new Rect(rect.x + x - 16f, rect.y, 16f, 16f), IsOpen, GUIContent.none, EditorStyles.foldout);

                    index++;
                    rect.y += height;
                }
                else rect.y += height;

                if (isOpen && rect.y <= maxY) //draw children
                {
                    for (var i = 0; i < children.Count; i++)
                    {
                        children[i].Draw(ref index, ref rect, minY, maxY);
                        if (rect.y > maxY) break;
                    }
                }
            }

            internal void RefreshChildren(string[] childrenIDs)
            {
                childCount = childrenIDs.Length;
                childrenHeight = 0;
                children = new List<TreeItem>();

                for (var i = 0; i < childCount; i++)
                {
                    string itemId = childrenIDs[i];

                    var item = new TreeItem
                    {
                        tree = tree,
                        parent = this,
                        id = itemId,
                        depth = depth + 1,
                        highlight = false,
                        height = tree.drawer.GetHeight(itemId),
                        childCount = tree.drawer.GetChildCount(itemId)
                    };

                    childrenHeight += item.height;
                    children.Add(item);
                }
            }
        }
    }
}