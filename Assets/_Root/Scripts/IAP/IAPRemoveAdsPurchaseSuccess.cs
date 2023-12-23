using Pancake.IAP;
using Pancake.Monetization;
using UnityEngine;

namespace Pancake.SceneFlow
{
    [CreateAssetMenu(fileName = "iap_removeads_purchase_success", menuName = "Pancake/IAP/Remove Ads Purchase Success Listener", order = 3004)]
    [EditorIcon("scriptable_event_listener")]
    public class IAPRemoveAdsPurchaseSuccess : IAPPurchaseSuccess
    {
        public override void Raise()
        {
            // remove ads
            AdStatic.IsRemoveAd = true;
        }
    }
}