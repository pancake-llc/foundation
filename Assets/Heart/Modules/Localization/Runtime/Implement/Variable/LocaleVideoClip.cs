using System;
using UnityEngine;
using UnityEngine.Video;

namespace Pancake.Localization
{
    [CreateAssetMenu(menuName = "Pancake/Localization/VideoClip", fileName = "videoclip_localizevalue", order = 8)]
    [EditorIcon("so_yellow_videoclip")]
    [Searchable]
    [Serializable]
    public class LocaleVideoClip : LocaleVariable<VideoClip>
    {
        [Serializable]
        private class VideoClipLocaleItem : LocaleItem<VideoClip>
        {
        };

        [SerializeField] private VideoClipLocaleItem[] items = new VideoClipLocaleItem[1];

        // ReSharper disable once CoVariantArrayConversion
        public override LocaleItemBase[] LocaleItems => items;
    }
}