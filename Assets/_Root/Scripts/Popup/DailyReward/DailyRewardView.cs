using System;
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
        [SerializeField] private BoolDailyVariable boolDailyVariable;

        [SerializeField, Array] private List<DailyRewardVariable> datas;
        [SerializeField, Array] private DayComponent[] days;
        private PopupContainer MainPopupContainer => PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER);

        protected override UniTask Initialize()
        {
            buttonClose.onClick.AddListener(OnButtonClosePressed);
            buttonShop.onClick.AddListener(OnButtonShopPressed);
            buttonClaim.onClick.AddListener(OnButtonClaimPressed);
            buttonClaimX5.onClick.AddListener(OnButtonClaimX5Pressed);
            boolDailyVariable.OnValueChanged += boolDailyVariable.RegisterTime;
            Refresh();
            return UniTask.CompletedTask;
        }

        private void OnButtonClaimPressed() { InternalClaim(); }

        private void InternalClaim(int coeffict = 1)
        {
            buttonClaim.gameObject.SetActive(false);
            buttonClaimX5.gameObject.SetActive(false);
            var data = datas[UserData.GetCurrentDayDailyReward() - 1];
            if (data.Value.typeReward == TypeRewardDailyReward.Coin)
            {
                UserData.AddCoin(data.Value.amount * coeffict);
                fxCoinSpawnEvent.Raise(days[UserData.GetCurrentDayDailyReward() - 1].transform.position, data.Value.amount * coeffict);
            }
            else
            {
                // unlock outfit
                // show popup unlock
                data.Value.outfitUnit.Value.isUnlocked = true;
            }

            if (UserData.GetCurrentDayDailyReward() == datas.Count)
            {
                UserData.SetCurrentDayDailyReward(1); // reset day
                UserData.NextWeekDailyReward(); // next week
            }
            else
            {
                UserData.NextDayDailyReward(); // next day
            }

            data.Value.isClaimed = true;
            boolDailyVariable.Value = true;
#if UNITY_EDITOR
            boolDailyVariable.Save();
#endif
            Refresh();
        }

        private void OnButtonClaimX5Pressed()
        {
            buttonClaim.gameObject.SetActive(false);
            buttonClaimX5.gameObject.SetActive(false);
            rewardVariable.Context()
                .Show()
                .OnCompleted(() => InternalClaim(5))
                .OnClosed(() =>
                {
                    buttonClaim.gameObject.SetActive(true);
                    buttonClaimX5.gameObject.SetActive(true);
                });
        }

        private void OnButtonClosePressed()
        {
            PlaySoundClose();
            PopupHelper.Close(transform);
        }

        private void Refresh()
        {
            boolDailyVariable.Load();
            if (!boolDailyVariable.Value)
            {
                buttonClaim.gameObject.SetActive(true);
                buttonClaimX5.gameObject.SetActive(true);
            }
            else
            {
                // alrealy claimed reward today
                buttonClaim.gameObject.SetActive(false);
                buttonClaimX5.gameObject.SetActive(false);
            }

            textValueX5.text = (datas[UserData.GetCurrentDayDailyReward() - 1].Value.amount * 5).ToString();
            for (var i = 0; i < days.Length; i++)
            {
                var dayComponent = days[i];
                dayComponent.Init(datas[i], boolDailyVariable);
            }
        }

        private void OnButtonShopPressed() { MainPopupContainer.Push<ShopPopup>(popupShop, true); }

        private void OnDestroy() { boolDailyVariable.OnValueChanged -= boolDailyVariable.RegisterTime; }
    }
}