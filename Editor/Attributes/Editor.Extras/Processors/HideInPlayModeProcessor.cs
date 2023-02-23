using Pancake.AttributeDrawer;
using UnityEngine;

[assembly: RegisterPropertyHideProcessor(typeof(HideInPlayModeProcessor))]

namespace Pancake.AttributeDrawer
{
    public class HideInPlayModeProcessor : PropertyHideProcessor<HideInPlayModeAttribute>
    {
        public override bool IsHidden(Property property) { return Application.isPlaying != Attribute.Inverse; }
    }
}