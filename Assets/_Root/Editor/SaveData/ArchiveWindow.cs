using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Pancake.Editor.SaveData
{
    internal class ArchiveWindow : EditorWindow
    {
        private SubWindow[] _windows;

        public SubWindow currentWindow;

        [MenuItem("Tools/Pancake/Archive/Types", false, 100)]
        public static void InitAndShowTypes()
        {
            // Get existing open window or if none, make a new one:
            ArchiveWindow window = (ArchiveWindow) EditorWindow.GetWindow(typeof(ArchiveWindow));
            if (window != null)
            {
                window.minSize = new Vector2(550, 600);
                window.Show();
                window.SetCurrentWindow(typeof(TypesWindow));
            }
        }

        public static void InitAndShowTypes(System.Type type)
        {
            // Get existing open window or if none, make a new one:
            ArchiveWindow window = (ArchiveWindow) EditorWindow.GetWindow(typeof(ArchiveWindow));
            if (window != null)
            {
                window.Show();
                var typesWindow = (TypesWindow) window.SetCurrentWindow(typeof(TypesWindow));
                typesWindow.SelectType(type);
            }
        }

        [MenuItem("Tools/Pancake/Archive/Settings", false, 99)]
        public static void InitAndShowSettings()
        {
            // Get existing open window or if none, make a new one:
            ArchiveWindow window = (ArchiveWindow) EditorWindow.GetWindow(typeof(ArchiveWindow));
            if (window != null)
            {
                window.minSize = new Vector2(550, 600);
                window.Show();
                window.SetCurrentWindow(typeof(SettingsWindow));
            }
        }

        public void InitSubWindows() { _windows = new SubWindow[] {new SettingsWindow(this), new TypesWindow(this)}; }

        private void OnLostFocus()
        {
            if (currentWindow != null) currentWindow.OnLostFocus();
        }

        private void OnFocus()
        {
            if (currentWindow != null) currentWindow.OnFocus();
        }

        private void OnDestroy()
        {
            if (currentWindow != null) currentWindow.OnDestroy();
        }

        private void OnEnable()
        {
            if (_windows == null) InitSubWindows();
            // Set the window name and icon.
            titleContent = new GUIContent("Save Data");

            // Get the last opened window and open it.
            if (currentWindow == null)
            {
                var currentWindowName = EditorPrefs.GetString("editor_window_currentwindow", _windows[0].name);
                for (int i = 0; i < _windows.Length; i++)
                {
                    if (_windows[i].name == currentWindowName)
                    {
                        currentWindow = _windows[i];
                        break;
                    }
                }
            }
        }

        private void OnHierarchyChange()
        {
            if (currentWindow != null) currentWindow.OnHierarchyChange();
        }

        private void OnGUI()
        {
            var style = EditorStyle.Get;

            // Display the menu.
            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < _windows.Length; i++)
            {
                if (GUILayout.Button(_windows[i].name, currentWindow == _windows[i] ? style.menuButtonSelected : style.menuButton))
                    SetCurrentWindow(_windows[i]);
            }

            EditorGUILayout.EndHorizontal();

            if (currentWindow != null) currentWindow.OnGUI();
        }

        private void SetCurrentWindow(SubWindow window)
        {
            if (currentWindow != null) currentWindow.OnLostFocus();
            currentWindow = window;
            currentWindow.OnFocus();
            EditorPrefs.SetString("editor_window_currentwindow", window.name);
        }

        private SubWindow SetCurrentWindow(System.Type type)
        {
            currentWindow.OnLostFocus();
            currentWindow = _windows.First(w => w.GetType() == type);
            EditorPrefs.SetString("editor_window_currentwindow", currentWindow.name);
            return currentWindow;
        }
    }

    internal abstract class SubWindow
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public string name;
        public EditorWindow parent;
        public abstract void OnGUI();

        public SubWindow(string name, EditorWindow parent)
        {
            this.name = name;
            this.parent = parent;
        }

        public virtual void OnLostFocus() { }

        public virtual void OnFocus() { }

        public virtual void OnDestroy() { }

        public virtual void OnHierarchyChange() { }
    }
}