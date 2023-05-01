using UnityEngine;
using UnityEngine.UI;

namespace Obvious.Soap
{
    [AddComponentMenu("Soap/Bindings/BindSlider")]
    [RequireComponent(typeof(Slider))]
    public class BindSlider : CacheComponent<Slider>
    {
        [SerializeField] private FloatVariable _floatVariable = null;

        protected override void Awake()
        {
            base.Awake();
            OnValueChanged(_floatVariable);
            _component.onValueChanged.AddListener(SetBoundVariable);
            _floatVariable.OnValueChanged += OnValueChanged;
        }

        private void OnDestroy()
        {
            _component.onValueChanged.RemoveListener(SetBoundVariable);
            _floatVariable.OnValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(float value)
        {
            _component.value = value;
        }

        private void SetBoundVariable(float value)
        {
            _floatVariable.Value = value;
        }
    }
}