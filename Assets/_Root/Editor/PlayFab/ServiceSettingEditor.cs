#if PANCAKE_PLAYFAB
using Pancake.GameService;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [CustomEditor(typeof(ServiceSettings))]
    public class ServiceSettingEditor : UnityEditor.Editor
    {
        public const string ENABLE_PLAYFABADMIN_API = "ENABLE_PLAYFABADMIN_API";
        public const string DISABLE_PLAYFABCLIENT_API = "DISABLE_PLAYFABCLIENT_API";
        public const string DISABLE_PLAYFABENTITY_API = "DISABLE_PLAYFABENTITY_API";
        public const string ENABLE_PLAYFABSERVER_API = "ENABLE_PLAYFABSERVER_API";
        public const string PLAYFAB_REQUEST_TIMING = "PLAYFAB_REQUEST_TIMING";
        
        public static bool callFromEditorWindow = false;
        private Property _titleId;
        private Property _secretKey;
        private Property _requestType;
        private Property _useCustomIdAsDefault;
        private Property _enableAdminApi;
        private Property _enableClientApi;
        private Property _enableEntityApi;
        private Property _enableServerApi;
        private Property _enableRequestTimesApi;
        private Property _infoRequestParams;

        private void Init()
        {
            _titleId = new Property(serializedObject.FindProperty("titleId"), new GUIContent("Title Id", "Title id of project"));
            _secretKey = new Property(serializedObject.FindProperty("secretKey"), new GUIContent("Secret Key", "Title secret key"));
            _requestType = new Property(serializedObject.FindProperty("requestType"), new GUIContent("Request Type", "Request type"));
            _useCustomIdAsDefault = new Property(serializedObject.FindProperty("useCustomIdAsDefault"),
                new GUIContent("Use Custom Id", "Use custom id instead of device id for specific splatform"));
            _enableAdminApi = new Property(serializedObject.FindProperty("enableAdminApi"), new GUIContent("Admin API", "Enable admin api"));
            _enableClientApi = new Property(serializedObject.FindProperty("enableClientApi"), new GUIContent("Client API", "Enable client api"));
            _enableEntityApi = new Property(serializedObject.FindProperty("enableEntityApi"), new GUIContent("Entity API", "Enable entity api"));
            _enableServerApi = new Property(serializedObject.FindProperty("enableServerApi"), new GUIContent("Server API", "Enable server api"));
            _enableRequestTimesApi = new Property(serializedObject.FindProperty("enableRequestTimesApi"),
                new GUIContent("Request Times API", "Enable request time api"));
            _infoRequestParams = new Property(serializedObject.FindProperty("infoRequestParams"), new GUIContent("Info Request Param"));
        }

        private void OnDisable()
        {
            EditorUtility.SetDirty(ServiceSettings.SharedSettings);
            AssetDatabase.SaveAssets();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Init();
            if (!callFromEditorWindow)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(
                    "This ScriptableObject holds all the settings of Playfab. Please go to menu Tools > Pancake > Playfab or click the button below to edit it.",
                    MessageType.Info);
                if (GUILayout.Button("Edit")) ServiceSettingWindow.ShowWindow();
                return;
            }

            EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling);

            Uniform.DrawUppercaseSection("PLAYFAB_SETTING",
                "SETTING",
                () =>
                {
                    EditorGUILayout.PropertyField(_titleId.property, _titleId.content);
                    EditorGUILayout.PropertyField(_secretKey.property, _secretKey.content);
                    EditorGUILayout.PropertyField(_requestType.property, _requestType.content);

                    ServiceSettings.SharedSettings.TitleId = ServiceSettings.TitleId;
#if ENABLE_PLAYFABSERVER_API || ENABLE_PLAYFABADMIN_API || UNITY_EDITOR
                    ServiceSettings.SharedSettings.DeveloperSecretKey = ServiceSettings.SecretKey;
#endif
                    ServiceSettings.SharedSettings.RequestType = ServiceSettings.RequestType;
                    Uniform.SpaceOneLine();
                    EditorGUILayout.PropertyField(_infoRequestParams.property, _infoRequestParams.content);
                });
            Uniform.SpaceOneLine();
            Uniform.DrawUppercaseSection("PLAYFAB_FEATURE",
                "API & FEATURE",
                () =>
                {
                    EditorGUILayout.PropertyField(_useCustomIdAsDefault.property, _useCustomIdAsDefault.content);
                    EditorGUILayout.PropertyField(_enableAdminApi.property, _enableAdminApi.content);
                    EditorGUILayout.PropertyField(_enableClientApi.property, _enableClientApi.content);
                    EditorGUILayout.PropertyField(_enableEntityApi.property, _enableEntityApi.content);
                    EditorGUILayout.PropertyField(_enableServerApi.property, _enableServerApi.content);
                    Uniform.SpaceTwoLine();
                    EditorGUILayout.PropertyField(_enableRequestTimesApi.property, _enableRequestTimesApi.content);

                    if (ServiceSettings.EnableAdminApi) InEditor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms(ENABLE_PLAYFABADMIN_API);
                    else InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms(ENABLE_PLAYFABADMIN_API);

                    if (ServiceSettings.EnableClientApi) InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms(DISABLE_PLAYFABCLIENT_API);
                    else InEditor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms(DISABLE_PLAYFABCLIENT_API);

                    if (ServiceSettings.EnableEntityApi) InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms(DISABLE_PLAYFABENTITY_API);
                    else InEditor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms(DISABLE_PLAYFABENTITY_API);

                    if (ServiceSettings.EnableServerApi) InEditor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms(ENABLE_PLAYFABSERVER_API);
                    else InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms(ENABLE_PLAYFABSERVER_API);

                    if (ServiceSettings.EnableRequestTimesApi) InEditor.ScriptingDefinition.AddDefineSymbolOnAllPlatforms(PLAYFAB_REQUEST_TIMING);
                    else InEditor.ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms(PLAYFAB_REQUEST_TIMING);
                });

            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif