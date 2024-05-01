using System;
using UnityEngine;

namespace Pancake.Localization
{
    [EditorIcon("icon_default")]
    public class LocaleTextAssetComponent : LocaleComponentGeneric<LocaleTextAsset, TextAsset>
    {
        protected override Type GetValueType() => typeof(string);

        protected override object GetLocaleValue() => Variable ? Variable.Value.text : null;
    }
}