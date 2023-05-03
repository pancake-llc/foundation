using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Scriptable
{
    [AddComponentMenu("Scriptable/Bindings/BindToggle")]
    [RequireComponent(typeof(Toggle))]
    public class BindToggle : CacheGameComponent<Toggle>
    {
        [SerializeField] private BoolVariable _boolVariable = null;

        protected override void Awake()
        {
            base.Awake();
            OnValueChanged(_boolVariable);
            component.onValueChanged.AddListener(SetBoundVariable);
            _boolVariable.OnValueChanged += OnValueChanged;
        }

        private void OnDestroy()
        {
            component.onValueChanged.RemoveListener(SetBoundVariable);
            _boolVariable.OnValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(bool value) { component.isOn = value; }

        private void SetBoundVariable(bool value) { _boolVariable.Value = value; }
    }
}