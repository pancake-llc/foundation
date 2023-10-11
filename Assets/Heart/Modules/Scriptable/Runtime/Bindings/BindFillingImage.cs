using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Scriptable
{
    [AddComponentMenu("Scriptable/Bindings/BindFillingImage")]
    [RequireComponent(typeof(Image))]
    public class BindFillingImage : CacheGameComponent<Image>
    {
        [SerializeField] private FloatVariable _floatVariable = null;
        [SerializeField] private FloatReference _maxValue = null;

        protected override void Awake()
        {
            base.Awake();
            component.type = Image.Type.Filled;

            Refresh(_floatVariable);
            _floatVariable.OnValueChanged += Refresh;
        }

        private void OnDestroy() { _floatVariable.OnValueChanged -= Refresh; }

        private void Refresh(float currentValue) { component.fillAmount = _floatVariable.Value / _maxValue.Value; }
    }
}