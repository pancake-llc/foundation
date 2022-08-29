using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pancake.Database
{
    public class Historizer : VisualElement
    {
        private List<Entity> _history;
        private readonly VisualElement _breadcrumbBar;
        private List<Button> _buttons; // todo highlight if current selection is inside history bar.

        private const int MAX_HISTORY_ITEMS = 7;
        private const int MAX_NAME_LENGTH = 16;

        public Historizer()
        {
            _breadcrumbBar = new ToolbarBreadcrumbs();
            _buttons = new List<Button>();
            Add(_breadcrumbBar);

            style.flexGrow = 1;
            style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);

            Dashboard.onCurrentEntityChanged -= AddAndHistorize;
            Dashboard.onCurrentEntityChanged += AddAndHistorize;

            RestoreHistory();
            BuildBreadcrumbs();
        }

        /// <summary>
        /// Store all of the current history object GUIDs into Settings.
        /// </summary>
        private void AddAndHistorize()
        {
            if (_history == null) _history = new List<Entity>();
            if (_history.Count > 0 && _history.Last() == Dashboard.CurrentSelectedEntity) return;
            if (_history.Contains(Dashboard.CurrentSelectedEntity)) return;

            _history.Add(Dashboard.CurrentSelectedEntity);
            if (_history.Count > MAX_HISTORY_ITEMS)
            {
                _history.Remove(_history.First());
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _history.Count; i++)
            {
                Entity assetFile = _history[i];
                sb.Append(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(assetFile)) + "|");
            }

            EditorSettings.Set(EditorSettings.ESettingKey.BreadcrumbBarGuids, sb.ToString());
            BuildBreadcrumbs();
        }

        /// <summary>
        /// Read Settings to obtain all history GUIDs and create the History List of objects from them.
        /// </summary>
        private void RestoreHistory()
        {
            _history = new List<Entity>();
            string guidblob = EditorSettings.Get(EditorSettings.ESettingKey.BreadcrumbBarGuids);
            if (string.IsNullOrEmpty(guidblob)) return;

            string[] split = guidblob.Split('|');
            foreach (string guid in split)
            {
                if (guid == string.Empty || guid.Contains('|')) continue;
                _history.Add(AssetDatabase.LoadAssetAtPath<Entity>(AssetDatabase.GUIDToAssetPath(guid)));
            }
        }

        private void BuildBreadcrumbs()
        {
            _breadcrumbBar.Clear();
            _buttons = new List<Button>();
            StyleBackground crumbFirst = (Texture2D) EditorGUIUtility.IconContent("breadcrump left").image;
            StyleBackground crumb = (Texture2D) EditorGUIUtility.IconContent("breadcrump mid").image;

            if (_history == null || _history.Count == 0) return;
            for (int i = 0; i < _history.Count; i++)
            {
                if (_history[i] == null) continue;
                int index = i;

                string title = "      blank";
                if (_history[i].Title.Length > 0)
                {
                    title = _history[i].Title.Length > MAX_NAME_LENGTH
                        ? "      " + _history[i].Title.Substring(0, MAX_NAME_LENGTH - 2) + "..."
                        : "      " + _history[i].Title;
                }

                Button btn = new Button(() => GoToHistoryIndex(index));
                btn.style.paddingBottom = 0;
                btn.style.paddingLeft = 0;
                btn.style.paddingRight = 0;
                btn.style.paddingTop = 0;
                btn.style.borderBottomLeftRadius = 0;
                btn.style.borderBottomRightRadius = 0;
                btn.style.borderTopRightRadius = 0;
                btn.style.borderTopLeftRadius = 0;
                btn.style.borderBottomWidth = 0;
                btn.style.borderLeftWidth = 0;
                btn.style.borderRightWidth = 0;
                btn.style.borderTopWidth = 0;
                btn.style.unitySliceLeft = 12;
                btn.style.unitySliceRight = 15;
                btn.style.marginTop = 0;
                btn.style.marginBottom = 0;
                btn.style.marginLeft = -15;
                btn.style.marginRight = 0;
                btn.style.flexGrow = 1;
                btn.style.flexShrink = 1;
                btn.style.width = 500;
                btn.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft);
                btn.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
                btn.style.unityBackgroundScaleMode = new StyleEnum<ScaleMode>(ScaleMode.StretchToFill);
                btn.style.backgroundColor = new StyleColor(Color.clear);
                btn.style.backgroundImage = i > 0 ? crumb : crumbFirst;

                btn.text = title;

                _buttons.Add(btn);
                _breadcrumbBar.Add(btn);
            }
        }

        public void GoToHistoryIndex(int index)
        {
            if (_history[index] == null) return;
            Dashboard.InspectAssetRemote(_history[index], _history[index].GetType());
        }
    }
}