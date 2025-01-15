using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditorInternal;

namespace UnityToolbarExtender
{
    internal class CustomToolbarSettingProvider : SettingsProvider
    {
        private SerializedObject _mToolbarSetting;
        private CustomToolbarSetting _setting;

        private Vector2 _scrollPos;
        private ReorderableList _elementsList;

        public CustomToolbarSettingProvider(string path, SettingsScope scopes = SettingsScope.User)
            : base(path, scopes)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            // base.OnActivate(searchContext, rootElement);
            _mToolbarSetting = CustomToolbarSetting.GetSerializedSetting();
            _setting = (_mToolbarSetting.targetObject as CustomToolbarSetting);
        }

        public static bool IsSettingAvailable() { return ScriptableSingleton<CustomToolbarSetting>.instance != null; }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            _elementsList ??= CustomToolbarReordableList.Create(_setting.Elements, OnMenuItemAdd);
            _elementsList.DoLayoutList();

            EditorGUILayout.EndScrollView();

            _mToolbarSetting.ApplyModifiedProperties();
            if (GUI.changed)
            {
                EditorUtility.SetDirty(_mToolbarSetting.targetObject);
                ToolbarExtender.OnGUI();
                _setting.Save();
            }
        }

        private void OnMenuItemAdd(object target)
        {
            _setting.Elements.Add(target as BaseToolbarElement);
            _mToolbarSetting.ApplyModifiedProperties();
            _setting.Save();
        }

        [SettingsProvider]
        public static SettingsProvider CreateCustomToolbarSettingProvider()
        {
            if (IsSettingAvailable())
            {
                var provider = new CustomToolbarSettingProvider("Project/Custom Toolbar", SettingsScope.Project);
                return provider;
            }

            return null;
        }
    }
}