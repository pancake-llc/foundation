using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityToolbarExtender
{
    [FilePath("UserSettings/CustomToolbarSetting.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class CustomToolbarSetting : ScriptableSingleton<CustomToolbarSetting>
    {
        [SerializeReference] private List<BaseToolbarElement> elements = new() {new ToolbarSides(), new ToolbarSpace(), new ToolbarTimeslider(),};

        internal List<BaseToolbarElement> Elements => elements;

        internal static SerializedObject GetSerializedSetting() { return new SerializedObject(instance); }

        internal void Save() { Save(true); }
    }
}