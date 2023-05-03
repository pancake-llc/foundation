using Pancake.Apex;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [HideMonoScript]
    [UnityEditor.FilePath("ProjectSettings/ApexSettings.asset", UnityEditor.FilePathAttribute.Location.ProjectFolder)]
    public sealed class ApexSettings : ScriptableSingleton<ApexSettings>
    {
        [SerializeField] private bool enabled = true;

        [SerializeField] private bool animate = true;

        [SerializeField] private bool validateOnStartup = false;

        [SerializeField] [Array(OnElementGUI = nameof(OnExceptTypeGUI), GetElementHeight = nameof(GetExceptTypeHeight))]
        private ExceptType[] exceptTypes;

        /// <summary>
        /// Saves the current state of the Apex settings.
        /// </summary>
        public void Save()
        {
            Save(true);
            ApplySettings();
        }

        /// <summary>
        /// Apply settings for internal classes.
        /// </summary>
        private void ApplySettings()
        {
            ApexUtility.Enabled = enabled;
            ApexGUIUtility.Animate = animate;

            ApexUtility.exceptTypes.Clear();
            ApexUtility.exceptTypes.Add(new ExceptType("UnityEventBase", true));

            if (exceptTypes != null)
            {
                for (int i = 0; i < exceptTypes.Length; i++)
                {
                    ExceptType exceptType = exceptTypes[i];
                    if (!ApexUtility.exceptTypes.Add(exceptType))
                    {
                        Debug.LogWarning(
                            $"<b>Message:</b> An attempt to add two identical types for an exception. Remove duplicate types: <color=red>{exceptType.GetName()}</color>");
                    }
                }
            }
        }

        #region [ExceptType GUI]

        private void OnExceptTypeGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty name = property.FindPropertyRelative("name");
            SerializedProperty subClasses = property.FindPropertyRelative("subClasses");

            position.x += 2;
            position.width -= 20;
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, name, GUIContent.none);

            position.x = position.xMax + ApexGUIUtility.VerticalSpacing;
            position.width = 20;
            EditorGUI.PropertyField(position, subClasses, GUIContent.none);
            GUI.backgroundColor = Color.white;
        }

        private float GetExceptTypeHeight(SerializedProperty element) { return EditorGUIUtility.singleLineHeight; }

        #endregion

        #region [Static Methods]

        /// <summary>
        /// Reset specific settings to default.
        /// </summary>
        /// <param name="settings">Settings reference.</param>
        public static void ResetSettings(ApexSettings settings)
        {
            settings.enabled = true;
            settings.animate = true;
            settings.exceptTypes = null;
        }

        /// <summary>
        /// Apply settings to internal classes at editor launch.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void LoadSettings() { instance.ApplySettings(); }

        #endregion

        #region [Getter / Setter]

        public bool Enabled() { return enabled; }

        public void Enabled(bool value) { enabled = value; }

        public bool Animate() { return animate; }

        public void Animate(bool value) { animate = value; }

        public bool ValidateOnStartup() { return validateOnStartup; }

        public void ValidateOnStartup(bool value) { validateOnStartup = value; }

        public ExceptType[] GetExceptTypes() { return exceptTypes; }

        public void SetExceptTypes(ExceptType[] exceptTypes) { this.exceptTypes = exceptTypes; }

        #endregion
    }
}