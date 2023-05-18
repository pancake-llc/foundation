using Pancake.Scriptable;

namespace Pancake.IAP
{
    using UnityEngine;

    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "scriptable_event_iap_func_product.asset", menuName = "Pancake/Scriptable/ScriptableEvents/IAP Func Product")]
    public class ScriptableEventIAPFuncProduct : ScriptableEventFunc<IAPDataVariable, bool>
    {
    }
}