using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Scriptable
{
    /// <summary>
    /// Binds a float variable to a filling image
    /// </summary>
    [AddComponentMenu("Scriptable/Bindings/BindFillingImage")]
    [RequireComponent(typeof(Image))]
    public class BindFillingImage : CacheGameComponent<Image>
    {
        [SerializeField] private FloatVariable floatVariable;
        [SerializeField] private FloatReference maxValue;

        protected override void Awake()
        {
            base.Awake();
            component.type = Image.Type.Filled;

            Refresh(floatVariable);
            floatVariable.OnValueChanged += Refresh;
        }

        private void OnDestroy() { floatVariable.OnValueChanged -= Refresh; }

        private void Refresh(float currentValue) { component.fillAmount = floatVariable.Value / maxValue.Value; }
    }
}