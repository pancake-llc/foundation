using UnityEngine;

namespace Pancake.Scriptable
{
    /// <summary>
    /// Binds a color variable to a renderer
    /// </summary>
    [AddComponentMenu("Scriptable/Bindings/BindRendererColor")]
    [RequireComponent(typeof(Renderer))]
    public class BindRendererColor : CacheGameComponent<Renderer>
    {
        [SerializeField] private ColorVariable colorVariable;

        private static readonly int Color1 = Shader.PropertyToID("_Color");

        protected override void Awake()
        {
            base.Awake();

            Refresh(colorVariable);
            colorVariable.OnValueChanged += Refresh;
        }

        private void OnDestroy() { colorVariable.OnValueChanged -= Refresh; }

        private void Refresh(Color color) { component.material.color = color; }
    }
}