using Pancake.Greenery;
using UnityEditor;
using UnityEngine;

namespace Pancake.GreeneryEditor
{
    public class GreeneryManagerModule : GreeneryEditorModule
    {
        private GreeneryManager activeManager;
        private GreeneryToolEditor toolEditor;

        public override void Initialize(GreeneryToolEditor toolEditor)
        {
            this.toolEditor = toolEditor;
            toolEditor.OnGUI += OnGUI;
        }

        public override void Release() { toolEditor.OnGUI -= OnGUI; }

        public override void OnGUI()
        {
            activeManager = GreeneryEditorUtilities.GetActiveManager();
            if (activeManager != null)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Active manager: " + activeManager.name);
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_icon dropdown@2x"), GUILayout.Width(20), GUILayout.Height(20)))
                {
                    GenericMenu managerMenu = new GenericMenu();
                    GreeneryManager[] managers = GameObject.FindObjectsOfType<GreeneryManager>();
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
            GreeneryManager manager = managerObject as GreeneryManager;
            EditorGUIUtility.PingObject(manager.gameObject);
            Selection.activeGameObject = manager.gameObject;
        }

        public override float GetHeight()
        {
            if (activeManager != null)
            {
                return EditorGUIUtility.singleLineHeight * 2;
            }
            else
            {
                return EditorGUIUtility.singleLineHeight * 4;
            }
        }

        public override void SaveSettings() { }
    }
}