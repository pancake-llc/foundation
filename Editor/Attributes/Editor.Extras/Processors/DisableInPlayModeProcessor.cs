using Pancake.Attribute;
using PancakeEditor.Attribute;
using UnityEngine;

[assembly: RegisterPropertyDisableProcessor(typeof(DisableInPlayModeProcessor))]

namespace PancakeEditor.Attribute
{
    public class DisableInPlayModeProcessor : PropertyDisableProcessor<DisableInPlayModeAttribute>
    {
        public override bool IsDisabled(Property property) { return Application.isPlaying != Attribute.Inverse; }
    }
}