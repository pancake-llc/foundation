using Pancake.IAP;
using UnityEngine;

namespace Pancake.SceneFlow
{
    [CreateAssetMenu(fileName = "iap_allpinskin_purchase_success", menuName = "Pancake/IAP/All Pin Skin Purchase Success Listener", order = 3000)]
    [EditorIcon("scriptable_event_listener")]
    public class IAPAllPinSkinPurchaseSuccess : IAPPurchaseSuccess
    {
        public override void Raise()
        {
            // unlock all skin
            Data.Save(Constant.IAP_UNLOCK_ALL_SKIN, true);
        }
    }
}