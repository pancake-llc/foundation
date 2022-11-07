using Pancake.Editor;
using UnityEngine;

[assembly: RegisterPropertyDisableProcessor(typeof(DisableInEditModeProcessor))]

namespace Pancake.Editor
{
    public class DisableInEditModeProcessor : PropertyDisableProcessor<DisableInEditModeAttribute>
    {
        public override bool IsDisabled(Property property) { return Application.isPlaying == Attribute.Inverse; }
    }
}