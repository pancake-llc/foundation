using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Pancake.Common;
using Pancake.Linq;
using Pancake.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Game.UI
{
    public class DailyRewardView : View
    {
        [SerializeField] private Button buttonClose;
        [SerializeField] private DailyRewardDayElement dayPrefab;
        [SerializeField] private Transform content;
        [SerializeField] private List<DailyRewardData> datas;
        [SerializeField] private DailyRewardData specialDayData;
        [SerializeField] private DailyRewardDayElement specialDayElement;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private Dictionary<EDailyRewardType, DayRewardComponent> _dayRewards = new();
        [SerializeField] private Color claimableColor;
        [SerializeField] private StringConstant coinCurrencyType;
        [SerializeField] private StringConstant gemCurrencyType;

        private readonly List<DailyRewardDayElement> _days = new();

        protected override UniTask Initialize()
        {
            buttonClose.onClick.AddListener(OnButtonClosePressed);

            foreach (var data in datas)
            {
                var dayElement = Instantiate(dayPrefab, content);
                dayElement.Setup(data,
                    GetRewardFunc,
                    GetFxCurrencyType,
                    claimableColor,
                    RefreshAll);
                _days.Add(dayElement);
            }

            specialDayElement.Setup(specialDayData,
                GetRewardFunc,
                GetFxCurrencyType,
                claimableColor,
                RefreshAll);
            App.Delay(0.1f,
                () =>
                {
                    int currentDay = UserData.GetDailyRewardDay();
                    var element = _days.Filter(d => d.Day == currentDay);
                    if (element.IsNullOrEmpty() || currentDay == 1) scroll.normalizedPosition = new Vector2(0, 1);
                    else scroll.ScrollTo(element[0].transform);
                });
            return UniTask.CompletedTask;
        }

        private void RefreshAll()
        {
            for (var i = 0; i < _days.Count; i++)
            {
                var day = _days[i];
                day.Setup(datas[i],
                    GetRewardFunc,
                    GetFxCurrencyType,
                    claimableColor,
                    RefreshAll);
            }

            specialDayElement.Setup(specialDayData,
                GetRewardFunc,
                GetFxCurrencyType,
                claimableColor,
                RefreshAll);
        }

        private DayRewardComponent GetRewardFunc(EDailyRewardType rewardType) { return _dayRewards[rewardType]; }

        private StringConstant GetFxCurrencyType(EDailyRewardType rewardType)
        {
            switch (rewardType)
            {
                case EDailyRewardType.Coin:
                    return coinCurrencyType;
                case EDailyRewardType.Gem:
                    return gemCurrencyType;
                case EDailyRewardType.Chest:
                default:
                    return null;
            }
        }

        private void OnButtonClosePressed()
        {
            PlaySoundClose();
            PopupHelper.Close(transform);
        }
    }
}