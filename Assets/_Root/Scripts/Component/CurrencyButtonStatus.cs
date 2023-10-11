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
        [SerializeField] private Button button;
        [SerializeField] private UIEffect uiEffect;

        private int _cost;

        public void Setup(int cost)
        {
            _cost = cost;
            textValue.SetText($"{_cost}");
            OnUpdateCurrency(UserData.GetCurrentCoin());
        }
        
        private void OnEnable() { updateCurrencyEvent.OnRaised += OnUpdateCurrency; }

        private void OnUpdateCurrency(int currentCurrency)
        {
            if (currentCurrency >= _cost)
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

        private void OnDisable() { updateCurrencyEvent.OnRaised -= OnUpdateCurrency; }
    }
}