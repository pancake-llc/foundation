using UnityEngine;

namespace Obvious.Soap
{
    [AddComponentMenu("Soap/Bindings/BindRendererColor")]
    [RequireComponent(typeof(Renderer))]
    public class BindRendererColor : CacheComponent<Renderer>
    {
        [SerializeField] private ColorVariable _colorVariable = null;

        private MaterialPropertyBlock _block = null;
        
        protected override void Awake()
        {
            base.Awake();
            _block = new MaterialPropertyBlock();
            
            Refresh(_colorVariable);
            _colorVariable.OnValueChanged += Refresh;
        }

        private void OnDestroy()
        {
            _colorVariable.OnValueChanged -= Refresh;
        }

        private void Refresh(Color color)
        {
            _block.SetColor("_Color",color);
            _component.SetPropertyBlock(_block);
        }
    }
}