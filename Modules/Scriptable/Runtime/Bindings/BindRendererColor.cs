using UnityEngine;

namespace Pancake.Scriptable
{
    [RequireComponent(typeof(Renderer))]
    public class BindRendererColor : CacheComponent<Renderer>
    {
        [SerializeField] private ColorVariable colorVariable;

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