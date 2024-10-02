using Pancake.Sound;
using PancakeEditor.Common;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Sound
{
    public class AudioIdPickupDrawer : OdinValueDrawer<AudioId>
    {
        private string _selectedId;

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var value = ValueEntry.SmartValue;
            var position = EditorGUILayout.GetControlRect();
            if (label != null)
            {
                var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
                EditorGUI.LabelField(labelRect, ObjectNames.NicifyVariableName(label.text.ToCamelCase()));
            }

            float prev = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 150;

            var buttonRect = new Rect(position.x + position.width * 0.4f, position.y, position.width * 0.6f, position.height);
            var buttonText = "Select type...";
            var buttonColor = Uniform.Error;

            if (!string.IsNullOrEmpty(value.id))
            {
                var allAudioAsset = ProjectDatabase.FindAll<AudioData>();
                foreach (var t in allAudioAsset)
                {
                    if (t.id == value.id)
                    {
                        buttonText = value.name;
                        buttonColor = new Color(0.99f, 0.5f, 0.24f, 0.31f);
                        _selectedId = value.id;
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
                        value.id = string.Empty;
                        value.name = string.Empty;
                        _selectedId = string.Empty;
                    });

                for (var i = 0; i < allAudioAsset.Count; i++)
                {
                    var audioData = allAudioAsset[i];
                    menu.AddItem(new GUIContent($"{audioData.name}"),
                        audioData.id == _selectedId,
                        () =>
                        {
                            value.id = audioData.id;
                            value.name = audioData.name;
                            _selectedId = audioData.id;
                        });
                }

                menu.DropDown(buttonRect);
            }

            GUI.backgroundColor = originalColor;

            EditorGUIUtility.labelWidth = prev;
            ValueEntry.SmartValue = value;
        }
    }
}