using System;
using System.Linq;
using UnityEditor;

namespace UnityToolbarExtender
{
    [InitializeOnLoad]
    public static class CustomToolbarInitializer
    {
        static CustomToolbarInitializer()
        {
            var setting = ScriptableSingleton<CustomToolbarSetting>.instance;
            setting.Elements.ForEach(element => element.Init());

            var leftTools = setting.Elements.TakeWhile(element => !(element is ToolbarSides)).ToList();
            var rightTools = setting.Elements.Except(leftTools).ToList();
            var leftToolsDrawActions = leftTools.Select(TakeDrawAction);
            var rightToolsDrawActions = rightTools.Select(TakeDrawAction);

            ToolbarExtender.LeftToolbarGUI.AddRange(leftToolsDrawActions);
            ToolbarExtender.RightToolbarGUI.AddRange(rightToolsDrawActions);
        }

        private static Action TakeDrawAction(BaseToolbarElement element)
        {
            Action action = element.DrawInToolbar;
            return action;
        }
    }
}