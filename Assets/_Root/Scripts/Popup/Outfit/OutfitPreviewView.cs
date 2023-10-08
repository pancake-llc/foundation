using Pancake.SceneFlow;
using Pancake.Spine;
using Pancake.Threading.Tasks;
using Spine.Unity;
using UnityEngine;

namespace Pancake.UI
{
    public sealed class OutfitPreviewView : View
    {
        [SerializeField] private SkeletonGraphic render;
        private CharacterOutfitContainer _outfitContainer;
        protected override UniTask Initialize() { return UniTask.CompletedTask; }

        public override async void Refresh()
        {
            await UniTask.WaitUntil(()=> _outfitContainer != null);
            render.ChangeSkin("full-skins/boy");
        }

        public void Setup(CharacterOutfitContainer outfitContainer) { _outfitContainer = outfitContainer; }
    }
}