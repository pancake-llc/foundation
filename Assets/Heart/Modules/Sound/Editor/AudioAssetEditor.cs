using Pancake.Sound;
using PancakeEditor.Common;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static Pancake.Sound.AudioEntity;

namespace PancakeEditor.Sound
{
    [CustomEditor(typeof(AudioAsset), true)]
    public class AudioAssetEditor : UnityEditor.Editor
    {
        private ReorderableList _entitiesList;
        private IUniqueIdGenerator _idGenerator;
        private EValidationErrorCode _entityIssue;
        public string CurrMessage { get; private set; }

        public IAudioAsset Asset { get; private set; }

        public void AddEntitiesNameChangeListener() { AudioEntityPropertyDrawer.OnEntityNameChanged += Verify; }

        public void RemoveEntitiesNameChangeListener() { AudioEntityPropertyDrawer.OnEntityNameChanged -= Verify; }

        public void Init(IUniqueIdGenerator idGenerator)
        {
            Asset = target as IAudioAsset;
            _idGenerator = idGenerator;
            InitReorderableList();
        }

        public void SetData(string guid, string assetName)
        {
            string assetGuidPropertyPath = EditorAudioEx.GetFieldName(nameof(IAudioAsset.AssetGuid));
            serializedObject.FindProperty(assetGuidPropertyPath).stringValue = guid;

            string assetNamePropertyPath = EditorAudioEx.GetBackingFieldName(nameof(IAudioAsset.AssetName));
            serializedObject.FindProperty(assetNamePropertyPath).stringValue = assetName;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private void InitReorderableList()
        {
            if (Asset != null)
            {
                _entitiesList = new ReorderableList(serializedObject,
                    serializedObject.FindProperty(nameof(AudioAsset.entities)),
                    true,
                    false,
                    true,
                    true)
                {
                    onAddCallback = OnAdd,
                    onRemoveCallback = OnRemove,
                    drawElementCallback = OnDrawElement,
                    elementHeightCallback = OnGetPropertyHeight,
                    onReorderCallback = OnReorder,
                };
            }

            void OnAdd(ReorderableList list)
            {
                var audioType = EAudioType.None;
                if (list.count > 0)
                {
                    var lastElementProp = list.serializedProperty.GetArrayElementAtIndex(list.count - 1);
                    int lastElementId = lastElementProp.FindPropertyRelative(EditorPropertyName.Id).intValue;
                    audioType = AudioExtension.GetAudioType(lastElementId);
                }

                ReorderableList.defaultBehaviours.DoAddButton(list);
                var newEntity = list.serializedProperty.GetArrayElementAtIndex(list.count - 1);
                EditorAudioEx.ResetEntitySerializedProperties(newEntity);
                AssignID(newEntity, audioType);
                serializedObject.ApplyModifiedProperties();
            }

            void OnRemove(ReorderableList list)
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                serializedObject.ApplyModifiedProperties();
            }

            void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                var elementProp = _entitiesList.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(rect, elementProp);
                if (EditorGUI.EndChangeCheck()) elementProp.serializedObject.ApplyModifiedProperties();
            }

            void OnReorder(ReorderableList list) { list.serializedProperty.serializedObject.ApplyModifiedProperties(); }

            float OnGetPropertyHeight(int index) { return EditorGUI.GetPropertyHeight(_entitiesList.serializedProperty.GetArrayElementAtIndex(index)); }
        }

        private void AssignID(SerializedProperty entityProp, EAudioType audioType) { AssignID(_idGenerator.GetSimpleUniqueId(audioType), entityProp); }

        private void AssignID(int id, SerializedProperty entityProp)
        {
            var idProp = entityProp.FindPropertyRelative(EditorPropertyName.Id);
            idProp.intValue = id;
            entityProp.serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Audio in Wizard"))
            {
                EditorApplication.ExecuteMenuItem("Tools/Pancake/Wizard");
                // Asset ??= target as IAudioAsset;
                // window.SelectAsset(Asset.AssetGuid);
                // Init(window.IdGenerator);
            }
        }

        public void DrawEntitiesList() { _entitiesList.DoLayoutList(); }

        public void SetAssetName(string newName)
        {
            var asset = Asset as AudioAsset;
            string path = AssetDatabase.GetAssetPath(asset);
            AssetDatabase.RenameAsset(path, newName);

            serializedObject.Update();
            serializedObject.FindProperty(EditorAudioEx.GetBackingFieldName(nameof(AudioAsset.AssetName))).stringValue = newName;
            serializedObject.ApplyModifiedProperties();
        }

        public SerializedProperty CreateNewEntity()
        {
            ReorderableList.defaultBehaviours.DoAddButton(_entitiesList);
            var entitiesProp = serializedObject.FindProperty(nameof(AudioAsset.entities));
            var newEntity = entitiesProp.GetArrayElementAtIndex(_entitiesList.count - 1);
            EditorAudioEx.ResetEntitySerializedProperties(newEntity);

            AssignID(newEntity, EAudioType.None);

            return newEntity;
        }

        public void SetClipList(SerializedProperty clipListProp, int index, AudioClip clip)
        {
            clipListProp.InsertArrayElementAtIndex(index);
            var elementProp = clipListProp.GetArrayElementAtIndex(index);
            elementProp.FindPropertyRelative(SoundClip.EditorPropertyName.AudioClip).objectReferenceValue = clip;
            elementProp.FindPropertyRelative(SoundClip.EditorPropertyName.Volume).floatValue = AudioConstant.FULL_VOLUME;
        }

        public void Verify()
        {
            if (VerifyAsset()) CurrMessage = string.Empty;
        }

        private bool VerifyAsset()
        {
            if (PancakeEditor.Common.Editor.IsInvalidName(Asset.AssetName, out var code))
            {
                switch (code)
                {
                    case EValidationErrorCode.IsNullOrEmpty:
                        CurrMessage = "Please enter an asset name";
                        break;
                    case EValidationErrorCode.StartWithNumber:
                        CurrMessage = "Name starts with number is not recommended";
                        break;
                    case EValidationErrorCode.ContainsInvalidWord:
                        CurrMessage = "Contains invalid words!";
                        break;
                    case EValidationErrorCode.ContainsWhiteSpace:
                        CurrMessage = "Name with whitespace is not recommended";
                        break;
                }

                return false;
            }

            if (EditorAudioEx.IsTempReservedName(Asset.AssetName))
            {
                CurrMessage = $"[{Asset.AssetName}] has been reserved for temp asset";
                return false;
            }

            return true;
        }
    }
}