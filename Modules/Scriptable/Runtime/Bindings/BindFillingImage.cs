using UnityEngine;
using UnityEngine.UI;

namespace Obvious.Soap
{
    [AddComponentMenu("Soap/Bindings/BindFillingImage")]
    [RequireComponent(typeof(Image))]
    public class BindFillingImage : CacheComponent<Image>
    {
        [SerializeField] private FloatVariable _floatVariable = null;
        [SerializeField] private FloatReference _maxValue = null;

        protected override void Awake()
        {
            base.Awake();
            _component.type = Image.Type.Filled;
            
            Refresh(_floatVariable);
            _floatVariable.OnValueChanged += Refresh;
        }

        private void OnDestroy()
        {
            _floatVariable.OnValueChanged -= Refresh;
        }

        private void Refresh(float currentValue)
        {
            _component.fillAmount = _floatVariable.Value / _maxValue.Value;
        }
    }
}