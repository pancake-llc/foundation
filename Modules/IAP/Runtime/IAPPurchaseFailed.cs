using UnityEngine;

namespace Pancake.IAP
{
    [EditorIcon("scriptable_event")]
    public abstract class IAPPurchaseFailed : ScriptableObject
    {
        public abstract void Raise();
    }
}