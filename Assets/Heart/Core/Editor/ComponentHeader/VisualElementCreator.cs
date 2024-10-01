using System;
using Pancake.Common;
using PancakeEditor.Common;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace PancakeEditor.ComponentHeader
{
    internal static class VisualElementCreator
    {
        private static Texture2D GetIcon(ButtonType buttonType)
        {
            switch (buttonType)
            {
                case ButtonType.Remove:
                    return EditorResources.IconRemoveComponent(Uniform.Theme);
                case ButtonType.PasteComponentValue:
                    return EditorResources.IconPasteComponentValues(Uniform.Theme);
                case ButtonType.CopyComponent:
                    return EditorResources.IconCopyComponent(Uniform.Theme);
                case ButtonType.LoadComponent:
                    return EditorResources.IconReloadComponent(Uniform.Theme);
                default:
                    throw new ArgumentOutOfRangeException(nameof(buttonType), buttonType, null);
            }
        }

        public static VisualElement CreateContainer(string headerElementName, Action onRefresh)
        {
            var container = new VisualElement {pickingMode = PickingMode.Ignore, style = {flexDirection = FlexDirection.RowReverse, height = 22}};

            var imageCreator = new ImageCreator(headerElementName, onRefresh);


            var hasILoadComponent = false;
            var hasBeenRenamed = false;

            foreach (var gameObject in Selection.gameObjects)
            {
                var component = gameObject.GetComponent(GetComponentName(headerElementName));
                if (component == null) hasBeenRenamed = true;
                else if (component is ILoadComponent) hasILoadComponent = true;
            }

            if (!hasBeenRenamed)
            {
                var removeComponentImage = imageCreator.CreateButton(ButtonType.Remove, DestroyComponent);
                var copyComponentElement = imageCreator.CreateButton(ButtonType.CopyComponent, CopyComponent);
                var pasteComponentValuesElement = imageCreator.CreateButton(ButtonType.PasteComponentValue, PasteComponentValues);
                container.Add(removeComponentImage);
                container.Add(pasteComponentValuesElement);
                container.Add(copyComponentElement);
            }

            if (hasILoadComponent)
            {
                var loadComponentElement = imageCreator.CreateButton(ButtonType.LoadComponent, LoadComponent);
                container.Add(loadComponentElement);
            }

            return container;
        }

        private static void DestroyComponent(Component component)
        {
            if (component == null)
            {
                Debug.LogWarning(
                    "This component cannot be removed because the component name has been changed or the source file no longer exists in the project, please use the RemoveComponent menu to remove it.");
                return;
            }

            Undo.DestroyObjectImmediate(component);
        }

        private static void LoadComponent(Component component)
        {
            var type = component.GetType();
            var map = type.GetInterfaceMap(typeof(ILoadComponent));

            for (var i = 0; i < map.InterfaceMethods.Length; i++)
            {
                if (map.InterfaceMethods[i].Name == "OnLoadComponents")
                {
                    var methodInfo = map.TargetMethods[i];
                    methodInfo.Invoke(component, null);
                    TooltipWindow.Show("Component Loaded!");
                    EditorUtility.SetDirty(component);
                    return;
                }
            }

            Debug.Log("Method not found");
        }

        private static void CopyComponent(Component component)
        {
            ComponentUtility.CopyComponent(component);
            Debug.Log($"Copied! '{component.GetType().Name}'");
            TooltipWindow.Show("Copied!");
        }

        private static void PasteComponentValues(Component component)
        {
            ComponentUtility.PasteComponentValues(component);
            Debug.Log($"Pasted! '{component.GetType().Name}'");
            TooltipWindow.Show("Pasted!");
        }

        private static string GetComponentName(string headerElementName)
        {
            return headerElementName switch
            {
                "TextMeshPro - TextHeader" => "TextMeshPro",
                "TextMeshPro - Text (UI)Header" => "TextMeshProUGUI",
                _ => headerElementName.Remove(headerElementName.Length - 6, 6).Replace(" ", "").Replace("(Script)", "")
            };
        }


        private sealed class ImageCreator
        {
            private readonly string _headerElementName;
            private readonly Action _onRefresh;

            public ImageCreator(string headerElementName, Action onRefresh)
            {
                _headerElementName = headerElementName;
                _onRefresh = onRefresh;
            }

            public Button CreateButton(ButtonType buttonType, Action<Component> action)
            {
                var button = new Button(() =>
                {
                    foreach (var gameObject in Selection.gameObjects)
                    {
                        var component = gameObject.GetComponent(GetComponentName(_headerElementName));
                        action?.Invoke(component);
                    }

                    _onRefresh();
                })
                {
                    style =
                    {
                        position = Position.Relative,
                        backgroundImage = GetIcon(buttonType),
                        backgroundColor = Color.clear,
                        borderTopWidth = 0,
                        borderBottomWidth = 0,
                        borderLeftWidth = 0,
                        borderRightWidth = 0,
                        paddingLeft = 0,
                        paddingRight = 0,
                        paddingTop = 0,
                        paddingBottom = 0,
                        marginLeft = 0,
                        marginRight = 0,
                        marginTop = 0,
                        marginBottom = 0,
                        top = 3,
                        right = 63 + (int) buttonType * 3,
                        width = 16,
                        height = 16
                    }
                };

                return button;
            }
        }
    }
}