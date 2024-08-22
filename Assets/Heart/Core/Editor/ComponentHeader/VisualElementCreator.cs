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
                case ButtonType.MoveUp:
                    return EditorResources.IconMoveUp(Uniform.Theme);
                case ButtonType.MoveDown:
                    return EditorResources.IconMoveDown(Uniform.Theme);
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
            var container = new VisualElement {pickingMode = PickingMode.Ignore, style = {flexDirection = FlexDirection.RowReverse, height = 22,}};

            var imageCreator = new ImageCreator(headerElementName, onRefresh);
            var removeComponentImage = imageCreator.CreateButton(ButtonType.Remove, DestroyComponent);
            var moveUpElement = imageCreator.CreateButton(ButtonType.MoveUp, x => ComponentUtility.MoveComponentUp(x));
            var moveDownElement = imageCreator.CreateButton(ButtonType.MoveDown, x => ComponentUtility.MoveComponentDown(x));
            var copyComponentElement = imageCreator.CreateButton(ButtonType.CopyComponent, CopyComponent);
            var pasteComponentValuesElement = imageCreator.CreateButton(ButtonType.PasteComponentValue, PasteComponentValues);
            var loadComponentElement = imageCreator.CreateButton(ButtonType.LoadComponent, LoadComponent);

            container.Add(removeComponentImage);
            container.Add(moveUpElement);
            container.Add(moveDownElement);
            container.Add(pasteComponentValuesElement);
            container.Add(copyComponentElement);
            container.Add(loadComponentElement);

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
            if (component is ILoadComponent)
            {
                var type = component.GetType();
                // Get the interface map for the ILoadComponent interface
                var map = type.GetInterfaceMap(typeof(ILoadComponent));

                // Find the corresponding method in the map
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
            else TooltipWindow.Show("Nothing happens, Need inherit from interface ILoadComponent!");
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
                    string componentName = _headerElementName switch
                    {
                        "TextMeshPro - TextHeader" => "TextMeshPro",
                        "TextMeshPro - Text (UI)Header" => "TextMeshProUGUI",

                        _ => _headerElementName.Remove(_headerElementName.Length - 6, 6).Replace(" ", "").Replace("(Script)", "")
                    };

                    foreach (var gameObject in Selection.gameObjects)
                    {
                        var component = gameObject.GetComponent(componentName);

                        action(component);
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
                        height = 16,
                    }
                };

                return button;
            }
        }
    }
}