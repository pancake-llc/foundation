using Coffee.UIEffects;
using Pancake.Scriptable;
using TMPro;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    public class CurrencyButtonStatus : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textValue;
        [SerializeField] private ScriptableEventInt updateCurrencyEvent;
        [SerializeField] private ScriptableEventNoParam updateImediateCurrencyEvent;
        [SerializeField] private Button button;
        [SerializeField] private UIEffect uiEffect;

        private int _cost;

        public void Setup(int cost)
        {
            _cost = cost;
            textValue.SetText($"{_cost}");
            OnUpdateCurrency();
        }

        private void OnEnable()
        {
            updateImediateCurrencyEvent.OnRaised += OnUpdateCurrency;
            updateCurrencyEvent.OnRaised += OnUpdateCurrency;
        }

        private void OnUpdateCurrency() { OnUpdateCurrency(0); }

        private void OnUpdateCurrency(int currentCurrency)
        {
            if (UserData.GetCurrentCoin() >= _cost)
            {
                textValue.color = Color.white;
                button.interactable = true;
                uiEffect.effectMode = EffectMode.None;
            }
            else
            {
                textValue.color = Color.red;
                button.interactable = false;
                uiEffect.effectMode = EffectMode.Grayscale;
            }
        }

        private void OnDisable()
        {
            updateImediateCurrencyEvent.OnRaised -= OnUpdateCurrency;
            updateCurrencyEvent.OnRaised -= OnUpdateCurrency;
        }
    }
}