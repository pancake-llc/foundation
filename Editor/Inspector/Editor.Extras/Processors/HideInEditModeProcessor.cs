using Pancake.Editor;
using UnityEngine;

[assembly: RegisterPropertyHideProcessor(typeof(HideInEditModeProcessor))]

namespace Pancake.Editor
{
    public class HideInEditModeProcessor : PropertyHideProcessor<HideInEditModeAttribute>
    {
        public override bool IsHidden(Property property) { return Application.isPlaying == Attribute.Inverse; }
    }
}