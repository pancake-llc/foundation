using Pancake.Common;
using Pancake.Sound;
using PancakeEditor.Common;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace PancakeEditor.Sound
{
    [CustomPropertyDrawer(typeof(SoundId))]
    public class SoundIdPropertyDrawer : PropertyDrawer
    {
        public const string DEFAULT_ID_NAME = "None";
        public const string ID_MISSING = "Missing";
        public const string TOOL_TIP = "refering to an AudioEntity";

        private readonly string _missingMessage = ID_MISSING.ToBold().ToItalic().SetColor(new Color(1f, 0.3f, 0.3f));
        private bool _isInit;
        private string _entityName;

        private readonly GUIStyle _dropdownStyle = new(EditorStyles.popup) {richText = true};

        private void Init(SerializedProperty idProp, SerializedProperty assetProp)
        {
            _isInit = true;

            if (idProp.intValue == 0)
            {
                _entityName = DEFAULT_ID_NAME;
                return;
            }

            if (idProp.intValue < 0)
            {
                _entityName = _missingMessage;
                return;
            }

            var audioType = AudioExtension.GetAudioType(idProp.intValue);
            if (!audioType.IsConcrete())
            {
                SetToMissing();
                return;
            }

            var asset = assetProp.objectReferenceValue as AudioAsset;
            if (asset != null && EditorAudioEx.TryGetEntityName(asset, idProp.intValue, out _entityName)) return;

            // TODO: Initializing this whenever an SoundId is created is not efficient. 
            foreach (string guid in LibraryDataContainer.Data.Settings.guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath) as AudioAsset;
                if (asset != null && EditorAudioEx.TryGetEntityName(asset, idProp.intValue, out _entityName))
                {
                    assetProp.objectReferenceValue = asset;
                    assetProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    return;
                }
            }

            SetToMissing();

            void SetToMissing()
            {
                idProp.intValue = -1;
                _entityName = _missingMessage;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var idProp = property.FindPropertyRelative(nameof(SoundId.id));
            var assetProp = property.FindPropertyRelative(SoundId.NameOf.SourceAsset);

            if (!_isInit) Init(idProp, assetProp);

            var suffixRect = EditorGUI.PrefixLabel(position, new GUIContent(property.displayName, TOOL_TIP));

            if (EditorGUI.DropdownButton(suffixRect, new GUIContent(_entityName, TOOL_TIP), FocusType.Keyboard, _dropdownStyle))
            {
                var dropdown = new SoundIdAdvancedDropdown(new AdvancedDropdownState(), OnSelect);
                dropdown.Show(suffixRect);
            }

            if (AudioEditorSetting.ShowAudioTypeOnSoundId && assetProp.objectReferenceValue is IAudioAsset audioAsset && idProp.intValue > 0)
            {
                var audioType = AudioExtension.GetAudioType(idProp.intValue);
                var audioTypeRect = suffixRect.DissolveHorizontal(0.7f);
                EditorGUI.DrawRect(audioTypeRect, AudioEditorSetting.Instance.GetAudioTypeColor(audioType));
                EditorGUI.LabelField(audioTypeRect, audioType.ToString(), Uniform.CenterRichLabel);
            }

            void OnSelect(int id, string name, ScriptableObject asset)
            {
                idProp.intValue = id;
                _entityName = name;
                assetProp.objectReferenceValue = asset;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}