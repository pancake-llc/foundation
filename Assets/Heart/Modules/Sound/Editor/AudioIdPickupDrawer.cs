using System.Linq;
using Pancake.Common;
using Pancake.Linq;
using Pancake.Sound;
using PancakeEditor.Common;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
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
            var buttonText = "Select Audio...";
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
                var allAudioAsset = ProjectDatabase.FindAll<AudioData>();
                var selector = new GenericSelector<AudioData>("Select Audio", allAudioAsset, false, item => item.name);

                selector.SetSelection(allAudioAsset.Filter(t => t.id == _selectedId));

                selector.SelectionConfirmed += selection =>
                {
                    var audioDatas = selection as AudioData[] ?? selection.ToArray();
                    if (audioDatas.IsNullOrEmpty()) return;

                    var audioData = audioDatas[0];
                    value.id = audioData.id;
                    value.name = audioData.name;
                    _selectedId = audioData.id;
                    GUIHelper.RequestRepaint();
                };

                selector.ShowInPopup();
            }

            GUI.backgroundColor = originalColor;

            EditorGUIUtility.labelWidth = prev;
            ValueEntry.SmartValue = value;
        }
    }
}