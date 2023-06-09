using Pancake.Apex;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [ViewTarget(typeof(SceneSelecterAttribute))]
    public sealed class SceneSelecterView : FieldView, ITypeValidationCallback
    {
        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        public override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            switch (serializedField.GetSerializedProperty().propertyType)
            {
                case SerializedPropertyType.Integer:
                    OnIntegerGUI(position, serializedField, label);
                    break;
                case SerializedPropertyType.String:
                    OnStringGUI(position, serializedField, label);
                    break;
            }
        }

        public void OnIntegerGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            bool isValid = true;
            string buttonLabel = "Select scene index...";
            if (serializedField.GetInteger() >= 0)
            {
                if (serializedField.GetInteger() <= EditorBuildSettings.scenes.Length - 1)
                {
                    EditorBuildSettingsScene scene = EditorBuildSettings.scenes[serializedField.GetInteger()];
                    buttonLabel = Path.GetFileNameWithoutExtension(scene.path);
                    if (!scene.enabled)
                    {
                        buttonLabel += " (Disabled)";
                        isValid = false;
                    }
                }
                else
                {
                    buttonLabel = "Failed load...";
                }
            }

            Color lastColor = GUI.color;
            if (!isValid)
            {
                GUI.color = Color.red;
            }

            if (GUI.Button(position, buttonLabel, EditorStyles.popup))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("None (-1)"),
                    false,
                    () =>
                    {
                        serializedField.SetInteger(-1);
                        serializedField.GetSerializedObject().ApplyModifiedProperties();
                    });
                for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                {
                    if (i == 0)
                    {
                        menu.AddSeparator("");
                    }

                    EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];
                    string sceneName = Path.GetFileNameWithoutExtension(scene.path);
                    if (scene.enabled)
                    {
                        int index = i;
                        menu.AddItem(new GUIContent($"{sceneName} ({i})"),
                            false,
                            () =>
                            {
                                serializedField.SetInteger(index);
                                serializedField.GetSerializedObject().ApplyModifiedProperties();
                            });
                    }
                    else
                    {
                        menu.AddDisabledItem(new GUIContent($"{sceneName} ({i})"));
                    }
                }

                menu.DropDown(position);
            }

            if (!isValid)
            {
                GUI.color = lastColor;
            }
        }

        public void OnStringGUI(Rect position, SerializedField element, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            bool isValid = true;
            string buttonLabel = "Select scene name...";
            if (!string.IsNullOrEmpty(element.GetString()))
            {
                for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                {
                    EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];
                    string sceneName = Path.GetFileNameWithoutExtension(scene.path);
                    if (sceneName == element.GetString())
                    {
                        buttonLabel = element.GetString();
                        if (!scene.enabled)
                        {
                            buttonLabel += " (Disabled)";
                        }

                        break;
                    }
                    else
                    {
                        buttonLabel = "Failed load...";
                    }
                }
            }

            Color lastColor = GUI.color;
            if (!isValid)
            {
                GUI.color = Color.red;
            }

            if (GUI.Button(position, buttonLabel, EditorStyles.popup))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("None (-1)"),
                    false,
                    () =>
                    {
                        element.SetString(string.Empty);
                        element.GetSerializedObject().ApplyModifiedProperties();
                    });
                for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                {
                    if (i == 0)
                    {
                        menu.AddSeparator("");
                    }

                    EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];
                    string sceneName = Path.GetFileNameWithoutExtension(scene.path);
                    if (scene.enabled)
                    {
                        menu.AddItem(new GUIContent($"{sceneName} ({i})"),
                            false,
                            () =>
                            {
                                element.SetString(sceneName);
                                element.GetSerializedObject().ApplyModifiedProperties();
                            });
                    }
                    else
                    {
                        menu.AddDisabledItem(new GUIContent($"{sceneName} ({i})"));
                    }
                }

                menu.DropDown(position);
            }

            if (!isValid)
            {
                GUI.color = lastColor;
            }
        }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.Integer || property.propertyType == SerializedPropertyType.String;
        }
    }
}