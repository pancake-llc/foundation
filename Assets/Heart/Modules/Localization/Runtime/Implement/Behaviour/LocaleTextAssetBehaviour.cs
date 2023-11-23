using System;
using UnityEngine;

namespace Pancake.Localization
{
    public class LocaleTextAssetBehaviour : LocaleBehaviourGeneric<LocaleTextAsset, TextAsset>
    {
        protected override Type GetValueType() => typeof(string);

        protected override object GetLocaleValue() => Variable ? Variable.Value.text : null;
    }
}