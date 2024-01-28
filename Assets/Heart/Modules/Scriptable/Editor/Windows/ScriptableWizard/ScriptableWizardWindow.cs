using System;
using System.Collections.Generic;
using System.Linq;
using Pancake.ExLibEditor;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Pancake.ScriptableEditor
{
    public class ScriptableWizardWindow : ScriptableWindowBase
    {
        protected override string HeaderTitle => "Scriptable";

        private Vector2 _scrollPosition = Vector2.zero;
        private Vector2 _categoryScrollPosition = Vector2.zero;
        private Vector2 _itemScrollPosition = Vector2.zero;
        private List<ScriptableBase> _scriptableObjects;
        private ScriptableType _currentType = ScriptableType.All;
        private FilterVariable _filterVariable = FilterVariable.All;
        private const float TAB_WIDTH = 55f;
        private Texture[] _icons;
        private readonly Color[] _colors = {Uniform.RichBlack, Uniform.GothicOlive, Uniform.Maroon, Uniform.ElegantNavy, Uniform.CrystalPurple};
        private string _searchText = "";
        private UnityEditor.Editor _editor;

        [SerializeField] private string currentFolderPath = "Assets";
        [SerializeField] private int selectedScriptableIndex;
        [SerializeField] private int typeTabIndex = -1;
        [SerializeField] private int categoryMask;
        [SerializeField] private bool isInitialized;
        [SerializeField] private ScriptableBase scriptableBase;
        [SerializeField] private ScriptableBase previousScriptableBase;
        [SerializeField] private FavoriteData favoriteData;
        [SerializeField] private bool categoryAsButtons;
        private List<ScriptableBase> Favorites => favoriteData.favorites;
        private const string PATH_KEY = "scriptable_wizard_path";
        private const string FAVORITE_KEY = "scriptable_wizard_favorites";
        private const string CATEGORIES_KEY = "soapwizard_categories";
        private const string CATEGORIES_LAYOUT_KEY = "soapwizard_categorieslayout";


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

        private enum FilterVariable
        {
            All,
            Saved,
            [InspectorName("Saved With Warning")] SavedApplicationStart,
            NotSaved
        }

        public new static ScriptableWizardWindow Show() => GetWindow<ScriptableWizardWindow>("Scriptable Wizard");

        [MenuItem("Tools/Pancake/Scriptable/Wizard %`")]
        private static void OpenScriptableWizard() => Show();

        protected override void OnEnable()
        {
            Init();
            if (isInitialized)
            {
                SelectTab(typeTabIndex);
                return;
            }

            SelectTab((int) _currentType, true); //default is 0
            isInitialized = true;
        }

        private void OnDisable()
        {
            var data = JsonUtility.ToJson(favoriteData, false);
            EditorPrefs.SetString(FAVORITE_KEY, data);
            EditorPrefs.SetInt(CATEGORIES_KEY, categoryMask);
            EditorPrefs.SetInt(CATEGORIES_LAYOUT_KEY, categoryAsButtons ? 1 : 0);
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
            _icons = new Texture[11];
            _icons[0] = EditorResources.StarEmpty;
            _icons[1] = EditorResources.ScriptableVariable;
            _icons[2] = EditorResources.ScriptableEvent;
            _icons[3] = EditorResources.ScriptableList;
            _icons[4] = EditorResources.StarFull;
            _icons[5] = EditorResources.IconPing;
            _icons[6] = EditorResources.IconEdit;
            _icons[7] = EditorResources.IconDuplicate;
            _icons[8] = EditorResources.IconDelete;
            _icons[9] = EditorResources.IconCategoryLayout;
            _icons[10] = EditorGUIUtility.IconContent("Folder Icon").image;
        }

        private void LoadSavedData()
        {
            currentFolderPath = EditorPrefs.GetString(PATH_KEY, "Assets");
            favoriteData = new FavoriteData();
            string favoriteDataJson = JsonUtility.ToJson(this.favoriteData, false);
            string data = EditorPrefs.GetString(FAVORITE_KEY, favoriteDataJson);
            categoryMask = EditorPrefs.GetInt(CATEGORIES_KEY, 1);
            categoryAsButtons = EditorPrefs.GetInt(CATEGORIES_LAYOUT_KEY, 0) != 0;
            JsonUtility.FromJsonOverwrite(data, this.favoriteData);
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            DrawFolder();
            GUILayout.Space(2);
            Uniform.DrawLine();
            GUILayout.Space(2);
            if (categoryAsButtons)
            {
                DrawCategoryAsButtons();
                GUILayout.Space(2);
                Uniform.DrawLine();
            }

            EditorGUILayout.BeginHorizontal();
            DrawLeftPanel();
            DrawRightPanel();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draw category header
        /// </summary>
        private void DrawCategoryHeader()
        {
            GUILayout.Space(2);
            var buttonStyle = new GUIStyle(GUI.skin.button) {margin = new RectOffset(2, 2, 0, 0), padding = new RectOffset(4, 4, 4, 4)};
            var buttonContent = new GUIContent(_icons[9], "Switch Categories Layout");
            if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.MaxWidth(25), GUILayout.MaxHeight(20))) categoryAsButtons = !categoryAsButtons;

            buttonContent = new GUIContent(_icons[6], "Edit Categories");
            if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.MaxWidth(25), GUILayout.MaxHeight(20)))
                PopupWindow.Show(new Rect(), new CategoryPopUpWindow(position));
            EditorGUILayout.LabelField("Categories", GUILayout.MaxWidth(70));
        }

        /// <summary>
        /// Draw toggle filter scriptabled enabled saved
        /// </summary>
        private void DrawFilterSavedVariable()
        {
            GUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent("Filter"), GUILayout.MaxWidth(30));
            if (typeTabIndex != (int) _currentType) OnTabSelected((ScriptableType) typeTabIndex, true);
            _filterVariable = (FilterVariable) EditorGUILayout.EnumPopup("", _filterVariable, GUILayout.MaxWidth(80));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCategoryAsLayerMask()
        {
            var height = 20f;
            var width = TAB_WIDTH * 5 + 5f;
            EditorGUILayout.BeginHorizontal(GUILayout.MaxHeight(height), GUILayout.MaxWidth(width));
            DrawCategoryHeader();
            var categories = ScriptableEditorSetting.Categories.ToArray();
            categoryMask = EditorGUILayout.MaskField(categoryMask, categories);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCategoryAsButtons()
        {
            const float height = 20f;
            var categories = ScriptableEditorSetting.Categories;
            EditorGUILayout.BeginHorizontal(GUILayout.MaxHeight(height));
            DrawCategoryHeader();
            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.margin = new RectOffset(2, 2, 0, 0);
            if (GUILayout.Button("Nothing", buttonStyle, GUILayout.Width(74), GUILayout.Height(height)))
            {
                categoryMask = 0;
            }

            if (GUILayout.Button("Everything", buttonStyle, GUILayout.Width(74f), GUILayout.Height(height)))
            {
                categoryMask = (1 << categories.Count) - 1;
            }

            GUILayout.Space(5f);
            _categoryScrollPosition = EditorGUILayout.BeginScrollView(_categoryScrollPosition);
            EditorGUILayout.BeginHorizontal(GUILayout.MaxHeight(height));

            buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
            buttonStyle.margin = new RectOffset(2, 2, 0, 0);
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.onHover.textColor = Color.white;
            for (int i = 0; i < categories.Count; i++)
            {
                var originalColor = GUI.backgroundColor;
                var isSelected = (categoryMask & (1 << i)) != 0;
                GUI.backgroundColor = isSelected ? Uniform.RichBlack.Lighten(.5f) : Color.gray;
                if (GUILayout.Button(categories[i], buttonStyle, GUILayout.Height(height)))
                {
                    categoryMask ^= 1 << i; //toggle the bit
                }

                GUI.backgroundColor = originalColor;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
            GUILayout.Space(2);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawFolder()
        {
            EditorGUILayout.BeginHorizontal();
            var buttonContent = new GUIContent(_icons[10], "Change Selected Folder");
            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.margin = new RectOffset(2, 2, 0, 0);
            if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.Height(20f), GUILayout.MaxWidth(40)))
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
            GUILayout.Space(2);
            var width = TAB_WIDTH * 5 + 2f; //offset to match

            var style = new GUIStyle(EditorStyles.toolbarButton);
            typeTabIndex = GUILayout.Toolbar(typeTabIndex, Enum.GetNames(typeof(ScriptableType)), style, GUILayout.MaxWidth(width));

            if (typeTabIndex != (int) _currentType) OnTabSelected((ScriptableType) typeTabIndex, true);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSearchBar()
        {
            var width = TAB_WIDTH * 5 + 4f;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Width(width));
            _searchText = EditorGUILayout.TextField(_searchText, EditorStyles.toolbarSearchField);

            if (GUILayout.Button("", GUI.skin.FindStyle("SearchCancelButton")))
            {
                _searchText = "";
                GUI.FocusControl(null);
            }

            GUILayout.EndHorizontal();
        }

        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical();
            if (!categoryAsButtons)
            {
                DrawCategoryAsLayerMask();
                GUILayout.Space(2);
                Uniform.DrawLine();
            }

            DrawTabs();
            if (typeTabIndex == 1) // tab variable
            {
                // draw toggle to filter scriptable has saved enabled
                DrawFilterSavedVariable();
            }

            DrawSearchBar();
            const float width = TAB_WIDTH * 5f;
            var color = GUI.backgroundColor;
            GUI.backgroundColor = _colors[(int) _currentType];
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(width), GUILayout.ExpandHeight(true));
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            GUI.backgroundColor = color;
            DrawScriptableBases(_scriptableObjects, typeTabIndex, width);
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Create Type", GUILayout.MaxHeight(ScriptableEditorSetting.BUTTON_HEIGHT)))
                PopupWindow.Show(new Rect(), new CreateTypePopUpWindow(position));

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private void DrawScriptableBases(List<ScriptableBase> scriptables, int tapIndex, float maxWidth)
        {
            if (scriptables is null) return;

            int count = scriptables.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var scriptable = scriptables[i];
                if (scriptable == null) continue;

                //filter category
                if ((categoryMask & (1 << scriptable.categoryIndex)) == 0) continue;

                // filter saved if it variable
                if (tapIndex == 1)
                {
                    switch (_filterVariable)
                    {
                        case FilterVariable.Saved:
                            var temp = scriptable as ISave;
                            if (temp == null || temp.Saved == false) continue;
                            break;
                        case FilterVariable.NotSaved when scriptable is ISave {Saved: true}:
                            continue;
                        case FilterVariable.SavedApplicationStart:
                        {
                            var isSave = scriptable as ISave;
                            var resetOn = scriptable as IResetOn;
                            if (isSave == null || resetOn == null || isSave.Saved == false || resetOn.ResetOn != ResetType.ApplicationStarts) continue;
                            break;
                        }
                    }
                }

                //filter search
                if (scriptable.name.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) < 0) continue;

                EditorGUILayout.BeginHorizontal();

                var icon = _currentType == ScriptableType.All || _currentType == ScriptableType.Favorite ? GetIconFor(scriptable) : _icons[(int) _currentType];

                var style = new GUIStyle(GUIStyle.none) {contentOffset = new Vector2(0, 5)};
                GUILayout.Box(icon, style, GUILayout.Width(18), GUILayout.Height(18));

                bool clicked = GUILayout.Toggle(selectedScriptableIndex == i,
                    scriptable.name,
                    new GUIStyle(GUI.skin.button) {alignment = TextAnchor.MiddleLeft},
                    GUILayout.MaxWidth(maxWidth - TAB_WIDTH - 8),
                    GUILayout.ExpandWidth(true));
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

            EditorGUILayout.BeginVertical(GUI.skin.box);
            _itemScrollPosition = EditorGUILayout.BeginScrollView(_itemScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandHeight(true));

            DrawUtilityButtons();

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
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawUtilityButtons()
        {
            EditorGUILayout.BeginHorizontal();
            var buttonStyle = new GUIStyle(GUI.skin.button) {padding = new RectOffset(8, 8, 8, 8)};
            var buttonContent = new GUIContent(_icons[5], "Select in Project");
            if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.MaxHeight(ScriptableEditorSetting.BUTTON_HEIGHT)))
            {
                Selection.activeObject = scriptableBase;
                EditorGUIUtility.PingObject(scriptableBase);
            }

            buttonContent = new GUIContent(_icons[6], "Rename");
            if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.MaxHeight(ScriptableEditorSetting.BUTTON_HEIGHT)))
                PopupWindow.Show(new Rect(), new RenamePopUpWindow(position, scriptableBase));

            buttonContent = new GUIContent(_icons[7], "Create Copy");
            if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.MaxHeight(ScriptableEditorSetting.BUTTON_HEIGHT)))
            {
                EditorCreator.CreateCopyAsset(scriptableBase);
                Refresh(_currentType);
            }

            buttonContent = new GUIContent(_icons[8], "Delete");
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.red.Lighten(.75f);
            if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.MaxHeight(ScriptableEditorSetting.BUTTON_HEIGHT)))
            {
                var isDeleted = EditorDialog.DeleteObjectWithConfirmation(scriptableBase);
                if (isDeleted)
                {
                    scriptableBase = null;
                    OnTabSelected(_currentType, true);
                }
            }

            GUI.backgroundColor = originalColor;
            EditorGUILayout.EndHorizontal();
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
            typeTabIndex = index;
            OnTabSelected((ScriptableType) typeTabIndex, deselect);
        }

        private Texture GetIconFor(ScriptableBase scriptableBase)
        {
            switch (scriptableBase)
            {
                case ScriptableVariableBase: return EditorResources.ScriptableVariable;
                case ScriptableEventBase: return EditorResources.ScriptableEvent;
                case ScriptableListBase: return EditorResources.ScriptableList;
                default: return EditorResources.ScriptableVariable;
            }
        }

        internal void SelectAndScrollTo(Object obj)
        {
            for (int i = 0; i < _scriptableObjects.Count; i++)
            {
                if (_scriptableObjects[i].Equals(obj))
                {
                    selectedScriptableIndex = i;
                    scriptableBase = obj as ScriptableBase;
                    _scrollPosition = new Vector2(0,
                        System.Math.Max(0, new GUIStyle(GUI.skin.button).CalcHeight(GUIContent.none, 0) * (_scriptableObjects.Count - i) - 50));
                    break;
                }
            }
        }
    }
}