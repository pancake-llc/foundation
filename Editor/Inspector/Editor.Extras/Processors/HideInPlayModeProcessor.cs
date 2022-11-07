using Pancake.Editor;
using UnityEngine;

[assembly: RegisterPropertyHideProcessor(typeof(HideInPlayModeProcessor))]

namespace Pancake.Editor
{
    public class HideInPlayModeProcessor : PropertyHideProcessor<HideInPlayModeAttribute>
    {
        public override bool IsHidden(Property property) { return Application.isPlaying != Attribute.Inverse; }
    }
}