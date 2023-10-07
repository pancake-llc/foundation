using Pancake.SceneFlow;
using Pancake.Threading.Tasks;
using UnityEngine;

namespace Pancake.UI
{
    public sealed class OutfitView : View
    {
        [SerializeField] private CharacterOutfitContainer outfitContainer;
        [SerializeField, SheetPickup] private string outfitSheetContents;
        private SheetContainer OutfitSlotsContainer => SheetContainer.Find("SheetOutfitSlots");


        protected override UniTask Initialize()
        {
            Setup();
            return UniTask.CompletedTask;
        }

        public override void Refresh() {  }

        private async void Setup()
        {
            await UniTask.WaitUntil(() => OutfitSlotsContainer != null);
            await OutfitSlotsContainer.Register(outfitSheetContents, sheetId: outfitSheetContents);
            OutfitSlotsContainer.Show(outfitSheetContents, false);
        }
    }
}