using UnityEngine;

namespace Pancake.IAP
{
    [EditorIcon("scriptable_event")]
    public class IAPPurchaseSuccess : ScriptableObject
    {
        public virtual void Raise() { }
    }
}