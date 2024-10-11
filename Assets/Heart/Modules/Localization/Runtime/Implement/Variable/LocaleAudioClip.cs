using System;
using UnityEngine;

namespace Pancake.Localization
{
    [CreateAssetMenu(menuName = "Pancake/Localization/AudioClip", fileName = "audioclip_localizevalue", order = 7)]
    [EditorIcon("so_yellow_audioclip")]
    [Searchable]
    [Serializable]
    public class LocaleAudioClip : LocaleVariable<AudioClip>
    {
        [Serializable]
        private class AudioClipLocaleItem : LocaleItem<AudioClip>
        {
        };

        [SerializeField] private AudioClipLocaleItem[] items = new AudioClipLocaleItem[1];

        // ReSharper disable once CoVariantArrayConversion
        public override LocaleItemBase[] LocaleItems => items;
    }
}