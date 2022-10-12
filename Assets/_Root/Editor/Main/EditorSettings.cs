using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [HideMono]
    public sealed class EditorSettings : ScriptableObject
    {
        private const string EDITOR_BUILD_SETTINGS_GUID = "Editor Settings Asset";

        [System.Serializable]
        public struct ExceptType
        {
            [SerializeField] private string name;

            [SerializeField] private bool subClasses;

            public ExceptType(string name, bool subClasses)
            {
                this.name = name;
                this.subClasses = subClasses;
            }

            #region [Getter / Setter]

            public string GetName() { return name; }

            public void SetName(string value) { name = value; }

            public bool SubClasses() { return subClasses; }

            public void SubClasses(bool value) { subClasses = value; }

            #endregion
        }

        [SerializeField] private bool attributeEnabled = true;

        [SerializeField] private bool attributeAnimate = true;

        [SerializeField]
        [ReorderableList(HeaderHeight = 0,
            OnElementGUI = nameof(OnExceptTypeGUI),
            GetElementLabel = nameof(GetExceptTypeLabel),
            GetElementHeight = nameof(GetExceptTypeHeight))]
        [Foldout("Except Types", Style = "Highlight")]
        private ExceptType[] attributeExceptTypes;

        #region [ExceptType GUI]

        private void OnExceptTypeGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect namePosition = new Rect(position.x + 4, position.y + 1, position.width - 20, EditorGUIUtility.singleLineHeight);
            SerializedProperty name = property.FindPropertyRelative("name");
            EditorGUI.PropertyField(namePosition, name, GUIContent.none);

            Rect subClassesePosition = new Rect(namePosition.xMax + 2, position.y, 20, namePosition.height);
            SerializedProperty subClasses = property.FindPropertyRelative("subClasses");
            EditorGUI.PropertyField(subClassesePosition, subClasses, GUIContent.none);
        }

        private float GetExceptTypeHeight(SerializedProperty element) { return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; }

        private GUIContent GetExceptTypeLabel(SerializedProperty array, int index)
        {
            SerializedProperty name = array.GetArrayElementAtIndex(index).FindPropertyRelative("name");
            return new GUIContent(name.stringValue);
        }

        #endregion

        #region [Static Methods]

        /// <summary>
        /// Get current editor settings asset.
        /// </summary>
        public static EditorSettings Current
        {
            get
            {
                if (!EditorBuildSettings.TryGetConfigObject<EditorSettings>(EDITOR_BUILD_SETTINGS_GUID, out EditorSettings settings))
                {
                    settings = AssetDatabase.LoadAllAssetsAtPath("Assets/_Root/EditorResources/").FirstOrDefault(a => a is EditorSettings) as EditorSettings;

                    if (settings == null)
                    {
                        settings = CreateInstance<EditorSettings>();
                        ResetSettings(settings);
                        var editorResourcePath = "Assets/_Root/EditorResources/";
                        if (!editorResourcePath.DirectoryExists()) editorResourcePath.CreateDirectory();
                        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/_Root/EditorResources/EditorSettings.asset");
                        AssetDatabase.CreateAsset(settings, path);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                    EditorBuildSettings.AddConfigObject(EDITOR_BUILD_SETTINGS_GUID, settings, true);
                }

                return settings;
            }
        }

        /// <summary>
        /// Reset specific settings to default.
        /// </summary>
        /// <param name="settings">Settings reference.</param>
        public static void ResetSettings(EditorSettings settings)
        {
            settings.attributeEnabled = true;
            settings.attributeAnimate = true;
            settings.attributeExceptTypes = null;
        }

        #endregion

        #region [Getter / Setter]

        public bool AttributeEnabled() { return attributeEnabled; }

        public void AttributeEnabled(bool value) { attributeEnabled = value; }

        public bool AttributeAnimate() { return attributeAnimate; }

        public void AttributeAnimate(bool value) { attributeAnimate = value; }

        public ExceptType[] GetAttributeExceptTypes() { return attributeExceptTypes; }

        public void SetAttributExceptTypes(ExceptType[] exceptTypes) { this.attributeExceptTypes = exceptTypes; }

        #endregion
    }
}