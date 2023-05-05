using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.IAP
{
    [Searchable]
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "iap_purchase_channel.asset", menuName = "Pancake/IAP/PurchaseEvent")]
    public class IAPPurchaseEvent : ScriptableEventFunc<ProductVarriable, IAPData>
    {
    }
}