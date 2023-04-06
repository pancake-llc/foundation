using Pancake.Attribute;
using PancakeEditor.Attribute;
using UnityEngine;

[assembly: RegisterPropertyHideProcessor(typeof(HideInPlayModeProcessor))]

namespace PancakeEditor.Attribute
{
    public class HideInPlayModeProcessor : PropertyHideProcessor<HideInPlayModeAttribute>
    {
        public override bool IsHidden(Property property) { return Application.isPlaying != Attribute.Inverse; }
    }
}