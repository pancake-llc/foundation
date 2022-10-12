using System.IO;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ViewTarget(typeof(SceneSelecterAttribute))]
    sealed class SceneSelecterView : FieldView, ITypeValidationCallback
    {
        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized element.</param>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void OnGUI(Rect position, SerializedField element, GUIContent label)
        {
            switch (element.serializedProperty.propertyType)
            {
                case SerializedPropertyType.Integer:
                    OnIntegerGUI(position, element, label);
                    break;
                case SerializedPropertyType.String:
                    OnStringGUI(position, element, label);
                    break;
            }
        }

        public void OnIntegerGUI(Rect position, SerializedField element, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            bool isValid = true;
            string buttonLabel = "Select scene index...";
            if (element.GetInteger() >= 0)
            {
                if (element.GetInteger() <= EditorBuildSettings.scenes.Length - 1)
                {
                    EditorBuildSettingsScene scene = EditorBuildSettings.scenes[element.GetInteger()];
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
                menu.AddItem(new GUIContent("None (-1)"), false, () => element.SetInteger(-1));
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
                        menu.AddItem(new GUIContent($"{sceneName} ({i})"), false, () => element.SetInteger(index));
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
                menu.AddItem(new GUIContent("None (-1)"), false, () => element.SetString(string.Empty));
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
                        menu.AddItem(new GUIContent($"{sceneName} ({i})"), false, () => element.SetString(sceneName));
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