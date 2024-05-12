using Alchemy.Inspector;
using Pancake.Component;
using TMPro;
using UnityEngine;

namespace Pancake.SceneFlow
{
    [EditorIcon("icon_default")]
    public class CurrencyUpdater : GameComponent
    {
        [SerializeField] private StringConstant type;

        [Blockquote("Update text with current currency of user")] [SerializeField]
        private TextMeshProUGUI text;

        private EventBinding<UpdateCurrencyEvent> _updateCurrency;
        private EventBinding<UpdateCurrencyWithValueEvent> _updateCurrencyWithValue;

        private void Awake()
        {
            _updateCurrency = new EventBinding<UpdateCurrencyEvent>(OnNoticeUpdateCoin);
            _updateCurrencyWithValue = new EventBinding<UpdateCurrencyWithValueEvent>(OnNoticeUpdateCoinWithValue);
            NoticeUpdateCoinImpl();
        }

        private void OnNoticeUpdateCoinWithValue(UpdateCurrencyWithValueEvent arg)
        {
            if (type.Value != arg.typeCurrency) return;
            NoticeUpdateCoinImpl(arg.value);
        }

        private void OnNoticeUpdateCoin(UpdateCurrencyEvent arg)
        {
            if (type.Value != arg.typeCurrency) return;
            NoticeUpdateCoinImpl();
        }

        private void OnEnable()
        {
            _updateCurrency.Listen = true;
            _updateCurrencyWithValue.Listen = true;
        }

        private void OnDisable()
        {
            _updateCurrency.Listen = false;
            _updateCurrencyWithValue.Listen = false;
        }

        private void NoticeUpdateCoinImpl(int value)
        {
            int previousCoin = int.Parse(text.text);
            text.text = $"{previousCoin + value}";
        }

        private void NoticeUpdateCoinImpl() { text.text = UserData.GetCurrentCoin().ToString(); }
    }
}