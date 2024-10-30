using Pancake.DebugView;

namespace Pancake.Game
{
    public class DebugView : DebugViewBase
    {
        private MessageBinding<ShowDebugMessage> _binding;

        private void OnEnable()
        {
            _binding ??= new MessageBinding<ShowDebugMessage>(OnShowDebug);
            _binding.Listen = true;
        }

        private void OnDisable() { _binding.Listen = false; }

        protected override void Configure()
        {
            base.Configure();
            pages.Add(new DailyRewardDebugPage());
        }

        public void OnShowDebug(ShowDebugMessage msg) { ShowDebug(); }
    }
}