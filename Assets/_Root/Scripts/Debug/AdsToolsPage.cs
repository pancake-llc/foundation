// using System.Threading.Tasks;
// using Pancake.DebugView;
// using Pancake.Monetization;
//
// namespace Pancake.SceneFlow
// {
//     public class AdsToolsPage : DefaultDebugPageBase
//     {
//         private PickerCellModel _statePickerModel;
//
//         protected override string Title => "Ads Tools";
//
//         public override Task Initialize()
//         {
//             string[] names = {"Enabled", "Disabled"};
//             _statePickerModel = new PickerCellModel {Text = "State"};
//             _statePickerModel.SetOptions(names, Advertising.IsRemoveAd ? 1 : 0);
//             _statePickerModel.ActiveOptionChanged += OnModelPickerValueChanged;
//             AddPicker(_statePickerModel);
//             AddButton("Show Banner", clicked: () => Advertising.Banner?.Show());
//             AddButton("Hide Banner", clicked: () => (Advertising.Banner as IBannerHide)?.Hide());
//             AddButton("Show Interstitial", clicked: () => Advertising.Inter?.Show());
//             AddButton("Show Rewarded", clicked: () => Advertising.Reward?.Show());
//
//             Reload();
//             return Task.CompletedTask;
//         }
//
//         private void OnModelPickerValueChanged(int state) { Advertising.IsRemoveAd = state == 1; }
//     }
// }