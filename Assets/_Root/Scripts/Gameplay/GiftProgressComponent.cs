using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.SceneFlow
{
    public class GiftProgressComponent : GameComponent
    {
        [SerializeField] private Image progressBar;
        [SerializeField] private TextMeshProUGUI textPercent;

        protected void OnEnable()
        {
            //progressValue.OnValueChanged += OnValueChanged;
        }

        private void OnValueChanged(int value)
        {
            textPercent.text = $"{value}%";
            progressBar.fillAmount = value / 100f;
        }

        protected void OnDisable()
        {
            //progressValue.OnValueChanged -= OnValueChanged;
        }
    }
}