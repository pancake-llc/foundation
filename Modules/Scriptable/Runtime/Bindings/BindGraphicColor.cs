using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Scriptable
{
    [RequireComponent(typeof(Graphic))]
    public class BindingGraphicColor : CacheComponent<Graphic>
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