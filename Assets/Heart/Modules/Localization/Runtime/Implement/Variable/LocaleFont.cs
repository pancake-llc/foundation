using System;
using System.Linq;
using UnityEngine;

namespace Pancake.Localization
{
    [CreateAssetMenu(menuName = "Pancake/Localization/Font", fileName = "font_localizevalue", order = 1)]
    [EditorIcon("scriptable_yellow_font")]
    public class LocaleFont : LocaleVariable<Font>
    {
        [Serializable]
        private class FontLocaleItem : LocaleItem<Font>
        {
        };

        [SerializeField] private FontLocaleItem[] items = new FontLocaleItem[1];
        public override LocaleItemBase[] LocaleItems => items.ToArray<LocaleItemBase>();
    }
}