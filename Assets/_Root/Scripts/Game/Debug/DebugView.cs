using Pancake.DebugView;
using VitalRouter;

namespace Pancake.Game
{
    [Routes]
    public partial class DebugView : DebugViewBase
    {
        private void Awake() { MapTo(Router.Default); }

        protected override void Configure()
        {
            base.Configure();
            pages.Add(new DailyRewardDebugPage());
        }

        public void OnShowDebug(ShowDebugCommand cmd) { ShowDebug(); }
    }
}