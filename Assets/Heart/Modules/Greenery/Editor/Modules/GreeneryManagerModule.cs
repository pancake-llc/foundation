using Pancake.Greenery;
using UnityEditor;
using UnityEngine;

namespace Pancake.GreeneryEditor
{
    [System.Serializable]
    public class GreeneryManagerModule : GreeneryEditorModule
    {
        private GreeneryManager _activeManager;
        private GreeneryToolEditor _toolEditor;

        public override void Initialize(GreeneryToolEditor toolEditor)
        {
            _toolEditor = toolEditor;
            toolEditor.OnGUI += OnGUI;
        }

        public override void Release() { _toolEditor.OnGUI -= OnGUI; }

        public override void OnGUI()
        {
            _activeManager = GreeneryEditorUtilities.GetActiveManager();
            if (_activeManager != null)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Active manager: " + _activeManager.name);
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_icon dropdown@2x"), GUILayout.Width(20), GUILayout.Height(20)))
                {
                    GenericMenu managerMenu = new GenericMenu();
                    GreeneryManager[] managers = Object.FindObjectsOfType<GreeneryManager>();
                    foreach (var manager in managers)
                    {
                        managerMenu.AddItem(new GUIContent(manager.name), false, OnSelectManager, manager);
                    }

                    managerMenu.ShowAsContext();
                }

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("No Greenery manager found in scene", MessageType.Warning);
                if (GUILayout.Button("Create"))
                {
                    GameObject greeneryManagerObject = new GameObject("GreeneryManager", typeof(GreeneryManager));
                    EditorGUIUtility.PingObject(greeneryManagerObject);
                }
            }
        }

        private void OnSelectManager(object managerObject)
        {
            var manager = managerObject as GreeneryManager;
            if (manager != null)
            {
                EditorGUIUtility.PingObject(manager.gameObject);
                Selection.activeGameObject = manager.gameObject;
            }
        }

        public override float GetHeight()
        {
            if (_activeManager != null) return EditorGUIUtility.singleLineHeight * 2;
            return EditorGUIUtility.singleLineHeight * 4;
        }

        public override void SaveSettings() { }
    }
}