using Pancake.Component;
using Pancake.Linq;
using Pancake.Monetization;
using Pancake.SceneFlow;
using Pancake.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.UI
{
    public sealed class OutfitColelctionView : View
    {
        [SerializeField] private OutfitType outfitType;
        [SerializeField] private int chunkSize;
        [SerializeField] private OutfitSlotBarComponent slotBarPrefab;
        [SerializeField] private Transform content;
        [SerializeField] private Button buttonFreeCoin;
        [SerializeField] private RewardVariable rewardAd;
        [SerializeField] private ScriptableEventVfxMagnet fxCoinSpawnEvent;
        [SerializeField] private int coinFreeValue = 500;

        private CharacterOutfit _datas;
        private OutfitSlotBarComponent[] _slotBars;

        protected override UniTask Initialize()
        {
            if (_slotBars != null)
            {
                var units = _datas.list.Chunk(chunkSize);
                for (int i = 0; i < units.Length; i++)
                {
                    _slotBars[i].Setup(units[i], outfitType);
                }
            }

            buttonFreeCoin.onClick.AddListener(OnButtonGetFreeCoinPressed);
            return UniTask.CompletedTask;
        }

        private void OnButtonGetFreeCoinPressed() { rewardAd.Context().OnCompleted(OnCompleteAdGetFreeCoin).Show(); }

        private void OnCompleteAdGetFreeCoin()
        {
            UserData.AddCoin(coinFreeValue);
            fxCoinSpawnEvent.Raise(buttonFreeCoin.transform.position, coinFreeValue);
        }

        public void Binding(CharacterOutfit filter, OutfitType outfitType)
        {
            this.outfitType = outfitType;
            _datas = filter;
            Setup();
        }

        public void Setup()
        {
            var units = _datas.list.Chunk(chunkSize);
            _slotBars = new OutfitSlotBarComponent[units.Length];
            for (int i = 0; i < units.Length; i++)
            {
                _slotBars[i] = Instantiate(slotBarPrefab, content);
                _slotBars[i].Setup(units[i], outfitType);
            }
        }
    }
}