#if PANCAKE_IAP
using Pancake.Scriptable;

namespace Pancake.IAP
{
    using UnityEngine;

    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "iap_func_product_chanel.asset", menuName = "Pancake/IAP/Func Product Event")]
    public class ScriptableEventIAPFuncProduct : ScriptableEventFuncT_TResult<IAPDataVariable, bool>
    {
    }
}
#endif