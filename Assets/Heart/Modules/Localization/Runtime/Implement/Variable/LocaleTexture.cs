using System;
using System.Linq;
using UnityEngine;

namespace Pancake.Localization
{
    [CreateAssetMenu(menuName = "Pancake/Localization/Texture", order = 6)]
    public class LocaleTexture : LocaleVariable<Texture>
    {
        [Serializable]
        private class TextureLocaleItem : LocaleItem<Texture>
        {
        };
        
        [SerializeField] private TextureLocaleItem[] items = new TextureLocaleItem[1];

        public override LocaleItemBase[] LocaleItems => items.ToArray<LocaleItemBase>();
    }
}