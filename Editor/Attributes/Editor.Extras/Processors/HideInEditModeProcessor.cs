using Pancake.AttributeDrawer;
using UnityEngine;

[assembly: RegisterPropertyHideProcessor(typeof(HideInEditModeProcessor))]

namespace Pancake.AttributeDrawer
{
    public class HideInEditModeProcessor : PropertyHideProcessor<HideInEditModeAttribute>
    {
        public override bool IsHidden(Property property) { return Application.isPlaying == Attribute.Inverse; }
    }
}