using System;
using System.Linq;
using UnityEngine;

namespace Pancake.Localization
{
    [CreateAssetMenu(menuName = "Pancake/Localization/Sprite", fileName = "sprite_localizevalue", order = 3)]
    [EditorIcon("so_yellow_sprite")]
    [Searchable]
    [Serializable]
    public class LocaleSprite : LocaleVariable<Sprite>
    {
        [Serializable]
        private class SpriteLocaleItem : LocaleItem<Sprite>
        {
        };

        [SerializeField] private SpriteLocaleItem[] items = new SpriteLocaleItem[1];

        // ReSharper disable once CoVariantArrayConversion
        public override LocaleItemBase[] LocaleItems => items.ToArray<LocaleItemBase>();
    }
}