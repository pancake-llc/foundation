using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Pancake.ApexEditor.Window.Searchable
{
    public sealed class SearchableWindow : EditorWindow
    {
        private const string SEARCH_FIELD_CONTROL_NAME = "Searchable Menu Window Search Field";

        private List<SearchItem> items = new List<SearchItem>();
        private SearchField searchField;
        private string searchText;
        private Vector2 scrollPosition;
        private float maxHeight;
        private bool flexableHeight;

        private void Awake()
        {
            maxHeight = 250;
            flexableHeight = true;
        }

        private void OnEnable()
        {
            searchField = new SearchField();
            searchText = string.Empty;
            scrollPosition = Vector2.zero;
        }

        private void OnGUI()
        {
            OnBackgroundGUI();
            OnSearchFieldGUI();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            OnItemsGUI();
            GUILayout.EndScrollView();

            if (flexableHeight)
            {
                Rect flexPosition = position;
                flexPosition.height = GetHeight();
                position = flexPosition;
            }

            Repaint();
        }

        private void OnBackgroundGUI()
        {
            Rect boxPosition = GUILayoutUtility.GetRect(0, 0);
            boxPosition.y -= 1;
            boxPosition.width = position.width;
            boxPosition.height = position.height;
            GUI.Box(boxPosition, GUIContent.none, ApexStyles.BoxHeader);
        }

        private void OnSearchFieldGUI()
        {
            Rect toolbarPosition = GUILayoutUtility.GetRect(0, 22);
            toolbarPosition.x += 5;
            toolbarPosition.y += 3;
            toolbarPosition.width -= 18.5f;

            GUI.Box(new Rect(toolbarPosition.x - 5, toolbarPosition.y - 4, toolbarPosition.width, 23), GUIContent.none, ApexStyles.BoxHeader);

            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName(SEARCH_FIELD_CONTROL_NAME);
            searchText = searchField.OnToolbarGUI(toolbarPosition, searchText);
            if (EditorGUI.EndChangeCheck())
            {
                OnSearchFieldChangedCallback?.Invoke(searchText);
            }


            if (GUI.Button(new Rect(toolbarPosition.xMax - 13, toolbarPosition.y - 4, 26, 23),
                    EditorGUIUtility.IconContent("winbtn_win_close"),
                    ApexStyles.BoxCenteredButton))
            {
                searchText = string.Empty;
                GUI.FocusControl(SEARCH_FIELD_CONTROL_NAME);
            }
        }

        private void OnItemsGUI()
        {
            for (int i = 0; i < items.Count; i++)
            {
                SearchItem item = items[i];
                string itemLabel = item.GetLabel().text.ToLower();
                if (!itemLabel.Contains(searchText.ToLower()))
                {
                    continue;
                }

                Rect position = GUILayoutUtility.GetRect(0, item.GetHeight());
                position.width -= 0.5f;
                position.height += 1;
                item.OnGUI(position);
            }
        }

        public void AddItem(SearchItem item)
        {
            item.OnClickCallback += Close;
            items.Add(item);
        }

        public void RemoveItem(SearchItem item)
        {
            item.OnClickCallback -= Close;
            items.Remove(item);
        }

        public void RemoveItem(int index)
        {
            items[index].OnClickCallback -= Close;
            items.RemoveAt(index);
        }

        public void RemoveItem(string label)
        {
            int? index = null;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].GetLabel().text == label)
                {
                    index = i;
                    break;
                }
            }

            if (index != null)
            {
                RemoveItem(index.Value);
            }
        }

        public void ClearItems()
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].OnClickCallback -= Close;
            }

            items.Clear();
        }

        public void Sort(Comparison<SearchItem> comparison) { items?.Sort(comparison); }

        public void Sort() { items?.Sort(AlphabeticalComparison); }

        public void ShowAsDropDown(Rect buttonRect)
        {
            Vector2 screenPoint = GUIUtility.GUIToScreenPoint(buttonRect.position);
            Rect windowPosition = buttonRect;
            windowPosition.position = screenPoint;
            position = windowPosition;
            ShowAsDropDown(windowPosition, new Vector2(windowPosition.width, GetHeight()));
        }

        public float GetItemsHeight()
        {
            float total = 1;
            for (int i = 0; i < items.Count; i++)
            {
                total += items[i].GetHeight();
            }

            return total;
        }

        public float GetHeight() { return Mathf.Clamp(GetItemsHeight() + 22, 42, maxHeight); }

        #region [Static Methods]

        public static SearchableWindow Create() { return CreateInstance<SearchableWindow>(); }

        #endregion

        #region [Comparison Implemetation]

        public Comparison<SearchItem> AlphabeticalComparison { get { return new Comparison<SearchItem>((s1, s2) => s1.GetLabel().text.CompareTo(s2.GetLabel().text)); } }

        #endregion

        #region [Event Callback Functions]

        /// <summary>
        /// Called when editing search field.
        /// </summary>
        public event Action<string> OnSearchFieldChangedCallback;

        #endregion

        #region [Getter / Setter]

        public float GetMaxHeight() { return maxHeight; }

        public void SetMaxHeight(float value) { maxHeight = value; }

        public bool FlexableHeight() { return flexableHeight; }

        public void FlexableHeight(bool value) { flexableHeight = value; }

        #endregion
    }
}