using Alchemy.Inspector;
using Pancake.Component;
using TMPro;
using UnityEngine;
using VitalRouter;

namespace Pancake.SceneFlow
{
    [EditorIcon("icon_default")]
    [Routes]
    public partial class CurrencyUpdater : GameComponent
    {
        [SerializeField] private StringConstant type;

        [Blockquote("Update text with current currency of user")] [SerializeField]
        private TextMeshProUGUI text;

        private void Awake()
        {
            MapTo(Router.Default);
            NoticeUpdateCoinImpl();
        }

        public void OnNoticeUpdateCoinWithValue(UpdateCurrencyWithValueCommand cmd)
        {
            if (type.Value != cmd.TypeCurrency) return;
            NoticeUpdateCoinImpl(cmd.Value);
        }

        public void OnNoticeUpdateCoin(UpdateCurrencyCommand cmd)
        {
            if (type.Value != cmd.TypeCurrency) return;
            NoticeUpdateCoinImpl();
        }

        private void NoticeUpdateCoinImpl(int value)
        {
            int previousCoin = int.Parse(text.text);
            text.text = $"{previousCoin + value}";
        }

        private void NoticeUpdateCoinImpl() { text.text = UserData.GetCurrentCoin().ToString(); }
    }
}