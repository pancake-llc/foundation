using System;
using UnityEngine;

namespace Pancake.Localization
{
    [CreateAssetMenu(menuName = "Pancake/Localization/Font", fileName = "font_localizevalue", order = 1)]
    [EditorIcon("so_yellow_font")]
    [Searchable]
    [Serializable]
    public class LocaleFont : LocaleVariable<Font>
    {
        [Serializable]
        private class FontLocaleItem : LocaleItem<Font>
        {
        };

        [SerializeField] private FontLocaleItem[] items = new FontLocaleItem[1];

        // ReSharper disable once CoVariantArrayConversion
        public override LocaleItemBase[] LocaleItems => items;
    }
}