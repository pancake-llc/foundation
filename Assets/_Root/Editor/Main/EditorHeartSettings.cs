using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [HideMono]
    public sealed class EditorHeartSettings : ScriptableObject
    {
        private const string EDITOR_BUILD_SETTINGS_GUID = "Editor Settings Asset";

        [System.Serializable]
        public class ExceptType
        {
            [SerializeField] private string name;

            [SerializeField] private bool subClasses;

            public ExceptType(string name, bool subClasses)
            {
                this.name = name;
                this.subClasses = subClasses;
            }

            public string Name => name;

            public bool SubClasses => subClasses;
        }

        [SerializeField] private bool enabled = true;

        [SerializeField] private bool animate = true;

        [SerializeField]
        [ReorderableList(HeaderHeight = 0,
            OnElementGUI = nameof(OnExceptTypeGUI),
            GetElementLabel = nameof(GetExceptTypeLabel),
            GetElementHeight = nameof(GetExceptTypeHeight))]
        [Foldout("Except Types", Style = "Highlight")]
        private ExceptType[] exceptTypes;

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
        public static EditorHeartSettings Current
        {
            get
            {
                if (!EditorBuildSettings.TryGetConfigObject<EditorHeartSettings>(EDITOR_BUILD_SETTINGS_GUID, out EditorHeartSettings settings))
                {
                    settings = InEditor.FindAllAssets<EditorHeartSettings>().FirstOrDefault(a => a != null) as EditorHeartSettings;

                    if (settings == null)
                    {
                        settings = CreateInstance<EditorHeartSettings>();
                        ResetSettings(settings);
                        var editorResourcePath = "Assets/_Root/Editor/";
                        if (!editorResourcePath.DirectoryExists()) editorResourcePath.CreateDirectory();
                        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/_Root/Editor/EditorHeartSettings.asset");
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
        /// <param name="heartSettings">Settings reference.</param>
        public static void ResetSettings(EditorHeartSettings heartSettings)
        {
            heartSettings.enabled = true;
            heartSettings.animate = true;
            heartSettings.exceptTypes = new[] {new ExceptType("UnityEvent", false), new ExceptType("Interpolator", false),};
        }

        #endregion

        public bool Enabled { get => enabled; set => enabled = value; }
        public bool Animate { get => animate; set => animate = value; }
        public ExceptType[] ExceptTypes { get => exceptTypes; set => exceptTypes = value; }
    }
}