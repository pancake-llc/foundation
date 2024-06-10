using Coffee.UIEffects;
using Pancake.Component;
using TMPro;
using UnityEngine.UI;
using VitalRouter;

namespace Pancake.SceneFlow
{
    using UnityEngine;

    [Routes]
    public partial class CurrencyButtonStatus : MonoBehaviour
    {
        [SerializeField] private StringConstant typeCurrency;
        [SerializeField] private TextMeshProUGUI textValue;
        [SerializeField] private Button button;
        [SerializeField] private UIEffect uiEffect;

        private int _cost;

        public void Setup(int cost)
        {
            _cost = cost;
            textValue.SetText($"{_cost}");
            OnUpdateCurrency(new UpdateCurrencyCommand(typeCurrency.Value));
        }

        public void OnUpdateCurrency(UpdateCurrencyCommand cmd)
        {
            // todo check typeCurrency
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
    }
}