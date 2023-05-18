using UnityEngine;

namespace Pancake.IAP
{
    [EditorIcon("scriptable_event")]
    public abstract class IAPPurchaseSuccess : ScriptableObject
    {
        public abstract void Raise();
    }
}