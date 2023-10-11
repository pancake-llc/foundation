using Pancake.IAP;
using UnityEngine;

namespace Pancake.SceneFlow
{
    [CreateAssetMenu(fileName = "iap_doublecoin_purchase_success", menuName = "Pancake/IAP/Double Coin Purchase Success Listener", order = 3002)]
    [EditorIcon("scriptable_event_listener")]
    public class IAPDoubleCoinPurchaseSuccess : IAPPurchaseSuccess
    {
        public override void Raise()
        {
            // unlock feature double coin
            Data.Save(Constant.IAP_DOUBLE_COIN, true);
        }
    }
}