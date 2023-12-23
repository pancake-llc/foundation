using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Pancake.Scriptable
{
    /// <summary>
    /// Binds a bool variable to a toggle
    /// </summary>
    [AddComponentMenu("Scriptable/Bindings/BindToggle")]
    [RequireComponent(typeof(Toggle))]
    public class BindToggle : CacheGameComponent<Toggle>
    {
         [SerializeField] private BoolVariable boolVariable;

        protected override void Awake()
        {
            base.Awake();
            OnValueChanged(boolVariable);
            component.onValueChanged.AddListener(SetBoundVariable);
            boolVariable.OnValueChanged += OnValueChanged;
        }

        private void OnDestroy()
        {
            component.onValueChanged.RemoveListener(SetBoundVariable);
            boolVariable.OnValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(bool value) { component.isOn = value; }

        private void SetBoundVariable(bool value) { boolVariable.Value = value; }
    }
}