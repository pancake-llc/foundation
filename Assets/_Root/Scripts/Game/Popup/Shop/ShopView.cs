using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using Pancake.Linq;
using Pancake.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Game.UI
{
    public sealed class ShopView : View
    {
#pragma warning disable 0414
        [SerializeField] private bool firstPurchaseDouble = true;
#pragma warning restore 0414
        [Header("Button")] [SerializeField] private UIButtonText buttonPackage;
        [SerializeField] private UIButtonText buttonGem;
        [SerializeField] private UIButtonText buttonItem;
        [SerializeField] private UIButtonText buttonEvent;
        [SerializeField] private Button buttonClose;

        [Header("Setting")] [SerializeField] private float durationAnimation = 0.1f;
        [SerializeField] private Color colorTextSelected;
        [SerializeField] private Color colorTextUnselected;
        [SerializeField] private Color colorTapSelected;
        [SerializeField] private Color colorTapUnselected;

        [Header("Pacakage")] [SerializeField] private ShopPackageTab packageTab;
        [SerializeField] private List<PackagePackData> packagesData;
        [SerializeField] private List<ShopItemRewardConfig> shopItemRewardConfigs;

        [Header("Gem")] [SerializeField] private ShopGemTap gemTap;
        [SerializeField] private List<PackagePackData> gemPackagesData;
        [SerializeField] private List<GemPackData> gemsData;

        private readonly Vector2 _maxTabHeigh = new(250, 107);
        private readonly Vector2 _minTabHeigh = new(250, 82);

        public enum ETapShop
        {
            Package,
            Gem,
            Item,
            Event,
        }

        private ETapShop _currentTap;

        private void Start() { Initialize(); }

        protected override UniTask Initialize()
        {
            _currentTap = ETapShop.Package;

            buttonClose.onClick.AddListener(OnButtonClosePressed);
            buttonPackage.onClick.AddListener(OnButtonPackagePressed);
            buttonGem.onClick.AddListener(OnButtonGemPressed);
            buttonItem.onClick.AddListener(OnButtonItemPressed);
            buttonEvent.onClick.AddListener(OnButtonEventPressed);
            RefreshVisual();
            return UniTask.CompletedTask;
        }

        private void RefreshVisual()
        {
            switch (_currentTap)
            {
                case ETapShop.Package:
                    packageTab.gameObject.SetActive(true);
                    gemTap.gameObject.SetActive(false);
                    packageTab.Initialize(packagesData, GetShopItemReward);
                    break;
                case ETapShop.Gem:
                    packageTab.gameObject.SetActive(false);
                    gemTap.gameObject.SetActive(true);
                    gemTap.Initialize(gemPackagesData, GetShopItemReward, gemsData);
                    break;
                case ETapShop.Item:
                    packageTab.gameObject.SetActive(false);
                    gemTap.gameObject.SetActive(false);
                    break;
                case ETapShop.Event:
                    packageTab.gameObject.SetActive(false);
                    gemTap.gameObject.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private ShopItemRewardConfig GetShopItemReward(EShopRewardType type) { return shopItemRewardConfigs.Filter(config => config.Type == type).First(); }

        private void OnButtonEventPressed()
        {
            if (_currentTap == ETapShop.Event) return;

            PlayAnimationDeselectedTab();
            _currentTap = ETapShop.Event;
            RefreshVisual();
            PlayAnimationSelectedTab();
        }

        private void OnButtonItemPressed()
        {
            if (_currentTap == ETapShop.Item) return;

            PlayAnimationDeselectedTab();
            _currentTap = ETapShop.Item;
            RefreshVisual();
            PlayAnimationSelectedTab();
        }

        private void OnButtonGemPressed()
        {
            if (_currentTap == ETapShop.Gem) return;

            PlayAnimationDeselectedTab();
            _currentTap = ETapShop.Gem;
            RefreshVisual();
            PlayAnimationSelectedTab();
        }

        private void OnButtonClosePressed()
        {
            PlaySoundClose();
            PopupHelper.Close(transform);
        }

        private void OnButtonPackagePressed()
        {
            if (_currentTap == ETapShop.Package) return;

            PlayAnimationDeselectedTab();
            _currentTap = ETapShop.Package;
            RefreshVisual();
            PlayAnimationSelectedTab();
        }

        private void PlayAnimationDeselectedTab()
        {
            var up = LMotion.Create(1f, 1.1f, durationAnimation);
            var down = LMotion.Create(1.1f, 1f, durationAnimation);
            var pos = LMotion.Create(_maxTabHeigh, _minTabHeigh, durationAnimation);
            switch (_currentTap)
            {
                case ETapShop.Package:
                    buttonPackage.Label.color = colorTextUnselected;
                    buttonPackage.image.color = colorTapUnselected;
                    up.WithEase(Ease.OutQuad)
                        .WithOnComplete(() => down.WithEase(Ease.OutBack).BindToLocalScaleY(buttonPackage.image.rectTransform))
                        .BindToLocalScaleY(buttonPackage.image.rectTransform);
                    pos.WithEase(Ease.Linear).BindToSizeDelta(buttonPackage.image.rectTransform);
                    break;
                case ETapShop.Gem:
                    buttonGem.Label.color = colorTextUnselected;
                    buttonGem.image.color = colorTapUnselected;
                    up.WithEase(Ease.OutQuad)
                        .WithOnComplete(() => down.WithEase(Ease.OutBack).BindToLocalScaleY(buttonGem.image.rectTransform))
                        .BindToLocalScaleY(buttonGem.image.rectTransform);
                    pos.WithEase(Ease.Linear).BindToSizeDelta(buttonGem.image.rectTransform);
                    break;
                case ETapShop.Item:
                    buttonItem.Label.color = colorTextUnselected;
                    buttonItem.image.color = colorTapUnselected;
                    up.WithEase(Ease.OutQuad)
                        .WithOnComplete(() => down.WithEase(Ease.OutBack).BindToLocalScaleY(buttonItem.image.rectTransform))
                        .BindToLocalScaleY(buttonItem.image.rectTransform);
                    pos.WithEase(Ease.Linear).BindToSizeDelta(buttonItem.image.rectTransform);
                    break;
                case ETapShop.Event:
                    buttonEvent.Label.color = colorTextUnselected;
                    buttonEvent.image.color = colorTapUnselected;
                    up.WithEase(Ease.OutQuad)
                        .WithOnComplete(() => down.WithEase(Ease.OutBack).BindToLocalScaleY(buttonEvent.image.rectTransform))
                        .BindToLocalScaleY(buttonEvent.image.rectTransform);
                    pos.WithEase(Ease.Linear).BindToSizeDelta(buttonEvent.image.rectTransform);
                    break;
            }
        }

        private void PlayAnimationSelectedTab()
        {
            var up = LMotion.Create(1f, 1.1f, durationAnimation);
            var down = LMotion.Create(1.1f, 1f, durationAnimation);
            var pos = LMotion.Create(_minTabHeigh, _maxTabHeigh, durationAnimation);
            switch (_currentTap)
            {
                case ETapShop.Package:
                    buttonPackage.Label.color = colorTextSelected;
                    buttonPackage.image.color = colorTapSelected;
                    up.WithEase(Ease.OutQuad)
                        .WithOnComplete(() => down.WithEase(Ease.OutBack).BindToLocalScaleY(buttonPackage.image.rectTransform))
                        .BindToLocalScaleY(buttonPackage.image.rectTransform);
                    pos.WithEase(Ease.Linear).BindToSizeDelta(buttonPackage.image.rectTransform);
                    break;
                case ETapShop.Gem:
                    buttonGem.Label.color = colorTextSelected;
                    buttonGem.image.color = colorTapSelected;
                    up.WithEase(Ease.OutQuad)
                        .WithOnComplete(() => down.WithEase(Ease.OutBack).BindToLocalScaleY(buttonGem.image.rectTransform))
                        .BindToLocalScaleY(buttonGem.image.rectTransform);
                    pos.WithEase(Ease.Linear).BindToSizeDelta(buttonGem.image.rectTransform);
                    break;
                case ETapShop.Item:
                    buttonItem.Label.color = colorTextSelected;
                    buttonItem.image.color = colorTapSelected;
                    up.WithEase(Ease.OutQuad)
                        .WithOnComplete(() => down.WithEase(Ease.OutBack).BindToLocalScaleY(buttonItem.image.rectTransform))
                        .BindToLocalScaleY(buttonItem.image.rectTransform);
                    pos.WithEase(Ease.Linear).BindToSizeDelta(buttonItem.image.rectTransform);
                    break;
                case ETapShop.Event:
                    buttonEvent.Label.color = colorTextSelected;
                    buttonEvent.image.color = colorTapSelected;
                    up.WithEase(Ease.OutQuad)
                        .WithOnComplete(() => down.WithEase(Ease.OutBack).BindToLocalScaleY(buttonEvent.image.rectTransform))
                        .BindToLocalScaleY(buttonEvent.image.rectTransform);
                    pos.WithEase(Ease.Linear).BindToSizeDelta(buttonEvent.image.rectTransform);
                    break;
            }
        }
    }
}