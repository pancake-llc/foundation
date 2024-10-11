using System;
using TMPro;
using UnityEngine;

namespace Pancake.Localization
{
    [CreateAssetMenu(menuName = "Pancake/Localization/TMPFont", fileName = "font_asset_localizevalue", order = 1)]
    [EditorIcon("so_yellow_fontasset")]
    [Searchable]
    [Serializable]
    public class LocaleTMPFont : LocaleVariable<TMP_FontAsset>
    {
        [Serializable]
        private class TMPFontLocaleItem : LocaleItem<TMP_FontAsset>
        {
        };

        [SerializeField] private TMPFontLocaleItem[] items = new TMPFontLocaleItem[1];

        // ReSharper disable once CoVariantArrayConversion
        public override LocaleItemBase[] LocaleItems => items;
    }
}