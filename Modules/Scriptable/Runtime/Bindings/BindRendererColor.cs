using Pancake.Attribute;
using UnityEngine;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_bind")]
    [RequireComponent(typeof(Renderer))]
    public class BindRendererColor : RefPassenger<Renderer>
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