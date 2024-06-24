using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System;
using Pancake.Sound;
using UnityEditor;

namespace PancakeEditor.Sound
{
    public class SoundIdAdvancedDropdown : AdvancedDropdown
    {
        private const int MINIMUM_LINES_COUNT = 10;

        private readonly Action<int, string, ScriptableObject> _onSelectItem;

        public SoundIdAdvancedDropdown(AdvancedDropdownState state, Action<int, string, ScriptableObject> onSelectItem)
            : base(state)
        {
            _onSelectItem = onSelectItem;
            minimumSize = new Vector2(0f, EditorGUIUtility.singleLineHeight * MINIMUM_LINES_COUNT);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem(nameof(AudioStatic));

            //var childCount = 0;
          
            foreach (string guid in LibraryDataContainer.Data.Settings.guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath(path, typeof(IAudioAsset)) as IAudioAsset;

                if (asset != null && !string.IsNullOrEmpty(asset.AssetName))
                {
                    AdvancedDropdownItem item = null;
                    foreach (var entity in asset.GetAllAudioEntities())
                    {
                        item ??= new AdvancedDropdownItem(asset.AssetName);
                        item.AddChild(new SoundIdAdvancedDropdownItem(entity.Name, entity.Id, asset as ScriptableObject));
                    }

                    root.AddChild(item);
                    //childCount++;
                }
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is SoundIdAdvancedDropdownItem audioItem) _onSelectItem?.Invoke(audioItem.soundId, audioItem.name, audioItem.sourceAsset);

            base.ItemSelected(item);
        }
    }
}