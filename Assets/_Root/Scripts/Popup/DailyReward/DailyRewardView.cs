using System.Collections.Generic;
using Pancake.Apex;
using Pancake.Component;
using Pancake.Monetization;
using Pancake.SceneFlow;
using Pancake.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class DailyRewardView : View
    {
        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonShop;
        [SerializeField] private Button buttonClaim;
        [SerializeField] private Button buttonClaimX5;
        [SerializeField] private TextMeshProUGUI textValueX5;
        [SerializeField, PopupPickup] private string popupShop;
        [SerializeField] private ScriptableEventVfxMagnet fxCoinSpawnEvent;
        [SerializeField] private RewardVariable rewardVariable;

        [SerializeField, Array] private List<DailyRewardVariable> datas;
        [SerializeField, Array] private DayComponent[] days;
        private PopupContainer MainPopupContainer => PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER);

        protected override UniTask Initialize()
        {
            buttonClose.onClick.AddListener(OnButtonClosePressed);
            buttonShop.onClick.AddListener(OnButtonShopPressed);
            buttonClaim.onClick.AddListener(OnButtonClaimPressed);
            buttonClaimX5.onClick.AddListener(OnButtonClaimX5Pressed);
            return UniTask.CompletedTask;
        }

        private void OnButtonClaimPressed()
        {
            buttonClaim.gameObject.SetActive(false);
            buttonClaimX5.gameObject.SetActive(false);
            var data = datas[UserData.GetCurrentDayDailyReward() - 1];
            if (data.Value.typeReward == TypeRewardDailyReward.Coin)
            {
                UserData.AddCoin(data.Value.amount);
                fxCoinSpawnEvent.Raise(days[UserData.GetCurrentDayDailyReward() - 1].transform.position, data.Value.amount);
            }
            else
            {
                // unlock outfit
                // show popup unlock
                data.Value.outfitUnit.Value.isUnlocked = true;
                data.Value.outfitUnit.Save();
            }

            if (UserData.GetCurrentDayDailyReward() == datas.Count)
            {
                UserData.SetCurrentDayDailyReward(1);
                UserData.NextWeekDailyReward();
            }

            data.Value.isClaimed = true;
        }

        private void OnButtonClaimX5Pressed()
        {
            buttonClaim.gameObject.SetActive(false);
            buttonClaimX5.gameObject.SetActive(false);
            rewardVariable.Context()
                .Show()
                .OnCompleted(() =>
                {
                    var data = datas[UserData.GetCurrentDayDailyReward() - 1];
                    if (data.Value.typeReward == TypeRewardDailyReward.Coin)
                    {
                        UserData.AddCoin(data.Value.amount * 5);
                        fxCoinSpawnEvent.Raise(days[UserData.GetCurrentDayDailyReward() - 1].transform.position, data.Value.amount * 5);
                    }

                    if (UserData.GetCurrentDayDailyReward() == datas.Count)
                    {
                        UserData.SetCurrentDayDailyReward(1);
                        UserData.NextWeekDailyReward();
                    }

                    data.Value.isClaimed = true;
                })
                .OnClosed(() =>
                {
                    buttonClaim.gameObject.SetActive(true);
                    buttonClaimX5.gameObject.SetActive(true);
                });
        }

        private void OnButtonClosePressed() { PopupHelper.Close(transform); }

        public override void Refresh()
        {
            textValueX5.text = (datas[UserData.GetCurrentDayDailyReward() - 1].Value.amount * 5).ToString();
            for (var i = 0; i < days.Length; i++)
            {
                var dayComponent = days[i];
                dayComponent.Init(datas[i]);
            }
        }

        private void OnButtonShopPressed() { MainPopupContainer.Push<ShopPopup>(popupShop, true); }
    }
}