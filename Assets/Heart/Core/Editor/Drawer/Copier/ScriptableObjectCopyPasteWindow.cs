using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PancakeEditor
{
    public class ScriptableObjectCopyPasteWindow : EditorWindow
    {
        private Object _selectedObject;
        private Object _lastSelectedObject;
        private string _searchString = "";

        private Vector2 _scrollPos;
        private Dictionary<FieldInfo, bool> _fieldInfoList = new Dictionary<FieldInfo, bool>();

        private static ScriptableObjectCopyPasteWindow instance;
        private int EligibleObjectCount => ScriptableObjectCopyPasteHandler.GetEligibleObjects(_selectedObject).Count;

        public static void OpenWindow(Object selectedObject)
        {
            if (instance == null)
            {
                instance = GetWindow<ScriptableObjectCopyPasteWindow>("Field List");
                instance.minSize = new Vector2(400, 300);
            }

            instance._selectedObject = selectedObject;
            instance.ListFields();
            instance.Show();
        }

        private void OnEnable()
        {
            ListFields();
        }

        private void OnGUI()
        {
            GUILayout.Label("Selected Object", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _selectedObject = EditorGUILayout.ObjectField(_selectedObject, typeof(ScriptableObject), true);

            // Add a refresh button next to the selectedObject field
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_RotateTool"), GUILayout.Width(30),
                    GUILayout.Height(20)))
            {
                ListFields();
            }

            EditorGUILayout.EndHorizontal();

            if (_selectedObject == null)
            {
                EditorGUILayout.HelpBox("Selected Object is null. Please select an object.", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space(10);
            GUILayout.Label("Search Fields", EditorStyles.boldLabel);
            string newSearchString = EditorGUILayout.TextField(_searchString);

            if (!string.Equals(newSearchString, _searchString))
            {
                _searchString = newSearchString;
                ListFields(); // refresh the field list when the search string changes
            }

            GUILayout.Label($"Eligible Objects: {EligibleObjectCount}");

            if (GUILayout.Button("Paste Selected Fields"))
            {
                Paste();
            }

            EditorGUILayout.Space(20);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            GUIStyle labelStyle = EditorStyles.label;
            GUIStyle toggleStyle = EditorStyles.toggle;
            GUIStyle backgroundStyle = GUI.skin.box;

            foreach (var fieldInfo in _fieldInfoList.Keys.ToArray())
            {
                // filter fields based on the search string
                if (fieldInfo.Name.ToLower().Contains(_searchString.ToLower()))
                {
                    EditorGUILayout.BeginHorizontal(backgroundStyle);

                    _fieldInfoList[fieldInfo] =
                        EditorGUILayout.Toggle(_fieldInfoList[fieldInfo], toggleStyle, GUILayout.Width(20));
                    EditorGUILayout.LabelField($"{fieldInfo.Name}:", labelStyle);
                    EditorGUILayout.LabelField(fieldInfo.GetValue(_selectedObject)?.ToString() ?? "null", labelStyle);

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);

            UpdateSelectedObjectAndListFields();
        }

        private void UpdateSelectedObjectAndListFields()
        {
            if (_selectedObject != _lastSelectedObject)
            {
                if (_selectedObject == null)
                    return;

                ListFields();
                _lastSelectedObject = _selectedObject;
            }
        }

        private void ListFields()
        {
            if (_selectedObject == null)
            {
                return;
            }

            // Remember the current toggle status of the fields before clearing the list
            var oldFieldInfoList = new Dictionary<FieldInfo, bool>(_fieldInfoList);
            _fieldInfoList.Clear();

            FieldInfo[] fields = _selectedObject.GetType()
                .GetFields(BindingFlags.Default)
                .Where(f => f.GetCustomAttributes(typeof(SerializeField), true).Length > 0 || f.IsPublic)
                .ToArray();

            foreach (var field in fields)
            {
                bool toggleStatus = !oldFieldInfoList.TryGetValue(field, out var value) || value;
                _fieldInfoList.Add(field, toggleStatus);
            }
        }


        private void Paste()
        {
            ScriptableObjectCopyPasteHandler.PasteSelectedValues(_selectedObject, _fieldInfoList);
        }
    }
}