using System;
using TMPro;
using UnityEngine;

namespace Pancake.Localization
{
    [CreateAssetMenu(menuName = "Pancake/Localization/TMPFont", fileName = "font_asset_localizevalue", order = 1)]
    [EditorIcon("scriptable_yellow_fontasset")]
    public class LocaleTMPFont : LocaleVariable<TMP_FontAsset>
    {
        [Serializable]
        private class FontLocaleItem : LocaleItem<TMP_FontAsset>
        {
        };

        [SerializeField] private FontLocaleItem[] items = new FontLocaleItem[1];
        
        // ReSharper disable once CoVariantArrayConversion
        public override LocaleItemBase[] LocaleItems => items;
    }
}