using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.IAP
{
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "scriptable_event_iap_purchase.asset", menuName = "Pancake/Scriptable/ScriptableEvents/IAP Purchase")]
    public class ScriptableEventIAPPurchase : ScriptableEvent<IAPDataVariable>
    {
    }
}