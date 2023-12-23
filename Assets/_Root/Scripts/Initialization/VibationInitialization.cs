using Pancake.Apex;

namespace Pancake.SceneFlow
{
    [HideMonoScript]
    public class VibationInitialization : Initialize
    {
        public override void Init() { Vibration.Init(); }
    }
}