using Pancake.Localization;
using UnityEngine;

namespace Pancake.Game.UI
{
    [CreateAssetMenu(menuName = "Pancake/Game/Shop/Preview Reward Data")]
    public class ShopItemRewardConfig : ScriptableObject
    {
        [SerializeField] private EShopRewardType type;
        [SerializeField] private Sprite icon;
        [SerializeField] private LocaleText itemName;
        [SerializeField] private string prefix;
        [SerializeField] private ShopItemReward prefab;

        public EShopRewardType Type => type;
        public Sprite Icon => icon;
        public string Prefix => prefix;
        public LocaleText ItemName => itemName;
        public ShopItemReward Prefab => prefab;
    }
}