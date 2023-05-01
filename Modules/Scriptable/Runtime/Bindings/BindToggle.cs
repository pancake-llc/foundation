using UnityEngine;
using UnityEngine.UI;

namespace Obvious.Soap
{
    [AddComponentMenu("Soap/Bindings/BindToggle")]
    [RequireComponent(typeof(Toggle))]
    public class BindToggle : CacheComponent<Toggle>
    {
        [SerializeField] private BoolVariable _boolVariable = null;

        protected override void Awake()
        {
            base.Awake();
            OnValueChanged(_boolVariable);
            _component.onValueChanged.AddListener(SetBoundVariable);
            _boolVariable.OnValueChanged += OnValueChanged;
        }

        private void OnDestroy()
        {
            _component.onValueChanged.RemoveListener(SetBoundVariable);
            _boolVariable.OnValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(bool value)
        {
            _component.isOn = value;
        }

        private void SetBoundVariable(bool value)
        {
            _boolVariable.Value = value;
        }
    }
}