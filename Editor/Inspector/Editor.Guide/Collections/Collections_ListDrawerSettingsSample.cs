namespace Pancake.Editor.Guide
{
    using System.Collections.Generic;
    using UnityEngine;

    public class Collections_ListDrawerSettingsSample : ScriptableObject
    {
        [ListDrawerSettings(Draggable = true, HideAddButton = false, HideRemoveButton = false, AlwaysExpanded = false)]
        public List<Material> list;

        [ListDrawerSettings(Draggable = false, AlwaysExpanded = true)]
        public Vector3[] vectors;
    }
}