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

        [SerializeField] [OnValueChanged(nameof(IncludeAllAssembliesChanged))]
        private bool includeAllAssemblies = true;

        [SerializeField] [DisableIf(nameof(includeAllAssemblies))] private bool includeDefaultAssemblies = true;

        [SerializeField] [Array] [HideIf(nameof(includeAllAssemblies))]
        private AssemblyScope[] assemblyScopes;

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
            ApexUtility.IncludeAllAssemblies = includeAllAssemblies;
            ApexUtility.IncludeDefaultAssemblies = includeDefaultAssemblies;
            ApexGUIUtility.Animate = animate;

            ApexUtility.AssemblyScopes.Clear();
            if (assemblyScopes != null)
            {
                for (int i = 0; i < assemblyScopes.Length; i++)
                {
                    AssemblyScope assemblyScope = assemblyScopes[i];
                    if (string.IsNullOrWhiteSpace(assemblyScope.GetName()))
                    {
                        Debug.LogWarning(
                            $"<b>Message:</b> An attempt to add assembly scope with empty or white space name. Invalid scope: <color=red>index {i + 1}</color>");
                        continue;
                    }

                    if (!ApexUtility.AssemblyScopes.Add(assemblyScope))
                    {
                        Debug.LogWarning(
                            $"<b>Message:</b> An attempt to add two identical assembly scope. Duplicate scopes: <color=red>{assemblyScope.GetName()}</color>");
                    }
                }
            }

            ApexUtility.ExceptTypes.Clear();
            ApexUtility.ExceptTypes.Add(new ExceptType("UnityEventBase", true));
            ApexUtility.ExceptTypes.Add(new ExceptType("Optional`1", true));
            ApexUtility.ExceptTypes.Add(new ExceptType("Bag`1", true));

            if (exceptTypes != null)
            {
                for (int i = 0; i < exceptTypes.Length; i++)
                {
                    ExceptType exceptType = exceptTypes[i];
                    if (string.IsNullOrWhiteSpace(exceptType.GetName()))
                    {
                        Debug.LogWarning($"<b>Message:</b> An attempt to add type with empty or white space name. Invalid type: <color=red>index {i + 1}</color>");
                        continue;
                    }

                    if (!ApexUtility.ExceptTypes.Add(exceptType))
                    {
                        Debug.LogWarning(
                            $"<b>Message:</b> An attempt to add two identical types for an exception. Remove duplicate types: <color=red>{exceptType.GetName()}</color>");
                    }
                }
            }
        }

        #region [Apex Callbacks]

        private void IncludeAllAssembliesChanged(SerializedProperty property)
        {
            if (property.boolValue)
            {
                includeDefaultAssemblies = true;
                property.serializedObject.Update();
            }
        }

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

        #region [Static]

        /// <summary>
        /// Reset specific settings to default.
        /// </summary>
        /// <param name="settings">Settings reference.</param>
        public static void ResetSettings(ApexSettings settings)
        {
            settings.enabled = true;
            settings.master = true;
            settings.animate = true;
            settings.assemblyScopes = null;
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

        public bool Master() { return master; }

        public void Master(bool value) { master = value; }

        public bool Animate() { return animate; }

        public void Animate(bool value) { animate = value; }

        public bool IncludeAllAssembly() { return includeAllAssemblies; }

        public void IncludeAllAssembly(bool value) { includeAllAssemblies = value; }

        public bool IncludeDefaultAssembly() { return includeDefaultAssemblies; }

        public void IncludeDefaultAssembly(bool value) { includeDefaultAssemblies = value; }

        public ExceptType[] GetExceptTypes() { return exceptTypes; }

        public void SetExceptTypes(ExceptType[] exceptTypes) { this.exceptTypes = exceptTypes; }

        public AssemblyScope[] GetAssemblyScopes() { return assemblyScopes; }

        public void SetAssemblyScopes(AssemblyScope[] value) { assemblyScopes = value; }

        #endregion
    }
}