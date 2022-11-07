using Pancake.Editor;
using UnityEngine;

[assembly: RegisterPropertyDisableProcessor(typeof(DisableInPlayModeProcessor))]

namespace Pancake.Editor
{
    public class DisableInPlayModeProcessor : PropertyDisableProcessor<DisableInPlayModeAttribute>
    {
        public override bool IsDisabled(Property property) { return Application.isPlaying != Attribute.Inverse; }
    }
}