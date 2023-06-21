using System;
using System.Collections.Generic;
using System.Linq;
using Pancake.ExLibEditor;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Pancake.ScriptableEditor
{
    public class ScriptableWizardWindow : ScriptableWindowBase
    {
        protected override string HeaderTitle => "Scriptable";

        private Vector2 _scrollPosition = Vector2.zero;
        private Vector2 _itemScrollPosition = Vector2.zero;
        private List<ScriptableBase> _scriptableObjects;
        private ScriptableType _currentType = ScriptableType.All;
        private readonly float _tabWidth = 55f;
        private readonly float _buttonHeight = 40f;
        private Texture[] _icons;
        private readonly Color[] _colors = {Color.gray, Color.cyan, Color.green, Color.yellow, Color.gray};
        private string _searchText = "";
        private UnityEditor.Editor _editor;

        [SerializeField] private string currentFolderPath = "Assets";
        [SerializeField] private int selectedScriptableIndex;
        [SerializeField] private int tabIndex = -1;
        [SerializeField] private bool isInitialized;
        [SerializeField] private ScriptableBase scriptableBase;
        [SerializeField] private ScriptableBase previousScriptableBase;
        [SerializeField] private FavoriteData favoriteData;

        private List<ScriptableBase> Favorites => favoriteData.favorites;
        public const string PATH_KEY = "scriptable_wizard_path";
        public const string FAVORITE_KEY = "scriptable_wizard_favorites";

        [Serializable]
        private class FavoriteData
        {
            [FormerlySerializedAs("Favorites")] public List<ScriptableBase> favorites = new List<ScriptableBase>();
        }

        private enum ScriptableType
        {
            All,
            Variable,
            Event,
            List,
            Favorite
        }

        public new static void Show() => GetWindow<ScriptableWizardWindow>("Scriptable Wizard");

        [MenuItem("Tools/Pancake/Scriptable/Wizard")]
        private static void OpenScriptableWizard() => Show();

        protected override void OnEnable()
        {
            Init();
            if (isInitialized)
            {
                SelectTab(tabIndex);
                return;
            }

            SelectTab((int) _currentType, true); //default is 0
            isInitialized = true;
        }

        private void OnDisable()
        {
            var data = JsonUtility.ToJson(favoriteData, false);
            EditorPrefs.SetString(FAVORITE_KEY, data);
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        protected override void Init()
        {
            base.Init();
            LoadAssets();
            LoadSavedData();
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (scriptableBase == null) return;
            if (state == PlayModeStateChange.EnteredPlayMode) scriptableBase.repaintRequest += OnRepaintRequested;
            else if (state == PlayModeStateChange.EnteredEditMode) scriptableBase.repaintRequest -= OnRepaintRequested;
        }

        private void OnRepaintRequested() { Repaint(); }

        private void LoadAssets()
        {
            _icons = new Texture[5];
            _icons[0] = EditorResources.StarEmpty;
            _icons[1] = EditorResources.ScriptableVariable;
            _icons[2] = EditorResources.ScriptableEvent;
            _icons[3] = EditorResources.ScriptableList;
            _icons[4] = EditorResources.StarFull;
        }

        private void LoadSavedData()
        {
            currentFolderPath = EditorPrefs.GetString(PATH_KEY, "Assets");
            favoriteData = new FavoriteData();
            string favoriteDataJson = JsonUtility.ToJson(this.favoriteData, false);
            string data = EditorPrefs.GetString(FAVORITE_KEY, favoriteDataJson);
            JsonUtility.FromJsonOverwrite(data, this.favoriteData);
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            DrawFolder();
            GUILayout.Space(2);
            Uniform.DrawLine(2);
            GUILayout.Space(2);
            DrawTabs();
            EditorGUILayout.BeginHorizontal();
            DrawLeftPanel();
            DrawRightPanel();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawFolder()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Change Folder", GUILayout.MaxWidth(150)))
            {
                var path = EditorUtility.OpenFolderPanel("Select folder to set path.", currentFolderPath, "");

                if (path.Contains(Application.dataPath))
                    path = path.Substring(path.LastIndexOf("Assets", StringComparison.Ordinal));

                if (!AssetDatabase.IsValidFolder(path))
                    EditorUtility.DisplayDialog("Error: File Path Invalid", "Make sure the path is a valid folder in the project.", "Ok");
                else
                {
                    currentFolderPath = path;
                    EditorPrefs.SetString(PATH_KEY, currentFolderPath);
                    OnTabSelected(_currentType, true);
                }
            }

            EditorGUILayout.LabelField(currentFolderPath, GUI.skin.textField);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTabs()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            var width = _tabWidth * 5 + 4f; //offset to match

            var style = new GUIStyle(EditorStyles.toolbarButton);
            tabIndex = GUILayout.Toolbar(tabIndex, Enum.GetNames(typeof(ScriptableType)), style, GUILayout.MaxWidth(width));

            if (tabIndex != (int) _currentType)
                OnTabSelected((ScriptableType) tabIndex, true);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSearchBar()
        {
            var width = _tabWidth * 5 + 1f;
            EditorGUILayout.BeginHorizontal(GUILayout.Width(width));
            _searchText = EditorGUILayout.TextField(_searchText, EditorStyles.textField, GUILayout.MaxWidth(width));

            //Draw placeholder Text
            if (string.IsNullOrEmpty(_searchText))
            {
                var guiColor = GUI.color;
                GUI.color = Color.grey;
                var rect = GUILayoutUtility.GetLastRect();
                rect.x += 3f; //little offset due to the search bar hiding the first letter ^^.
                EditorGUI.LabelField(rect, "Search:");
                GUI.color = guiColor;
            }

            //Clear Button
            GUI.SetNextControlName("clearButton");
            if (GUILayout.Button("Clear"))
            {
                _searchText = "";
                //focus the button to defocus the TextField and so clear the text inside!
                GUI.FocusControl("clearButton");
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical();
            DrawSearchBar();
            var width = _tabWidth * 5f;

            var color = GUI.backgroundColor;
            GUI.backgroundColor = _colors[(int) _currentType];
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(width), GUILayout.ExpandHeight(true));

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandHeight(true));
            GUI.backgroundColor = color;

            DrawScriptableBases(_scriptableObjects);

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Create Type", GUILayout.MaxHeight(_buttonHeight)))
                PopupWindow.Show(new Rect(), new CreateTypePopUpWindow(position));

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private void DrawScriptableBases(List<ScriptableBase> scriptables)
        {
            if (scriptables is null)
                return;

            var count = scriptables.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                var scriptable = scriptables[i];
                if (scriptable == null) continue;

                //filter search
                if (scriptable.name.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) < 0) continue;

                EditorGUILayout.BeginHorizontal();

                var icon = _currentType == ScriptableType.All || _currentType == ScriptableType.Favorite ? GetIconFor(scriptable) : _icons[(int) _currentType];

                var style = new GUIStyle(GUIStyle.none) {contentOffset = new Vector2(0, 5)};
                GUILayout.Box(icon, style, GUILayout.Width(18), GUILayout.Height(18));

                var clicked = GUILayout.Toggle(selectedScriptableIndex == i, scriptable.name, Uniform.FoldoutButton, GUILayout.ExpandWidth(true));
                DrawFavorite(scriptable);
                EditorGUILayout.EndHorizontal();
                if (clicked)
                {
                    selectedScriptableIndex = i;
                    scriptableBase = scriptable;
                }
            }
        }

        private void DrawFavorite(ScriptableBase scriptableBase)
        {
            var icon = Favorites.Contains(scriptableBase) ? _icons[4] : _icons[0];
            var style = new GUIStyle(GUIStyle.none) {margin = {top = 5}};
            if (GUILayout.Button(icon, style, GUILayout.Width(16), GUILayout.Height(16)))
            {
                if (Favorites.Contains(scriptableBase)) Favorites.Remove(scriptableBase);
                else Favorites.Add(scriptableBase);
            }
        }

        private void DrawRightPanel()
        {
            if (scriptableBase == null) return;

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            _itemScrollPosition = EditorGUILayout.BeginScrollView(_itemScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandHeight(true));

            //Draw Selected Scriptable
            if (_editor == null || scriptableBase != previousScriptableBase)
            {
                if (previousScriptableBase != null) previousScriptableBase.repaintRequest -= OnRepaintRequested;
                UnityEditor.Editor.CreateCachedEditor(scriptableBase, null, ref _editor);
                previousScriptableBase = scriptableBase;
                scriptableBase.repaintRequest += OnRepaintRequested;
            }

            _editor.DrawHeader();
            _editor.OnInspectorGUI();
            Uniform.DrawLine();
            GUILayout.FlexibleSpace();
            DrawSelectedButtons();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedButtons()
        {
            if (GUILayout.Button("Select in Project", GUILayout.MaxHeight(_buttonHeight)))
            {
                Selection.activeObject = scriptableBase;
                EditorGUIUtility.PingObject(scriptableBase);
            }

            if (GUILayout.Button("Rename", GUILayout.MaxHeight(_buttonHeight)))
                PopupWindow.Show(new Rect(), new RenamePopUpWindow(position, scriptableBase));

            if (GUILayout.Button("Create Copy", GUILayout.MaxHeight(_buttonHeight)))
            {
                EditorCreator.CreateCopyAsset(scriptableBase);
                Refresh(_currentType);
            }

            var deleteButtonStyle = new GUIStyle(GUI.skin.button);
            deleteButtonStyle.normal.textColor = Color.red;
            deleteButtonStyle.hover.textColor = Color.red;
            deleteButtonStyle.active.textColor = Color.red;
            if (GUILayout.Button("Delete", deleteButtonStyle, GUILayout.MaxHeight(_buttonHeight)))
            {
                bool isDeleted = EditorDialog.DeleteObjectWithConfirmation(scriptableBase);
                if (isDeleted)
                {
                    scriptableBase = null;
                    OnTabSelected(_currentType, true);
                }
            }
        }

        private void OnTabSelected(ScriptableType type, bool deselectCurrent = false)
        {
            Refresh(type);
            _currentType = type;
            if (deselectCurrent)
            {
                selectedScriptableIndex = -1;
                scriptableBase = null;
            }
        }

        private void Refresh(ScriptableType type)
        {
            switch (type)
            {
                case ScriptableType.All:
                    _scriptableObjects = ProjectDatabase.FindAll<ScriptableBase>(currentFolderPath);
                    break;
                case ScriptableType.Variable:
                    var variables = ProjectDatabase.FindAll<ScriptableVariableBase>(currentFolderPath);
                    _scriptableObjects = variables.Cast<ScriptableBase>().ToList();
                    break;
                case ScriptableType.Event:
                    var events = ProjectDatabase.FindAll<ScriptableEventBase>(currentFolderPath);
                    _scriptableObjects = events.Cast<ScriptableBase>().ToList();
                    break;
                case ScriptableType.List:
                    var lists = ProjectDatabase.FindAll<ScriptableListBase>(currentFolderPath);
                    _scriptableObjects = lists.Cast<ScriptableBase>().ToList();
                    break;
                case ScriptableType.Favorite:
                    _scriptableObjects = Favorites;
                    break;
            }
        }

        private void SelectTab(int index, bool deselect = false)
        {
            tabIndex = index;
            OnTabSelected((ScriptableType) tabIndex, deselect);
        }

        private Texture GetIconFor(ScriptableBase scriptableBase)
        {
            switch (scriptableBase)
            {
                case ScriptableVariableBase _:
                    return EditorResources.ScriptableVariable;
                case ScriptableEventBase _:
                    return EditorResources.ScriptableEvent;
                case ScriptableListBase _:
                    return EditorResources.ScriptableList;
                default: return EditorResources.ScriptableVariable;
            }
        }
    }
}