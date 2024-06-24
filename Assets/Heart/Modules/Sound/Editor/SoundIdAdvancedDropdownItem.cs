using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace PancakeEditor.Sound
{
    public class SoundIdAdvancedDropdownItem : AdvancedDropdownItem
    {
        public readonly int soundId;
        public readonly ScriptableObject sourceAsset;

        public SoundIdAdvancedDropdownItem(string name, int soundId, ScriptableObject asset)
            : base(name)
        {
            this.soundId = soundId;
            sourceAsset = asset;
        }
    }
}