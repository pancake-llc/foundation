﻿using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Scriptable
{
    [RequireComponent(typeof(Image))]
    public class BindFillingImage : CacheComponent<Image>
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