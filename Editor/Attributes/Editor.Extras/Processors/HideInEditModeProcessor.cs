using Pancake.Attribute;
using PancakeEditor.Attribute;
using UnityEngine;

[assembly: RegisterPropertyHideProcessor(typeof(HideInEditModeProcessor))]

namespace PancakeEditor.Attribute
{
    public class HideInEditModeProcessor : PropertyHideProcessor<HideInEditModeAttribute>
    {
        public override bool IsHidden(Property property) { return Application.isPlaying == Attribute.Inverse; }
    }
}