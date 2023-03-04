using System.Collections.Generic;
using System.Linq;
using Pancake.Scriptable;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Scriptable
{
    public class ScriptableWizard : EditorWindow
    {
        private enum ScriptableType
        {
            All,
            Variable,
            Event,
            List
        }

        private Vector2 _scrollPosition = Vector2.zero;
        private Vector2 _itemScrollPosition = Vector2.zero;
        private List<ScriptableBase> _scriptableObjects;
        private ScriptableType _currentType = ScriptableType.All;
        private Color[] _colors = {Color.gray, Color.cyan, Color.green, Color.yellow};

        private const float TAB_WIDTH = 65f;
        private const float BUTTON_HEIGHT = 40f;

        [SerializeField] private string currentFolder = "Assets";
        [SerializeField] private int selectedScriptableIndex;
        [SerializeField] private int tabIndex = -1;
        [SerializeField] private bool isInitialized;
        [SerializeField] private ScriptableBase scriptableBase;

        [MenuItem("Tools/Pancake/Scriptable/Wizard")]
        public new static void Show() => GetWindow<ScriptableWizard>("Scriptable Wizard");

        private void OnEnable()
        {
            if (isInitialized)
            {
                SelectTab(tabIndex);
                return;
            }

            SelectTab((int) _currentType, true);
            isInitialized = true;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            Uniform.DrawHeader("Scriptable Wizard");
            GUILayout.Space(2);
            DrawFolder();
            Uniform.DrawLine(2);
            DrawTabs();
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginHorizontal();
            DrawScriptableList();
            DrawSelectedScriptable();
            EditorGUILayout.EndHorizontal();
        }

        private void SelectTab(int index, bool deselect = false)
        {
            tabIndex = index;
            OnTabSelected((ScriptableType) tabIndex, deselect);
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
                    _scriptableObjects = Editor.FindAll<ScriptableBase>(currentFolder);
                    break;
                case ScriptableType.Variable:
                    var variables = Editor.FindAll<ScriptableVariableBase>(currentFolder);
                    _scriptableObjects = variables.Cast<ScriptableBase>().ToList();
                    break;
                case ScriptableType.Event:
                    var events = Editor.FindAll<ScriptableEventBase>(currentFolder);
                    _scriptableObjects = events.Cast<ScriptableBase>().ToList();
                    break;
                case ScriptableType.List:
                    var lists = Editor.FindAll<ScriptableListBase>(currentFolder);
                    _scriptableObjects = lists.Cast<ScriptableBase>().ToList();
                    break;
            }
        }

        private void DrawFolder()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Change Folder", GUILayout.MaxWidth(150)))
            {
                string path = EditorUtility.OpenFolderPanel("Select folder to set path.", currentFolder, "");

                if (path.Contains(Application.dataPath)) path = path.Substring(path.LastIndexOf("Assets"));

                if (!AssetDatabase.IsValidFolder(path))
                {
                    EditorUtility.DisplayDialog("Error: File Path Invalid", "Make sure the path is a valid folder in the project.", "Ok");
                }
                else
                {
                    currentFolder = path;
                    OnTabSelected(_currentType, true);
                }
            }

            EditorGUILayout.LabelField(currentFolder, GUI.skin.textField);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTabs()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            const float width = TAB_WIDTH * 4;

            var style = new GUIStyle(EditorStyles.toolbarButton);
            tabIndex = GUILayout.Toolbar(tabIndex, System.Enum.GetNames(typeof(ScriptableType)), style, GUILayout.MaxWidth(width));

            if (tabIndex != (int) _currentType) OnTabSelected((ScriptableType) tabIndex, true);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawScriptableList()
        {
            const float width = TAB_WIDTH * 4f;

            var color = GUI.backgroundColor;
            GUI.backgroundColor = _colors[(int) _currentType];
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(width), GUILayout.ExpandHeight(true));

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandHeight(true));
            GUI.backgroundColor = color;

            DrawScriptableBases(_scriptableObjects);

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Create Type", GUILayout.MaxHeight(BUTTON_HEIGHT))) PopupWindow.Show(new Rect(), new CreateTypeWindow(position));

            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedScriptable()
        {
            if (scriptableBase == null) return;

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            //Draw name and icon
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            var icon = GetIconFor(_currentType, scriptableBase);
            GUILayout.Box(icon, GUIStyle.none, GUILayout.Width(32), GUILayout.Height(32));
            GUILayout.Label(scriptableBase.name, EditorStyles.label);
            EditorGUILayout.EndHorizontal();

            _itemScrollPosition = EditorGUILayout.BeginScrollView(_itemScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.ExpandHeight(true));

            var editor = UnityEditor.Editor.CreateEditor(scriptableBase);
            editor.OnInspectorGUI();

            Uniform.DrawLine();
            GUILayout.FlexibleSpace();
            DrawSelectedButtons();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawScriptableBases(List<ScriptableBase> scriptables)
        {
            if (scriptables is null)
                return;

            var count = 0;
            foreach (var scriptable in scriptables)
            {
                if (scriptable == null) continue;

                EditorGUILayout.BeginHorizontal();

                var icon = GetIconFor(_currentType, scriptable);

                var style = new GUIStyle(GUIStyle.none) {contentOffset = new Vector2(0, 5)};
                GUILayout.Box(icon, style, GUILayout.Width(18), GUILayout.Height(18));

                var clicked = GUILayout.Toggle(selectedScriptableIndex == count, scriptable.name, GUI.skin.button, GUILayout.ExpandWidth(true));

                EditorGUILayout.EndHorizontal();
                if (clicked)
                {
                    selectedScriptableIndex = count;
                    scriptableBase = scriptable;
                }

                count++;
            }
        }

        private Texture2D GetIconFor(ScriptableBase scriptableBase)
        {
            switch (scriptableBase)
            {
                case ScriptableVariableBase _: return EditorResources.ScriptableVariable;
                case ScriptableEventBase _: return EditorResources.ScriptableEvent;
                case ScriptableListBase _: return EditorResources.ScriptableList;
                default: return EditorResources.ScriptableEventListener;
            }
        }

        private Texture GetIconFor(ScriptableType type, ScriptableBase scriptableBase)
        {
            switch (type)
            {
                case ScriptableType.Variable: return EditorResources.ScriptableVariable;
                case ScriptableType.Event: return EditorResources.ScriptableEvent;
                case ScriptableType.List: return EditorResources.ScriptableList;
                default: return GetIconFor(scriptableBase);
            }
        }

        private void DrawSelectedButtons()
        {
            if (GUILayout.Button("Select in Project", GUILayout.MaxHeight(BUTTON_HEIGHT)))
                EditorGUIUtility.PingObject(scriptableBase);

            if (GUILayout.Button("Rename", GUILayout.MaxHeight(BUTTON_HEIGHT)))
                PopupWindow.Show(new Rect(), new RenameWindow(position, scriptableBase));

            if (GUILayout.Button("Create Copy", GUILayout.MaxHeight(BUTTON_HEIGHT)))
            {
                Editor.CreateCopyAsset(scriptableBase);
                Refresh(_currentType);
            }

            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Delete", GUI.skin.button, GUILayout.MaxHeight(BUTTON_HEIGHT)))
            {
                var isDeleted = Editor.DeleteObjectWithConfirmation(scriptableBase);
                if (isDeleted)
                {
                    scriptableBase = null;
                    OnTabSelected(_currentType, true);
                }
            }

            GUI.backgroundColor = Color.white;
        }
    }
}