using System;
using System.Linq;
using UnityEngine;

namespace Pancake.Localization
{
    [CreateAssetMenu(menuName = "Pancake/Localization/AudioClip", order = 7)]
    [EditorIcon("scriptable_yellow_audioclip")]
    public class LocaleAudioClip : LocaleVariable<AudioClip>
    {
        [Serializable]
        private class AudioClipLocaleItem : LocaleItem<AudioClip>
        {
        };

        [SerializeField] private AudioClipLocaleItem[] items = new AudioClipLocaleItem[1];
        public override LocaleItemBase[] LocaleItems => items.ToArray<LocaleItemBase>();
    }
}