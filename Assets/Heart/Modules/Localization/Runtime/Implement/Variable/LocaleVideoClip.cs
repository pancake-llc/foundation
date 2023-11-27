using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

namespace Pancake.Localization
{
    [CreateAssetMenu(menuName = "Pancake/Localization/VideoClip", order = 8)]
    public class LocaleVideoClip : LocaleVariable<VideoClip>
    {
        [Serializable]
        private class VideoClipLocaleItem : LocaleItem<VideoClip>
        {
        };

        [SerializeField] private VideoClipLocaleItem[] items = new VideoClipLocaleItem[1];
        public override LocaleItemBase[] LocaleItems => items.ToArray<LocaleItemBase>();
    }
}