using Pancake.AttributeDrawer;
using UnityEngine;

[assembly: RegisterPropertyDisableProcessor(typeof(DisableInEditModeProcessor))]

namespace Pancake.AttributeDrawer
{
    public class DisableInEditModeProcessor : PropertyDisableProcessor<DisableInEditModeAttribute>
    {
        public override bool IsDisabled(Property property) { return Application.isPlaying == Attribute.Inverse; }
    }
}