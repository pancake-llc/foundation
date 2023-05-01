using System;
using System.Collections.Generic;
using System.Linq;
using PancakeEditor;
using UnityEditor;
using UnityEngine;

namespace Obvious.Soap.Editor
{
    public class SoapWizardWindow : SoapWindowBase
    {
        protected override string HeaderTitle => "Soap Wizard";

        private Vector2 _scrollPosition = Vector2.zero;
        private Vector2 _itemScrollPosition = Vector2.zero;
        private List<ScriptableBase> _scriptableObjects;
        private ScriptableType _currentType = ScriptableType.All;
        private readonly float _tabWidth = 55f;
        private readonly float _buttonHeight = 40f;
        private Texture[] _icons;
        private readonly Color[] _colors = {Color.gray, Color.cyan, Color.green, Color.yellow, Color.gray};
        private string _searchText = "";

        [SerializeField] private string _currentFolderPath = "Assets";
        [SerializeField] private int _selectedScriptableIndex;
        [SerializeField] private int _tabIndex = -1;
        [SerializeField] private bool _isInitialized;
        [SerializeField] private ScriptableBase _scriptableBase;
        [SerializeField] private FavoriteData _favoriteData;

        private List<ScriptableBase> Favorites => _favoriteData.Favorites;
        public const string PathKey = "SoapWizard_Path";
        public const string FavoriteKey = "SoapWizard_Favorites";

        [Serializable]
        private class FavoriteData
        {
            public List<ScriptableBase> Favorites = new List<ScriptableBase>();
        }

        private enum ScriptableType
        {
            All,
            Variable,
            Event,
            List,
            Favorite
        }

        [MenuItem("Window/Obvious/Soap/Soap Wizard")]
        public new static void Show() => GetWindow<SoapWizardWindow>("Soap Wizard");

        [MenuItem("Tools/Obvious/Soap/Soap Wizard")]
        private static void OpenSoapWizard() => Show();

        protected override void OnEnable()
        {
            Init();
            if (_isInitialized)
            {
                SelectTab(_tabIndex);
                return;
            }

            SelectTab((int) _currentType, true); //default is 0
            _isInitialized = true;
        }

        private void OnDisable()
        {
            var data = JsonUtility.ToJson(_favoriteData, false);
            EditorPrefs.SetString(FavoriteKey, data);
        }

        protected override void Init()
        {
            base.Init();
            LoadAssets();
            LoadSavedData();
        }

        private void LoadAssets()
        {
            _icons = new Texture[5];
            _icons[0] = EditorGUIUtility.IconContent("Favorite On Icon").image;
            _icons[1] = Resources.Load<Texture>("Icons/icon_scriptableVariable");
            _icons[2] = Resources.Load<Texture>("Icons/icon_scriptableEvent");
            _icons[3] = Resources.Load<Texture>("Icons/icon_scriptableList");
            _icons[4] = EditorGUIUtility.IconContent("Favorite Icon").image;
        }

        private void LoadSavedData()
        {
            _currentFolderPath = EditorPrefs.GetString(PathKey, "Assets");
            _favoriteData = new FavoriteData();
            var favoriteDataJson = JsonUtility.ToJson(_favoriteData, false);
            var favoriteData = EditorPrefs.GetString(FavoriteKey, favoriteDataJson);
            JsonUtility.FromJsonOverwrite(favoriteData, _favoriteData);
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
                var path = EditorUtility.OpenFolderPanel("Select folder to set path.", _currentFolderPath, "");

                if (path.Contains(Application.dataPath))
                    path = path.Substring(path.LastIndexOf("Assets"));

                if (!AssetDatabase.IsValidFolder(path))
                    EditorUtility.DisplayDialog("Error: File Path Invalid", "Make sure the path is a valid folder in the project.", "Ok");
                else
                {
                    _currentFolderPath = path;
                    EditorPrefs.SetString(PathKey, _currentFolderPath);
                    OnTabSelected(_currentType, true);
                }
            }

            EditorGUILayout.LabelField(_currentFolderPath, GUI.skin.textField);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTabs()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            var width = _tabWidth * 5 + 4f; //offset to match

            var style = new GUIStyle(EditorStyles.toolbarButton);
            _tabIndex = GUILayout.Toolbar(_tabIndex, Enum.GetNames(typeof(ScriptableType)), style, GUILayout.MaxWidth(width));

            if (_tabIndex != (int) _currentType)
                OnTabSelected((ScriptableType) _tabIndex, true);

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

            var count = 0;
            foreach (var scriptable in scriptables)
            {
                if (scriptable == null)
                    continue;

                //filter search
                if (scriptable.name.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                EditorGUILayout.BeginHorizontal();

                var icon = _currentType == ScriptableType.All || _currentType == ScriptableType.Favorite ? GetIconFor(scriptable) : _icons[(int) _currentType];

                var style = new GUIStyle(GUIStyle.none);
                style.contentOffset = new Vector2(0, 5);
                GUILayout.Box(icon, style, GUILayout.Width(18), GUILayout.Height(18));

                var clicked = GUILayout.Toggle(_selectedScriptableIndex == count, scriptable.name, Uniform.FoldoutButton, GUILayout.ExpandWidth(true));

                EditorGUILayout.EndHorizontal();
                if (clicked)
                {
                    _selectedScriptableIndex = count;
                    _scriptableBase = scriptable;
                }

                //HandleRightClick(count);
                count++;
            }
        }

        private void DrawRightPanel()
        {
            if (_scriptableBase == null)
                return;

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            DrawRightPanelHeader();

            _itemScrollPosition = EditorGUILayout.BeginScrollView(_itemScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandHeight(true));

            //Draw Selected Scriptable
            var editor = UnityEditor.Editor.CreateEditor(_scriptableBase);
            editor.OnInspectorGUI();
            //needed to refresh directly even if not focused during play mode.
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            Uniform.DrawLine();
            GUILayout.FlexibleSpace();
            DrawSelectedButtons();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }


        private void DrawRightPanelHeader()
        {
            //Draw name and icon
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            var icon = _currentType == ScriptableType.All || _currentType == ScriptableType.Favorite ? GetIconFor(_scriptableBase) : _icons[(int) _currentType];
            GUILayout.Box(icon, GUIStyle.none, GUILayout.Width(32), GUILayout.Height(32));
            GUILayout.Label(_scriptableBase.name, EditorStyles.label);
            GUILayout.FlexibleSpace();

            //Draw Favorite
            GUILayout.Label("Favorite", EditorStyles.label);
            icon = Favorites.Contains(_scriptableBase) ? _icons[4] : _icons[0];
            var style = new GUIStyle(GUIStyle.none);
            //style.margin.top = 2;
            if (GUILayout.Button(icon, style, GUILayout.Width(20), GUILayout.Height(20)))
            {
                if (Favorites.Contains(_scriptableBase))
                    Favorites.Remove(_scriptableBase);
                else
                    Favorites.Add(_scriptableBase);
            }

            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSelectedButtons()
        {
            if (GUILayout.Button("Select in Project", GUILayout.MaxHeight(_buttonHeight)))
            {
                Selection.activeObject = _scriptableBase;
                EditorGUIUtility.PingObject(_scriptableBase);
            }

            if (GUILayout.Button("Rename", GUILayout.MaxHeight(_buttonHeight)))
                PopupWindow.Show(new Rect(), new RenamePopUpWindow(position, _scriptableBase));

            if (GUILayout.Button("Create Copy", GUILayout.MaxHeight(_buttonHeight)))
            {
                PancakeEditor.Editor.CreateCopyAsset(_scriptableBase);
                Refresh(_currentType);
            }

            var deleteButtonStyle = new GUIStyle(GUI.skin.button);
            deleteButtonStyle.normal.textColor = Color.red;
            deleteButtonStyle.hover.textColor = Color.red;
            deleteButtonStyle.active.textColor = Color.red;
            if (GUILayout.Button("Delete", deleteButtonStyle, GUILayout.MaxHeight(_buttonHeight)))
            {
                bool isDeleted = PancakeEditor.Editor.DeleteObjectWithConfirmation(_scriptableBase);
                if (isDeleted)
                {
                    _scriptableBase = null;
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
                _selectedScriptableIndex = -1;
                _scriptableBase = null;
            }
        }

        private void Refresh(ScriptableType type)
        {
            switch (type)
            {
                case ScriptableType.All:
                    _scriptableObjects = PancakeEditor.Editor.FindAll<ScriptableBase>(_currentFolderPath);
                    break;
                case ScriptableType.Variable:
                    var variables = PancakeEditor.Editor.FindAll<ScriptableVariableBase>(_currentFolderPath);
                    _scriptableObjects = variables.Cast<ScriptableBase>().ToList();
                    break;
                case ScriptableType.Event:
                    var events = PancakeEditor.Editor.FindAll<ScriptableEventBase>(_currentFolderPath);
                    _scriptableObjects = events.Cast<ScriptableBase>().ToList();
                    break;
                case ScriptableType.List:
                    var lists = PancakeEditor.Editor.FindAll<ScriptableListBase>(_currentFolderPath);
                    _scriptableObjects = lists.Cast<ScriptableBase>().ToList();
                    break;
                case ScriptableType.Favorite:
                    _scriptableObjects = Favorites;
                    break;
            }
        }

        private void SelectTab(int index, bool deselect = false)
        {
            _tabIndex = index;
            OnTabSelected((ScriptableType) _tabIndex, deselect);
        }

        private Texture GetIconFor(ScriptableBase scriptableBase)
        {
            var iconIndex = 0;
            switch (scriptableBase)
            {
                case ScriptableVariableBase _:
                    iconIndex = 1;
                    break;
                case ScriptableEventBase _:
                    iconIndex = 2;
                    break;
                case ScriptableListBase _:
                    iconIndex = 3;
                    break;
            }

            return _icons[iconIndex];
        }
    }
}