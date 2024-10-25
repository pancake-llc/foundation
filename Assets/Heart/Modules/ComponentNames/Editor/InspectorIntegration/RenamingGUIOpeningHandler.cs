using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;
using Sisus.Shared.EditorOnly;

namespace Sisus.ComponentNames.Editor
{
    [InitializeOnLoad]
	internal static class RenamingGUIOpeningHandler
	{
        private static readonly GUIContent tooltip = new("", "");
        private static readonly GUIContent label = new("");

        static RenamingGUIOpeningHandler()
		{
            ComponentHeader.BeforeHeaderGUI -= BeginComponentHeader;
            ComponentHeader.BeforeHeaderGUI += BeginComponentHeader;
            ComponentName.InspectorTitleChanged -= OnInspectorTitleChanged;
            ComponentName.InspectorTitleChanged += OnInspectorTitleChanged;

            static void OnInspectorTitleChanged(Component component, NameWithSuffix newTitle) => InjectTitleIntoInternalCache(component, ComponentName.GetInspectorTitle(component));
		}

        private static float BeginComponentHeader(Component component, Rect headerRect, bool headerIsSelected, bool headerSupportsRichText)
		{
            HandleOpeningRenamingGUI(component, headerRect, headerIsSelected);

			var inspectorTitle = ComponentName.GetInspectorTitle(component);
			string titleWithSuffixPlainText = inspectorTitle.ToString(richText:true);
            if(headerSupportsRichText)
			{
                InjectTitleIntoInternalCache(component, inspectorTitle.ToString(richText:true));
            }
            else
            {
				InjectTitleIntoInternalCache(component, titleWithSuffixPlainText);
            }

            var tooltipRect = headerRect;
            tooltipRect.x += 60f;
            tooltipRect.width -= 125f + 16 * 4 + 3 * 3;

            label.text = titleWithSuffixPlainText;
            float titleWidth = EditorStyles.boldLabel.CalcSize(label).x;
            if(titleWidth < tooltipRect.width)
			{
                tooltipRect.width = titleWidth;
			}

            tooltip.tooltip = ComponentTooltip.Get(component);
            GUI.Label(tooltipRect, tooltip);

            return 0f;
        }

        private static void HandleOpeningRenamingGUI(Component component, Rect headerRect, bool headerIsSelected)
        {
            switch(Event.current.rawType)
            {
                case EventType.ValidateCommand:
                    if(Event.current.commandName == "Rename" && headerIsSelected)
                    {
                        Event.current.Use();
                        BeginRenamingComponent(component, headerRect);
                    }
                    break;
                case EventType.KeyDown:
                    if(!headerIsSelected)
                    {
                        break;
                    }

                    switch(Event.current.keyCode)
                    {
                        case KeyCode.F2:
                        case KeyCode.KeypadEnter:
                        case KeyCode.Return:
                            Event.current.Use();
                            BeginRenamingComponent(component, headerRect);
                            break;
                    }
                    break;
            }

            if(NameContainer.StartingToRename == component)
			{
                NameContainer.StartingToRename = null;
                BeginRenamingComponent(component, headerRect);
            }
        }

        private static void BeginRenamingComponent(Component component, Rect headerRect)
        {
            var openAt = InScreenSpace(GetLabelRect(headerRect));
            RenameComponentWindow.Open(openAt, component);
        }

        private static void InjectTitleIntoInternalCache([DisallowNull] Component component, string title)
		{
			var componentType = component.GetType();
			ComponentName.EnsureOriginalInspectorTitleIsCached(component, componentType);
			ObjectNamesUtility.InternalInspectorTitlesCache[componentType] = title;
		}

        private static Rect InScreenSpace(Rect rect)
        {
            rect.position = GUIUtility.GUIToScreenPoint(rect.position);
            return rect;
        }

        private static Rect GetLabelRect(Rect headerRect)
        {
            var headerLabelRect = headerRect;

            headerLabelRect.x = 54f;

            // Fixes Transform header label rect position.
            // For some reason the Transform header rect starts
            // lower and is shorter than all other headers.
            if(headerLabelRect.height < 22f)
            {
                headerLabelRect.y -= 22f - 15f;
            }

            headerLabelRect.width = Screen.width - 123f;
            headerLabelRect.height = 20f;

            return headerLabelRect;
        }
    }
}