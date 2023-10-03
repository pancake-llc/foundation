using Pancake.IAP;
using Pancake.Monetization;
using UnityEngine;

namespace Pancake.SceneFlow
{
    [CreateAssetMenu(fileName = "iap_vip_purchase_success", menuName = "Pancake/IAP/Vip Purchase Success Listener", order = 3005)]
    [EditorIcon("scriptable_event_listener")]
    public class IAPVipPurchaseSuccess : IAPPurchaseSuccess
    {
        public override void Raise()
        {
            // remove ads
            AdStatic.IsRemoveAd = true;

            // double coin
            Data.Save(Constant.IAP_DOUBLE_COIN, true);

            // unlock all pin skin
            Data.Save(Constant.IAP_UNLOCK_ALL_SKIN, true);

            // save state purchase pack vip
            Data.Save(Constant.IAP_VIP, true);
        }
    }
}