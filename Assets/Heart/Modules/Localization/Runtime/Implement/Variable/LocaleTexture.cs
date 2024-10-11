using System;
using UnityEngine;

namespace Pancake.Localization
{
    [CreateAssetMenu(menuName = "Pancake/Localization/Texture", fileName = "texture_localizevalue", order = 6)]
    [EditorIcon("so_yellow_texture")]
    [Searchable]
    [Serializable]
    public class LocaleTexture : LocaleVariable<Texture>
    {
        [Serializable]
        private class TextureLocaleItem : LocaleItem<Texture>
        {
        };

        [SerializeField] private TextureLocaleItem[] items = new TextureLocaleItem[1];

        // ReSharper disable once CoVariantArrayConversion
        public override LocaleItemBase[] LocaleItems => items;
    }
}