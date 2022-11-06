namespace Pancake.Editor.Guide
{
    using System;
    using UnityEngine;

    public class Styling_PropertyTooltipSample : ScriptableObject
    {
        [PropertyTooltip("This is tooltip")] public Rect rect;

        [PropertyTooltip("$" + nameof(DynamicTooltip))] public Vector3 vec;

        public string DynamicTooltip => DateTime.Now.ToShortTimeString();
    }
}