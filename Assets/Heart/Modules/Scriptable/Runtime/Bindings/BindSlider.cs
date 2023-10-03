using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Scriptable
{
    [AddComponentMenu("Scriptable/Bindings/BindSlider")]
    [RequireComponent(typeof(Slider))]
    public class BindSlider : CacheGameComponent<Slider>
    {
        [SerializeField] private FloatVariable _floatVariable = null;

        protected override void Awake()
        {
            base.Awake();
            OnValueChanged(_floatVariable);
            component.onValueChanged.AddListener(SetBoundVariable);
            _floatVariable.OnValueChanged += OnValueChanged;
        }

        private void OnDestroy()
        {
            component.onValueChanged.RemoveListener(SetBoundVariable);
            _floatVariable.OnValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(float value) { component.value = value; }

        private void SetBoundVariable(float value) { _floatVariable.Value = value; }
    }
}