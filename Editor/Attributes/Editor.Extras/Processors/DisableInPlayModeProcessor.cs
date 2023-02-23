using Pancake.AttributeDrawer;
using UnityEngine;

[assembly: RegisterPropertyDisableProcessor(typeof(DisableInPlayModeProcessor))]

namespace Pancake.AttributeDrawer
{
    public class DisableInPlayModeProcessor : PropertyDisableProcessor<DisableInPlayModeAttribute>
    {
        public override bool IsDisabled(Property property) { return Application.isPlaying != Attribute.Inverse; }
    }
}