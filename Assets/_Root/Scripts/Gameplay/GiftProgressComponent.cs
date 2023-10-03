using Pancake.Scriptable;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    public class GiftProgressComponent : GameComponent
    {
        [SerializeField] private IntVariable progressValue;
        [SerializeField] private ScriptableEventNoParam collectGiftEvent;
        [SerializeField] private Image progressBar;
        [SerializeField] private TextMeshProUGUI textPercent;

        protected override void OnEnabled() { progressValue.OnValueChanged += OnValueChanged; }

        private void OnValueChanged(int value)
        {
            textPercent.text = $"{value}%";
            progressBar.fillAmount = value / 100f;
        }

        protected override void OnDisabled() { progressValue.OnValueChanged -= OnValueChanged; }
    }
}