using Pancake.Attribute;
using PancakeEditor.Attribute;
using UnityEngine;

[assembly: RegisterPropertyDisableProcessor(typeof(DisableInEditModeProcessor))]

namespace PancakeEditor.Attribute
{
    public class DisableInEditModeProcessor : PropertyDisableProcessor<DisableInEditModeAttribute>
    {
        public override bool IsDisabled(Property property) { return Application.isPlaying == Attribute.Inverse; }
    }
}