using Pancake.Attribute;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_bind")]
    [RequireComponent(typeof(Image))]
    public class BindFillingImage : RefPassenger<Image>
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