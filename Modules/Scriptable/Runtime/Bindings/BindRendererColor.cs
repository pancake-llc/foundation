using UnityEngine;
using UnityEngine.Serialization;

namespace Pancake.Scriptable
{
    [AddComponentMenu("Scriptable/Bindings/BindRendererColor")]
    [RequireComponent(typeof(Renderer))]
    public class BindRendererColor : CacheGameComponent<Renderer>
    {
        [SerializeField] private ColorVariable colorVariable = null;

        private MaterialPropertyBlock _block = null;

        protected override void Awake()
        {
            base.Awake();
            _block = new MaterialPropertyBlock();

            Refresh(colorVariable);
            colorVariable.OnValueChanged += Refresh;
        }

        private void OnDestroy() { colorVariable.OnValueChanged -= Refresh; }

        private void Refresh(Color color)
        {
            _block.SetColor("_Color", color);
            component.SetPropertyBlock(_block);
        }
    }
}