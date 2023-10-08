using System.Collections.Generic;
using Pancake.Linq;
using Pancake.SceneFlow;
using Pancake.Threading.Tasks;
using UnityEngine;

namespace Pancake.UI
{
    public sealed class OutfitColelctionView : View
    {
        [SerializeField] private OutfitType outfitType;
        [SerializeField] private int chunkSize;
        [SerializeField] private OutfitSlotBarComponent slotBarPrefab;
        [SerializeField] private Transform content;

        private CharacterOutfit _datas;
        private bool _isBinded;

        protected override UniTask Initialize()
        {
            Setup();
            return UniTask.CompletedTask;
        }

        public override void Refresh() { }

        public void Binding(CharacterOutfit filter)
        {
            _datas = filter;
            _isBinded = true;
        }

        public async void Setup()
        {
            await UniTask.WaitUntil(() => _isBinded);
            var units = _datas.list.Chunk(chunkSize);
            for (int i = 0; i < units.Length; i++)
            {
                var bar = Instantiate(slotBarPrefab, content);
                bar.Setup(units[i]);
            }
        }
    }
}