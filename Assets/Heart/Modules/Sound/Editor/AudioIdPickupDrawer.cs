using Alchemy.Editor;
using Pancake.Sound;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomAttributeDrawer(typeof(AudioPickupAttribute))]
public class AudioIdPickupDrawer : AlchemyAttributeDrawer
{
    public override void OnCreateElement()
    {
        var idProperty = SerializedProperty.FindPropertyRelative("id");
        var nameProperty = SerializedProperty.FindPropertyRelative("name");

        // Create a container for the horizontal layout
        var container = new VisualElement {style = {flexDirection = FlexDirection.Row, alignItems = Align.Center}};

        var label = new Label(SerializedProperty.name) {style = {marginRight = 25}};

        var textField = new TextField
        {
            value = nameProperty.stringValue,
            isReadOnly = true,
            focusable = false,
            style =
            {
                flexGrow = 1, // Make the text field expand to fill the available space
                marginRight = 5 // Add some margin to the right for spacing
            }
        };
        textField.SetEnabled(false);

        var button = new Button {text = "Select type..."};

        // Set the initial text of the button
        if (!string.IsNullOrEmpty(idProperty.stringValue))
        {
            var allAudioAsset = ProjectDatabase.FindAll<AudioData>();
            foreach (var t in allAudioAsset)
            {
                if (t.id == idProperty.stringValue)
                {
                    button.text = nameProperty.stringValue;
                    textField.value = nameProperty.stringValue;
                    break;
                }

                button.text = "Failed load...";
            }
        }

        button.clicked += () =>
        {
            var menu = new GenericMenu();
            var allAudioAsset = ProjectDatabase.FindAll<AudioData>();
            menu.AddItem(new GUIContent("None (-1)"),
                false,
                () =>
                {
                    SetAndApplyProperty(idProperty, string.Empty);
                    textField.value = string.Empty;
                    button.text = "Select type...";
                });
            for (var i = 0; i < allAudioAsset.Count; i++)
            {
                if (i == 0) menu.AddSeparator("");
                int cachei = i;
                menu.AddItem(new GUIContent($"{allAudioAsset[cachei].name} ({cachei})"),
                    false,
                    () =>
                    {
                        SetAndApplyProperty(idProperty, allAudioAsset[cachei].id);
                        SetAndApplyProperty(nameProperty, allAudioAsset[cachei].name);
                        textField.value = allAudioAsset[cachei].name;
                        button.text = allAudioAsset[cachei].name;
                    });
            }

            menu.DropDown(new Rect(button.worldBound.position, Vector2.zero));
        };


        container.Add(label);
        container.Add(textField);
        container.Add(button);

        // Add the container to the target element, which will hide default UI
        TargetElement.Clear();
        TargetElement.Add(container);
    }

    private void SetAndApplyProperty(SerializedProperty property, string value)
    {
        property.stringValue = value;
        property.serializedObject.ApplyModifiedProperties();
    }
}