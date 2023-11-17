using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Scriptable
{
    /// <summary>
    /// Binds a float variable to a slider
    /// </summary>
    [AddComponentMenu("Scriptable/Bindings/BindSlider")]
    [RequireComponent(typeof(Slider))]
    public class BindSlider : CacheGameComponent<Slider>
    {
        [SerializeField] private FloatVariable floatVariable;

        protected override void Awake()
        {
            base.Awake();
            OnValueChanged(floatVariable);
            component.onValueChanged.AddListener(SetBoundVariable);
            floatVariable.OnValueChanged += OnValueChanged;
        }

        private void OnDestroy()
        {
            component.onValueChanged.RemoveListener(SetBoundVariable);
            floatVariable.OnValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(float value) { component.value = value; }

        private void SetBoundVariable(float value) { floatVariable.Value = value; }
    }
}