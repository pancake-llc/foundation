using Pancake.Apex;
using Pancake.Scriptable;
using TMPro;
using UnityEngine;

namespace Pancake.SceneFlow
{
    [EditorIcon("cs")]
    public class CoinUpdater : GameComponent
    {
        [Message("Update text coin with current coin of user")] [SerializeField]
        private TextMeshProUGUI textCoin;

        [Message("Update text coin with temp value")] [SerializeField]
        private ScriptableEventNoParam eventUpdateCoin;

        [SerializeField] private ScriptableEventInt updateCoinWithValue;

        private void Start()
        {
            OnNoticeUpdateCoin();
            if (eventUpdateCoin != null) eventUpdateCoin.OnRaised += OnNoticeUpdateCoin;
            if (updateCoinWithValue != null) updateCoinWithValue.OnRaised += OnNoticeUpdateCoin;
        }

        private void OnNoticeUpdateCoin(int value)
        {
            int previousCoin = int.Parse(textCoin.text);
            textCoin.text = $"{previousCoin + value}";
        }

        private void OnNoticeUpdateCoin() { textCoin.text = UserData.GetCurrentCoin().ToString(); }
    }
}