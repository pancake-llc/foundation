using Pancake.IAP;
using UnityEngine;

namespace Pancake.Game.UI
{
    [CreateAssetMenu(fileName = "gem_purchase_success_event", menuName = "Pancake/Game/Shop/Gem Purchase Success Event", order = 33)]
    [EditorIcon("so_blue_event")]
    public class GemPurchaseSuccess : IAPPurchaseSuccess
    {
        [SerializeField] private int amount;

        public override void Raise()
        {
            int amoutAdded = amount;
            bool isFirstPurchase = UserData.GetFirstPurchase();
            if (!isFirstPurchase)
            {
                amoutAdded *= 2;
                UserData.SetFirstPurchase(true);
            }
            // todo
        }
    }
}