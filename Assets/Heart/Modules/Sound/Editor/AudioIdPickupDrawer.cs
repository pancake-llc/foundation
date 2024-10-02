using Pancake.Sound;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Sound
{
    [CustomPropertyDrawer(typeof(AudioPickupAttribute))]
    public class AudioIdPickupDrawer : PropertyDrawer
    {
        private string _selectedId;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.type != nameof(AudioId)) return;

            var idProperty = property.FindPropertyRelative("id");
            var nameProperty = property.FindPropertyRelative("name");

            EditorGUI.BeginProperty(position, label, property);
            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            EditorGUI.LabelField(labelRect, label);
            var buttonRect = new Rect(position.x + EditorGUIUtility.labelWidth + 2, position.y, position.width - EditorGUIUtility.labelWidth - 2, position.height);
            var buttonText = "Select type...";
            var buttonColor = Uniform.Error;

            if (!string.IsNullOrEmpty(idProperty.stringValue))
            {
                var allAudioAsset = ProjectDatabase.FindAll<AudioData>();
                foreach (var t in allAudioAsset)
                {
                    if (t.id == idProperty.stringValue)
                    {
                        buttonText = nameProperty.stringValue;
                        buttonColor = new Color(0.99f, 0.5f, 0.24f, 0.31f);
                        _selectedId = idProperty.stringValue;
                        break;
                    }

                    buttonText = "Failed load...";
                    buttonColor = Uniform.Error;
                }
            }

            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = buttonColor;
            if (GUI.Button(buttonRect, buttonText))
            {
                var menu = new GenericMenu();
                var allAudioAsset = ProjectDatabase.FindAll<AudioData>();

                menu.AddItem(new GUIContent("None"),
                    _selectedId == string.Empty,
                    () =>
                    {
                        SetAndApplyProperty(idProperty, string.Empty);
                        SetAndApplyProperty(nameProperty, string.Empty);
                        _selectedId = string.Empty;
                    });

                for (var i = 0; i < allAudioAsset.Count; i++)
                {
                    var audioData = allAudioAsset[i];
                    menu.AddItem(new GUIContent($"{audioData.name}"),
                        audioData.id == _selectedId,
                        () =>
                        {
                            SetAndApplyProperty(idProperty, audioData.id);
                            SetAndApplyProperty(nameProperty, audioData.name);
                            _selectedId = audioData.id;
                        });
                }

                menu.DropDown(buttonRect);
            }

            GUI.backgroundColor = originalColor;
            EditorGUI.EndProperty();
        }

        private void SetAndApplyProperty(SerializedProperty property, string value)
        {
            property.stringValue = value;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}