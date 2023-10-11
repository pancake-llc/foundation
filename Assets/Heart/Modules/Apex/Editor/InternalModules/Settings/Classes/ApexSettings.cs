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

        [SerializeField] private bool master = true;

        [SerializeField] private bool animate = true;

        [SerializeField] [Array(OnGUI = nameof(OnExceptTypeGUI), GetHeight = nameof(GetExceptTypeHeight))]
        private ExceptType[] exceptTypes;

        /// <summary>
        /// Saves the current state of the Apex settings.
        /// </summary>
        public void Save()
        {
            ApplySettings();
            Save(true);
        }

        /// <summary>
        /// Apply settings for internal classes.
        /// </summary>
        private void ApplySettings()
        {
            ApexUtility.Enabled = enabled;
            ApexUtility.Master = master;
            ApexGUIUtility.Animate = animate;

            ApexUtility.ExceptTypes.Clear();
            ApexUtility.ExceptTypes.Add(new ExceptType("UnityEventBase", true));
            ApexUtility.ExceptTypes.Add(new ExceptType("Optional`1", true));

            if (exceptTypes != null)
            {
                for (int i = 0; i < exceptTypes.Length; i++)
                {
                    ExceptType exceptType = exceptTypes[i];
                    if (!ApexUtility.ExceptTypes.Add(exceptType))
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

            position.x = position.xMax + EditorGUIUtility.standardVerticalSpacing;
            position.width = 20;
            EditorGUI.PropertyField(position, subClasses, GUIContent.none);
            GUI.backgroundColor = Color.white;
        }

        private float GetExceptTypeHeight(SerializedProperty array, int index) { return EditorGUIUtility.singleLineHeight; }

        #endregion

        #region [Static Methods]

        /// <summary>
        /// Reset specific settings to default.
        /// </summary>
        /// <param name="settings">Settings reference.</param>
        public static void ResetSettings(ApexSettings settings)
        {
            settings.enabled = true;
            settings.master = true;
            settings.animate = true;
            settings.exceptTypes = null;
        }

        /// <summary>
        /// Apply settings when Unity loads.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void LoadSettings() { instance.ApplySettings(); }

        #endregion

        #region [Getter / Setter]

        public bool Enabled() { return enabled; }

        public void Enabled(bool value) { enabled = value; }
        
        public bool Master()
        {
            return master;
        }

        public void Master(bool value)
        {
            master = value;
        }

        public bool Animate() { return animate; }

        public void Animate(bool value) { animate = value; }

        public ExceptType[] GetExceptTypes() { return exceptTypes; }

        public void SetExceptTypes(ExceptType[] exceptTypes) { this.exceptTypes = exceptTypes; }

        #endregion
    }
}