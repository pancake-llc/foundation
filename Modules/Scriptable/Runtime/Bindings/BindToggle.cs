using Pancake.Attribute;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_bind")]
    [RequireComponent(typeof(Toggle))]
    public class BindToggle : RefPassenger<Toggle>
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