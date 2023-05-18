using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.IAP
{
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "scriptable_event_iap_product.asset", menuName = "Pancake/Scriptable/ScriptableEvents/IAP Product")]
    public class ScriptableEventIAPProduct : ScriptableEvent<IAPDataVariable>
    {
    }
}