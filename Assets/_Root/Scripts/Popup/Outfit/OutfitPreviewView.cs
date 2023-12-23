using System;
using System.Collections.Generic;
using System.Linq;
using Pancake.Linq;
using Pancake.SceneFlow;
using Pancake.Scriptable;
using Pancake.Spine;
using Pancake.Threading.Tasks;
using Spine.Unity;
using UnityEngine;

namespace Pancake.UI
{
    public sealed class OutfitPreviewView : View
    {
        [SerializeField] private SkeletonGraphic render;
        [SerializeField] private CharacterOutfitContainer outfitContainer;
        [SerializeField] private ScriptableEventNoParam eventUpdatePreview;
        [SerializeField] private ScriptableEventPreviewLockedOutfit eventPreviewLockedOutfit;
        [SerializeField, SpineAnimation] private string dressUpAnim;
        [SerializeField, SpineAnimation] private string idleAnim;

        private DelayHandle _handleDelayDressUp;
        private readonly Dictionary<OutfitType, string> _currentPreviewOutfit = new Dictionary<OutfitType, string>();

        protected override UniTask Initialize()
        {
            eventUpdatePreview.OnRaised += Refresh;
            eventPreviewLockedOutfit.OnRaised += RefreshByType;
            Refresh();
            return UniTask.CompletedTask;
        }

        private void Refresh()
        {
            string hat = GetSkinOrDefault(OutfitType.Hat);
            string shirt = GetSkinOrDefault(OutfitType.Shirt);
            string shoes = GetSkinOrDefault(OutfitType.Shoe);

            if (!_currentPreviewOutfit.TryAdd(OutfitType.Hat, hat)) _currentPreviewOutfit[OutfitType.Hat] = hat;
            if (!_currentPreviewOutfit.TryAdd(OutfitType.Shirt, shirt)) _currentPreviewOutfit[OutfitType.Shirt] = shirt;
            if (!_currentPreviewOutfit.TryAdd(OutfitType.Shoe, shoes)) _currentPreviewOutfit[OutfitType.Shoe] = shoes;

            render.ChangeSkins(BuildCharacterSkins());

            App.CancelDelay(_handleDelayDressUp);
            render.PlayOnly(dressUpAnim, loop: false);
            _handleDelayDressUp = App.Delay(target: render, render.Duration(dressUpAnim), () => render.PlayOnly(idleAnim, true));
        }

        private void RefreshByType(OutfitType type, string skinId)
        {
            string hat;
            string shirt;
            string shoes;
            switch (type)
            {
                case OutfitType.Hat:
                    hat = skinId;
                    shirt = GetSkinOrDefault(OutfitType.Shirt);
                    shoes = GetSkinOrDefault(OutfitType.Shoe);
                    break;
                case OutfitType.Shirt:
                    hat = GetSkinOrDefault(OutfitType.Hat);
                    shirt = skinId;
                    shoes = GetSkinOrDefault(OutfitType.Shoe);
                    break;
                case OutfitType.Shoe:
                    hat = GetSkinOrDefault(OutfitType.Hat);
                    shirt = GetSkinOrDefault(OutfitType.Shirt);
                    shoes = skinId;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            
            if (!_currentPreviewOutfit.TryAdd(OutfitType.Hat, hat)) _currentPreviewOutfit[OutfitType.Hat] = hat;
            if (!_currentPreviewOutfit.TryAdd(OutfitType.Shirt, shirt)) _currentPreviewOutfit[OutfitType.Shirt] = shirt;
            if (!_currentPreviewOutfit.TryAdd(OutfitType.Shoe, shoes)) _currentPreviewOutfit[OutfitType.Shoe] = shoes;
            
            render.ChangeSkins(BuildCharacterSkins());
            
            App.CancelDelay(_handleDelayDressUp);
            render.PlayOnly(dressUpAnim, loop: false);
            _handleDelayDressUp = App.Delay(target: render, render.Duration(dressUpAnim), () => render.PlayOnly(idleAnim, true));
        }

        /// <summary>
        /// get default skin by <paramref name="type"/> (skin type is beginer gift)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private string GetSkinOrDefault(OutfitType type)
        {
            string skinId;
            switch (type)
            {
                case OutfitType.Hat:
                    skinId = UserData.GetCurrentSkinHat();
                    if (!string.IsNullOrEmpty(skinId))
                    {
                        string tempId = skinId;
                        skinId = outfitContainer.outfits.First(t => t.type == OutfitType.Hat).list.First(o => o.Value.id.Equals(tempId)).Value.skinId;
                    }

                    break;
                case OutfitType.Shirt:
                    skinId = UserData.GetCurrentSkinShirt();
                    if (!string.IsNullOrEmpty(skinId))
                    {
                        string tempId = skinId;
                        skinId = outfitContainer.outfits.First(t => t.type == OutfitType.Shirt).list.First(o => o.Value.id.Equals(tempId)).Value.skinId;
                    }

                    break;
                case OutfitType.Shoe:
                    skinId = UserData.GetCurrentSkinShoes();
                    if (!string.IsNullOrEmpty(skinId))
                    {
                        string tempId = skinId;
                        skinId = outfitContainer.outfits.First(t => t.type == OutfitType.Shoe).list.First(o => o.Value.id.Equals(tempId)).Value.skinId;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            if (string.IsNullOrEmpty(skinId))
            {
                var result = outfitContainer.outfits.Filter(o => o.type == type).First().list.Filter(v => v.Value.unlockType == OutfitUnlockType.BeginnerGift).First();
                skinId = result.Value.skinId;
            }

            return skinId;
        }

        private string[] BuildCharacterSkins()
        {
            var list = new List<string>
            {
                "skin-base", // body
                "eyes/green", // eyes
                "nose/short" // nose
            };
            list.AddRange(_currentPreviewOutfit.Values.ToList());
            return list.ToArray();
        }

        private void OnDestroy()
        {
            eventUpdatePreview.OnRaised -= Refresh;
            eventPreviewLockedOutfit.OnRaised -= RefreshByType;
        }
    }
}