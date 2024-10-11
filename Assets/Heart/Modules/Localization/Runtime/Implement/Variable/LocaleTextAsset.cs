using System;
using System.Linq;
using UnityEngine;

namespace Pancake.Localization
{
    [CreateAssetMenu(menuName = "Pancake/Localization/TextAsset", fileName = "textasset_localizevalue", order = 5)]
    [EditorIcon("so_yellow_textasset")]
    [Searchable]
    [Serializable]
    public class LocaleTextAsset : LocaleVariable<TextAsset>
    {
        [Serializable]
        private class TextAssetLocaleItem : LocaleItem<TextAsset>
        {
        };

        [SerializeField] private TextAssetLocaleItem[] items = new TextAssetLocaleItem[1];

        // ReSharper disable once CoVariantArrayConversion
        public override LocaleItemBase[] LocaleItems => items;
    }
}