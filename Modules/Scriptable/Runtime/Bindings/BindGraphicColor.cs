using Pancake.Attribute;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_bind")]
    [RequireComponent(typeof(Graphic))]
    public class BindingGraphicColor : RefPassenger<Graphic>
    {
        [SerializeField] private ColorVariable colorVariable;

        protected override void Awake()
        {
            base.Awake();
            Refresh(colorVariable);
            colorVariable.OnValueChanged += Refresh;
        }

        private void OnDestroy() { colorVariable.OnValueChanged -= Refresh; }

        private void Refresh(Color color) { component.color = color; }
    }
}