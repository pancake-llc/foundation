using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Scriptable
{
    /// <summary>
    /// Binds a color variable to a graphic (works with UI and SpriteRenderer)
    /// </summary>
    [AddComponentMenu("Scriptable/Bindings/BindGraphicColor")]
    [RequireComponent(typeof(Graphic))]
    public class BindGraphicColor : CacheGameComponent<Graphic>
    {
        [SerializeField] private ColorVariable colorVariable = null;

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