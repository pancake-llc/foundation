#if PANCAKE_IAP
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.IAP
{
    [Searchable]
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "iap_purchase_product_chanel.asset", menuName = "Pancake/IAP/Product Event")]
    public class ScriptableEventIAPProduct : ScriptableEvent<IAPDataVariable>
    {
    }
}
#endif