using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Scriptable
{
    [AddComponentMenu("Scriptable/Bindings/BindGraphicColor")]
    [RequireComponent(typeof(Graphic))]
    public class BindGraphicColor : CacheGameComponent<Graphic>
    {
        [SerializeField] private ColorVariable _colorVariable = null;

        protected override void Awake()
        {
            base.Awake();
            Refresh(_colorVariable);
            _colorVariable.OnValueChanged += Refresh;
        }

        private void OnDestroy() { _colorVariable.OnValueChanged -= Refresh; }

        private void Refresh(Color color) { component.color = color; }
    }
}