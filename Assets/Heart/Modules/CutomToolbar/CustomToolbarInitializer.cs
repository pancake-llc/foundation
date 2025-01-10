using System;
using System.Collections.Generic;
using System.Linq;
using Pancake;
using UnityEditor;

namespace UnityToolbarExtender
{
    [InitializeOnLoad]
    public static class CustomToolbarInitializer
    {
        static CustomToolbarInitializer()
        {
            var elements = MakeSetting();

            elements.ForEach(element => element.Init());

            var leftTools = elements.TakeWhile(element => !(element is ToolbarSides)).ToList();
            var rightTools = elements.Except(leftTools).ToList();
            var leftToolsDrawActions = leftTools.Select(TakeDrawAction);
            var rightToolsDrawActions = rightTools.Select(TakeDrawAction);

            ToolbarExtender.LeftToolbarGUI.AddRange(leftToolsDrawActions);
            ToolbarExtender.RightToolbarGUI.AddRange(rightToolsDrawActions);
        }

        internal static List<BaseToolbarElement> MakeSetting()
        {
            var elements = new List<BaseToolbarElement>();

            // left side
            if (HeartEditorSettings.ToolbarTimeScale.leftSide && HeartEditorSettings.ToolbarTimeScale.enabled)
            {
                elements.Add(new ToolbarTimeslider(width: HeartEditorSettings.ToolbarTimeScale.width));
            }

            elements.Add(new ToolbarSides());

            // right side
            elements.Add(new ToolbarSpace());
            if (!HeartEditorSettings.ToolbarTimeScale.leftSide && HeartEditorSettings.ToolbarTimeScale.enabled)
            {
                elements.Add(new ToolbarTimeslider(width: HeartEditorSettings.ToolbarTimeScale.width));
            }

            return elements;
        }

        private static Action TakeDrawAction(BaseToolbarElement element)
        {
            Action action = element.DrawInToolbar;
            return action;
        }
    }
}