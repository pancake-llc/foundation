using System;

namespace Pancake.Game
{
    [EditorIcon("icon_default")]
    [Serializable]
    public class VibrationInitialization : BaseInitialization
    {
        public override void Init() { Vibration.Init(); }
    }
}