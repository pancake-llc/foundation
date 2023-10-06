using Pancake.SceneFlow;
using Pancake.Threading.Tasks;
using UnityEngine;

namespace Pancake.UI
{
    public sealed class OutfitView : View
    {
        [SerializeField] private CharacterOutfitContainer outfitContainer;
        protected override UniTask Initialize() { return UniTask.CompletedTask; }

        public override void Refresh() { }
    }
}