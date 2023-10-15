using Pancake.Linq;
using Pancake.SceneFlow;
using Pancake.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class OutfitView : View
    {
        [SerializeField] private Button buttonClose;
        [SerializeField] private CharacterOutfitContainer outfitContainer;
        [SerializeField] private AssetReferenceGameObject skinHatCollectionSheet;
        [SerializeField] private AssetReferenceGameObject skinShirtCollectionSheet;
        [SerializeField] private AssetReferenceGameObject skinShoesCollectionSheet;
        [SerializeField, SheetPickup] private string outfitSheetPreview;
        [SerializeField] private Button buttonHat;
        [SerializeField] private Button buttonShirt;
        [SerializeField] private Button buttonShoes;
        [SerializeField] private Button buttonShop;
        [SerializeField, PopupPickup] private string popupShop;
        [SerializeField] private TabView tabHat;
        [SerializeField] private TabView tabShirt;
        [SerializeField] private TabView tabShoes;
        private SheetContainer OutfitSlotsContainer => SheetContainer.Find("SheetOutfitSlots");
        private SheetContainer SheetOutfitPreview => SheetContainer.Find("SheetOutfitPreview");
        private PopupContainer MainPopupContainer => PopupContainer.Find(Constant.MAIN_POPUP_CONTAINER);


        private OutfitType _currentOutfitType = OutfitType.Hat;
        private OutfitCollectionSheet _currentOutfitCollection;

        protected override UniTask Initialize()
        {
            buttonClose.onClick.AddListener(OnButtonClosePressed);
            buttonHat.onClick.AddListener(OnButtonHatPressed);
            buttonShirt.onClick.AddListener(OnButtonShirtPressed);
            buttonShoes.onClick.AddListener(OnButtonShoesPressed);
            buttonShop.onClick.AddListener(OnButtonShopPressed);
            Setup();
            SetupPreview();
            return UniTask.CompletedTask;
        }

        private void OnButtonShopPressed() { MainPopupContainer.Push<ShopPopup>(popupShop, true); }

        private void OnButtonShoesPressed()
        {
            if (_currentOutfitType == OutfitType.Shoe) return;
            _currentOutfitType = OutfitType.Shoe;
            OutfitSlotsContainer.Show(skinShoesCollectionSheet.RuntimeKey.ToString(), false);
            tabHat.Deactive(true);
            tabShirt.Deactive(true);
            tabShoes.Active(true);
        }

        private void OnButtonShirtPressed()
        {
            if (_currentOutfitType == OutfitType.Shirt) return;
            _currentOutfitType = OutfitType.Shirt;
            OutfitSlotsContainer.Show(skinShirtCollectionSheet.RuntimeKey.ToString(), false);
            tabHat.Deactive(true);
            tabShirt.Active(true);
            tabShoes.Deactive(true);
        }

        private void OnButtonHatPressed()
        {
            if (_currentOutfitType == OutfitType.Hat) return;
            _currentOutfitType = OutfitType.Hat;
            OutfitSlotsContainer.Show(skinHatCollectionSheet.RuntimeKey.ToString(), false);
            tabHat.Active(true);
            tabShirt.Deactive(true);
            tabShoes.Deactive(true);
        }

        private void OnButtonClosePressed()
        {
            PlaySoundClose();
            PageHelper.Close(transform, false);
        }

        private async void Setup()
        {
            await UniTask.WaitUntil(() => OutfitSlotsContainer != null);
#pragma warning disable 4014
            OutfitSlotsContainer.Register(skinShoesCollectionSheet.RuntimeKey.ToString(),
                sheetId: skinShoesCollectionSheet.RuntimeKey.ToString(),
                onLoad: t =>
                {
                    _currentOutfitCollection = (OutfitCollectionSheet) t.sheet;
                    _currentOutfitCollection.view.Binding(outfitContainer.outfits.Filter(o => o.type == OutfitType.Shoe).FirstOrDefault(), OutfitType.Shoe);
                });
            OutfitSlotsContainer.Register(skinShirtCollectionSheet.RuntimeKey.ToString(),
                sheetId: skinShirtCollectionSheet.RuntimeKey.ToString(),
                onLoad: t =>
                {
                    _currentOutfitCollection = (OutfitCollectionSheet) t.sheet;
                    _currentOutfitCollection.view.Binding(outfitContainer.outfits.Filter(o => o.type == OutfitType.Shirt).FirstOrDefault(), OutfitType.Shirt);
                });
            await OutfitSlotsContainer.Register(skinHatCollectionSheet.RuntimeKey.ToString(),
                sheetId: skinHatCollectionSheet.RuntimeKey.ToString(),
                onLoad: t =>
                {
                    _currentOutfitCollection = (OutfitCollectionSheet) t.sheet;
                    _currentOutfitCollection.view.Binding(outfitContainer.outfits.Filter(o => o.type == OutfitType.Hat).FirstOrDefault(), OutfitType.Hat);
                });
            OutfitSlotsContainer.Show(skinHatCollectionSheet.RuntimeKey.ToString(), false);
#pragma warning restore 4014
            tabHat.Active(true);
            tabShirt.Deactive(true);
            tabShoes.Deactive(true);
        }

        private async void SetupPreview()
        {
            await UniTask.WaitUntil(() => SheetOutfitPreview != null);
            await SheetOutfitPreview.Register(outfitSheetPreview, sheetId: outfitSheetPreview);
            await SheetOutfitPreview.Show(outfitSheetPreview, false);
        }
    }
}