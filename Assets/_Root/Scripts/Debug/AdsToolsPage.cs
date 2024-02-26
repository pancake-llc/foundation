using System.Threading.Tasks;
using Pancake.DebugView;
using Pancake.Monetization;

namespace Pancake.SceneFlow
{
    public class AdsToolsPage : DefaultDebugPageBase
    {
        private BannerVariable _bannerVariable;
        private InterVariable _interVariable;
        private RewardVariable _rewardVariable;
        private PickerCellModel _statePickerModel;

        public void Setup(BannerVariable banner, InterVariable inter, RewardVariable reward)
        {
            _bannerVariable = banner;
            _interVariable = inter;
            _rewardVariable = reward;
        }

        protected override string Title => "Ads Tools";

        public override Task Initialize()
        {
            string[] names = {"Enabled", "Disabled"};
            _statePickerModel = new PickerCellModel {Text = "State"};
            _statePickerModel.SetOptions(names, AdStatic.IsRemoveAd ? 1 : 0);
            _statePickerModel.ActiveOptionChanged += OnModelPickerValueChanged;
            AddPicker(_statePickerModel);
            AddButton("Show Banner", clicked: () => _bannerVariable.Context().Show());
            AddButton("Hide Banner", clicked: () => (_bannerVariable.Context() as IBannerHide)?.Hide());
            AddButton("Show Interstitial", clicked: () => _interVariable.Context().Show());
            AddButton("Show Rewarded", clicked: () => _rewardVariable.Context().Show());

            Reload();
            return Task.CompletedTask;
        }

        private void OnModelPickerValueChanged(int state) { AdStatic.IsRemoveAd = state == 1; }
    }
}