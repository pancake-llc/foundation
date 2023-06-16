namespace Pancake.IAP
{
    using UnityEngine;

    [EditorIcon("scriptable_event")]
    public abstract class IAPPurchaseFailed : ScriptableObject
    {
        public abstract void Raise();
    }
}