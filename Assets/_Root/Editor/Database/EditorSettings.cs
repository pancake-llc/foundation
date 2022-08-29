using UnityEditor;

namespace Pancake.Database
{
    public static class EditorSettings
    {
        public enum ESettingKey
        {
            CurrentEntityGuid,
            CurrentGroupName,
            BreadcrumbBarGuids,
            SelectedEntityGuids,
            SearchGroups,
            SearchEntities,
        }

        public static string Get(ESettingKey data) { return EditorPrefs.GetString(data.ToString()); }

        public static void Set(ESettingKey data, string value) { EditorPrefs.SetString(data.ToString(), value); }
    }
}