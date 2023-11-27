using System;
using System.Linq;
using UnityEngine;

namespace Pancake.Localization
{
    [CreateAssetMenu(menuName = "Pancake/Localization/Texture", fileName = "texture_localizevalue", order = 6)]
    [EditorIcon("scriptable_yellow_texture")]
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