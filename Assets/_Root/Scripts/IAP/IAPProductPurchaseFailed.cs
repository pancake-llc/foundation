using Pancake.IAP;
using UnityEngine;

namespace Pancake.SceneFlow
{
    [CreateAssetMenu(fileName = "iap_product_purchase_failed", menuName = "Pancake/IAP/Product Purchase Faild Listener", order = 3003)]
    [EditorIcon("scriptable_event_listener")]
    public class IAPProductPurchaseFailed : IAPPurchaseFailed
    {
        public override void Raise() { }
    }
}