using Pancake.IAP;
using UnityEngine;

namespace Pancake.SceneFlow
{
    [CreateAssetMenu(fileName = "iap_coin_purchase_success", menuName = "Pancake/IAP/Coin Purchase Success Listener", order = 3001)]
    [EditorIcon("scriptable_event_listener")]
    public class IAPCoinPurchaseSuccess : IAPPurchaseSuccess
    {
        [SerializeField] private int amount;

        public override void Raise() { UserData.AddCoin(amount); }
    }
}