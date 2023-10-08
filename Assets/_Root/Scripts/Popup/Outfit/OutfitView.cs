using Pancake.Linq;
using Pancake.SceneFlow;
using Pancake.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class OutfitView : View
    {
        [SerializeField] private Button buttonClose;
        [SerializeField] private CharacterOutfitContainer outfitContainer;
        [SerializeField, SheetPickup] private string outfitSheetContents;
        [SerializeField, SheetPickup] private string outfitSheetPreview;
        private SheetContainer OutfitSlotsContainer => SheetContainer.Find("SheetOutfitSlots");
        private SheetContainer SheetOutfitPreview => SheetContainer.Find("SheetOutfitPreview");


        protected override UniTask Initialize()
        {
            buttonClose.onClick.AddListener(OnbuttonClosePressed);
            Setup();
            SetupPreview();
            return UniTask.CompletedTask;
        }

        private void OnbuttonClosePressed()
        {
            PageHelper.Close(transform, false);
        }

        public override void Refresh() {  }

        private async void Setup()
        {
            await UniTask.WaitUntil(() => OutfitSlotsContainer != null);
            await OutfitSlotsContainer.Register(outfitSheetContents, sheetId: outfitSheetContents, onLoad: t =>
            {
                (t.sheet as OutfitCollectionSheet)?.view.Binding(outfitContainer.outfits.Filter(o => o.type == OutfitType.Hat).FirstOrDefault());
            });
            OutfitSlotsContainer.Show(outfitSheetContents, false);
        }

        private async void SetupPreview()
        {
            await UniTask.WaitUntil(()=>SheetOutfitPreview != null);
            await SheetOutfitPreview.Register(outfitSheetPreview,
                sheetId: outfitSheetPreview,
                onLoad: t =>
                {
                });
            SheetOutfitPreview.Show(outfitSheetPreview, false);
        }
    }
}