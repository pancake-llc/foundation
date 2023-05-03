using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public class FinderSearchView
    {
        private bool _caseSensitive;
        private string _searchTerm = string.Empty;

        public static GUIStyle toolbarSearchField;
        public static GUIStyle toolbarSearchFieldCancelButton;
        public static GUIStyle toolbarSearchFieldCancelButtonEmpty;

        public static void InitSearchStyle()
        {
            toolbarSearchField = "ToolbarSeachTextFieldPopup";
            toolbarSearchFieldCancelButton = "ToolbarSeachCancelButton";
            toolbarSearchFieldCancelButtonEmpty = "ToolbarSeachCancelButtonEmpty";
        }

        public bool DrawLayout()
        {
            bool dirty = false;

            if (toolbarSearchField == null)
            {
                InitSearchStyle();
            }

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                bool v = GUILayout.Toggle(_caseSensitive, "Aa", EditorStyles.toolbarButton, GUILayout.Width(24f));
                if (v != _caseSensitive)
                {
                    _caseSensitive = v;
                    dirty = true;
                }

                GUILayout.Space(2f);
                string value = GUILayout.TextField(_searchTerm, toolbarSearchField, GUILayout.Width(140f));
                if (_searchTerm != value)
                {
                    _searchTerm = value;
                    dirty = true;
                }

                GUIStyle style = string.IsNullOrEmpty(_searchTerm) ? toolbarSearchFieldCancelButtonEmpty : toolbarSearchFieldCancelButton;
                if (GUILayout.Button("Cancel", style))
                {
                    _searchTerm = string.Empty;
                    dirty = true;
                }
            }
            GUILayout.EndHorizontal();

            return dirty;
        }
    }
}