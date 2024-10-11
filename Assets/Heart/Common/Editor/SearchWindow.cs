using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace PancakeEditor.Common
{
    public class SearchWindow : ScriptableObject, ISearchWindowProvider, ISearchWindow, ISearchWindowEntry
    {
        [Flags]
        public enum SortType
        {
            None = 0,
            Directory = 1,
            Alphabet = 2
        }

        private struct Entry
        {
            public readonly GUIContent content;
            public readonly object data;
            public readonly Action<object> onSelect;

            public Entry(GUIContent content, object data, Action<object> onSelect)
            {
                this.content = content;
                this.data = data;
                this.onSelect = onSelect;
            }
        }

        private string _title = string.Empty;
        private Texture2D _emptyIcon;
        private SortType _sortType = SortType.Directory | SortType.Alphabet;
        private readonly List<Entry> _entries = new();

        /// <summary>
        /// Generates data to populate the search window.
        /// </summary>
        /// <param name="context">Contextual data initially passed the window when first created.</param>
        /// <returns>Returns the list of SearchTreeEntry objects displayed in the search window.</returns>
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            if (_sortType != SortType.None)
            {
                _entries.Sort(SortEntriesByGroup);
            }

            var treeEntries = new List<SearchTreeEntry> {new SearchTreeGroupEntry(new GUIContent(_title), 0)};

            var groups = new List<string>();
            for (var i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];

                var group = string.Empty;
                string[] paths = entry.content.text.Split('/');
                int length = paths.Length - 1;
                for (var j = 0; j < length; j++)
                {
                    string path = paths[j];

                    group += path;
                    if (!groups.Contains(group))
                    {
                        treeEntries.Add(new SearchTreeGroupEntry(new GUIContent(path), j + 1));
                        groups.Add(group);
                    }

                    group += "/";
                }

                entry.content.text = paths[length];
                var searchTreeEntry = new SearchTreeEntry(entry.content);
                searchTreeEntry.userData = i;
                searchTreeEntry.level = paths.Length;
                treeEntries.Add(searchTreeEntry);
            }

            return treeEntries;
        }

        /// <summary>
        /// Selects an entry in the search tree list.
        /// </summary>
        /// <param name="searchTreeEntry">The selected entry.</param>
        /// <param name="context">Contextual data to pass to the search window when it is first created.</param>
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var entry = _entries[(int) searchTreeEntry.userData];
            if (entry.onSelect != null)
            {
                entry.onSelect.Invoke(entry.data);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add new entry.
        /// </summary>
        /// <param name="content">The text and icon of the search entry.</param>
        /// <param name="data">A user specified object for attaching application specific data to a search tree entry.</param>
        /// <param name="onSelect">Action with data argument, which called after entry is selected.</param>
        public void AddEntry(GUIContent content, object data, Action<object> onSelect) { _entries.Add(new Entry(content, data, onSelect)); }

        /// <summary>
        /// Add new entry.
        /// </summary>
        /// <param name="content">The text and icon of the search entry.</param>
        /// <param name="onSelect">Action which called after entry is selected.</param>
        public void AddEntry(GUIContent content, Action onSelect) { _entries.Add(new Entry(content, null, _ => onSelect?.Invoke())); }

        /// <summary>
        /// Add new entry.
        /// </summary>
        /// <param name="name">The name of the search entry.</param>
        /// <param name="data">A user specified object for attaching application specific data to a search tree entry.</param>
        /// <param name="onSelect">Action with data argument, which called after entry is selected.</param>
        public void AddEntry(string name, object data, Action<object> onSelect) { AddEntry(new GUIContent(name), data, onSelect); }

        /// <summary>
        /// Add new entry.
        /// </summary>
        /// <param name="name">The name of the search entry.</param>
        /// <param name="onSelect">Action which called after entry is selected.</param>
        public void AddEntry(string name, Action onSelect) { AddEntry(new GUIContent(name), null, _ => onSelect?.Invoke()); }

        /// <summary>
        /// Add new indented entity.
        /// </summary>
        /// <param name="name">Name of none label.</param>
        /// <param name="onSelect">Action which called after entry is selected.</param>
        public void AddEntityIndented(string name, Action onSelect)
        {
            var content = new GUIContent(name, GetEmptyIcon());
            AddEntry(content, onSelect);
        }

        /// <summary>
        /// Add new indented entity.
        /// </summary>
        /// <param name="name">Name of none label.</param>
        /// <param name="data">A user specified object for attaching application specific data to a search tree entry.</param>
        /// <param name="onSelect">Action with data argument, which called after entry is selected.</param>
        public void AddEntityIndented(string name, object data, Action<object> onSelect)
        {
            var content = new GUIContent(name, GetEmptyIcon());
            AddEntry(content, data, onSelect);
        }


        /// <summary>
        /// Open search window.
        /// </summary>
        /// <param name="position">Window position in screen space.</param>
        /// <param name="width">Requested width of the window. Set to 0.0f to use the default width.</param>
        /// <param name="height">Requested height of the window. Set to 0.0f to use the default height.</param>
        public void Open(Vector2 position, float width = 0, float height = 0)
        {
            UnityEditor.Experimental.GraphView.SearchWindow.Open(new SearchWindowContext(position, width, height), this);
        }

        /// <summary>
        /// Open search window in mouse position.
        /// </summary>
        /// <param name="width">Requested width of the window. Set to 0.0f to use the default width.</param>
        /// <param name="height">Requested height of the window. Set to 0.0f to use the default height.</param>
        public void Open(float width = 0, float height = 0)
        {
            var position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            Open(position, width, height);
        }

        /// <summary>
        /// Open search window in button position.
        /// </summary>
        /// <param name="buttonRect">Rectangle of GUI button.</param>
        /// <param name="width">Requested width of the window. Set to 0.0f to use the button width.</param>
        /// <param name="height">Requested height of the window. Set to 0.0f to use the default height.</param>
        public void Open(Rect buttonRect, float width = 0, float height = 0)
        {
            var screenRect = GUIUtility.GUIToScreenRect(buttonRect);

            var position = screenRect.position;
            position.x += screenRect.width / 2;
            position.y += screenRect.height + 15;

            width = Mathf.Max(0, width);
            width = width != 0 ? width : screenRect.width;

            Open(position, width != 0 ? width : screenRect.width, height);
        }

        /// <summary>
        /// Sort entries by paths.
        /// </summary>
        /// <param name="lhs">Left hand side entry.</param>
        /// <param name="rhs">Right hand side entry.</param>
        private int SortEntriesByGroup(Entry lhs, Entry rhs)
        {
            string[] lhsPaths = lhs.content.text.Split('/');
            string[] rhsPaths = rhs.content.text.Split('/');

            int lhsLength = lhsPaths.Length;
            int rhsLength = rhsPaths.Length;
            int minLength = Mathf.Min(lhsLength, rhsLength);

            for (var i = 0; i < minLength; i++)
            {
                if ((_sortType & SortType.Directory) != 0)
                {
                    if (minLength - 1 == i)
                    {
                        int compareDepth = rhsLength.CompareTo(lhsLength);
                        if (compareDepth != 0) return compareDepth;
                    }
                }

                if ((_sortType & SortType.Alphabet) != 0)
                {
                    int compareText = lhsPaths[i].CompareTo(rhsPaths[i]);
                    if (compareText != 0) return compareText;
                }
            }

            return 0;
        }

        /// <summary>
        /// Get empty icon.
        /// </summary>
        private Texture GetEmptyIcon()
        {
            if (_emptyIcon == null)
            {
                _emptyIcon = new Texture2D(1, 1);
                _emptyIcon.SetPixel(0, 0, Color.clear);
                _emptyIcon.Apply();
            }

            return _emptyIcon;
        }

        #region [Static Methods]

        /// <summary>
        /// Create ExLib search window.
        /// </summary>
        /// <param name="title">Window title.</param>
        /// <returns>SearchWindow instance.</returns>
        public static SearchWindow Create(string title)
        {
            var window = CreateInstance<SearchWindow>();
            window.SetTitle(title);
            return window;
        }

        #endregion

        #region [Getter / Setter]

        public string GetTitle() { return _title; }

        public void SetTitle(string value) { _title = value; }

        public SortType GetSortType() { return _sortType; }

        public void SetSortType(SortType value) { _sortType = value; }

        #endregion
    }
}